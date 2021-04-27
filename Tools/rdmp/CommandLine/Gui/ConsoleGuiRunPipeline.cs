// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiRunPipeline : Window,IPipelineRunner, IDataLoadEventListener
    {
        private readonly IBasicActivateItems activator;
        private IPipelineUseCase useCase;
        private IPipeline pipeline;

        private TextView results;
        private TableView tableView;
        public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

        GracefulCancellationTokenSource cancellation;
        private int? exitCode;

        HashSet<IDataLoadEventListener> additionals = new HashSet<IDataLoadEventListener>();
        private PipelineRunner runner;
        private PipelineEngineEventArgs successArgs;
        private DataTable progressDataTable;

        public ConsoleGuiRunPipeline(IBasicActivateItems activator,IPipelineUseCase useCase, IPipeline pipeline)
        {
            Modal = true;
            this.activator = activator;
            this.useCase = useCase;
            this.pipeline = pipeline;

            Width = Dim.Fill();
            Height = Dim.Fill();

            Add(new Label("Pipeline:" + pipeline.Name));

            var btnRun = new Button("Run") { Y = 1 };
            btnRun.Clicked += BtnRun_Clicked;
            Add(btnRun);

            var btnCancel = new Button("Cancel") { Y = 1, X = Pos.Right(btnRun) };
            btnCancel.Clicked += BtnCancel_Clicked;
            Add(btnCancel);

            var btnClose = new Button("Close") { Y = 1, X = Pos.Right(btnCancel) };
            btnClose.Clicked += () => Application.RequestStop();
            Add(btnClose);

            Add(tableView = new TableView() { Y = 2, Width = Dim.Fill(), Height = 6 });

            progressDataTable = new DataTable();
            progressDataTable.Columns.Add("Name");
            progressDataTable.Columns.Add("Progress",typeof(int));

            tableView.Table = progressDataTable;

            Add(results = new TextView() { Y = Pos.Bottom(tableView), Width = Dim.Fill(), Height = Dim.Fill()});
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

            runner = new PipelineRunner(useCase, pipeline);
            foreach(var l in additionals)
            {
                runner.AdditionalListeners.Add(l);
            }
            runner.PipelineExecutionFinishedsuccessfully += Runner_PipelineExecutionFinishedsuccessfully;
            
            // clear old results
            results.Text = "";

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
                if(activator.YesNo("Close Window?","Pipeline completed successfully"))
                {
                    Application.RequestStop();
                }
                else
                {
                    return;
                }
            }
            else
            {
                // auto close because user wants no popup
                Application.RequestStop();
            }
        }


        public void OnNotify(object sender, NotifyEventArgs e)
        {
            results.Text += e.Message.Replace("\r\n","\n") + '\n';
            results.SetNeedsDisplay();
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

            tableView.Update();
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
    }
}