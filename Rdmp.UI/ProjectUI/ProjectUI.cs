// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.ProjectUI;

/// <summary>
/// Allows you to view/edit a data extraction project including the extraction configurations that make it up (See ExtractionConfigurationUI).
/// 
/// <para>First make sure your Project has a nice unique name that lets you rapidly identify it.  Next choose the 'Extraction Directory', this is the location where extracted data will be
/// generated (See ExecuteExtractionUI).  Make sure that the extraction directory is accessible to every data analyst who is using the software / working on the project (e.g. it could
/// be a shared network drive).</para>
/// 
/// <para>Optionally you can specify a Ticket for logging time/issues against (See TicketingSystemConfigurationUI)</para>
/// 
/// <para>Add a ProjectNumber, this number must be unique.  This number must match the project number of the cohorts you intend to use with the project in the Cohort Database (you only need
/// to worry about a mismatch here if you are manually hacking your cohort database or if you change the project number halfway through its lifecycle).</para>
///  
/// <para>Right clicking in the datagrid will allow you to create new Extraction Configurations for the project or edit existing ones.  An extraction configuration is a collection of
/// datasets linked against a cohort private identifier and released against an anonymous project specific identifier (See ExtractableCohortUI and ExtractionConfigurationUI).  Once
/// you have a few Extraction Configurations, they will appear in the datagrid too.</para>
/// 
/// <para>Selecting 'Check Project' will check all current and released extraction configurations in the project for problems (empty result sets, broken extraction SQL etc).</para>
///  
/// </summary>
public partial class ProjectUI : ProjectUI_Design, ISaveableUI
{
    private Project _project;

    private void SetCohorts()
    {
        if (_project?.ProjectNumber == null)
            return;

        if (Activator.CoreChildProvider is not DataExportChildProvider dxChildProvider)
            return;
        extractableCohortCollection1.SetItemActivator(Activator);

        extractableCohortCollection1.SetupFor(dxChildProvider.Cohorts
            .Where(c => c.ExternalProjectNumber == _project.ProjectNumber).ToArray());
    }

    //menu item setup
    private ContextMenuStrip menu = new();
    private ToolStripMenuItem mi_SetDescription = new("Set Description");

    /// <summary>
    /// Set when the user right clicks a row, so that we can reference the row in the handlers of the ToolStripMenuItems
    /// </summary>
    private int _rightClickedRowExtractionConfigurationID = -1;


    public ProjectUI()
    {
        InitializeComponent();

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
        mi_SetDescription.Click += mi_SetDescription_Click;

        tcMasterTicket.Title = "Master Ticket";
        tcMasterTicket.TicketTextChanged += tcMasterTicket_TicketTextChanged;

        AssociatedCollection = RDMPCollection.DataExport;
    }


    public void RefreshLists()
    {
        dataGridView1.DataSource = LoadDatagridFor(_project);
    }


    protected override void SetBindings(BinderWithErrorProviderFactory rules, Project databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", p => p.ID);
        Bind(tbName, "Text", "Name", p => p.Name);
    }

    public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        //now load the UI form
        _project = databaseObject;

        dataGridView1.DataSource = LoadDatagridFor(_project);
        tcMasterTicket.TicketText = _project.MasterTicket;
        tbExtractionDirectory.Text = _project.ExtractionDirectory;
        tbProjectNumber.Text = _project.ProjectNumber.ToString();

        dataGridView1.Invalidate();

