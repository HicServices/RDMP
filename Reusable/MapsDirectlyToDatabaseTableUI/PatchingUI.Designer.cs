namespace MapsDirectlyToDatabaseTableUI
{
    partial class PatchingUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblHostAssembly = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.btnAttemptPatching = new System.Windows.Forms.Button();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPatchingAssembly = new System.Windows.Forms.Label();
            this.lblDatabaseVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(339, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "The following Patch related problems  were detected in your database:";
            // 
            // lblHostAssembly
            // 
            this.lblHostAssembly.AutoSize = true;
            this.lblHostAssembly.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblHostAssembly.Location = new System.Drawing.Point(112, 50);
            this.lblHostAssembly.Name = "lblHostAssembly";
            this.lblHostAssembly.Size = new System.Drawing.Size(92, 15);
            this.lblHostAssembly.TabIndex = 2;
            this.lblHostAssembly.Text = "Hosting Assembly";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Patching Assembly:";
            // 
            // checksUI1
            // 
            this.checksUI1.Location = new System.Drawing.Point(12, 168);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1271, 442);
            this.checksUI1.TabIndex = 4;
            // 
            // btnAttemptPatching
            // 
            this.btnAttemptPatching.Location = new System.Drawing.Point(15, 139);
            this.btnAttemptPatching.Name = "btnAttemptPatching";
            this.btnAttemptPatching.Size = new System.Drawing.Size(181, 23);
            this.btnAttemptPatching.TabIndex = 5;
            this.btnAttemptPatching.Text = "List Problems And Attempt Fixes";
            this.btnAttemptPatching.UseVisualStyleBackColor = true;
            this.btnAttemptPatching.Click += new System.EventHandler(this.btnAttemptPatching_Click);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(13, 97);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(93, 13);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Current Database:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hosting Assembly:";
            // 
            // lblPatchingAssembly
            // 
            this.lblPatchingAssembly.AutoSize = true;
            this.lblPatchingAssembly.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPatchingAssembly.Location = new System.Drawing.Point(112, 74);
            this.lblPatchingAssembly.Name = "lblPatchingAssembly";
            this.lblPatchingAssembly.Size = new System.Drawing.Size(98, 15);
            this.lblPatchingAssembly.TabIndex = 2;
            this.lblPatchingAssembly.Text = "Patching Assembly";
            // 
            // lblDatabaseVersion
            // 
            this.lblDatabaseVersion.AutoSize = true;
            this.lblDatabaseVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDatabaseVersion.Location = new System.Drawing.Point(111, 97);
            this.lblDatabaseVersion.Name = "lblDatabaseVersion";
            this.lblDatabaseVersion.Size = new System.Drawing.Size(93, 15);
            this.lblDatabaseVersion.TabIndex = 2;
            this.lblDatabaseVersion.Text = "Database Version";
            // 
            // PatchingUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1297, 622);
            this.Controls.Add(this.btnAttemptPatching);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblDatabaseVersion);
            this.Controls.Add(this.lblPatchingAssembly);
            this.Controls.Add(this.lblHostAssembly);
            this.Controls.Add(this.label1);
            this.Name = "PatchingUI";
            this.Text = "PatchingUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblHostAssembly;
        private System.Windows.Forms.Label label4;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnAttemptPatching;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPatchingAssembly;
        private System.Windows.Forms.Label lblDatabaseVersion;
    }
}