// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;

/// <summary>
/// Lets you view/edit a single SQL file execution load task.  This SQL script will be run at the appropriate time in the data load (depending on which stage it is at and the order in the
/// stage.  The SQL will be executed on the database/server that corresponds to the stage.  So an Adjust RAW script cannot modify STAGING since those tables won't even exist at the time of
/// execution and might even be on a different server.
/// 
/// <para>You should avoid modifying Live tables directly with SQL since it circumvents the 'no duplication', 'RAW->STAGING->LIVE super transaction' model of RDMP.</para>
/// </summary>
public partial class SqlProcessTaskUI : SqlProcessTaskUI_Design, ISaveableUI
{
    private Scintilla _scintilla;
    private ProcessTask _processTask;
    private AutoCompleteProviderWin _autoComplete;

    public SqlProcessTaskUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _processTask = databaseObject;

        LoadFile();

        loadStageIconUI1.Setup(activator.CoreIconProvider, _processTask.LoadStage);
        loadStageIconUI1.Left = tbID.Right + 2;

        CommonFunctionality.AddChecks(_processTask);
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ProcessTask databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", p => p.ID);
        Bind(tbName, "Text", "Name", p => p.Name);
        Bind(tbPath, "Text", "Path", p => p.Path);
    }

    private bool _bLoading;

    private void LoadFile()
    {
        if (_processTask.ProcessTaskType == ProcessTaskType.SQLBakFile)
        {
            return;
        }
        _bLoading = true;
        try
        {
            if (_scintilla == null)
            {
                var factory = new ScintillaTextEditorFactory();
                _scintilla = factory.Create(new RDMPCombineableFactory());
                groupBox1.Controls.Add(_scintilla);
                _scintilla.SavePointLeft += ScintillaOnSavePointLeft;
                ObjectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
            }

            SetupAutocomplete();

            try
            {
                _scintilla.Text = File.ReadAllText(_processTask.Path);
                _scintilla.SetSavePoint();
            }
            catch (Exception e)
            {
                CommonFunctionality.Fatal($"Could not open file {_processTask.Path}", e);
            }
        }
        finally
        {
            _bLoading = false;
        }
    }


    private void SetupAutocomplete()
    {
        //if there's an old one dispose it
        if (_autoComplete == null)
            _autoComplete = new AutoCompleteProviderWin(_processTask.LoadMetadata.GetQuerySyntaxHelper());
        else
            _autoComplete.Clear();

        foreach (var table in _processTask.LoadMetadata.GetDistinctTableInfoList(false))
            _autoComplete.Add(table, _processTask.LoadStage);

        _autoComplete.RegisterForEvents(_scintilla);
    }

    private bool objectSaverButton1_BeforeSave(DatabaseEntity arg)
    {
        File.WriteAllText(_processTask.Path, _scintilla.Text);
        _scintilla.SetSavePoint();

        return true;
    }

    private void ScintillaOnSavePointLeft(object sender, EventArgs eventArgs)
    {
        if (_bLoading)
            return;

        ObjectSaverButton1.Enable(true);
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = _processTask.ProcessTaskType == ProcessTaskType.SQLBakFile ? "BAK Files |*.bak" : "Sql Files|*.sql",
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
            LoadFile();
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SqlProcessTaskUI_Design, UserControl>))]
public abstract class SqlProcessTaskUI_Design : RDMPSingleDatabaseObjectControl<ProcessTask>
{
}