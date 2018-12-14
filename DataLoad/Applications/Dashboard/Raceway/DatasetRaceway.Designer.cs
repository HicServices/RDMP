namespace Dashboard.Raceway
{
    partial class DatasetRaceway
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatasetRaceway));
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAddCatalogue = new System.Windows.Forms.ToolStripButton();
            this.btnAddExtractableDatasetPackage = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ddShowPeriod = new System.Windows.Forms.ToolStripComboBox();
            this.cbIgnoreRowCounts = new System.Windows.Forms.ToolStripButton();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.racewayRenderArea = new Dashboard.Raceway.RacewayRenderAreaUI();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 609);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "(Buckets are only shown for months where there is data)";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddCatalogue,
            this.btnAddExtractableDatasetPackage,
            this.btnRemoveAll,
            this.toolStripLabel1,
            this.ddShowPeriod,
            this.cbIgnoreRowCounts});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1045, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAddCatalogue
            // 
            this.btnAddCatalogue.Image = ((System.Drawing.Image)(resources.GetObject("btnAddCatalogue.Image")));
            this.btnAddCatalogue.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddCatalogue.Name = "btnAddCatalogue";
            this.btnAddCatalogue.Size = new System.Drawing.Size(49, 22);
            this.btnAddCatalogue.Text = "Add";
            this.btnAddCatalogue.Click += new System.EventHandler(this.btnAddCatalogue_Click);
            // 
            // btnAddExtractableDatasetPackage
            // 
            this.btnAddExtractableDatasetPackage.Image = ((System.Drawing.Image)(resources.GetObject("btnAddExtractableDatasetPackage.Image")));
            this.btnAddExtractableDatasetPackage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddExtractableDatasetPackage.Name = "btnAddExtractableDatasetPackage";
            this.btnAddExtractableDatasetPackage.Size = new System.Drawing.Size(49, 22);
            this.btnAddExtractableDatasetPackage.Text = "Add";
            this.btnAddExtractableDatasetPackage.Click += new System.EventHandler(this.btnAddExtractableDatasetPackage_Click);
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveAll.Image")));
            this.btnRemoveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Size = new System.Drawing.Size(54, 22);
            this.btnRemoveAll.Text = "Clear";
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(73, 22);
            this.toolStripLabel1.Text = "Show Period";
            // 
            // ddShowPeriod
            // 
            this.ddShowPeriod.Name = "ddShowPeriod";
            this.ddShowPeriod.Size = new System.Drawing.Size(121, 25);
            this.ddShowPeriod.DropDownClosed += new System.EventHandler(this.ddShowPeriod_DropDownClosed);
            // 
            // cbIgnoreRowCounts
            // 
            this.cbIgnoreRowCounts.CheckOnClick = true;
            this.cbIgnoreRowCounts.Image = ((System.Drawing.Image)(resources.GetObject("cbIgnoreRowCounts.Image")));
            this.cbIgnoreRowCounts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cbIgnoreRowCounts.Name = "cbIgnoreRowCounts";
            this.cbIgnoreRowCounts.Size = new System.Drawing.Size(84, 22);
            this.cbIgnoreRowCounts.Text = "No Counts";
            this.cbIgnoreRowCounts.CheckedChanged += new System.EventHandler(this.cbIgnoreRowCounts_CheckedChanged);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(515, 241);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 8;
            // 
            // racewayRenderArea
            // 
            this.racewayRenderArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.racewayRenderArea.Location = new System.Drawing.Point(0, 28);
            this.racewayRenderArea.Margin = new System.Windows.Forms.Padding(0);
            this.racewayRenderArea.Name = "racewayRenderArea";
            this.racewayRenderArea.Size = new System.Drawing.Size(1045, 578);
            this.racewayRenderArea.TabIndex = 9;
            // 
            // DatasetRaceway
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.racewayRenderArea);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DatasetRaceway";
            this.Size = new System.Drawing.Size(1045, 622);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnAddCatalogue;
        private System.Windows.Forms.ToolStripButton btnRemoveAll;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private RacewayRenderAreaUI racewayRenderArea;
        private System.Windows.Forms.ToolStripButton btnAddExtractableDatasetPackage;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox ddShowPeriod;
        private System.Windows.Forms.ToolStripButton cbIgnoreRowCounts;
    }
}
