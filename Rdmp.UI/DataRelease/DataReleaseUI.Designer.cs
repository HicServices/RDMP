using BrightIdeasSoftware;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.DataRelease
{
    partial class DataReleaseUI
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkAndExecuteUI1 = new CheckAndExecuteUI();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tlvReleasePotentials = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvReleaseability = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvReleasePotentials)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkAndExecuteUI1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(697, 646);
            this.panel1.TabIndex = 19;
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(697, 646);
            this.checkAndExecuteUI1.TabIndex = 19;
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
            this.splitContainer1.Panel1.Controls.Add(this.tlvReleasePotentials);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(1025, 650);
            this.splitContainer1.SplitterDistance = 320;
            this.splitContainer1.TabIndex = 20;
            // 
            // tlvReleasePotentials
            // 
            this.tlvReleasePotentials.AllColumns.Add(this.olvName);
            this.tlvReleasePotentials.AllColumns.Add(this.olvReleaseability);
            this.tlvReleasePotentials.CellEditUseWholeCell = false;
            this.tlvReleasePotentials.CheckBoxes = true;
            this.tlvReleasePotentials.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvReleaseability});
            this.tlvReleasePotentials.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvReleasePotentials.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvReleasePotentials.HierarchicalCheckboxes = true;
            this.tlvReleasePotentials.Location = new System.Drawing.Point(0, 0);
            this.tlvReleasePotentials.Name = "tlvReleasePotentials";
            this.tlvReleasePotentials.RowHeight = 19;
            this.tlvReleasePotentials.ShowGroups = false;
            this.tlvReleasePotentials.ShowImagesOnSubItems = true;
            this.tlvReleasePotentials.Size = new System.Drawing.Size(316, 646);
            this.tlvReleasePotentials.TabIndex = 17;
            this.tlvReleasePotentials.UseCompatibleStateImageBehavior = false;
            this.tlvReleasePotentials.View = System.Windows.Forms.View.Details;
            this.tlvReleasePotentials.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
            // 
            // olvReleaseability
            // 
            this.olvReleaseability.Text = "Releasability";
            this.olvReleaseability.Width = 150;
            // 
            // DataReleaseUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DataReleaseUI";
            this.Size = new System.Drawing.Size(1025, 650);
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvReleasePotentials)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.TreeListView tlvReleasePotentials;
        private OLVColumn olvName;
        private OLVColumn olvReleaseability;


    }
}
