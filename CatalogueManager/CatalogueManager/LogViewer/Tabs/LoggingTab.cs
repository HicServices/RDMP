using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// </summary>
    public class LoggingTab : LoggingTab_Design
    {
        private TextBox tbContentFilter;
        private Label label1;
        private PictureBox pbRemoveFilter;
        private Label lblFilter;
        protected DataGridView dataGridView1;

        public LoggingTab()
        {
            InitializeComponent();

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.CellMouseClick += DataGridView1OnCellMouseClick;
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
            this.tbContentFilter.Size = new System.Drawing.Size(804, 20);
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
            // LoggingTab
            // 
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

        private string _customFilter;
        private string _freeTextFilter;
        private LogViewerFilter _filter;

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

        protected void SetFilter(string customFilter)
        {
            _customFilter = customFilter;
            RegenerateFilters();
        }

        public virtual void SetFilter(LogViewerFilter filter)
        {
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

            var lm = new LogManager(databaseObject);
            var dt = FetchDataTable(lm);
            LoadDataTable(dt);
        }

        public override string GetTabName()
        {
            return GetType().Name.Replace("Logging","").Replace("Tab","") + "(" +base.GetTabName() + ")";
        }

        protected virtual DataTable FetchDataTable(LogManager lm)
        {
            return null;
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
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoggingTab_Design, UserControl>))]
    public abstract class LoggingTab_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>
    {

    }
}
