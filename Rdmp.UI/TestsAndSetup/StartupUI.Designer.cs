using Rdmp.UI.ChecksUI;
using ReusableLibraryCode.Checks;

namespace Rdmp.UI.TestsAndSetup
{
    partial class StartupUI : ICheckNotifier
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
            this.lblProgress = new System.Windows.Forms.Label();
            this.pbLoadProgress = new System.Windows.Forms.ProgressBar();
            this.btnChoosePlatformDatabases = new System.Windows.Forms.Button();
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.pbDisconnected = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDisconnected)).BeginInit();
            this.SuspendLayout();
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(46, 40);
            this.lblProgress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(65, 15);
            this.lblProgress.TabIndex = 2;
            this.lblProgress.Text = "lblProgress";
            // 
            // pbLoadProgress
            // 
            this.pbLoadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLoadProgress.Location = new System.Drawing.Point(49, 18);
            this.pbLoadProgress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbLoadProgress.Name = "pbLoadProgress";
            this.pbLoadProgress.Size = new System.Drawing.Size(1056, 12);
            this.pbLoadProgress.TabIndex = 1;
            // 
            // btnChoosePlatformDatabases
            // 
            this.btnChoosePlatformDatabases.Location = new System.Drawing.Point(485, 62);
            this.btnChoosePlatformDatabases.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnChoosePlatformDatabases.Name = "btnChoosePlatformDatabases";
            this.btnChoosePlatformDatabases.Size = new System.Drawing.Size(170, 27);
            this.btnChoosePlatformDatabases.TabIndex = 4;
            this.btnChoosePlatformDatabases.Text = "Set Platform Databases...";
            this.btnChoosePlatformDatabases.UseVisualStyleBackColor = true;
            this.btnChoosePlatformDatabases.Click += new System.EventHandler(this.BtnChoosePlatformDatabases_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(9, 10);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(29, 29);
            this.ragSmiley1.TabIndex = 3;
            // 
            // pbDisconnected
            // 
            this.pbDisconnected.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbDisconnected.Location = new System.Drawing.Point(8, 6);
            this.pbDisconnected.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbDisconnected.Name = "pbDisconnected";
            this.pbDisconnected.Size = new System.Drawing.Size(34, 33);
            this.pbDisconnected.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbDisconnected.TabIndex = 5;
            this.pbDisconnected.TabStop = false;
            this.pbDisconnected.Visible = false;
            this.pbDisconnected.Click += new System.EventHandler(this.pbDisconnected_Click);
            // 
            // StartupUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 95);
            this.Controls.Add(this.pbDisconnected);
            this.Controls.Add(this.btnChoosePlatformDatabases);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.pbLoadProgress);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "StartupUI";
            this.Text = "Startup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartupUIMainForm_FormClosing);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.StartupUIMainForm_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbDisconnected)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar pbLoadProgress;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnChoosePlatformDatabases;
        private System.Windows.Forms.PictureBox pbDisconnected;
    }
}