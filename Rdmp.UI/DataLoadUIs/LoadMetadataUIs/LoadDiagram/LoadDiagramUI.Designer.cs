using BrightIdeasSoftware;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    partial class LoadDiagramUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadDiagramUI));
            this.loadStateUI1 = new LoadStateUI();
            this.cbOnlyShowDynamicColumns = new System.Windows.Forms.CheckBox();
            this.tlvLoadedTables = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDataType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadedTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadStateUI1
            // 
            this.loadStateUI1.BackColor = System.Drawing.Color.Wheat;
            this.loadStateUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.loadStateUI1.Location = new System.Drawing.Point(0, 0);
            this.loadStateUI1.Name = "loadStateUI1";
            this.loadStateUI1.Size = new System.Drawing.Size(392, 19);
            this.loadStateUI1.TabIndex = 6;
            // 
            // cbOnlyShowDynamicColumns
            // 
            this.cbOnlyShowDynamicColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbOnlyShowDynamicColumns.AutoSize = true;
            this.cbOnlyShowDynamicColumns.Location = new System.Drawing.Point(41, 539);
            this.cbOnlyShowDynamicColumns.Name = "cbOnlyShowDynamicColumns";
            this.cbOnlyShowDynamicColumns.Size = new System.Drawing.Size(224, 17);
            this.cbOnlyShowDynamicColumns.TabIndex = 3;
            this.cbOnlyShowDynamicColumns.Text = "Only Show Columns Which Vary By Stage";
            this.cbOnlyShowDynamicColumns.UseVisualStyleBackColor = true;
            this.cbOnlyShowDynamicColumns.CheckedChanged += new System.EventHandler(this.cbOnlyShowDynamicColumns_CheckedChanged);
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
            this.tlvLoadedTables.Location = new System.Drawing.Point(3, 3);
            this.tlvLoadedTables.Name = "tlvLoadedTables";
            this.tlvLoadedTables.RowHeight = 19;
            this.tlvLoadedTables.ShowGroups = false;
            this.tlvLoadedTables.Size = new System.Drawing.Size(386, 504);
            this.tlvLoadedTables.TabIndex = 0;
            this.tlvLoadedTables.UseCompatibleStateImageBehavior = false;
            this.tlvLoadedTables.View = System.Windows.Forms.View.Details;
            this.tlvLoadedTables.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
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
            this.label1.Location = new System.Drawing.Point(3, 516);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 513);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(348, 20);
            this.tbFilter.TabIndex = 8;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(129, 214);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(104, 101);
            this.pbLoading.TabIndex = 170;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pbLoading);
            this.panel1.Controls.Add(this.tlvLoadedTables);
            this.panel1.Controls.Add(this.cbOnlyShowDynamicColumns);
            this.panel1.Controls.Add(this.tbFilter);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(392, 559);
            this.panel1.TabIndex = 171;
            // 
            // LoadDiagramUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.loadStateUI1);
            this.Name = "LoadDiagramUI";
            this.Size = new System.Drawing.Size(392, 578);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadedTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvLoadedTables;
        private OLVColumn olvName;
        private OLVColumn olvDataType;
        private System.Windows.Forms.CheckBox cbOnlyShowDynamicColumns;
        private OLVColumn olvState;
        private StateDiscovery.LoadStateUI loadStateUI1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.Panel panel1;
    }
}
