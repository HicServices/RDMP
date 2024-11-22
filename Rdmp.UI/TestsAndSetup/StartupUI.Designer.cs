using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.TestsAndSetup
{
    sealed partial class StartupUI : ICheckNotifier
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
            lblProgress = new System.Windows.Forms.Label();
            pbLoadProgress = new System.Windows.Forms.ProgressBar();
            btnChoosePlatformDatabases = new System.Windows.Forms.Button();
            ragSmiley1 = new RAGSmiley();
            pbDisconnected = new System.Windows.Forms.PictureBox();
            btnQuickStart = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pbDisconnected).BeginInit();
            SuspendLayout();
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new System.Drawing.Point(46, 40);
            lblProgress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new System.Drawing.Size(65, 15);
            lblProgress.TabIndex = 2;
            lblProgress.Text = "lblProgress";
            // 
            // pbLoadProgress
            // 
            pbLoadProgress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pbLoadProgress.Location = new System.Drawing.Point(49, 18);
            pbLoadProgress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbLoadProgress.Name = "pbLoadProgress";
            pbLoadProgress.Size = new System.Drawing.Size(1056, 12);
            pbLoadProgress.TabIndex = 1;
            // 
            // btnChoosePlatformDatabases
            // 
            btnChoosePlatformDatabases.Location = new System.Drawing.Point(422, 62);
            btnChoosePlatformDatabases.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnChoosePlatformDatabases.Name = "btnChoosePlatformDatabases";
            btnChoosePlatformDatabases.Size = new System.Drawing.Size(170, 27);
            btnChoosePlatformDatabases.TabIndex = 4;
            btnChoosePlatformDatabases.Text = "Set Platform Databases...";
            btnChoosePlatformDatabases.UseVisualStyleBackColor = true;
            btnChoosePlatformDatabases.Click += BtnChoosePlatformDatabases_Click;
            // 
            // ragSmiley1
            // 
            ragSmiley1.AlwaysShowHandCursor = false;
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(9, 10);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(29, 29);
            ragSmiley1.TabIndex = 3;
            // 
            // pbDisconnected
            // 
            pbDisconnected.Cursor = System.Windows.Forms.Cursors.Hand;
            pbDisconnected.Location = new System.Drawing.Point(8, 6);
            pbDisconnected.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbDisconnected.Name = "pbDisconnected";
            pbDisconnected.Size = new System.Drawing.Size(34, 33);
            pbDisconnected.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pbDisconnected.TabIndex = 5;
            pbDisconnected.TabStop = false;
            pbDisconnected.Visible = false;
            pbDisconnected.Click += pbDisconnected_Click;
            // 
            // btnQuickStart
            // 
            btnQuickStart.Location = new System.Drawing.Point(599, 62);
            btnQuickStart.Name = "btnQuickStart";
            btnQuickStart.Size = new System.Drawing.Size(170, 27);
            btnQuickStart.TabIndex = 6;
            btnQuickStart.Text = "Use Local File System";
            btnQuickStart.UseVisualStyleBackColor = true;
            btnQuickStart.Click += btnLocalFileSystem_Click;
            // 
            // StartupUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1140, 95);
            Controls.Add(btnQuickStart);
            Controls.Add(pbDisconnected);
            Controls.Add(btnChoosePlatformDatabases);
            Controls.Add(ragSmiley1);
            Controls.Add(lblProgress);
            Controls.Add(pbLoadProgress);
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "StartupUI";
            Text = "Startup";
            FormClosing += StartupUIMainForm_FormClosing;
            KeyUp += StartupUIMainForm_KeyUp;
            ((System.ComponentModel.ISupportInitialize)pbDisconnected).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar pbLoadProgress;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnChoosePlatformDatabases;
        private System.Windows.Forms.PictureBox pbDisconnected;
        private System.Windows.Forms.Button btnQuickStart;
    }
}