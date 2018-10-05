using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using ReusableUIComponents;
using Cursor = System.Windows.Forms.Cursor;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// TECHNICAL:Base class for all the other logging tabs e.g. LoggingDataSourcesTab
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
    public class LoggingTab : LoggingTab_Design
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
        protected LogManager LogManager;

        public LoggingTab()
        {
            InitializeComponent();

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.CellMouseClick += DataGridView1OnCellMouseClick;

            TopX = UpdateTopX();
        }

        private int UpdateTopX()
        {
            try
            {
                var result = int.Parse(tbTop.Text);
                if(result <=0)
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

                if(menu.Items.Count != 0)
                    menu.Items.Add(new ToolStripSeparator());

                menu.Items.Add("View as Text", null, (s, ex) => WideMessageBox.Show(sb.ToString()));

                menu.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            
            var cmd = GetCommands(e.RowIndex).FirstOrDefault();

            if(cmd != null)
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
        
        protected void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoggingTab));
            this.tbContentFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pbRemoveFilter = new System.Windows.Forms.PictureBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTop = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).BeginInit();
            this.SuspendLayout();
            // 
            // tbContentFilter
            // 
            this.tbContentFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbContentFilter.Location = new System.Drawing.Point(35, 542);
            this.tbContentFilter.Name = "tbContentFilter";
            this.tbContentFilter.Size = new System.Drawing.Size(628, 20);
            this.tbContentFilter.TabIndex = 8;
            this.tbContentFilter.TextChanged += new System.EventHandler(this.tbContentFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 545);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Filter:";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(3, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(836, 536);
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
            this.pbRemoveFilter.Visible = false;
            this.pbRemoveFilter.Click += new System.EventHandler(this.pbRemoveFilter_Click);
            // 
            // lblFilter
            // 
            this.lblFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFilter.BackColor = System.Drawing.Color.Goldenrod;
            this.lblFilter.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblFilter.Location = new System.Drawing.Point(2, 3);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(821, 19);
            this.lblFilter.TabIndex = 9;
            this.lblFilter.Text = "Filtered Object";
            this.lblFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFilter.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(669, 545);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Top:";
            // 
            // tbTop
            // 
            this.tbTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTop.Location = new System.Drawing.Point(704, 542);
            this.tbTop.Name = "tbTop";
            this.tbTop.Size = new System.Drawing.Size(138, 20);
            this.tbTop.TabIndex = 12;
            this.tbTop.Text = "10000";
            this.tbTop.TextChanged += new System.EventHandler(this.tbTop_TextChanged);
            // 
            // LoggingTab
            // 
            this.Controls.Add(this.tbTop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbRemoveFilter);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.tbContentFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "LoggingTab";
            this.Size = new System.Drawing.Size(842, 571);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemoveFilter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

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
            {
                lblFilter.Visible = false;
                pbRemoveFilter.Visible = false;
                dataGridView1.Top = 0;
                dataGridView1.Height += lblFilter.Bottom;
            }
            else
            {
                lblFilter.Visible = true;
                pbRemoveFilter.Visible = true;
                lblFilter.Text = filter.ToString();
                dataGridView1.Top = lblFilter.Bottom;
                dataGridView1.Height = tbContentFilter.Top - dataGridView1.Top;
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

            LogManager = new LogManager(databaseObject);
            FetchDataTable();
        }

        public override string GetTabName()
        {
            return GetTableEnum() + "(" +base.GetTabName() + ")";
        }

        protected virtual LoggingTables GetTableEnum()
        {
            return LoggingTables.None;
        }

        private void FetchDataTable()
        {
            LoadDataTable(LogManager.GetTable(GetTableEnum(), IDFilter, TopX));
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
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoggingTab_Design, UserControl>))]
    public abstract class LoggingTab_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>
    {

    }
}
