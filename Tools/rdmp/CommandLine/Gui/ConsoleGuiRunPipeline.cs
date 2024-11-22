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
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Terminal.Gui;

// The .Designer.cs file was built with https://github.com/tznind/TerminalGuiDesigner
// Use that tool for editing it

namespace Rdmp.Core.CommandLine.Gui;

public sealed partial class ConsoleGuiRunPipeline : IPipelineRunner, IDataLoadEventListener, IListDataSource
{
    private readonly IBasicActivateItems _activator;
    private readonly IPipelineUseCase _useCase;
    private readonly IPipeline _pipeline;
    public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

    private GracefulCancellationTokenSource _cancellation;
    private int? _exitCode;

    private readonly HashSet<IDataLoadEventListener> _additionals = new();
    private PipelineRunner _runner;
    private PipelineEngineEventArgs _successArgs;
    private readonly DataTable _progressDataTable;

    private readonly List<NotifyEventArgs> _notifyEventArgs = new();

    public int Count => _notifyEventArgs.Count;
    public int Length => _notifyEventArgs.Count;

    private readonly ColorScheme _red;
    private readonly ColorScheme _yellow;
    private readonly ColorScheme _white;

    public ConsoleGuiRunPipeline(IBasicActivateItems activator, IPipelineUseCase useCase, IPipeline pipeline)
    {
        Modal = true;

        _red = ColorSettings.Instance.Red;
        _yellow = ColorSettings.Instance.Yellow;
        _white = ColorSettings.Instance.White;

        InitializeComponent();

        this._activator = activator;
        _useCase = useCase;
        _pipeline = pipeline;

        ColorScheme = ConsoleMainWindow.ColorScheme;
        var compatiblePipelines = useCase
            .FilterCompatiblePipelines(activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>())
            .ToArray();

        Width = Dim.Fill();
        Height = Dim.Fill();

        if (pipeline == null && compatiblePipelines.Length == 1) _pipeline = compatiblePipelines[0];

        combobox1.Source = new ListWrapper(compatiblePipelines);
        combobox1.AddKeyBinding(Key.CursorDown, Command.Expand);

        if (_pipeline != null)
            combobox1.SelectedItem = combobox1.Source.ToList().IndexOf(_pipeline);

        _progressDataTable = _tableView.Table;
        if (_progressDataTable?.Columns["Progress"] != null) _progressDataTable.Columns["Progress"].DataType = typeof(int);

        _results.KeyPress += Results_KeyPress;
        btnRun.Clicked += BtnRun_Clicked;
        btnCancel.Clicked += BtnCancel_Clicked;
        btnClose.Clicked += static () => Application.RequestStop();

        _results.Source = this;
    }

    private void Results_KeyPress(KeyEventEventArgs obj)
    {
        if (obj.KeyEvent.Key == Key.Enter && _results.HasFocus)
        {
            if (_results.SelectedItem <= _notifyEventArgs.Count)
            {
                var selected = _notifyEventArgs[_results.SelectedItem];

                if (selected.Exception != null || selected.ProgressEventType == ProgressEventType.Error)
                    _activator.ShowException(selected.Message, selected.Exception);
                else
                    _activator.Show(selected.Message);
            }

            obj.Handled = true;
        }
    }

    private void BtnCancel_Clicked()
    {
        if (_cancellation == null)
        {
            MessageBox.ErrorQuery("Not Started", "Pipeline execution has not started yet", "Ok");
            return;
        }

        if (_cancellation.Token.IsAbortRequested)
            MessageBox.ErrorQuery("Already Cancelled", "Cancellation already issued", "Ok");
        else
            _cancellation.Abort();
    }

    private void BtnRun_Clicked()
    {
        if (_cancellation != null)
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

        _runner = new PipelineRunner(_useCase, p);
        foreach (var l in _additionals) _runner.AdditionalListeners.Add(l);

        _runner.PipelineExecutionFinishedsuccessfully += Runner_PipelineExecutionFinishedsuccessfully;

        // clear old results
        _results.Text = "";
        _results.SelectedItem = 0;
        Task.Run(() =>
        {
            try
            {
                _cancellation = new GracefulCancellationTokenSource();
                _exitCode = _runner.Run(_activator.RepositoryLocator, this,
                    new FromDataLoadEventListenerToCheckNotifier(this), _cancellation.Token);
                _cancellation = null;
            }
            catch (Exception ex)
            {
                OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, ex.Message, ex));
                _cancellation = null;
            }
        });
    }

    private void Runner_PipelineExecutionFinishedsuccessfully(object sender, PipelineEngineEventArgs args)
    {
        _successArgs = args;
        if (UserSettings.ShowPipelineCompletedPopup)
            Application.MainLoop.Invoke(static () =>
            {
                if (MessageBox.Query("Pipeline completed successfully", "Close Window?", "Yes", "No") == 0)
                    Application.RequestStop();
            });
        else
            // auto close because user wants no popup
            Application.RequestStop();
    }


    public void OnNotify(object sender, NotifyEventArgs e)
    {
        lock (_notifyEventArgs)
        {
            _notifyEventArgs.Insert(0, e);
            Application.MainLoop.Invoke(() => _results.SetNeedsDisplay());
        }
    }


    public void OnProgress(object sender, ProgressEventArgs e)
    {
        lock (_progressDataTable)
        {
            var existing = _progressDataTable.Rows.Cast<DataRow>()
                .FirstOrDefault(r => Equals(r["Name"], e.TaskDescription));
            if (existing != null)
                existing["Progress"] = e.Progress.Value;
            else
                _progressDataTable.Rows.Add(e.TaskDescription, e.Progress.Value);
        }

        Application.MainLoop.Invoke(() => _tableView.Update());
    }

    public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token)
    {
        // this blocks while the window is run
        Application.Run(this, ConsoleMainWindow.ExceptionPopup);

        // run the event after the window has closed
        if (_successArgs != null) PipelineExecutionFinishedsuccessfully?.Invoke(this, _successArgs);

        return _exitCode ?? throw new OperationCanceledException();
    }

    public void SetAdditionalProgressListener(IDataLoadEventListener toAdd)
    {
        if (_runner != null)
            _runner.AdditionalListeners.Add(toAdd);
        else
            _additionals.Add(toAdd);
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width,
        int start = 0)
    {
        if (item >= _notifyEventArgs.Count) return;

        var str = $"{_notifyEventArgs[item].ProgressEventType} {_notifyEventArgs[item].Message}";

        str = str.Length > width ? str[..width] : str.PadRight(width, ' ');

        var colour = _notifyEventArgs[item].ProgressEventType switch
        {
            ProgressEventType.Error => _red,
            ProgressEventType.Warning => _yellow,
            _ => _white
        };
        driver.SetAttribute(selected ? colour.Focus : colour.Normal);

        _results.Move(col, line);
        driver.AddStr(str);
    }

    public bool IsMarked(int item) => false;

    public void SetMark(int item, bool value)
    {
    }

    public IList ToList() => _notifyEventArgs;
}