namespace Rdmp.UI.SimpleDialogs.Cohorts
{
    partial class CohortCommitProjectSelectionUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortCommitProjectSelectionUI));
            btnCurrentProject = new System.Windows.Forms.Button();
            btnNewProject = new System.Windows.Forms.Button();
            btnExistingProject = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnCurrentProject
            // 
            btnCurrentProject.Location = new System.Drawing.Point(12, 12);
            btnCurrentProject.Name = "btnCurrentProject";
            btnCurrentProject.Size = new System.Drawing.Size(187, 23);
            btnCurrentProject.TabIndex = 0;
            btnCurrentProject.Text = "This Project";
            btnCurrentProject.UseVisualStyleBackColor = true;
            btnCurrentProject.Click += btnCurrentProject_Click;
            // 
            // btnNewProject
            // 
            btnNewProject.Location = new System.Drawing.Point(12, 41);
            btnNewProject.Name = "btnNewProject";
            btnNewProject.Size = new System.Drawing.Size(187, 23);
            btnNewProject.TabIndex = 1;
            btnNewProject.Text = "A New Project";
            btnNewProject.UseVisualStyleBackColor = true;
            btnNewProject.Click += btnNewProject_Click;
            // 
            // btnExistingProject
            // 
            btnExistingProject.Location = new System.Drawing.Point(12, 70);
            btnExistingProject.Name = "btnExistingProject";
            btnExistingProject.Size = new System.Drawing.Size(187, 23);
            btnExistingProject.TabIndex = 2;
            btnExistingProject.Text = "An Existing Project";
            btnExistingProject.UseVisualStyleBackColor = true;
            btnExistingProject.Click += btnExistingProject_Click;
            // 
            // CohortCommitProjectSelectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(238, 194);
            Controls.Add(btnExistingProject);
            Controls.Add(btnNewProject);
            Controls.Add(btnCurrentProject);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "CohortCommitProjectSelectionUI";
            Text = "Commit Cohort";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnCurrentProject;
        private System.Windows.Forms.Button btnNewProject;
        private System.Windows.Forms.Button btnExistingProject;
    }
}