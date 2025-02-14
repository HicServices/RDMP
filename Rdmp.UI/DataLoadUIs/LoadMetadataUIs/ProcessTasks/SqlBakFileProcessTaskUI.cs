// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;

/// <summary>
/// Lets you view/edit a single SQL file execution load task.  This SQL script will be run at the appropriate time in the data load (depending on which stage it is at and the order in the
/// stage.  The SQL will be executed on the database/server that corresponds to the stage.  So an Adjust RAW script cannot modify STAGING since those tables won't even exist at the time of
/// execution and might even be on a different server.
/// 
/// <para>You should avoid modifying Live tables directly with SQL since it circumvents the 'no duplication', 'RAW->STAGING->LIVE super transaction' model of RDMP.</para>
/// </summary>
public partial class SqlBakFileProcessTaskUI : SqlBakFileProcessTask_Design, ISaveableUI
{
    private ProcessTask _processTask;

    public SqlBakFileProcessTaskUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _processTask = databaseObject;

        loadStageIconUI1.Setup(activator.CoreIconProvider, _processTask.LoadStage);
        loadStageIconUI1.Left = tbID.Right + 2;
        if (!string.IsNullOrWhiteSpace(databaseObject.SerialisableConfiguration))
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(databaseObject.SerialisableConfiguration);
            config.TryGetValue("PrimaryFilePhysicalName", out var primary);
            if (primary is not null)
                tbPrimaryFile.Text = primary;
            config.TryGetValue("LogFilePhysicalName", out var log);
            if (log is not null)
                tbLogFile.Text = log;
        }
        CommonFunctionality.AddChecks(_processTask);
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ProcessTask databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", p => p.ID);
        Bind(tbName, "Text", "Name", p => p.Name);
        Bind(tbPath, "Text", "Path", p => p.Path);
    }

    private void tbPrimary_TextChanged(object sender, EventArgs e)
    {
        var config = new Dictionary<string, string>();
        if (_processTask.SerialisableConfiguration is not null)
            config = JsonSerializer.Deserialize<Dictionary<string, string>>(_processTask.SerialisableConfiguration);
        config["PrimaryFilePhysicalName"] = tbPrimaryFile.Text;
        _processTask.SerialisableConfiguration = JsonSerializer.Serialize(config);
    }

    private void tbLog_TextChanged(object sender, EventArgs e)
    {
        var config = new Dictionary<string, string>();
        if (_processTask.SerialisableConfiguration is not null)
            config = JsonSerializer.Deserialize<Dictionary<string, string>>(_processTask.SerialisableConfiguration);
        config["LogFilePhysicalName"] = tbLogFile.Text;
        _processTask.SerialisableConfiguration = JsonSerializer.Serialize(config);
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "BAK Files |*.bak",
            CheckFileExists = true
        };

        string oldFileName = null;
        //open the browse dialog at the location of the currently specified file
        if (!string.IsNullOrWhiteSpace(_processTask.Path))
        {
            var fi = new FileInfo(_processTask.Path);
            oldFileName = fi.Name;

            if (fi.Exists && fi.Directory != null)
                ofd.InitialDirectory = fi.Directory.FullName;
        }

        if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
        {
            //replace the old name with the new name for example if user specified task name is 'Run bob.sql to rename all 'Roberts' to 'Bob' then the user selects a different file e.g. "truncateAllTables.sql" then the new name becomes Run truncateAllTables.sql to rename all 'Roberts' to 'Bob'
            if (oldFileName != null)
                _processTask.Name = _processTask.Name.Replace(oldFileName, Path.GetFileName(ofd.FileName));

            _processTask.Path = ofd.FileName;
            _processTask.SaveToDatabase();

            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_processTask));
        }
    }

    private void label3_Click(object sender, EventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SqlBakFileProcessTask_Design, UserControl>))]
public abstract class SqlBakFileProcessTask_Design : RDMPSingleDatabaseObjectControl<ProcessTask>;