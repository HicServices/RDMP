// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.Logging;

/// <summary>
/// <para>Displays all the activity going on within the RDMP that has been recorded in the logging database.  This includes data extractions, data loads, data quality runs etc.  This
/// information is stored in a relational database format including:</para>
/// 
/// <para>Task - The overarching type of task e.g. 'Data Extraction', 'Loading Biochemistry' etc</para>
/// <para>Run - Each time data has flown from one set of locations to another, this encapsulates one execution e.g. An attempt to load 3 Biochemistry files on 2016-02-05 at 5AM</para>
/// <para>Table Loads - Each run will have 0 or more Table Loads, these are destinations for the data being handled and may include flat file locations such as during data export to csv</para>
/// <para>Data Sources - Each table can have an explicit source which might be a flat file being loaded or an SQL query in the case of data extraction.</para>
/// <para>Fatal Errors - Any crash that happened during a run should appear in this view</para>
/// <para>Progress Messages - A log of every progress message generated during the run will appear here</para>
/// 
/// </summary>
public class LoggingTabUI : LoggingTab_Design
{
    private readonly ToolStripTextBox tbContentFilter = new();
    private readonly ToolStripLabel label1 = new("Filter:");
    private readonly ToolStripLabel label2 = new("Top:");
    private readonly ToolStripTextBox tbTop = new() { Text = "10000" };
    private readonly ToolStripButton cbPreferNewer = new("Newest") { CheckOnClick = true, Checked = true };

    private Label lblCurrentFilter;
    private PictureBox pbRemoveFilter;
    private DataGridView dataGridView1;


    private LogViewerFilter Filter = new(LoggingTables.DataLoadTask);

    private int TopX;
    private string _freeTextFilter;
    private Panel pFilter;
    private LogManager LogManager;

    private NavigationTrack<LogViewerFilter> _navigationTrack;
    private Panel panel1;
    private ToolStripButton _back;

    public LoggingTabUI()
    {
        InitializeComponent();

        dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        dataGridView1.CellMouseClick += DataGridView1OnCellMouseClick;

        TopX = UpdateTopX();

        //start with no filter
        panel1.Controls.Remove(pFilter);

        tbTop.TextChanged += tbTop_TextChanged;
        tbContentFilter.TextChanged += tbContentFilter_TextChanged;
        cbPreferNewer.CheckedChanged += cbPreferNewer_CheckedChanged;
    }

    private int UpdateTopX()
    {
        try
        {
            var result = int.Parse(tbTop.Text);
            if (result <= 0)
            {
                tbTop.ForeColor = Color.Red;
                return 1000;
            }

            tbTop.ForeColor = Color.Black;
            return result;
        }
        catch (Exception)
        {
            tbTop.ForeColor = Color.Red;
            return 1000;
        }
    }

