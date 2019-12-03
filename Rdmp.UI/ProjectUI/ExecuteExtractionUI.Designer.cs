

using BrightIdeasSoftware;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.ProjectUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecuteExtractionUI));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.tlvDatasets = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.helpIcon1 = new HelpIcon();
            this.checkAndExecuteUI1 = new CheckAndExecuteUI();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).BeginInit();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.tlvDatasets);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.helpIcon1);
            this.splitContainer1.Panel2.Controls.Add(this.checkAndExecuteUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1025, 650);
            this.splitContainer1.SplitterDistance = 320;
            this.splitContainer1.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 626);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(51, 623);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(262, 20);
            this.tbFilter.TabIndex = 29;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // tlvDatasets
            // 
            this.tlvDatasets.AllColumns.Add(this.olvName);
            this.tlvDatasets.AllColumns.Add(this.olvState);
            this.tlvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvDatasets.CellEditUseWholeCell = false;
            this.tlvDatasets.CheckBoxes = true;
            this.tlvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvState});
            this.tlvDatasets.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDatasets.Location = new System.Drawing.Point(0, 0);
            this.tlvDatasets.Name = "tlvDatasets";
            this.tlvDatasets.RowHeight = 19;
            this.tlvDatasets.ShowGroups = false;
            this.tlvDatasets.ShowImagesOnSubItems = true;
            this.tlvDatasets.Size = new System.Drawing.Size(318, 617);
            this.tlvDatasets.TabIndex = 28;
            this.tlvDatasets.UseCompatibleStateImageBehavior = false;
            this.tlvDatasets.View = System.Windows.Forms.View.Details;
            this.tlvDatasets.VirtualMode = true;
            this.tlvDatasets.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.tlvDatasets_ItemChecked);
            this.tlvDatasets.SelectedIndexChanged += new System.EventHandler(this.olvDatasets_SelectedIndexChanged);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            this.olvName.Width = 118;
            this.olvName.MinimumWidth = 100;
            // 
            // olvState
            // 
            this.olvState.Groupable = false;
            this.olvState.Text = "Extraction State";
            this.olvState.Width = 160;
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.Location = new System.Drawing.Point(674, 3);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 32;
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(699, 645);
            this.checkAndExecuteUI1.TabIndex = 28;
            // 
            // ExecuteExtractionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ExecuteExtractionUI";
            this.Size = new System.Drawing.Size(1025, 650);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private TreeListView tlvDatasets;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private CheckAndExecuteUI checkAndExecuteUI1;
        private OLVColumn olvName;
        private OLVColumn olvState;
        private HelpIcon helpIcon1;
    }
}