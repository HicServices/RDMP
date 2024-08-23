// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

public partial class ConsoleGuiCohortIdentificationConfigurationUI
{
    private readonly IBasicActivateItems _activator;
    private CohortIdentificationConfigurationUICommon Common = new ();
    private bool _isDisposed;
    private List<object> RowObjects = new();
    private bool _contextMenuShowing = false;

    public ConsoleGuiCohortIdentificationConfigurationUI(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic)
    {
        InitializeComponent();

        Modal = true;

        _activator = activator;
        Common.Activator = activator;

        Common.Compiler.CoreChildProvider = activator.CoreChildProvider;

        Common.Configuration = cic;
        Common.Compiler.CohortIdentificationConfiguration = cic;

        cbCumulativeTotals.Toggled += e => { Common.SetShowCumulativeTotals(cbCumulativeTotals.Checked); };
        btnClearCache.Clicked += () =>
        {
            var cmd = new ExecuteCommandClearQueryCache(activator, Common.Configuration);
            if (!cmd.IsImpossible)
                cmd.Execute();
        };

        tableview1.CellActivated += Tableview1_CellActivated;
        tableview1.KeyPress += Tableview1_KeyPress;
        tbTimeout.TextChanged += s =>
        {
            if (int.TryParse(tbTimeout.Text.ToString(), out var t)) Common.Timeout = t;
        };

        btnRun.Clicked += () => { Common.StartAll(() => { }, RunnerOnPhaseChanged); };
        btnClose.Clicked += () =>
        {
            if (!Common.ConsultAboutClosing())
                Application.RequestStop();
        };
        btnAbort.Clicked += () => { Common.CancelAll(); };
        btnCommitCohort.Clicked += () =>
        {
            var cmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator, null)
                .SetTarget(cic);
            if (!cmd.IsImpossible)
                cmd.Execute();
            else
                MessageBox.ErrorQuery("Cannot Commit", cmd.ReasonCommandImpossible);
        };
        Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(2), RefreshTableCallback);

        // don't wait 2s for first build of the table
        BuildTable();

        ((ConsoleGuiActivator)activator).Published += Activator_Published;
    }

    private void Activator_Published(IMapsDirectlyToDatabaseTable obj)
    {
        Common.Activator = _activator;
        var descendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(obj);


        //if publish event was for a child of the cic (_cic is in the objects descendancy i.e. it sits below our cic)
        if (descendancy != null && descendancy.Parents.Contains(Common.Configuration))
        {
            //Go up descendency list clearing out the tasks above (and including) e.Object because it has changed
            foreach (var o in descendancy.Parents.Union(new[] { obj }))
            {
                var key = Common.GetKey(o);
                if (key != null)
                    Common.Compiler.CancelTask(key, true);
            }

            //TODO: this doesn't clear the compiler
            Common.RecreateAllTasks();
        }
    }

    private void Tableview1_KeyPress(KeyEventEventArgs obj)
    {
        var col = tableview1.SelectedColumn;
        var row = tableview1.SelectedRow;

        if (!IsValidSelection(col, row))
            return;

        var o = RowObjects[row];

        if (obj.KeyEvent.Key == Key.DeleteChar && o is IDeleteable d)
        {
            var cmdDelete = new ExecuteCommandDelete(_activator, d);
            if (!cmdDelete.IsImpossible)
                cmdDelete.Execute();
        }
    }

    private void RunnerOnPhaseChanged(object sender, EventArgs e)
    {
        Application.MainLoop.Invoke(() =>
        {
            lblState.Text = UsefulStuff.PascalCaseStringToHumanReadable(Common.Runner.ExecutionPhase.ToString());
        });

        Common.RecreateAllTasks(false);
    }

    private void Tableview1_CellActivated(TableView.CellActivatedEventArgs obj)
    {
        if (!IsValidSelection(obj.Col, obj.Row))
            return;

        var o = RowObjects[obj.Row];
        if (o == null)
            return;
        var col = tableview1.Table.Columns[obj.Col];

        switch (col.ColumnName)
        {
            case "Name":
            {
                var p = tableview1.CellToScreen(obj.Col, obj.Row);

                if (p == null)
                    return;

                var factory = new ConsoleGuiContextMenuFactory(_activator);
                var menu = factory.Create(p.Value.X,p.Value.Y,Array.Empty<object>(), o);
                if (menu != null)
                {
                    menu.Position = p.Value;
                    _contextMenuShowing = true;
                    menu.Show();
                    menu.MenuBar.MenuAllClosed += () => _contextMenuShowing = false;
                }

                break;
            }
            case "Working":
            {
                var key = Common.GetKey(o);
                if (key?.CrashMessage != null) _activator.ShowException("Task Crashed", key.CrashMessage);

                break;
            }
            case "Execute":
                Common.ExecuteOrCancel(o);
                break;
        }
    }

    private bool IsValidSelection(int col, int row)
    {
        if (col < 0 || row < 0)
            return false;

        return col <= tableview1.Table.Columns.Count && row <= tableview1.Table.Rows.Count;
    }

    protected override void Dispose(bool disposing)
    {
        _isDisposed = true;
        base.Dispose(disposing);
    }

    private bool RefreshTableCallback(MainLoop m)
    {
        // suspend rebuilding the table while other views are showing
        if (!IsCurrentTop)
            return true;

        if (!_isDisposed)
            BuildTable();

        return !_isDisposed;
    }

    private void BuildTable()
    {
        if (_contextMenuShowing)
            return;

        var childProvider = _activator.CoreChildProvider;

        var tbl = tableview1.Table;
        tbl.Rows.Clear();
        RowObjects.Clear();

        AddToTable(tbl, Common.Configuration, childProvider, 0);

        tableview1.Update();
    }


    private void AddToTable(DataTable tbl, object o, ICoreChildProvider childProvider, int indents)
    {
        var r = tbl.Rows.Add();
        RowObjects.Add(o);

        r["Name"] = new string(' ', indents) + o.ToString();
        r["Execute"] = Common.ExecuteAspectGetter(o);
        r["Cached"] = Common.Cached_AspectGetter(o);
        r["Count"] = Common.Count_AspectGetter(o);
        r["CumulativeTotal"] = Common.CumulativeTotal_AspectGetter(o);
        r["Working"] = Common.Working_AspectGetter(o);
        r["Time"] = Common.Time_AspectGetter(o);
        r["Catalogue"] = CohortIdentificationConfigurationUICommon.Catalogue_AspectGetter(o);
        r["ID"] = o is IMapsDirectlyToDatabaseTable m ? m.ID : DBNull.Value;

        var children = childProvider.GetChildren(o).ToList();
        children.Sort(new OrderableComparer(null));

        foreach (var c in children) AddToTable(tbl, c, childProvider, indents++);
    }
}