    private void DataGridView1OnCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex == -1)
            return;

        if (e.Button == MouseButtons.Right)
        {
            var menu = new ContextMenuStrip();

            foreach (var cmd in GetCommands(e.RowIndex))
            {
                var cmd1 = cmd;
                var mi = new ToolStripMenuItem(cmd.GetCommandName(), null, (s, x) => cmd1.Execute());
                menu.Items.Add(mi);
            }

            if (menu.Items.Count != 0)
                menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("View as text", null,
                (s, ex) => WideMessageBox.Show("Full Text", dataGridView1.Rows[e.RowIndex]));

            menu.Show(Cursor.Position.X, Cursor.Position.Y);
        }
    }

    private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex == -1)
            return;

        var cmd = GetCommands(e.RowIndex).FirstOrDefault();

        if (cmd != null)
            cmd.Execute();
        else
            WideMessageBox.Show("Full Text", dataGridView1.Rows[e.RowIndex]);
    }

    private IEnumerable<BasicCommandExecution> GetCommands(int rowIdnex)
    {
        var rowId = (int)dataGridView1.Rows[rowIdnex].Cells["ID"].Value;

        switch (Filter.LoggingTable)
        {
            case LoggingTables.DataLoadRun:

                yield return new ExecuteCommandViewLogs(Activator,
                    new LogViewerFilter(LoggingTables.ProgressLog) { Run = rowId });
                yield return new ExecuteCommandViewLogs(Activator,
                    new LogViewerFilter(LoggingTables.FatalError) { Run = rowId });
                yield return new ExecuteCommandViewLogs(Activator,
                    new LogViewerFilter(LoggingTables.TableLoadRun) { Run = rowId });

                yield return new ExecuteCommandExportLoggedDataToCsv(Activator,
                    new LogViewerFilter(LoggingTables.ProgressLog) { Run = rowId });
                break;
            case LoggingTables.DataLoadTask:
                yield return new ExecuteCommandViewLogs(Activator,
                    new LogViewerFilter(LoggingTables.DataLoadRun) { Task = rowId });
                break;

            case LoggingTables.TableLoadRun:
                yield return new ExecuteCommandViewLogs(Activator,
                    new LogViewerFilter(LoggingTables.DataSource) { Table = rowId });
                break;
        }
    }

    private static void AddFreeTextSearchColumn(DataTable dt)
    {
        var dcRowString = dt.Columns.Add("_RowString", typeof(string));
        var sb = new StringBuilder();
        foreach (DataRow dataRow in dt.Rows)
        {
            sb.Clear();
            for (var i = 0; i < dt.Columns.Count - 1; i++)
            {
                sb.Append(dataRow[i]);
                sb.Append('\t');
            }

            dataRow[dcRowString] = sb.ToString();
        }
    }

    #region InitializeComponent

    private void InitializeComponent()
    {
        var resources = new ComponentResourceManager(typeof(LoggingTabUI));
        dataGridView1 = new DataGridView();
        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;

        pbRemoveFilter = new PictureBox();
        lblCurrentFilter = new Label();
        pFilter = new Panel();
        panel1 = new Panel();
        ((ISupportInitialize)dataGridView1).BeginInit();
        ((ISupportInitialize)pbRemoveFilter).BeginInit();
        pFilter.SuspendLayout();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // dataGridView1
        // 
        dataGridView1.AllowUserToAddRows = false;
        dataGridView1.AllowUserToDeleteRows = false;
        dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridView1.Dock = DockStyle.Fill;
        dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
        dataGridView1.Location = new Point(0, 0);
        dataGridView1.Name = "dataGridView1";
        dataGridView1.ReadOnly = true;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dataGridView1.Size = new Size(842, 571);
        dataGridView1.TabIndex = 6;
        // 
        // pbRemoveFilter
        // 
        pbRemoveFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pbRemoveFilter.BackColor = Color.Goldenrod;
        pbRemoveFilter.Image = (Image)resources.GetObject("pbRemoveFilter.Image");
        pbRemoveFilter.Location = new Point(820, 3);
        pbRemoveFilter.Name = "pbRemoveFilter";
        pbRemoveFilter.Size = new Size(19, 19);
        pbRemoveFilter.SizeMode = PictureBoxSizeMode.CenterImage;
        pbRemoveFilter.TabIndex = 10;
        pbRemoveFilter.TabStop = false;
        pbRemoveFilter.Click += pbRemoveFilter_Click;
        // 
        // lblCurrentFilter
        // 
        lblCurrentFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left
                                                   | AnchorStyles.Right;
        lblCurrentFilter.BackColor = Color.Goldenrod;
        lblCurrentFilter.ForeColor = SystemColors.ControlLightLight;
        lblCurrentFilter.Location = new Point(3, 3);
        lblCurrentFilter.Name = "lblCurrentFilter";
        lblCurrentFilter.Size = new Size(816, 19);
        lblCurrentFilter.TabIndex = 9;
        lblCurrentFilter.Text = "Filtered Object";
        lblCurrentFilter.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pFilter
        // 
        pFilter.Controls.Add(lblCurrentFilter);
        pFilter.Controls.Add(pbRemoveFilter);
        pFilter.Dock = DockStyle.Top;
        pFilter.Location = new Point(0, 0);
        pFilter.Name = "pFilter";
        pFilter.Size = new Size(842, 26);
        pFilter.TabIndex = 14;
        // 
        // panel1
        // 
        panel1.Controls.Add(pFilter);
        panel1.Controls.Add(dataGridView1);
        panel1.Dock = DockStyle.Fill;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(842, 571);
        panel1.TabIndex = 15;
        // 
        // LoggingTabUI
        // 
        Controls.Add(panel1);
        Name = "LoggingTabUI";
        Size = new Size(842, 571);
        ((ISupportInitialize)dataGridView1).EndInit();
        ((ISupportInitialize)pbRemoveFilter).EndInit();
        pFilter.ResumeLayout(false);
        panel1.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private void LoadDataTable(DataTable dt)
    {
        AddFreeTextSearchColumn(dt);

        dataGridView1.DataSource = dt;
        dataGridView1.Columns["_RowString"].Visible = false;

        ((DataTable)dataGridView1.DataSource).DefaultView.Sort = "ID DESC";
    }

    private void tbContentFilter_TextChanged(object sender, EventArgs e)
    {
        _freeTextFilter = $"[_RowString] LIKE '%{tbContentFilter.Text}%'";
        RegenerateFilters();
    }

    public void SetFilter(LogViewerFilter filter)
    {
        if (
            _navigationTrack is { Current: not null } //there is a back navigation stack setup
            && filter != _navigationTrack.Current //we are not doing a Back operation
        )
            _navigationTrack.Current.Tag =
                tbContentFilter
                    .Text; //Since user is making a new navigation to a new location, record the current text filter to preserve it for Back operations.


        Filter = filter;

        //push the old filter
        _navigationTrack?.Append(Filter);
        if (_back != null)
            _back.Enabled = _navigationTrack?.CanBack() == true;

        if (filter.IsEmpty)
        {
            panel1.Controls.Remove(pFilter);
        }
        else
        {
            panel1.Controls.Add(pFilter);

            lblCurrentFilter.Text = filter.ToString();
        }

        FetchDataTable();

        //clear/restore the current user entered text filter
        tbContentFilter.Text = filter.Tag as string ?? "";
    }

    private void RegenerateFilters()
    {
        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = _freeTextFilter;
    }

    public override void SetDatabaseObject(IActivateItems activator, ExternalDatabaseServer databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        if (_navigationTrack == null)
        {
            //what happens when user clicks back/forward
            _navigationTrack = new NavigationTrack<LogViewerFilter>(f => true, f =>
            {
                if (f.LoggingTable != LoggingTables.None)
                {
                    var cmd = new ExecuteCommandViewLogs(activator, f);
                    cmd.Execute();
                }
            });

            //set the initial filter
            _navigationTrack.Append(Filter);
            _back = new ToolStripButton("Back", FamFamFamIcons.Back.ImageToBitmap(),
                (s, e) => _navigationTrack.Back(true))
            { DisplayStyle = ToolStripItemDisplayStyle.Image };
        }

        CommonFunctionality.Add(_back);

        CommonFunctionality.Add(label1);
        CommonFunctionality.Add(tbContentFilter);

        CommonFunctionality.Add(label2);
        CommonFunctionality.Add(tbTop);

        CommonFunctionality.Add(cbPreferNewer);

        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.DataLoadTask))
            { OverrideCommandName = "All Tasks" });
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.DataLoadRun))
            { OverrideCommandName = "All Runs" });
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.FatalError))
            { OverrideCommandName = "All Errors" });
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.TableLoadRun))
            { OverrideCommandName = "All Tables Loaded" });
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.DataSource))
            { OverrideCommandName = "All Data Sources" });
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewLogs(activator, new LogViewerFilter(LoggingTables.ProgressLog))
            { OverrideCommandName = "All Progress Logs" });


        if (!databaseObject.DiscoverExistence(DataAccessContext.Logging, out var reason))
        {
            activator.KillForm(ParentForm, $"Database {databaseObject} did not exist:{reason}");
            return;
        }

        LogManager = new LogManager(databaseObject);
        FetchDataTable();
    }

    public override string GetTabName() => "Log Viewer";

    private void FetchDataTable()
    {
        if (Filter.LoggingTable != LoggingTables.None)
            LoadDataTable(LogManager.GetTable(Filter, TopX, cbPreferNewer.Checked));
    }

    public void SelectRowWithID(int rowIDToSelect)
    {
        dataGridView1.ClearSelection();
        foreach (DataGridViewRow row in dataGridView1.Rows)
            if (Convert.ToInt32(row.Cells["ID"].Value) == rowIDToSelect)
            {
                //scroll to it
                dataGridView1.CurrentCell = row.Cells[0];

                foreach (DataGridViewCell cell in row.Cells)
                    cell.Selected = true;

                row.Selected = true;

                break;
            }
    }

    private void pbRemoveFilter_Click(object sender, EventArgs e)
    {
        //get a fresh clear filter (but targeting the same table)
        SetFilter(new LogViewerFilter(Filter.LoggingTable));
    }

    private void tbTop_TextChanged(object sender, EventArgs e)
    {
        TopX = UpdateTopX();
        FetchDataTable();
    }

    private void cbPreferNewer_CheckedChanged(object sender, EventArgs e)
    {
        FetchDataTable();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoggingTab_Design, UserControl>))]
public abstract class LoggingTab_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>;