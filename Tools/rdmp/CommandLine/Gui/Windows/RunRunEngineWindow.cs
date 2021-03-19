using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Startup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows
{
    class RunRunEngineWindow : Window
    {
        private Process process;
        private TextView textView;
        private readonly IBasicActivateItems activator;
        private readonly Func<RDMPCommandLineOptions> commandGetter;
        private object timer;

        public RunRunEngineWindow(IBasicActivateItems activator, Func<RDMPCommandLineOptions> commandGetter)
        {
            Modal = true;

            var check = new Button("Check") { X = 0 };
            check.Clicked += () => Check();
            this.Add(check);

            var execute = new Button("Execute") { X = Pos.Right(check) };
            execute.Clicked += () => Execute();
            this.Add(execute);

            var clear = new Button("Clear Output") { X = Pos.Right(execute) };
            clear.Clicked += () => ClearOutput();
            this.Add(clear);

            var abort = new Button("Abort") { X = Pos.Right(clear) };
            abort.Clicked += () => Abort();
            this.Add(abort);

            var close = new Button("Close") { X = Pos.Right(abort) };
            close.Clicked += () =>
            {
                Application.MainLoop.RemoveTimeout(timer);
                Application.RequestStop();
            };
                        
            this.Add(close);

            textView = new TextView() { ReadOnly = true, Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };
            this.Add(textView);

            this.activator = activator;
            this.commandGetter = commandGetter;

            timer = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(300), Tick);
        }

        private bool Tick(MainLoop arg)
        {
            textView.SetNeedsDisplay();
            return true;
        }

        private void ClearOutput()
        {
            textView.Text = "";
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
                activator.ShowException("Error Aborting Process", ex);
            }
        }

        private void Execute()
        {
            try
            {
                var opts = commandGetter();
                opts.Command = CommandLineActivity.run;

                AdjustCommand(opts);

                Run(() => opts);
            }
            catch (Exception ex)
            {
                activator.ShowException("Error Starting Execute", ex);
            }
        }

        private void AdjustCommand(RDMPCommandLineOptions opts)
        {
            if(opts is DleOptions dleOpts)
            {
                var lmd = activator.RepositoryLocator.CatalogueRepository.GetObjectByID<LoadMetadata>(dleOpts.LoadMetadata);
                
                if(lmd.LoadProgresses.Any())
                {
                    var lp = (LoadProgress)activator.SelectOne("Load Progres", lmd.LoadProgresses, null, true);
                    if (lp == null)
                        return;

                    dleOpts.LoadProgress = lp.ID;

                    if (activator.SelectValueType("Days to Load", typeof(int), lp.DefaultNumberOfDaysToLoadEachTime, out object chosen))
                        dleOpts.DaysToLoad = (int)chosen;
                    else
                        return;
                }
            }
        }

        private void Check()
        {
            try
            {
                var opts = commandGetter();
                opts.Command = CommandLineActivity.check;
                Run(() => opts);
            }
            catch (Exception ex)
            {
                activator.ShowException("Error Starting Checks", ex);
            }
        }

        private void Run(Func<RDMPCommandLineOptions> commandGetter)
        {
            ClearOutput();

            var binary = "./rdmp" + (EnvironmentInfo.IsLinux ? "" : ".exe");

            if (!File.Exists(binary))
            {
                MessageBox.ErrorQuery("Could not find rdmp binary", $"Could not find {binary}", "Ok");
                return;
            }
            var cmd = new ExecuteCommandGenerateRunCommand(activator, commandGetter);
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
                    textView.Text = line + "\n" + textView.Text?.Replace("\r\n","\n");
                }
            });
        }

    }
}
