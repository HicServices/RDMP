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
            this.olvDate = new BrightIdeasSoftware.OLVColumn();
            this.olvDescription = new BrightIdeasSoftware.OLVColumn();
            this.taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.btnExpandAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeListView1
            // 
            this.treeListView1.AllColumns.Add(this.olvName);
            this.treeListView1.AllColumns.Add(this.olvUser);
            this.treeListView1.AllColumns.Add(this.olvDate);
            this.treeListView1.AllColumns.Add(this.olvDescription);
            this.treeListView1.CellEditUseWholeCell = false;
            this.treeListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvUser,
            this.olvDate,
            this.olvDescription});
            this.treeListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListView1.Location = new System.Drawing.Point(0, 72);
            this.treeListView1.Name = "treeListView1";
            this.treeListView1.ShowGroups = false;
            this.treeListView1.Size = new System.Drawing.Size(800, 378);
            this.treeListView1.TabIndex = 1;
            this.treeListView1.View = System.Windows.Forms.View.Details;
            this.treeListView1.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Text = "Name";
            this.olvName.Width = 150;
            // 
            // olvUser
            // 
            this.olvUser.AspectName = "Username";
            this.olvUser.MinimumWidth = 100;
            this.olvUser.Text = "User";
            this.olvUser.Width = 100;
            // 
            // olvDate
            // 
            this.olvDate.AspectName = "Date";
            this.olvDate.Text = "Date";
            this.olvDate.Width = 200;
            // 
            // olvDescription
            // 
            this.olvDescription.AspectName = "Commit.Description";
            this.olvDescription.Text = "Description";
            this.olvDescription.Width = 200;
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
            // panel1
            // 
            this.panel1.Controls.Add(this.btnExpandAll);
            this.panel1.Controls.Add(this.tbFilter);
            this.panel1.Controls.Add(this.lblFilter);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 42);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 30);
            this.panel1.TabIndex = 3;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(45, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(671, 23);
            this.tbFilter.TabIndex = 1;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(3, 6);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(36, 15);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "Filter:";
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandAll.Location = new System.Drawing.Point(722, 3);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(75, 23);
            this.btnExpandAll.TabIndex = 2;
            this.btnExpandAll.Text = "Expand All";
            this.btnExpandAll.UseVisualStyleBackColor = true;
            this.btnExpandAll.Click += new System.EventHandler(this.btnExpandAll_Click);
            // 
            // CommitsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeListView1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.taskDescriptionLabel1);
            this.Name = "CommitsUI";
            this.Size = new System.Drawing.Size(800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.treeListView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Button btnExpandAll;
    }
}