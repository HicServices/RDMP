// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NStack;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terminal.Gui;


namespace Rdmp.Core.CommandLine.Gui;

public partial class ConsoleGuiUserSettings {
    private Dictionary<CheckBox, PropertyInfo> checkboxDictionary = new();

    public IBasicActivateItems _activator { get; }

    public ConsoleGuiUserSettings(IBasicActivateItems activator) {

        Modal = true;

        InitializeComponent();

        _activator = activator;

        tableview1.Update();

        RegisterCheckbox(cbHideCohortBuilderContainersInFind, nameof(UserSettings.ScoreZeroForCohortAggregateContainers));
        RegisterCheckbox(cbEnableCommits, nameof(UserSettings.EnableCommits));
        RegisterCheckbox(cbStrictValidationForContainers, nameof(UserSettings.StrictValidationForCohortBuilderContainers));
        RegisterCheckbox(cbHideEmptyTableLoadRunAudits, nameof(UserSettings.HideEmptyTableLoadRunAudits));
        RegisterCheckbox(cbShowPipelineCompletedPopup, nameof(UserSettings.ShowPipelineCompletedPopup));
        RegisterCheckbox(cbSkipCohortBuilderValidationOnCommit, nameof(UserSettings.SkipCohortBuilderValidationOnCommit));
        RegisterCheckbox(cbAlwaysJoinEverything, nameof(UserSettings.AlwaysJoinEverything));
        RegisterCheckbox(cbSelectiveRefresh, nameof(UserSettings.SelectiveRefresh));
        RegisterCheckbox(cbUseAliasInsteadOfTransformInGroupByAggregateGraphs, nameof(UserSettings.UseAliasInsteadOfTransformInGroupByAggregateGraphs));

        tbCreateDatabaseTimeout.Text = UserSettings.CreateDatabaseTimeout.ToString();
        tbCreateDatabaseTimeout.TextChanged += tbCreateDatabaseTimeout_TextChanged;

        tbArchiveTriggerTimeout.Text = UserSettings.ArchiveTriggerTimeout.ToString();
        tbArchiveTriggerTimeout.TextChanged += tbArchiveTriggerTimeout_TextChanged;

        var dt = tableview1.Table;
        foreach(var ec in ErrorCodes.KnownCodes)
        {
            dt.Rows.Add(ec.Code, UserSettings.GetErrorReportingLevelFor(ec).ToString(), ec.Message);
        }

        tableview1.CellActivated += Tableview1_CellActivated;
        tableview1.FullRowSelect = true;
    }

    private void Tableview1_CellActivated(TableView.CellActivatedEventArgs e)
    {
        if (e.Row == -1)
            return;

        var row = tableview1.Table.Rows[e.Row];

        var code = ErrorCodes.KnownCodes.SingleOrDefault(ec => ec.Code == (string)row[0]);

        if (code == null)
            return;

        if(_activator.SelectEnum(new DialogArgs {
               WindowTitle = "New Treatment"
           }, typeof(CheckResult), out var newValue))
        {
            UserSettings.SetErrorReportingLevelFor(code, (CheckResult)newValue);
            row[1] = newValue.ToString();
                
            Application.MainLoop.Invoke(()=>tableview1.Update());
        }
    }

    private void RegisterCheckbox(CheckBox cb, string propertyName)
    {
        // remember about this checkbox for later
        var prop = typeof(UserSettings).GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
        checkboxDictionary.Add(cb, prop);

        // set initial value from UserSettings
        cb.Checked = (bool)prop.GetValue(null);

        // register callback
        cb.Toggled += (c)=>CheckboxCheckedChanged(cb,c);

        // add help
        AddTooltip(cb, propertyName);
    }


    private void CheckboxCheckedChanged(CheckBox cb, bool oldValue)
    {
        checkboxDictionary[cb].SetValue(null, cb.Checked);
    }
    private void AddTooltip(View cb, string propertyName)
    {
        var helpText = _activator.CommentStore.GetDocumentationIfExists($"{ nameof(UserSettings)}.{propertyName}", false);

        if (string.IsNullOrWhiteSpace(helpText))
        {
            return;
        }

        var btn = new Button
        {
            Text = "Info",
            IsDefault = false,
            X = 45,
            Y = Pos.Top(cb)
        };

        btn.Clicked += ()=>_activator.Show(propertyName,helpText);
        tabView1.Tabs.First().View.Add(btn);
    }

    private void tbCreateDatabaseTimeout_TextChanged(ustring s)
    {
        if (int.TryParse(tbCreateDatabaseTimeout.Text.ToString(), out var result))
        {
            UserSettings.CreateDatabaseTimeout = result;
        }
    }
    private void tbArchiveTriggerTimeout_TextChanged(ustring s)
    {

        if (int.TryParse(tbArchiveTriggerTimeout.Text.ToString(), out var result))
        {
            UserSettings.ArchiveTriggerTimeout = result;
        }
    }
}