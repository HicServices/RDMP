

using BrightIdeasSoftware;

namespace DataExportManager.ProjectUI
{
    partial class ExecuteExtractionUI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cbSkipValidation = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTopX = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.olvDatasets = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvDatasets)).BeginInit();
            this.SuspendLayout();
            // 
            // cbSkipValidation
            // 
            this.cbSkipValidation.AutoSize = true;
            this.cbSkipValidation.Checked = true;
            this.cbSkipValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSkipValidation.Location = new System.Drawing.Point(6, 18);
            this.cbSkipValidation.Name = "cbSkipValidation";
            this.cbSkipValidation.Size = new System.Drawing.Size(96, 17);
            this.cbSkipValidation.TabIndex = 11;
            this.cbSkipValidation.Text = "Skip Validation";
            this.cbSkipValidation.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Limit (TOP X), X=";
            // 
            // tbTopX
            // 
            this.tbTopX.Location = new System.Drawing.Point(100, 34);
            this.tbTopX.Name = "tbTopX";
            this.tbTopX.Size = new System.Drawing.Size(94, 20);
            this.tbTopX.TabIndex = 18;
            this.tbTopX.TextChanged += new System.EventHandler(this.tbTopX_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbSkipValidation);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbTopX);
            this.groupBox2.Location = new System.Drawing.Point(195, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(203, 59);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(964, 27);
            this.panel1.TabIndex = 27;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer1.Panel1.Controls.Add(this.olvDatasets);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Controls.Add(this.checkAndExecuteUI1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(1292, 576);
            this.splitContainer1.SplitterDistance = 320;
            this.splitContainer1.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 552);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(51, 549);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(262, 20);
            this.tbFilter.TabIndex = 29;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // olvDatasets
            // 
            this.olvDatasets.AllColumns.Add(this.olvName);
            this.olvDatasets.AllColumns.Add(this.olvState);
            this.olvDatasets.AllColumns.Add(this.olvID);
            this.olvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvDatasets.CellEditUseWholeCell = false;
            this.olvDatasets.CheckBoxes = true;
            this.olvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvState});
            this.olvDatasets.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvDatasets.Location = new System.Drawing.Point(0, 0);
            this.olvDatasets.Name = "olvDatasets";
            this.olvDatasets.RowHeight = 19;
            this.olvDatasets.ShowGroups = false;
            this.olvDatasets.ShowImagesOnSubItems = true;
            this.olvDatasets.Size = new System.Drawing.Size(318, 543);
            this.olvDatasets.TabIndex = 28;
            this.olvDatasets.UseCompatibleStateImageBehavior = false;
            this.olvDatasets.View = System.Windows.Forms.View.Details;
            this.olvDatasets.VirtualMode = true;
            this.olvDatasets.SelectedIndexChanged += new System.EventHandler(this.olvDatasets_SelectedIndexChanged);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            this.olvName.Width = 118;
            // 
            // olvState
            // 
            this.olvState.Groupable = false;
            this.olvState.Text = "State";
            // 
            // olvID
            // 
            this.olvID.AspectName = "ID";
            this.olvID.IsVisible = false;
            this.olvID.Text = "ID";
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 27);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(964, 545);
            this.checkAndExecuteUI1.TabIndex = 28;
            // 
            // ExecuteExtractionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ExecuteExtractionUI";
            this.Size = new System.Drawing.Size(1292, 576);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvDatasets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbSkipValidation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTopX;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TreeListView olvDatasets;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private CatalogueManager.SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private OLVColumn olvName;
        private OLVColumn olvState;
        private OLVColumn olvID;
    }
}