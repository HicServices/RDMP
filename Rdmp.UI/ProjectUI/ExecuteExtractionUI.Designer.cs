

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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFilter = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.tlvDatasets = new BrightIdeasSoftware.TreeListView();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvState = new BrightIdeasSoftware.OLVColumn();
            this.helpIcon1 = new Rdmp.UI.SimpleControls.HelpIcon();
            this.checkAndExecuteUI1 = new Rdmp.UI.SimpleControls.CheckAndExecuteUI();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.tlvDatasets);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.helpIcon1);
            this.splitContainer1.Panel2.Controls.Add(this.checkAndExecuteUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1196, 750);
            this.splitContainer1.SplitterDistance = 373;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 29;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblFilter);
            this.panel1.Controls.Add(this.tbFilter);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 712);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(369, 34);
            this.panel1.TabIndex = 32;
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(3, 8);
            this.lblFilter.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(36, 15);
            this.lblFilter.TabIndex = 30;
            this.lblFilter.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(39, 5);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(0, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(326, 23);
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
            this.tlvDatasets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tlvDatasets.CellEditUseWholeCell = false;
            this.tlvDatasets.CheckBoxes = true;
            this.tlvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvState});
            this.tlvDatasets.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDatasets.HideSelection = false;
            this.tlvDatasets.Location = new System.Drawing.Point(-2, -2);
            this.tlvDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tlvDatasets.Name = "tlvDatasets";
            this.tlvDatasets.RowHeight = 19;
            this.tlvDatasets.ShowGroups = false;
            this.tlvDatasets.ShowImagesOnSubItems = true;
            this.tlvDatasets.Size = new System.Drawing.Size(371, 714);
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
            this.olvName.Groupable = false;
            this.olvName.MinimumWidth = 100;
            this.olvName.Text = "Name";
            this.olvName.Width = 118;
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
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(782, 9);
            this.helpIcon1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(26, 25);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(26, 25);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(26, 25);
            this.helpIcon1.SuppressClick = false;
            this.helpIcon1.TabIndex = 32;
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(809, 745);
            this.checkAndExecuteUI1.TabIndex = 28;
            // 
            // ExecuteExtractionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ExecuteExtractionUI";
            this.Size = new System.Drawing.Size(1196, 750);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private TreeListView tlvDatasets;
        private CheckAndExecuteUI checkAndExecuteUI1;
        private OLVColumn olvName;
        private OLVColumn olvState;
        private HelpIcon helpIcon1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox tbFilter;
    }
}