using Rdmp.Core.Curation.Data;

namespace Rdmp.UI.SimpleDialogs
{
    partial class CommitsUI
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
            this.treeListView1 = new BrightIdeasSoftware.TreeListView();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvUser = new BrightIdeasSoftware.OLVColumn();
            this.olvDescription = new BrightIdeasSoftware.OLVColumn();
            this.olvDate = new BrightIdeasSoftware.OLVColumn();
            this.taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeListView1
            // 
            this.treeListView1.AllColumns.Add(olvName);
            this.treeListView1.AllColumns.Add(olvUser);
            this.treeListView1.AllColumns.Add(olvDate);
            this.treeListView1.AllColumns.Add(olvDescription);
            this.treeListView1.CellEditUseWholeCell = false;
            this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvUser,
            this.olvDate,
            this.olvDescription});
            this.treeListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView1.Location = new System.Drawing.Point(0, 42);
            this.treeListView1.Name = "treeListView1";
            this.treeListView1.ShowGroups = false;
            this.treeListView1.Size = new System.Drawing.Size(800, 408);
            this.treeListView1.TabIndex = 1;
            this.treeListView1.View = System.Windows.Forms.View.Details;
            this.treeListView1.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.Text = "Name";
            this.olvName.AspectName = "ToString";
            this.olvName.Width = 150;
            this.olvName.Width = 150;
            this.olvName.Sortable = true;
            // 
            // olvUser
            // 
            this.olvUser.Text = "User";
            this.olvUser.AspectName = nameof(Commit.Username);
            this.olvUser.Width = 100;
            this.olvUser.MinimumWidth = 100;
            // 
            // olvDate
            // 
            this.olvDate.Text = "Date";
            this.olvDate.Width = 120;
            this.olvDate.Width = 120;
            this.olvDate.AspectName = nameof(Commit.Date);
            // 
            // olvDescription
            // 
            this.olvDescription.Text = "Description";
            this.olvDescription.Width = 200;
            this.olvDate.Width = 200;
            this.olvDescription.AspectName = nameof(Commit.Description);
            // 
            // taskDescriptionLabel1
            // 
            this.taskDescriptionLabel1.AutoSize = true;
            this.taskDescriptionLabel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
            this.taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            this.taskDescriptionLabel1.Size = new System.Drawing.Size(800, 42);
            this.taskDescriptionLabel1.TabIndex = 2;
            // 
            // CommitsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.treeListView1);
            this.Controls.Add(this.taskDescriptionLabel1);
            this.Name = "CommitsUI";
            this.Text = "CommitsUI";
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private BrightIdeasSoftware.TreeListView treeListView1;
        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvUser;
        private BrightIdeasSoftware.OLVColumn olvDescription;
        private BrightIdeasSoftware.OLVColumn olvDate;
        private TaskDescriptionLabel taskDescriptionLabel1;
    }
}