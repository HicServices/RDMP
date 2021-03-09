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
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.DataAccess;


using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.Logging
{
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
        private ToolStripTextBox tbContentFilter = new ToolStripTextBox();
        private ToolStripLabel label1 = new ToolStripLabel("Filter:");
        private ToolStripLabel label2 = new ToolStripLabel("Top:");
        private ToolStripTextBox tbTop = new ToolStripTextBox(){Text = "10000" };
        private ToolStripButton cbPreferNewer = new ToolStripButton("Newest"){CheckOnClick =true,Checked = true};

        private Label lblCurrentFilter;
        private PictureBox pbRemoveFilter;
        private DataGridView dataGridView1;

        
        private LogViewerFilter Filter = new LogViewerFilter(LoggingTables.DataLoadTask);

        private int TopX;
        private string _freeTextFilter;
        private Panel pFilter;
        private LogManager LogManager;
        
        NavigationTrack<LogViewerFilter> _navigationTrack;
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

            tbTop.TextChanged += new System.EventHandler(this.tbTop_TextChanged);
            tbContentFilter.TextChanged += new System.EventHandler(this.tbContentFilter_TextChanged);
            cbPreferNewer.CheckedChanged += new System.EventHandler(this.cbPreferNewer_CheckedChanged);
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

                foreach (BasicCommandExecution cmd in GetCommands(e.RowIndex))
                {
                    BasicCommandExecution cmd1 = cmd;
                    var mi = new ToolStripMenuItem(cmd.GetCommandName(), null, (s, x) => cmd1.Execute());
                    menu.Items.Add(mi);
                }
                                
                if (menu.Items.Count != 0)
                    menu.Items.Add(new ToolStripSeparator());

                menu.Items.Add("View as text", null, (s, ex) => WideMessageBox.Show("Full Text", dataGridView1.Rows[e.RowIndex]));

                menu.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
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

                    yield return new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.ProgressLog) { Run = rowId });
                    yield return new ExecuteCommandViewLogs(Activator,  new LogViewerFilter(LoggingTables.FatalError) { Run = rowId });
                    yield return new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.TableLoadRun) { Run = rowId });

                    yield return new ExecuteCommandExportLoggedDataToCsv(Activator, new LogViewerFilter(LoggingTables.ProgressLog) { Run = rowId });
                    break;
                case LoggingTables.DataLoadTask:
                    yield return new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.DataLoadRun) { Task = rowId });
                    break;

                case LoggingTables.TableLoadRun:
                    yield return new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.DataSource) { Table = rowId });
                    break;

            }
        }

        private void AddFreeTextSearchColumn(DataTable dt)
        {
            DataColumn dcRowString = dt.Columns.Add("_RowString", typeof(string));
            foreach (DataRow dataRow in dt.Rows)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dt.Columns.Count - 1; i++)
                {
                    sb.Append(dataRow[i]);
                    sb.Append("\t");
                }
                dataRow[dcRowString] = sb.ToString();
            }
        }
        #region InitializeComponent
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoggingTabUI));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pbRemoveFilter = new System.Windows.Forms.PictureBox();
            this.lblCurrentFilter = new System.Windows.Forms.Label();
            this.pFilter = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).BeginInit();
            this.pFilter.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(842, 571);
            this.dataGridView1.TabIndex = 6;
            // 
            // pbRemoveFilter
            // 
            this.pbRemoveFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbRemoveFilter.BackColor = System.Drawing.Color.Goldenrod;
            this.pbRemoveFilter.Image = ((System.Drawing.Image)(resources.GetObject("pbRemoveFilter.Image")));
            this.pbRemoveFilter.Location = new System.Drawing.Point(820, 3);
            this.pbRemoveFilter.Name = "pbRemoveFilter";
            this.pbRemoveFilter.Size = new System.Drawing.Size(19, 19);
            this.pbRemoveFilter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbRemoveFilter.TabIndex = 10;
            this.pbRemoveFilter.TabStop = false;
            this.pbRemoveFilter.Click += new System.EventHandler(this.pbRemoveFilter_Click);
            // 
            // lblCurrentFilter
            // 
            this.lblCurrentFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentFilter.BackColor = System.Drawing.Color.Goldenrod;
            this.lblCurrentFilter.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCurrentFilter.Location = new System.Drawing.Point(3, 3);
            this.lblCurrentFilter.Name = "lblCurrentFilter";
            this.lblCurrentFilter.Size = new System.Drawing.Size(816, 19);
            this.lblCurrentFilter.TabIndex = 9;
            this.lblCurrentFilter.Text = "Filtered Object";
            this.lblCurrentFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pFilter
            // 
            this.pFilter.Controls.Add(this.lblCurrentFilter);
            this.pFilter.Controls.Add(this.pbRemoveFilter);
            this.pFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pFilter.Location = new System.Drawing.Point(0, 0);
            this.pFilter.Name = "pFilter";
            this.pFilter.Size = new System.Drawing.Size(842, 26);
            this.pFilter.TabIndex = 14;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pFilter);
            this.panel1.Controls.Add(this.dataGridView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(842, 571);
            this.panel1.TabIndex = 15;
            // 
            // LoggingTabUI
            // 
            this.Controls.Add(this.panel1);
            this.Name = "LoggingTabUI";
            this.Size = new System.Drawing.Size(842, 571);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).EndInit();
            this.pFilter.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

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
            _freeTextFilter = string.Format("[_RowString] LIKE '%{0}%'", tbContentFilter.Text);
            RegenerateFilters();
        }

        public void SetFilter(LogViewerFilter filter)
        {
            if(
                _navigationTrack != null && _navigationTrack.Current != null //there is a back navigation stack setup
                && filter != _navigationTrack.Current //we are not doing a Back operation
                )
                    _navigationTrack.Current.Tag = tbContentFilter.Text; //Since user is making a new navigation to a new location, record the current text filter to preserve it for Back operations.
                        

            Filter = filter;
            
            //push the old filter
            if(_navigationTrack != null)
                _navigationTrack.Append(Filter);
            if(_back != null)
                _back.Enabled = _navigationTrack.CanBack();

            if (filter.IsEmpty)
                panel1.Controls.Remove(pFilter);
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

            if(_navigationTrack == null)
            {
                //what happens when user clicks back/forward
                _navigationTrack = new NavigationTrack<LogViewerFilter>(f=>true,f=>
                {
                    if(f.LoggingTable != LoggingTables.None)
                    {
                        var cmd = new ExecuteCommandViewLogs(activator,f);
                        cmd.Execute();
                    }
                });

                //set the initial filter
                _navigationTrack.Append(Filter);
                _back = new ToolStripButton("Back",FamFamFamIcons.Back,(s,e)=>_navigationTrack.Back(true)){DisplayStyle = ToolStripItemDisplayStyle.Image };
            }
            
            CommonFunctionality.Add(_back);

            CommonFunctionality.Add(label1);
            CommonFunctionality.Add(tbContentFilter);

            CommonFunctionality.Add(label2);
            CommonFunctionality.Add(tbTop);

            CommonFunctionality.Add(cbPreferNewer);

            
            if (!databaseObject.DiscoverExistence(DataAccessContext.Logging, out string reason))
            {
                activator.KillForm(ParentForm, "Database " + databaseObject + " did not exist:" + reason);
                return;
            }

            LogManager = new LogManager(databaseObject);
            FetchDataTable();
        }

        public override string GetTabName()
        {
            return "Log Viewer";
        }

        private void FetchDataTable()
        {
            if (Filter.LoggingTable != LoggingTables.None)
                LoadDataTable(LogManager.GetTable(Filter, TopX, cbPreferNewer.Checked));
        }

        public void SelectRowWithID(int rowIDToSelect)
        {
            dataGridView1.ClearSelection();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
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
        }

        private void pbRemoveFilter_Click(object sender, EventArgs e)
        {
            //get a fresh clear filter (but targetting the same table)
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
    public abstract class LoggingTab_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>
    {

    }
}
