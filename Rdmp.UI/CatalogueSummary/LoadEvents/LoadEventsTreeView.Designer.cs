using BrightIdeasSoftware;
using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.CatalogueSummary.LoadEvents
{
    partial class LoadEventsTreeView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadEventsTreeView));
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.llLoading = new System.Windows.Forms.LinkLabel();
            this.treeView1 = new BrightIdeasSoftware.TreeListView();
            this.olvDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDescription = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeView1)).BeginInit();
            this.SuspendLayout();
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.Location = new System.Drawing.Point(278, 215);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(22, 22);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoading.TabIndex = 172;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // llLoading
            // 
            this.llLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.llLoading.AutoSize = true;
            this.llLoading.Location = new System.Drawing.Point(306, 218);
            this.llLoading.Name = "llLoading";
            this.llLoading.Size = new System.Drawing.Size(122, 13);
            this.llLoading.TabIndex = 171;
            this.llLoading.TabStop = true;
            this.llLoading.Text = "Cancel Loading Logging";
            this.llLoading.Visible = false;
            this.llLoading.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llLoading_LinkClicked);
            // 
            // treeView1
            // 
            this.treeView1.AllColumns.Add(this.olvDate);
            this.treeView1.AllColumns.Add(this.olvDescription);
            this.treeView1.CellEditUseWholeCell = false;
            this.treeView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvDate,
            this.olvDescription});
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.FullRowSelect = true;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.ShowGroups = false;
            this.treeView1.Size = new System.Drawing.Size(706, 482);
            this.treeView1.TabIndex = 170;
            this.treeView1.UseCompatibleStateImageBehavior = false;
            this.treeView1.View = System.Windows.Forms.View.Details;
            this.treeView1.VirtualMode = true;
            this.treeView1.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.treeView1_ColumnRightClick);
            this.treeView1.ItemActivate += new System.EventHandler(this.treeView1_ItemActivate);
            this.treeView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyUp);
            // 
            // olvDate
            // 
            this.olvDate.AspectName = "StartTime";
            this.olvDate.Text = "Date";
            this.olvDate.Width = 150;
            // 
            // olvDescription
            // 
            this.olvDescription.AspectName = "ToString";
            this.olvDescription.Text = "Description";
            this.olvDescription.MinimumWidth = 100;
            // 
            // LoadEventsTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.llLoading);
            this.Controls.Add(this.treeView1);
            this.Name = "LoadEventsTreeView";
            this.Size = new System.Drawing.Size(706, 482);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.LinkLabel llLoading;
        private TreeListView treeView1;
        private OLVColumn olvDescription;
        private OLVColumn olvDate;
    }
}
