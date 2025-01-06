// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Versioning;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Every dataset (Catalogue) can have its own Logging task and Logging server.  If you have multiple logging servers (e.g. a test logging server and a live logging server). You
/// can configure each of these independently.  If you only have one logging server then just set the live logging server.
/// 
/// <para>Once you have set the logging server you should create or select an existing task (e.g. 'Loading Biochemistry' might be a good logging task for Biochemistry dataset).  All datasets
/// in a given load (see LoadMetadataUI) must share the same logging task so it is worth considering the naming for example you might call the task 'Loading Hospital Data' and another
/// 'Loading Primary Care Data'.</para>
/// 
/// <para>Data Extraction always gets logged under a task called 'Data Extraction' but the server you select here will be the one that it is logged against when the dataset is extracted.</para>
/// 
/// <para>You can configure defaults for the logging servers of new datasets through ManageExternalServers dialog (See ManageExternalServers)</para>
/// </summary>
public partial class ChooseLoggingTaskUI : RDMPUserControl, ICheckNotifier
{
    private Catalogue _catalogue;
    private string expectedDatabaseTypeString = "HIC.Logging.Database";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Catalogue Catalogue
    {
        get => _catalogue;
        set
        {
            _catalogue = value;
            RefreshUIFromDatabase();
        }
    }

    private void RefreshUIFromDatabase()
    {
        if (_catalogue == null)
            return;

        var servers = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>()
            .Where(s => string.Equals(expectedDatabaseTypeString, s.CreatedByAssembly)).ToArray();

        ddLoggingServer.Items.Clear();
        ddLoggingServer.Items.AddRange(servers);

        ExternalDatabaseServer liveserver = null;

        if (_catalogue.LiveLoggingServer_ID != null)
        {
            liveserver = ddLoggingServer.Items.Cast<ExternalDatabaseServer>()
                .SingleOrDefault(i => i.ID == (int)_catalogue.LiveLoggingServer_ID);
            ddLoggingServer.SelectedItem = liveserver ?? throw new Exception(
                $"Catalogue '{_catalogue}' lists its Live Logging Server as '{_catalogue.LiveLoggingServer}' did not appear in combo box, possibly it is not marked as a '{expectedDatabaseTypeString}' server? Try editing it in Locations=>Manage External Servers");
        }

        try
        {
            //load data tasks (new architecture)
            //if the catalogue knows its logging server - populate values
            if (liveserver != null)
            {
                var lm = new LogManager(liveserver);

                foreach (var t in lm.ListDataTasks())
                    if (!cbxDataLoadTasks.Items.Contains(t))
                        cbxDataLoadTasks.Items.Add(t);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Problem getting the list of DataTasks from the new logging architecture:{ex.Message}");
        }

        if (!string.IsNullOrWhiteSpace(_catalogue.LoggingDataTask))
            cbxDataLoadTasks.Text = _catalogue.LoggingDataTask;

        CheckNameExists();
    }

    public ChooseLoggingTaskUI()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (Activator == null)
            return;

        RefreshUIFromDatabase();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        RefreshTasks();
    }

    private void RefreshTasks()
    {
        var liveserver = ddLoggingServer.SelectedItem as ExternalDatabaseServer;
        var server = DataAccessPortal.ExpectServer(liveserver, DataAccessContext.Logging);

        if (liveserver != null)
        {
            cbxDataLoadTasks.Items.Clear();

            try
            {
                var lm = new LogManager(server);
                cbxDataLoadTasks.Items.AddRange(lm.ListDataTasks());
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }
    }


    private void cbxDataLoadTasks_SelectedIndexChanged(object sender, EventArgs e)
    {
        _catalogue.LoggingDataTask = (string)cbxDataLoadTasks.SelectedItem;
        _catalogue.SaveToDatabase();
    }

    private void label1_Click(object sender, EventArgs e)
    {
    }

    private void ddLoggingServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddLoggingServer.SelectedItem == null)
        {
            _catalogue.LiveLoggingServer_ID = null;
            _catalogue.SaveToDatabase();
            return;
        }

        _catalogue.LiveLoggingServer_ID = ((ExternalDatabaseServer)ddLoggingServer.SelectedItem).ID;
        _catalogue.SaveToDatabase();
        RefreshTasks();
    }

    private void cbxDataLoadTasks_TextChanged(object sender, EventArgs e)
    {
        if (_catalogue == null)
            return;

        _catalogue.LoggingDataTask = cbxDataLoadTasks.Text;
        _catalogue.SaveToDatabase();
    }

    private void btnCreateNewLoggingTask_Click(object sender, EventArgs e)
    {
        try
        {
            var liveServer = ddLoggingServer.SelectedItem as ExternalDatabaseServer;

            var target = "";

            var toCreate = cbxDataLoadTasks.Text;

            if (liveServer != null)
                target = $"{liveServer.Server}.{liveServer.Database}";

            if (string.IsNullOrEmpty(target))
            {
                MessageBox.Show("You must select a logging server");
                return;
            }

            if (Activator.YesNo($"Create a new dataset and new data task called \"{toCreate}\" in {target}",
                    "Create new logging task"))
            {
                if (liveServer != null)
                {
                    var checker = new LoggingDatabaseChecker(liveServer);
                    checker.Check(this);

                    new LogManager(liveServer)
                        .CreateNewLoggingTaskIfNotExists(toCreate);
                }

                MessageBox.Show("Done");

                RefreshTasks();
            }

            RefreshUIFromDatabase();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        if (args.ProposedFix != null)
            return MakeChangePopup.ShowYesNoMessageBoxToApplyFix(null, args.Message, args.ProposedFix);
        //if it is sucessful user doesn't need to be spammed with messages
        if (args.Result == CheckResult.Success)
            return true;

        //its a warning or an error possibly with an exception attached
        if (args.Ex != null)
            ExceptionViewer.Show(args.Message, args.Ex);
        else
            MessageBox.Show(args.Message);

        return false;
    }

    private void cbxDataLoadTasks_Leave(object sender, EventArgs e)
    {
        CheckNameExists();
    }

    private void cbxDataLoadTasks_KeyUp(object sender, KeyEventArgs e)
    {
        CheckNameExists();
    }

    private void CheckNameExists()
    {
        ragSmiley1.Reset();

        if (string.IsNullOrWhiteSpace(cbxDataLoadTasks.Text))
            ragSmiley1.Warning(new Exception("You must provide a Data Task name e.g. 'Loading my cool dataset'"));
        else if (!cbxDataLoadTasks.Items.Contains(cbxDataLoadTasks.Text))
            ragSmiley1.Fatal(new Exception(
                $"Task '{cbxDataLoadTasks.Text}' does not exist yet, select 'Create' to create it"));
    }

    private void btnCreateNewLoggingServer_Click(object sender, EventArgs e)
    {
        CreatePlatformDatabase.CreateNewExternalServer(Activator.RepositoryLocator.CatalogueRepository,
            PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher());
        RefreshUIFromDatabase();
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        if (sender == btnClearLive)
            ddLoggingServer.SelectedItem = null;
    }
}