using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using log4net;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that executes a single .exe file specified by the user in a ProcessTask with ProcessTaskType Executable.  The exe will be given command line
    /// arguments for the database connection / loading directory via RuntimeArgumentCollection
    /// </summary>
    public class ExecutableRuntimeTask : RuntimeTask
    {
        public string ExeFilepath { get; set; }
        public string ErrorText { get; set; }
        private Process _currentProcess;

        private readonly ILog Log = LogManager.GetLogger(typeof(ExecutableRuntimeTask));
        
        public ExecutableRuntimeTask(IProcessTask processTask, RuntimeArgumentCollection args) : base(processTask, args)
        {
            ExeFilepath = processTask.Path;
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + ProcessTask.Name + "'"));

            var info = new ProcessStartInfo
            {
                FileName = ExeFilepath,
                Arguments = CreateArgString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Starting '" + info.FileName + "' with args '" + info.Arguments + "'"));

            _currentProcess = new Process {StartInfo = info};
            _currentProcess.OutputDataReceived += (sender, eventArgs) => Log.Info(eventArgs.Data);

            try
            {
                _currentProcess.Start();
            }
            catch (Exception e)
            {
                throw new Exception("Executable process '" + info.FileName + "' could not be started: " + e);
            }

            _currentProcess.BeginOutputReadLine();
            var reader = _currentProcess.StandardError;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Executable has been started successfully"));
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Waiting for executable to complete"));

            try
            {
                _currentProcess.WaitForExit();
            }
            catch (Exception e)
            {
                ErrorText = reader.ReadToEnd();
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, e.ToString(),e));
                throw new Exception("Exception whilst waiting for the executable process to finish: " + e);
            }

            var exitCode = ParseExitCode(_currentProcess.ExitCode);
            job.OnNotify(this, 
                new NotifyEventArgs(exitCode != ExitCodeType.Error ? ProgressEventType.Information : ProgressEventType.Error, "Executable has exited with state '" + exitCode + "'"));

            if (exitCode == ExitCodeType.Error)
            {
                ErrorText = reader.ReadToEnd();
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, ErrorText));
                throw new Exception("Executable process '" + info.FileName + " " + info.Arguments + "' failed: " + ErrorText);
            }
            _currentProcess = null;

            return exitCode;
        }



        public string CreateArgString()
        {
            var args = new List<string>();
            RuntimeArguments.IterateAllArguments((name, value) =>
            {
                if(value != null)
                    args.Add(CommandLineHelper.CreateArgString(name, value));
                return true;
            });
            return String.Join(" ", args);
        }

        private ExitCodeType ParseExitCode(int value)
        {
            ExitCodeType exitCode;
            var success = Enum.TryParse(value.ToString(), out exitCode);
            if (!success)
                throw new ArgumentException("Could not parse exit code from value: " + value);
            return exitCode;
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
                throw new Exception("Could not abort process '" + _currentProcess.ProcessName + "'" + e);
            }
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            
        }

        public override void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(ExeFilepath))
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Executable ProcessTask " + ProcessTask + " does not have a path specified",
                        CheckResult.Fail));
                return;
            }
            
            CommandLineParser parser = new CommandLineParser();

            //see if we can find what their executable is
            var exeParsed = parser.Parse(ExeFilepath).ToArray();

            //if it is an SQL file they are pointing us at by accident
            if (new FileInfo(exeParsed[0]).Extension.Equals(".sql")) //yes it is 
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "ProcessTask called " + ProcessTask.Name +
                        " is marked as an Executable but seems to point at an SQL file.  You should set the process task type to SQLFile instead",
                        CheckResult.Fail));

            //we cant tell
            if (exeParsed.Length == 0)
                throw new Exception("Could not parse process task " + ProcessTask.Path +
                                    " into a valid command line instruction for process task " + ProcessTask);

            //the first argument in a parsed windows command will be bob.exe, make sure it exists
            if (File.Exists(exeParsed[0]))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Found file " + exeParsed[0] + " referenced by " + ProcessTask.Name,
                        CheckResult.Success));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Process Task called " + ProcessTask.Name + " references a file called " + exeParsed[0] + " which does not exist at this time.", CheckResult.Warning));

        }

        public override string ToString()
        {
            return String.IsNullOrEmpty(ExeFilepath) ? "No executable" : ExeFilepath + " " + CreateArgString();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}