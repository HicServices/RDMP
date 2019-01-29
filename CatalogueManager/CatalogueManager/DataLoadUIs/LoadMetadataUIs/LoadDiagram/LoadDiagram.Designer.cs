using BrightIdeasSoftware;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    partial class LoadDiagram
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadDiagram));
            this.btnDiscoverTables = new System.Windows.Forms.Button();
            this.loadStateUI1 = new CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery.LoadStateUI();
            this.helpIconDiscoverTables = new ReusableUIComponents.HelpIcon();
            this.cbOnlyShowDynamicColumns = new System.Windows.Forms.CheckBox();
            this.ragSmiley1 = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.tlvLoadedTables = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDataType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadedTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDiscoverTables
            // 
            this.btnDiscoverTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiscoverTables.Location = new System.Drawing.Point(287, 0);
            this.btnDiscoverTables.Name = "btnDiscoverTables";
            this.btnDiscoverTables.Size = new System.Drawing.Size(71, 19);
            this.btnDiscoverTables.TabIndex = 4;
            this.btnDiscoverTables.Text = "Fetch State";
            this.btnDiscoverTables.UseVisualStyleBackColor = true;
            this.btnDiscoverTables.Click += new System.EventHandler(this.btnDiscoverTables_Click);
            // 
            // loadStateUI1
            // 
            this.loadStateUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadStateUI1.BackColor = System.Drawing.Color.Wheat;
            this.loadStateUI1.Location = new System.Drawing.Point(0, 0);
            this.loadStateUI1.Name = "loadStateUI1";
            this.loadStateUI1.Size = new System.Drawing.Size(263, 19);
            this.loadStateUI1.TabIndex = 6;
            // 
            // helpIconDiscoverTables
            // 
            this.helpIconDiscoverTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIconDiscoverTables.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconDiscoverTables.BackgroundImage")));
            this.helpIconDiscoverTables.Location = new System.Drawing.Point(364, 0);
            this.helpIconDiscoverTables.Name = "helpIconDiscoverTables";
            this.helpIconDiscoverTables.Size = new System.Drawing.Size(19, 19);
            this.helpIconDiscoverTables.TabIndex = 5;
            // 
            // cbOnlyShowDynamicColumns
            // 
            this.cbOnlyShowDynamicColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbOnlyShowDynamicColumns.AutoSize = true;
            this.cbOnlyShowDynamicColumns.Location = new System.Drawing.Point(6, 557);
            this.cbOnlyShowDynamicColumns.Name = "cbOnlyShowDynamicColumns";
            this.cbOnlyShowDynamicColumns.Size = new System.Drawing.Size(224, 17);
            this.cbOnlyShowDynamicColumns.TabIndex = 3;
            this.cbOnlyShowDynamicColumns.Text = "Only Show Columns Which Vary By Stage";
            this.cbOnlyShowDynamicColumns.UseVisualStyleBackColor = true;
            this.cbOnlyShowDynamicColumns.CheckedChanged += new System.EventHandler(this.cbOnlyShowDynamicColumns_CheckedChanged);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(263, -1);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 2;
            // 
            // tlvLoadedTables
            // 
            this.tlvLoadedTables.AllColumns.Add(this.olvName);
            this.tlvLoadedTables.AllColumns.Add(this.olvState);
            this.tlvLoadedTables.AllColumns.Add(this.olvDataType);
            this.tlvLoadedTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvLoadedTables.CellEditUseWholeCell = false;
            this.tlvLoadedTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvState,
            this.olvDataType});
            this.tlvLoadedTables.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvLoadedTables.Location = new System.Drawing.Point(6, 25);
            this.tlvLoadedTables.Name = "tlvLoadedTables";
            this.tlvLoadedTables.RowHeight = 19;
            this.tlvLoadedTables.ShowGroups = false;
            this.tlvLoadedTables.Size = new System.Drawing.Size(386, 500);
            this.tlvLoadedTables.TabIndex = 0;
            this.tlvLoadedTables.UseCompatibleStateImageBehavior = false;
            this.tlvLoadedTables.View = System.Windows.Forms.View.Details;
            this.tlvLoadedTables.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Name";
            // 
            // olvState
            // 
            this.olvState.Text = "State";
            this.olvState.Width = 96;
            // 
            // olvDataType
            // 
            this.olvDataType.AspectName = "";
            this.olvDataType.Text = "Data Type";
            this.olvDataType.Width = 90;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 534);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 531);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(291, 20);
            this.tbFilter.TabIndex = 8;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(338, 531);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(51, 22);
            this.btnExpandOrCollapse.TabIndex = 169;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(159, 216);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(104, 101);
            this.pbLoading.TabIndex = 170;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // LoadDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.btnExpandOrCollapse);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.cbOnlyShowDynamicColumns);
            this.Controls.Add(this.helpIconDiscoverTables);
            this.Controls.Add(this.btnDiscoverTables);
            this.Controls.Add(this.loadStateUI1);
            this.Controls.Add(this.tlvLoadedTables);
            this.Name = "LoadDiagram";
            this.Size = new System.Drawing.Size(392, 578);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadedTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvLoadedTables;
        private OLVColumn olvName;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley1;
        private OLVColumn olvDataType;
        private System.Windows.Forms.CheckBox cbOnlyShowDynamicColumns;
        private System.Windows.Forms.Button btnDiscoverTables;
        private ReusableUIComponents.HelpIcon helpIconDiscoverTables;
        private OLVColumn olvState;
        private StateDiscovery.LoadStateUI loadStateUI1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Button btnExpandOrCollapse;
        private System.Windows.Forms.PictureBox pbLoading;
    }
}
