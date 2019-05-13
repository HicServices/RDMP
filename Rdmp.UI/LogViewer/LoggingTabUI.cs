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
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Logging;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.LogViewer
{
    /// <summary>
    /// TECHNICAL:Base class for all the other logging tabs e.g. <see cref="LoggingDataSourcesTabUI"/>
    ///
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
        private TextBox tbContentFilter;
        private Label label1;
        private PictureBox pbRemoveFilter;
        private Label lblFilter;
        private Label label2;
        private TextBox tbTop;
        protected DataGridView dataGridView1;

        protected LogViewerFilter IDFilter = new LogViewerFilter();

        protected int TopX;
        private string _customFilter;
        private string _freeTextFilter;
        private Panel panel1;
        private CheckBox cbPreferNewer;
        private Panel pFilter;
        protected LogManager LogManager;

        public LoggingTabUI()
        {
            InitializeComponent();

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.CellMouseClick += DataGridView1OnCellMouseClick;

            TopX = UpdateTopX();

            //start with no filter
            Controls.Remove(pFilter);
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

                foreach (ExecuteCommandViewLoggedData cmd in GetCommands(e.RowIndex))
                {
                    ExecuteCommandViewLoggedData cmd1 = cmd;
                    var mi = new ToolStripMenuItem(cmd.GetCommandName(), null, (s, x) => cmd1.Execute());
                    menu.Items.Add(mi);
                }

                var row = dataGridView1.Rows[e.RowIndex];

                StringBuilder sb = new StringBuilder();

                foreach (DataGridViewColumn c in dataGridView1.Columns)
                    if (c.Visible)
                        sb.AppendLine(c.Name + ":" + row.Cells[c.Name].Value);

                if (menu.Items.Count != 0)
                    menu.Items.Add(new ToolStripSeparator());

                menu.Items.Add("View as text", null, (s, ex) => WideMessageBox.Show("Full Text", sb.ToString(), WideMessageBoxTheme.Help));

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
        }


        protected virtual IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIdnex)
        {
            return new ExecuteCommandViewLoggedData[0];
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
        protected void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(LoggingTabUI));
            tbContentFilter = new TextBox();
            label1 = new Label();
            dataGridView1 = new DataGridView();
            pbRemoveFilter = new PictureBox();
            lblFilter = new Label();
            label2 = new Label();
            tbTop = new TextBox();
            panel1 = new Panel();
            cbPreferNewer = new CheckBox();
            pFilter = new Panel();
            ((ISupportInitialize)dataGridView1).BeginInit();
            ((ISupportInitialize)pbRemoveFilter).BeginInit();
            panel1.SuspendLayout();
            pFilter.SuspendLayout();
            SuspendLayout();
            // 
            // tbContentFilter
            // 
            tbContentFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
            tbContentFilter.Location = new Point(44, 3);
            tbContentFilter.Name = "tbContentFilter";
            tbContentFilter.Size = new Size(577, 20);
            tbContentFilter.TabIndex = 8;
            tbContentFilter.TextChanged += new EventHandler(tbContentFilter_TextChanged);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 6);
            label1.Name = "label1";
            label1.Size = new Size(32, 13);
            label1.TabIndex = 7;
            label1.Text = "Filter:";
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
            pbRemoveFilter.Click += new EventHandler(pbRemoveFilter_Click);
            // 
            // lblFilter
            // 
            lblFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
            lblFilter.BackColor = Color.Goldenrod;
            lblFilter.ForeColor = SystemColors.ControlLightLight;
            lblFilter.Location = new Point(3, 3);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(816, 19);
            lblFilter.TabIndex = 9;
            lblFilter.Text = "Filtered Object";
            lblFilter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(725, 6);
            label2.Name = "label2";
            label2.Size = new Size(29, 13);
            label2.TabIndex = 11;
            label2.Text = "Top:";
            // 
            // tbTop
            // 
            tbTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tbTop.Location = new Point(760, 3);
            tbTop.Name = "tbTop";
            tbTop.Size = new Size(73, 20);
            tbTop.TabIndex = 12;
            tbTop.Text = "10000";
            tbTop.TextChanged += new EventHandler(tbTop_TextChanged);
            // 
            // panel1
            // 
            panel1.Controls.Add(cbPreferNewer);
            panel1.Controls.Add(tbContentFilter);
            panel1.Controls.Add(tbTop);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 543);
            panel1.Name = "panel1";
            panel1.Size = new Size(842, 28);
            panel1.TabIndex = 13;
            // 
            // cbPreferNewer
            // 
            cbPreferNewer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbPreferNewer.AutoSize = true;
            cbPreferNewer.Location = new Point(627, 5);
            cbPreferNewer.Name = "cbPreferNewer";
            cbPreferNewer.Size = new Size(92, 17);
            cbPreferNewer.TabIndex = 13;
            cbPreferNewer.Text = "Fetch Newest";
            cbPreferNewer.UseVisualStyleBackColor = true;
            cbPreferNewer.Checked = true;
            cbPreferNewer.CheckedChanged += new EventHandler(cbPreferNewer_CheckedChanged);
            // 
            // pFilter
            // 
            pFilter.Controls.Add(lblFilter);
            pFilter.Controls.Add(pbRemoveFilter);
            pFilter.Dock = DockStyle.Top;
            pFilter.Location = new Point(0, 0);
            pFilter.Name = "pFilter";
            pFilter.Size = new Size(842, 26);
            pFilter.TabIndex = 14;
            // 
            // LoggingTab
            // 
            Controls.Add(pFilter);
            Controls.Add(panel1);
            Controls.Add(dataGridView1);
            Name = "LoggingTabUI";
            Size = new Size(842, 571);
            ((ISupportInitialize)dataGridView1).EndInit();
            ((ISupportInitialize)pbRemoveFilter).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            pFilter.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        protected void LoadDataTable(DataTable dt)
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
            IDFilter = filter;

            if (filter.IsEmpty)
                Controls.Remove(pFilter);
            else
            {
                Controls.Add(pFilter);
                lblFilter.Text = filter.ToString();
            }

            FetchDataTable();
        }

        private void RegenerateFilters()
        {
            string rowFilter = "";

            if (!string.IsNullOrWhiteSpace(_customFilter) && !string.IsNullOrWhiteSpace(_freeTextFilter))
                rowFilter = _customFilter + " AND " + _freeTextFilter;
            else if (!string.IsNullOrWhiteSpace(_customFilter))
                rowFilter = _customFilter;
            else
                rowFilter = _freeTextFilter;

            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = rowFilter;
        }

        public override void SetDatabaseObject(IActivateItems activator, ExternalDatabaseServer databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            if(!databaseObject.DiscoverExistence(DataAccessContext.Logging, out string reason))
            {
                activator.KillForm(ParentForm,"Database " + databaseObject + " did not exist:" + reason);
                return;
            }
                
            LogManager = new LogManager(databaseObject);
            FetchDataTable();
        }

        public override string GetTabName()
        {
            return GetTableEnum() + "(" + base.GetTabName() + ")";
        }

        protected virtual LoggingTables GetTableEnum()
        {
            return LoggingTables.None;
        }

        private void FetchDataTable()
        {
            LoadDataTable(LogManager.GetTable(GetTableEnum(), IDFilter, TopX, cbPreferNewer.Checked));
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
            SetFilter(new LogViewerFilter());
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
