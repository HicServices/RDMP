using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// TECHNICAL:Base class for all the other logging tabs e.g. LoggingDataSourcesTab
    /// </summary>
    public class LoggingTab :UserControl
    {
        private TextBox tbContentFilter;
        private Label label1;
        protected DataGridView dataGridView1;
        protected LogViewerFilterCollection _filters;

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
            this.tbContentFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // tbContentFilter
            // 
            this.tbContentFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbContentFilter.Location = new System.Drawing.Point(73, 542);
            this.tbContentFilter.Name = "tbContentFilter";
            this.tbContentFilter.Size = new System.Drawing.Size(766, 20);
            this.tbContentFilter.TabIndex = 8;
            this.tbContentFilter.TextChanged += new System.EventHandler(this.tbContentFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 545);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Content Filter:";
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
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(836, 528);
            this.dataGridView1.TabIndex = 6;
            // 
            // LoggingTab
            // 
            this.Controls.Add(this.tbContentFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "LoggingTab";
            this.Size = new System.Drawing.Size(842, 571);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected bool _bLoaded = false;
        private string _customFilter;
        private string _freeTextFilter;

        protected void LoadDataTable(DataTable dt)
        {
            if(_bLoaded)
                throw new Exception("Data table already loaded, why are you trying to load it again?");

            _bLoaded = true;
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
    }
}
