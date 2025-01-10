// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.Core.Setting;

namespace Rdmp.UI.LoadExecutionUIs;

/// <summary>
/// Runs the Data Load Engine on a single LoadMetadata.  This user interface is intended for manually executing and debugging loads.
/// 
/// <para>You can only attempt to launch a data load if the checks are all passing (or giving Warnings that you understand and are not concerned about).  </para>
/// 
/// <para>Once started the load progress will appear and show as data is loaded into RAW, migrated to STAGING and committed to LIVE (See  'RAW Bubble, STAGING Bubble, LIVE Model'
/// in UserManual.md for full implementation details).</para>
/// 
/// <para>There are various options for debugging for example you can override and stop the data load after RAW is populated (in which case the load will crash out early allowing
/// you to evaluated the RAW data in a database environment conducive with debugging dataset issues). </para>
/// </summary>
public partial class ExecuteLoadMetadataUI : DatasetLoadControl_Design
{
    private LoadMetadata _loadMetadata;
    private ILoadProgress[] _allLoadProgresses;

    private ToolStripComboBox dd_DebugOptions = new();


    private enum DebugOptions
    {
        RunNormally,
        StopAfterRAW,
        StopAfterSTAGING,
        SkipArchiving
    }

    public ExecuteLoadMetadataUI()
    {
        InitializeComponent();

        helpIconRunRepeatedly.SetHelpText("Run Repeatedly",
            "By default running a scheduled load will run the number of days specified as a single load (e.g. 5 days of data).  Ticking this box means that if the load is successful a further 5 days will be executed and again until either a data load fails or the load is up to date.");
        helpIconAbortShouldCancel.SetHelpText("Abort Behaviour",
            "By default clicking the Abort button (in Controls) will issue an Abort flag on a run which usually results in it completing the current stage (e.g. Migrate RAW to STAGING) then stop.  Ticking this button in a Load Progress based load will make the button instead issue a Cancel notification which means the data load will complete the current LoadProgress and then not start a new one.  This is only an option when you have ticked 'Run Repeatedly' (left)");

        AssociatedCollection = RDMPCollection.DataLoad;

        checkAndExecuteUI1.CommandGetter = AutomationCommandGetter;
        checkAndExecuteUI1.StateChanged += SetButtonStates;

        dd_DebugOptions.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        dd_DebugOptions.ComboBox.DataSource = Enum.GetValues(typeof(DebugOptions));
    }

    public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _loadMetadata = databaseObject;

        checkAndExecuteUI1.SetItemActivator(activator);

        checkAndExecuteUI1.AllowsYesNoToAll = false;

        if (activator.IsInteractive)
        {
            var showYestoAllNotoAlldataloadcheck = false;
            var showYestoAllNotoAlldataloadcheckSetting = activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Setting>().FirstOrDefault(static s => s.Key == "ToggleYestoAllNotoAlldataloadcheck");
            if (showYestoAllNotoAlldataloadcheckSetting is not null) showYestoAllNotoAlldataloadcheck = Convert.ToBoolean(showYestoAllNotoAlldataloadcheckSetting.Value);
            checkAndExecuteUI1.AllowsYesNoToAll = showYestoAllNotoAlldataloadcheck;
        }

        SetButtonStates(null, null);

        SetLoadProgressGroupBoxState();


        CommonFunctionality.Add(new ExecuteCommandViewLoadDiagram(activator, _loadMetadata));

        CommonFunctionality.AddToMenu(new ExecuteCommandEditLoadMetadataDescription(activator, _loadMetadata));

        CommonFunctionality.Add(new ExecuteCommandViewLogs(activator, (LoadMetadata)databaseObject));

        CommonFunctionality.Add(dd_DebugOptions);
    }

    private void SetLoadProgressGroupBoxState()
    {
        _allLoadProgresses = _loadMetadata.LoadProgresses;

        if (_allLoadProgresses.Any())
        {
            //there are some load progresses
            gbLoadProgresses.Visible = true;

            //give the user the dropdown options for which load progress he wants to run
            var loadProgressData = new Dictionary<int, string>();

            //if there are more than 1 let them pick any
            if (_allLoadProgresses.Length > 1)
                loadProgressData.Add(0, "Any Available");

            foreach (var loadProgress in _allLoadProgresses)
                loadProgressData.Add(loadProgress.ID, loadProgress.Name);

            ddLoadProgress.DataSource = new BindingSource(loadProgressData, null);
            ddLoadProgress.DisplayMember = "Value";
            ddLoadProgress.ValueMember = "Key";
        }
        else
        {
            gbLoadProgresses.Visible = false;
        }
    }

    private void SetButtonStates(object sender, EventArgs e)
    {
        gbLoadProgresses.Enabled = checkAndExecuteUI1.ChecksPassed;
    }

    private RDMPCommandLineOptions AutomationCommandGetter(CommandLineActivity activityRequested)
    {
        var lp = GetLoadProgressIfAny();

        var debugOpts = (DebugOptions)dd_DebugOptions.SelectedItem;

        var options = new DleOptions
        {
            Command = activityRequested,
            LoadMetadata = _loadMetadata.ID.ToString(),
            Iterative = cbRunIteratively.Checked,
            DaysToLoad = Convert.ToInt32(udDaysPerJob.Value),
            DoNotArchiveData = debugOpts != DebugOptions.RunNormally,
            StopAfterRAW = debugOpts == DebugOptions.StopAfterRAW,
            StopAfterSTAGING = debugOpts is DebugOptions.StopAfterRAW or DebugOptions.StopAfterSTAGING
        };

        if (lp != null)
            options.LoadProgress = lp.ID.ToString();

        return options;
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        base.ConsultAboutClosing(sender, e);
        checkAndExecuteUI1.ConsultAboutClosing(sender, e);
    }

    private LoadProgress GetLoadProgressIfAny()
    {
        if (ddLoadProgress.SelectedIndex == -1)
            return null;

        var scheduleItem = (KeyValuePair<int, string>)ddLoadProgress.SelectedItem;
        return scheduleItem.Key == 0
            ? null
            : Activator.RepositoryLocator.CatalogueRepository.GetObjectByID<LoadProgress>(scheduleItem.Key);
    }


    private void ddLoadProgress_SelectedIndexChanged(object sender, EventArgs e)
    {
        var loadProgress = GetLoadProgressIfAny();

        if (loadProgress == null)
        {
            var progresses = _loadMetadata.LoadProgresses.ToArray();
            if (progresses.Length == 1)
                udDaysPerJob.Value = progresses[0].DefaultNumberOfDaysToLoadEachTime;
        }
        else
        {
            udDaysPerJob.Value = loadProgress.DefaultNumberOfDaysToLoadEachTime;
        }
    }

    public override string GetTabName() => $"Execution:{base.GetTabName()}";

    private void cbRunIteratively_CheckedChanged(object sender, EventArgs e)
    {
        //can only cancel between runs if we are running multiple runs
        cbAbortShouldActuallyCancelInstead.Enabled = cbRunIteratively.Checked;
    }

    private void btnRefreshLoadProgresses_Click(object sender, EventArgs e)
    {
        SetLoadProgressGroupBoxState();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DatasetLoadControl_Design, UserControl>))]
public abstract class DatasetLoadControl_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
{
}