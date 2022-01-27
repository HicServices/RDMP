// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiRunPipeline : Window,IPipelineRunner, IDataLoadEventListener, IListDataSource
    {
        private readonly IBasicActivateItems activator;
        private IPipelineUseCase _useCase;
        private IPipeline _pipeline;
        private ListView _results;
        private TableView _tableView;
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

        Label _lblPipeline;
        private Pipeline[] _compatiblePipelines;

        public ConsoleGuiRunPipeline(IBasicActivateItems activator,IPipelineUseCase useCase, IPipeline pipeline)
        {
            Modal = true;
            this.activator = activator;
            this._useCase = useCase;
            this._pipeline = pipeline;

            ColorScheme = ConsoleMainWindow.ColorScheme;
            _compatiblePipelines = useCase.FilterCompatiblePipelines(activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>()).ToArray();

            Width = Dim.Fill();
            Height = Dim.Fill();

            if(pipeline == null && _compatiblePipelines.Length == 1)
            {
                this._pipeline = _compatiblePipelines[0];
            }
                        
            Add(_lblPipeline = new Label("Pipeline:" + (this._pipeline?.Name ?? "<<NOT SELECTED>>")) { Width = Dim.Fill()-20});

            var btnChoose = new Button("Choose Pipeline") { X = Pos.Right(_lblPipeline)};
            btnChoose.Clicked += BtnChoose_Clicked;
            Add(btnChoose);

            var btnRun = new Button("Run") { Y = 1 };
            btnRun.Clicked += BtnRun_Clicked;
            Add(btnRun);

            var btnCancel = new Button("Cancel") { Y = 1, X = Pos.Right(btnRun) };
            btnCancel.Clicked += BtnCancel_Clicked;
            Add(btnCancel);

            var btnClose = new Button("Close") { Y = 1, X = Pos.Right(btnCancel) };
            btnClose.Clicked += () => Application.RequestStop();
            Add(btnClose);

            Add(_tableView = new TableView() { Y = 2, Width = Dim.Fill(), Height = 7 });
            _tableView.Style.ShowHorizontalHeaderOverline = false;
            _tableView.Style.AlwaysShowHeaders = true;
            _tableView.Style.ShowVerticalCellLines = true;
            _tableView.Style.ShowHorizontalHeaderUnderline = false;

            progressDataTable = new DataTable();
            progressDataTable.Columns.Add("Name");
            progressDataTable.Columns.Add("Progress",typeof(int));

            _tableView.Table = progressDataTable;

            Add(_results = new ListView(this) { Y = Pos.Bottom(_tableView), Width = Dim.Fill(), Height = Dim.Fill()});
            _results.KeyPress += Results_KeyPress;


        }

        private void BtnChoose_Clicked()
        {
            _pipeline = (IPipeline)activator.SelectOne("Pipeline", _compatiblePipelines);
            _lblPipeline.Text = "Pipeline:" + (_pipeline?.Name ?? "<<NOT SELECTED>>");
        }

        private void Results_KeyPress(KeyEventEventArgs obj)
        {
            if(obj.KeyEvent.Key == Key.Enter && _results.HasFocus)
            {
                if(_results.SelectedItem <= notifyEventArgs.Count)
                {
                    var selected = notifyEventArgs[_results.SelectedItem];
                    
                    if(selected.Exception != null || selected.ProgressEventType == ProgressEventType.Error)
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
            if(cancellation == null)
            {
                MessageBox.ErrorQuery("Not Started","Pipeline execution has not started yet", "Ok");
                return;
            }

            if(cancellation.Token.IsAbortRequested)
            {
                MessageBox.ErrorQuery("Already Cancelled","Cancellation already issued","Ok");
            }
            else
            {
                cancellation.Abort();
            }
        }

        private void BtnRun_Clicked()
        {
            if(cancellation != null)
            {
                MessageBox.ErrorQuery("Already Running", "Pipeline is already running", "Ok");
                return;
            }

            runner = new PipelineRunner(_useCase, _pipeline);
            foreach(var l in additionals)
            {
                runner.AdditionalListeners.Add(l);
            }
            runner.PipelineExecutionFinishedsuccessfully += Runner_PipelineExecutionFinishedsuccessfully;
            
            // clear old results
            _results.Text = "";
            _results.SelectedItem = 0;
            Task.Run(()=>
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
            lock(notifyEventArgs)
            {
                notifyEventArgs.Insert(0,e);
                _results.SetNeedsDisplay();
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

            _tableView.Update();
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            // this blocks while the window is run
            Application.Run(this);

            // run the event after the window has closed
            if(successArgs != null)
            {
                PipelineExecutionFinishedsuccessfully?.Invoke(this, successArgs);
            }

            return exitCode ?? throw new OperationCanceledException();
        }

        public void SetAdditionalProgressListener(IDataLoadEventListener toAdd)
        {
            if(runner != null)
            {
                runner.AdditionalListeners.Add(toAdd);
            }
            else
            {
                additionals.Add(toAdd);
            }
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start=0)
        {
            if(item >= notifyEventArgs.Count)
            {
                return;
            }

            var str = notifyEventArgs[item].ProgressEventType + " " + notifyEventArgs[item].Message;

            if(str.Length > width)
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