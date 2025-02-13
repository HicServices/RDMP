// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Startup;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows;

internal class RunEngineWindow<T> : Window, IListDataSource where T : RDMPCommandLineOptions
{
    private Process _process;
    private readonly ListView _results;
    protected readonly IBasicActivateItems BasicActivator;
    private readonly Func<T> _commandGetter;

    private readonly Lock _lockList = new();
    private readonly List<string> _consoleOutput = [];
    private readonly ColorScheme _red;
    private readonly ColorScheme _yellow;
    private readonly ColorScheme _white;

    public int Count { get; private set; }

    public int Length => Count;

    public RunEngineWindow(IBasicActivateItems activator, Func<T> commandGetter)
    {
        _red = ColorSettings.Instance.Red;
        _yellow = ColorSettings.Instance.Yellow;
        _white = ColorSettings.Instance.White;

        Modal = true;
        ColorScheme = ConsoleMainWindow.ColorScheme;

        var check = new Button("_Check") { X = 0 };
        check.Clicked += Check;
        Add(check);

        var execute = new Button("_Execute") { X = Pos.Right(check) };
        execute.Clicked += Execute;
        Add(execute);

        var clear = new Button("C_lear Output") { X = Pos.Right(execute) };
        clear.Clicked += ClearOutput;
        Add(clear);

        var abort = new Button("A_bort") { X = Pos.Right(clear) };
        abort.Clicked += Abort;
        Add(abort);

        var close = new Button("Cl_ose") { X = Pos.Right(abort) };
        close.Clicked += () => { Application.RequestStop(); };

        Add(close);

        _results = new ListView(this) { Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };
        Add(_results);
        _results.KeyPress += Results_KeyPress;

        BasicActivator = activator;
        this._commandGetter = commandGetter;
    }

    private void Results_KeyPress(KeyEventEventArgs obj)
    {
        if (obj.KeyEvent.Key != Key.Enter || !_results.HasFocus) return;

        var listIdx = _results.SelectedItem;
        var list = _results.Source.ToList();

        if (listIdx < list.Count)
        {
            var selected = list[listIdx];
            BasicActivator.Show(selected.ToString());
        }

        obj.Handled = true;
    }

    private void ClearOutput()
    {
        lock (_lockList)
        {
            _results.SelectedItem = 0;
            _consoleOutput.Clear();
            Count = 0;
            _results.SetNeedsDisplay();
        }
    }

    private void Abort()
    {
        try
        {
            _process?.Kill();
        }
        catch (Exception ex)
        {
            BasicActivator.ShowException("Error Aborting Process", ex);
        }
    }

    private void Execute()
    {
        try
        {
            var opts = _commandGetter();
            opts.Command = CommandLineActivity.run;

            AdjustCommand(opts, opts.Command);

            Run(() => opts);
        }
        catch (Exception ex)
        {
            BasicActivator.ShowException("Error Starting Execute", ex);
        }
    }

    /// <summary>
    /// Override in subclasses to get last minute choices e.g. what pipeline to use for an extraction
    /// </summary>
    /// <param name="opts"></param>
    protected virtual void AdjustCommand(T opts, CommandLineActivity activity)
    {
    }

    private void Check()
    {
        try
        {
            var opts = _commandGetter();
            opts.Command = CommandLineActivity.check;

            AdjustCommand(opts, opts.Command);

            Run(() => opts);
        }
        catch (Exception ex)
        {
            BasicActivator.ShowException("Error Starting Checks", ex);
        }
    }

    private void Run(Func<T> commandGetter)
    {
        ClearOutput();

        var expectedFileName = $"rdmp{(EnvironmentInfo.IsLinux ? "" : ".exe")}";

        // try in the location we ran from
        var binary = Path.Combine(UsefulStuff.GetExecutableDirectory().FullName, expectedFileName);

        if (!File.Exists(binary))
            // the program that launched this code isn't rdmp.exe.  Maybe rdmp is in the current directory though
            binary = $"./{expectedFileName}";

        if (!File.Exists(binary))
        {
            MessageBox.ErrorQuery("Could not find rdmp binary", $"Could not find {binary}", "Ok");
            return;
        }

        var cmd = new ExecuteCommandGenerateRunCommand(BasicActivator, commandGetter);
        var args = cmd.GetCommandText(true);

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = binary,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        Task.Run(() =>
        {
            _process.Start();

            while (!_process.StandardOutput.EndOfStream)
            {
                var line = _process.StandardOutput.ReadLine().Trim();

                lock (_lockList)
                {
                    _consoleOutput.Insert(0, line);
                    Count++;
                    Application.MainLoop.Invoke(() => _results.SetNeedsDisplay());
                }
            }
        });
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width,
        int start = 0)
    {
        lock (_lockList)
        {
            if (item >= Count) return;

            var str = _consoleOutput[item];

            str = str.Length > width ? str[..width] : str.PadRight(width, ' ');

            _results.Move(col, line);

            ColorScheme scheme;
            if (str.Contains("ERROR"))
                scheme = _red;
            else if (str.Contains("WARN"))
                scheme = _yellow;
            else
                scheme = _white;

            driver.SetAttribute(selected ? scheme.Focus : scheme.Normal);
            driver.AddStr(str);
        }
    }

    public bool IsMarked(int item) => false;

    public void SetMark(int item, bool value)
    {
    }

    public IList ToList()
    {
        lock (_lockList)
            return _consoleOutput.AsReadOnly();
    }
}