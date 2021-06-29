// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Startup;
using ReusableLibraryCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows
{

    class RunEngineWindow<T> : Window, IListDataSource where T : RDMPCommandLineOptions
    {
        private Process process;
        private ListView _results;
        protected readonly IBasicActivateItems BasicActivator;
        private readonly Func<T> commandGetter;

        private object lockList = new object();
        private List<string> consoleOutput = new List<string>();
        public int Count => consoleOutput.Count;
        public int Length => consoleOutput.Count;
        public RunEngineWindow(IBasicActivateItems activator, Func<T> commandGetter)
        {
            Modal = true;
            ColorScheme = ConsoleMainWindow.ColorScheme;

            var check = new Button("_Check") { X = 0 };
            check.Clicked += () => Check();
            Add(check);

            var execute = new Button("_Execute") { X = Pos.Right(check) };
            execute.Clicked += () => Execute();
            Add(execute);

            var clear = new Button("C_lear Output") { X = Pos.Right(execute) };
            clear.Clicked += () => ClearOutput();
            Add(clear);

            var abort = new Button("A_bort") { X = Pos.Right(clear) };
            abort.Clicked += () => Abort();
            Add(abort);

            var close = new Button("Cl_ose") { X = Pos.Right(abort) };
            close.Clicked += () =>
            {
                Application.RequestStop();
            };

            Add(close);

            _results = new ListView(this) { Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };
            Add(_results);
            _results.KeyPress += Results_KeyPress;

            BasicActivator = activator;
            this.commandGetter = commandGetter;
        }

        private void Results_KeyPress(KeyEventEventArgs obj)
        {
            if (obj.KeyEvent.Key == Key.Enter && _results.HasFocus)
            {
                var listIdx = _results.SelectedItem;
                var list = _results.Source.ToList();

                if (listIdx < list.Count)
                {
                    var selected = list[listIdx];
                    BasicActivator.Show(selected.ToString());
                }

                obj.Handled = true;
            }
        }
        private void ClearOutput()
        {
            lock (lockList)
            {
                consoleOutput.Clear();
                _results.SetNeedsDisplay();
            }
        }

        private void Abort()
        {
            try
            {
                if (process != null)
                    process.Kill();
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
                var opts = commandGetter();
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
                var opts = commandGetter();
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

            string expectedFileName = "rdmp" + (EnvironmentInfo.IsLinux ? "" : ".exe");

            // try in the location we ran from 
            var binary = Path.Combine(UsefulStuff.GetExecutableDirectory().FullName, expectedFileName);

            if (!File.Exists(binary))
            {
                // the program that launched this code isn't rdmp.exe.  Maybe rdmp is in the current directory though
                binary = "./" + expectedFileName;
            }

            if (!File.Exists(binary))
            {
                MessageBox.ErrorQuery("Could not find rdmp binary", $"Could not find {binary}", "Ok");
                return;
            }
            var cmd = new ExecuteCommandGenerateRunCommand(BasicActivator, commandGetter);
            var args = cmd.GetCommandText(true);

            process = new Process
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
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine().Trim();

                    lock(lockList)
                    {
                        consoleOutput.Add(line);
                        _results.SetNeedsDisplay();
                    }
                    
                }
            });
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
        {

            lock (lockList)
            {
                if (item >= consoleOutput.Count)
                {
                    return;
                }

                var str = consoleOutput[item];

                if (str.Length > width)
                {
                    str = str.Substring(0, width);
                }
                else
                {
                    str = str.PadRight(width,' ');
                }

                _results.Move(col, line);
                driver.AddStr(str);
            }                
        }

        public bool IsMarked(int item)
        {
            return false;
        }

        public void SetMark(int item, bool value)
        {
        }

        public IList ToList()
        {
            return consoleOutput;
        }
    }
}