        SetCohorts();
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        tcMasterTicket.SetItemActivator(activator);
    }

    private static DataTable LoadDatagridFor(Project value)
    {
        var configurations = value?.ExtractionConfigurations;

        if (configurations == null || configurations.Length == 0)
            return null;

        var dtToReturn = new DataTable();

        dtToReturn.Columns.Add("ID");
        dtToReturn.Columns.Add("Name");
        dtToReturn.Columns.Add("Date Created");
        dtToReturn.Columns.Add("Username");
        dtToReturn.Columns.Add("Status");
        dtToReturn.Columns.Add("Description");
        dtToReturn.Columns.Add("Separator");

        dtToReturn.Columns.Add("Cohort");

        dtToReturn.Columns.Add("RequestTicket");
        dtToReturn.Columns.Add("ReleaseTicket");
        dtToReturn.Columns.Add("ClonedFrom");


        dtToReturn.Columns.Add("Datasets");

        foreach (ExtractionConfiguration configuration in configurations)
        {
            var r = dtToReturn.Rows.Add();

            r["ID"] = configuration.ID;
            r["Name"] = configuration.Name;
            r["Date Created"] = configuration.dtCreated;
            r["Username"] = configuration.Username;

            if (configuration.IsReleased)
                r["Status"] = "Frozen (Because Released)";
            else
                r["Status"] = "Editable";

            r["Description"] = configuration.Description;
            r["Separator"] = configuration.Separator;

            if (configuration.Cohort_ID == null)
                r["Cohort"] = "None";
            else
                try
                {
                    r["Cohort"] = configuration.Cohort.ToString();
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show(ex);
                    r["Cohort"] = "Error retrieving Cohort";
                }

            r["RequestTicket"] = configuration.RequestTicket;
            r["ReleaseTicket"] = configuration.ReleaseTicket;
            r["ClonedFrom"] = configuration.ClonedFrom_ID;


            r["Datasets"] =
                string.Join(",", configuration.GetAllExtractableDataSets().Select(ds => ds.ToString()));
        }

        return dtToReturn;
    }

    #region Right Click Context Menu

    #region Menu Items

    private void mi_SetDescription_Click(object sender, EventArgs e)
    {
        var toSetDescriptionOn =
            Activator.RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(
                _rightClickedRowExtractionConfigurationID);

        if (toSetDescriptionOn.IsReleased)
            return;

        var dialog = new TypeTextOrCancelDialog("Description", "Enter a Description for the Extraction:", 1000,
            toSetDescriptionOn.Description);

        dialog.ShowDialog(this);

        if (dialog.DialogResult == DialogResult.OK)
        {
            toSetDescriptionOn.Description = dialog.ResultText;
            toSetDescriptionOn.SaveToDatabase();
            RefreshLists();
        }
    }

    private void mi_ChooseFileSeparator_Click(object sender, EventArgs e)
    {
        var toSetDescriptionOn =
            Activator.RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(
                _rightClickedRowExtractionConfigurationID);

        if (toSetDescriptionOn.IsReleased)
            return;

        var dialog = new TypeTextOrCancelDialog("Separator", "Choose a character(s) separator", 3,
            toSetDescriptionOn.Separator);

        dialog.ShowDialog(this);

        if (dialog.DialogResult == DialogResult.OK)
        {
            toSetDescriptionOn.Separator = dialog.ResultText;
            toSetDescriptionOn.SaveToDatabase();
            RefreshLists();
        }
    }

    #endregion


    #region Menu popup/setup

    private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        //note that this only deals with clicking cells, to see what happens hwen user clicks in blank area of datagrid see dataGridView1_MouseClick
        if (e.RowIndex >= 0)
            if (e.Button == MouseButtons.Right)
            {
                menu.Items.Clear();


                _rightClickedRowExtractionConfigurationID =
                    int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());

                var selectedExtractionConfiguration =
                    Activator.RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(
                        _rightClickedRowExtractionConfigurationID);

                menu.Items.Clear();

                if (!selectedExtractionConfiguration.IsReleased)
                    menu.Items.Add(mi_SetDescription);

                menu.Show(Cursor.Position.X, Cursor.Position.Y);
            }
    }

    #endregion

    #endregion


    private void btnShowExtractionDirectory_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbExtractionDirectory.Text)) return;
        try
        {
            UsefulStuff.ShowPathInWindowsExplorer(new DirectoryInfo(tbExtractionDirectory.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void tbExtractionDirectory_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (!tbExtractionDirectory.Text.StartsWith("\\") && !Directory.Exists(tbExtractionDirectory.Text))
            {
                tbExtractionDirectory.ForeColor = Color.Red;
            }
            else
            {
                _project.ExtractionDirectory = tbExtractionDirectory.Text;
                tbExtractionDirectory.ForeColor = Color.Black;
            }
        }
        catch (Exception)
        {
            tbExtractionDirectory.ForeColor = Color.Red;
        }
    }

    private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == -1 || e.RowIndex == -1)
            return;

        if (dataGridView1.Columns[e.ColumnIndex].Name == "Description")
        {
            //simulate a right click by setting the ID and calling the handler directly
            _rightClickedRowExtractionConfigurationID =
                int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());
            mi_SetDescription_Click(null, null);
        }

        if (dataGridView1.Columns[e.ColumnIndex].Name == "Separator")
        {
            //simulate a right click by setting the ID and calling the handler directly
            _rightClickedRowExtractionConfigurationID =
                int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());
            mi_ChooseFileSeparator_Click(null, null);
        }
    }

    private void tbProjectNumber_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbProjectNumber.Text))
        {
            _project.ProjectNumber = null;
            return;
        }

        try
        {
            _project.ProjectNumber = int.Parse(tbProjectNumber.Text);
            tbProjectNumber.ForeColor = Color.Black;
            _project.SaveToDatabase();
        }
        catch (Exception)
        {
            tbProjectNumber.ForeColor = Color.Red;
        }
    }

    private void tcMasterTicket_TicketTextChanged(object sender, EventArgs e)
    {
        _project.MasterTicket = tcMasterTicket.TicketText;
    }

    public void SwitchToCutDownUIMode()
    {
        dataGridView1.Visible = false;
        lblExtractions.Visible = false;
        Height = 190;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var fdlg = new FolderBrowserDialog();

        if (fdlg.ShowDialog() == DialogResult.OK)
            tbExtractionDirectory.Text = fdlg.SelectedPath;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ProjectUI_Design, UserControl>))]
public abstract class ProjectUI_Design : RDMPSingleDatabaseObjectControl<Project>
{
}