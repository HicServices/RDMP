// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using Rdmp.Core.CommandLine.Runners;
using ReusableLibraryCode.Progress;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;

// The .Designer.cs file was built with https://github.com/tznind/TerminalGuiDesigner
// Use that tool for editing it

namespace Rdmp.Core.CommandLine.Gui
{
    public partial class ConsoleGuiRunPipeline : Window, IPipelineRunner, IDataLoadEventListener, IListDataSource
    {
        private readonly IBasicActivateItems activator;
        private IPipelineUseCase _useCase;
        private IPipeline _pipeline;
        public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

        GracefulCancellationTokenSource cancellation;
        private int? exitCode;

        HashSet<IDataLoadEventListener> additionals = new HashSet<IDataLoadEventListener>();
        private PipelineRunner runner;
        private PipelineEngineEventArgs successArgs;
        private DataTable progressDataTable;

        private List<NotifyEventArgs> notifyEventArgs = new List<NotifyEventArgs>();

        public int Count => notifyEventArgs.Count;
        public int Length => notifyEventArgs.Count;

        private Pipeline[] _compatiblePipelines;

        public ConsoleGuiRunPipeline(IBasicActivateItems activator, IPipelineUseCase useCase, IPipeline pipeline)
        {
            Modal = true;

            InitializeComponent();

            this.activator = activator;
            this._useCase = useCase;
            this._pipeline = pipeline;

            ColorScheme = ConsoleMainWindow.ColorScheme;
            _compatiblePipelines = useCase.FilterCompatiblePipelines(activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>()).ToArray();

            Width = Dim.Fill();
            Height = Dim.Fill();

            if (pipeline == null && _compatiblePipelines.Length == 1)
            {
                this._pipeline = _compatiblePipelines[0];
            }

            combobox1.Source = new ListWrapper(_compatiblePipelines);
            combobox1.AddKeyBinding(Key.CursorDown, Command.Expand);

            if (pipeline != null)
                combobox1.Text = _pipeline.ToString();

            progressDataTable = this._tableView.Table;
            progressDataTable.Columns["Progress"].DataType = typeof(int);

            _results.KeyPress += Results_KeyPress;
            btnRun.Clicked += BtnRun_Clicked;
            btnCancel.Clicked += BtnCancel_Clicked;
            btnClose.Clicked += () => Application.RequestStop();

            _results.Source = this;
        }

        private void Results_KeyPress(KeyEventEventArgs obj)
        {
            if (obj.KeyEvent.Key == Key.Enter && _results.HasFocus)
            {
                if (_results.SelectedItem <= notifyEventArgs.Count)
                {
                    var selected = notifyEventArgs[_results.SelectedItem];

                    if (selected.Exception != null || selected.ProgressEventType == ProgressEventType.Error)
                    {
                        activator.ShowException(selected.Message, selected.Exception);
                    }
                    else
                    {
                        activator.Show(selected.Message);
                    }
                }

                obj.Handled = true;
            }
        }

        private void BtnCancel_Clicked()
        {
            if (cancellation == null)
            {
                MessageBox.ErrorQuery("Not Started", "Pipeline execution has not started yet", "Ok");
                return;
            }

            if (cancellation.Token.IsAbortRequested)
            {
                MessageBox.ErrorQuery("Already Cancelled", "Cancellation already issued", "Ok");
            }
            else
            {
                cancellation.Abort();
            }
        }

        private void BtnRun_Clicked()
        {
            if (cancellation != null)
            {
                MessageBox.ErrorQuery("Already Running", "Pipeline is already running", "Ok");
                return;
            }


            IPipeline p;

            try
            {
                p = combobox1.Source.ToList()[combobox1.SelectedItem] as IPipeline ?? _pipeline;

            }
            catch (Exception)
            {

                MessageBox.ErrorQuery("No pipeline selected", "Select which Pipeline to use to run the data", "Ok");
                return;
            }

            runner = new PipelineRunner(_useCase, p);
            foreach (var l in additionals)
            {
                runner.AdditionalListeners.Add(l);
            }

            runner.PipelineExecutionFinishedsuccessfully += Runner_PipelineExecutionFinishedsuccessfully;

            // clear old results
            _results.Text = "";
            _results.SelectedItem = 0;
            Task.Run(() =>
            {
                try
                {
                    cancellation = new GracefulCancellationTokenSource();
                    exitCode = runner.Run(activator.RepositoryLocator, this, new FromDataLoadEventListenerToCheckNotifier(this), cancellation.Token);
                    cancellation = null;
                }
                catch (Exception ex)
                {
                    OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, ex.Message, ex));
                    cancellation = null;
                }
            });

        }

        private void Runner_PipelineExecutionFinishedsuccessfully(object sender, PipelineEngineEventArgs args)
        {
            successArgs = args;
            if (UserSettings.ShowPipelineCompletedPopup)
            {
                Application.MainLoop.Invoke(() =>
                {
                    if (MessageBox.Query("Pipeline completed successfully", "Close Window?", "Yes", "No") == 0)
                    {
                        Application.RequestStop();
                    }
                });
            }
            else
            {
                // auto close because user wants no popup
                Application.RequestStop();
            }
        }


        public void OnNotify(object sender, NotifyEventArgs e)
        {
            lock (notifyEventArgs)
            {
                notifyEventArgs.Insert(0, e);
                Application.MainLoop.Invoke(() => _results.SetNeedsDisplay());
            }
        }



        public void OnProgress(object sender, ProgressEventArgs e)
        {
            lock (progressDataTable)
            {
                var existing = progressDataTable.Rows.Cast<DataRow>().FirstOrDefault(r => Equals(r["Name"], e.TaskDescription));
                if (existing != null)
                {
                    existing["Progress"] = e.Progress.Value;
                }
                else
                {
                    progressDataTable.Rows.Add(e.TaskDescription, e.Progress.Value);
                }
            }

            Application.MainLoop.Invoke(() => _tableView.Update());
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            // this blocks while the window is run
            Application.Run(this);

            // run the event after the window has closed
            if (successArgs != null)
            {
                PipelineExecutionFinishedsuccessfully?.Invoke(this, successArgs);
            }

            return exitCode ?? throw new OperationCanceledException();
        }

        public void SetAdditionalProgressListener(IDataLoadEventListener toAdd)
        {
            if (runner != null)
            {
                runner.AdditionalListeners.Add(toAdd);
            }
            else
            {
                additionals.Add(toAdd);
            }
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
        {
            if (item >= notifyEventArgs.Count)
            {
                return;
            }

            var str = notifyEventArgs[item].ProgressEventType + " " + notifyEventArgs[item].Message;

            if (str.Length > width)
            {
                str = str.Substring(0, width);
            }
            else
            {
                str = str.PadRight(width, ' ');
            }

            _results.Move(col, line);
            driver.AddStr(str);
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
            return notifyEventArgs;
        }
    }
}
