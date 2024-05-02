// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
///     RuntimeTask that executes a single .exe file specified by the user in a ProcessTask with ProcessTaskType
///     Executable.  The exe will be given command line
///     arguments for the database connection / loading directory via RuntimeArgumentCollection
/// </summary>
public class ExecutableRuntimeTask : RuntimeTask
{
    public string ExeFilepath { get; set; }
    public string ErrorText { get; set; }
    private Process _currentProcess;

    public ExecutableRuntimeTask(IProcessTask processTask, RuntimeArgumentCollection args) : base(processTask, args)
    {
        ExeFilepath = processTask.Path;
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"About to run Task '{ProcessTask.Name}'"));

        var info = new ProcessStartInfo
        {
            FileName = ExeFilepath,
            Arguments = CreateArgString(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Starting '{info.FileName}' with args '{info.Arguments}'"));

        _currentProcess = new Process { StartInfo = info };
        _currentProcess.OutputDataReceived += (sender, eventArgs) =>
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, eventArgs.Data));

        try
        {
            _currentProcess.Start();
        }
        catch (Exception e)
        {
            throw new Exception($"Executable process '{info.FileName}' could not be started: {e}");
        }

        _currentProcess.BeginOutputReadLine();
        var reader = _currentProcess.StandardError;

        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, "Executable has been started successfully"));
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Waiting for executable to complete"));

        try
        {
            _currentProcess.WaitForExit();
        }
        catch (Exception e)
        {
            ErrorText = reader.ReadToEnd();
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.ToString(), e));
            throw new Exception($"Exception whilst waiting for the executable process to finish: {e}");
        }

        var exitCode = ParseExitCode(_currentProcess.ExitCode);
        job.OnNotify(this,
            new NotifyEventArgs(
                exitCode != ExitCodeType.Error ? ProgressEventType.Information : ProgressEventType.Error,
                $"Executable has exited with state '{exitCode}'"));

        if (exitCode == ExitCodeType.Error)
        {
            ErrorText = reader.ReadToEnd();
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, ErrorText));
            throw new Exception($"Executable process '{info.FileName} {info.Arguments}' failed: {ErrorText}");
        }

        _currentProcess = null;

        return exitCode;
    }


    public string CreateArgString()
    {
        var args = new List<string>();
        RuntimeArguments.IterateAllArguments((name, value) =>
        {
            if (value != null)
                args.Add(CommandLineHelper.CreateArgString(name, value));
            return true;
        });
        return string.Join(" ", args);
    }

    private static ExitCodeType ParseExitCode(int value)
    {
        var success = Enum.TryParse(value.ToString(), out ExitCodeType exitCode);
        return !success ? throw new ArgumentException($"Could not parse exit code from value: {value}") : exitCode;
    }

    public override bool Exists()
    {
        return File.Exists(ExeFilepath);
    }

    public override void Abort(IDataLoadEventListener postDataLoadEventListener)
    {
        if (_currentProcess == null) return;

        try
        {
            if (!_currentProcess.HasExited)
                _currentProcess.Kill();
        }
        catch (Exception e)
        {
            throw new Exception($"Could not abort process '{_currentProcess.ProcessName}'{e}");
        }
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(ExeFilepath))
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs($"Executable ProcessTask {ProcessTask} does not have a path specified",
                    CheckResult.Fail));
            return;
        }

        var parser = new CommandLineParser();

        //see if we can find what their executable is
        var exeParsed = parser.Parse(ExeFilepath).ToArray();

        //if it is an SQL file they are pointing us at by accident
        if (new FileInfo(exeParsed[0]).Extension.Equals(".sql")) //yes it is
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"ProcessTask called {ProcessTask.Name} is marked as an Executable but seems to point at an SQL file.  You should set the process task type to SQLFile instead",
                    CheckResult.Fail));

        //we cant tell
        if (exeParsed.Length == 0)
            throw new Exception(
                $"Could not parse process task {ProcessTask.Path} into a valid command line instruction for process task {ProcessTask}");

        //the first argument in a parsed windows command will be bob.exe, make sure it exists
        if (File.Exists(exeParsed[0]))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found file {exeParsed[0]} referenced by {ProcessTask.Name}",
                    CheckResult.Success));
        else
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Process Task called {ProcessTask.Name} references a file called {exeParsed[0]} which does not exist at this time.",
                    CheckResult.Warning));
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(ExeFilepath) ? "No executable" : $"{ExeFilepath} {CreateArgString()}";
    }

    public static XmlSchema GetSchema()
    {
        return null;
    }
}