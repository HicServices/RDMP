using BrightIdeasSoftware;

namespace Rdmp.UI.SimpleDialogs
{
    partial class SetProjectsForCatalogueUI
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
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            button2 = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            fastObjectListView1 = new FastObjectListView();
            Project = new OLVColumn();
            ProjectID = new OLVColumn();
            label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)fastObjectListView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new System.Drawing.Point(515, 399);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(204, 23);
            button1.TabIndex = 0;
            button1.Text = "Restrict to Selected Projects";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 4);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(49, 15);
            label1.TabIndex = 2;
            label1.Text = "Projects";
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(423, 399);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(75, 23);
            button2.TabIndex = 3;
            button2.Text = "Validate";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(423, 22);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(0, 15);
            label2.TabIndex = 4;
            // 
            // fastObjectListView1
            // 
            fastObjectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { Project, ProjectID });
            fastObjectListView1.Location = new System.Drawing.Point(12, 22);
            fastObjectListView1.Name = "fastObjectListView1";
            fastObjectListView1.ShowGroups = false;
            fastObjectListView1.Size = new System.Drawing.Size(389, 400);
            fastObjectListView1.TabIndex = 5;
            fastObjectListView1.View = System.Windows.Forms.View.Details;
            fastObjectListView1.VirtualMode = true;
            fastObjectListView1.SelectedIndexChanged += fastObjectListView1_SelectedIndexChanged;
            // 
            // Project
            // 
            Project.MinimumWidth = 100;
            Project.Text = "Project";
            Project.Width = 100;
            // 
            // ProjectID
            // 
            ProjectID.MinimumWidth = 20;
            ProjectID.Text = "ID";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(430, 427);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(0, 15);
            label3.TabIndex = 6;
            // 
            // SetProjectsForCatalogueUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(label3);
            Controls.Add(fastObjectListView1);
            Controls.Add(label2);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(button1);
            Name = "SetProjectsForCatalogueUI";
            Text = "Restrict to Specific Projects";
            ((System.ComponentModel.ISupportInitialize)fastObjectListView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private BrightIdeasSoftware.FastObjectListView fastObjectListView1;
        private OLVColumn Project;
        private OLVColumn ProjectID;
        private System.Windows.Forms.Label label3;
    }
}