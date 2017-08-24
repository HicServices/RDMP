namespace CatalogueManager.DataQualityUIs
{
    partial class DQEExecutionControl
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
            this.executionProgressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gbLoadChecks = new System.Windows.Forms.GroupBox();
            this.btnRerunChecks = new System.Windows.Forms.Button();
            this.dqePreRunCheckerUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.dlgFilePicker = new System.Windows.Forms.OpenFileDialog();
            this.btnExecute = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.gbLoadChecks.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // executionProgressUI1
            // 
            this.executionProgressUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executionProgressUI1.Location = new System.Drawing.Point(0, 0);
            this.executionProgressUI1.Name = "executionProgressUI1";
            this.executionProgressUI1.Size = new System.Drawing.Size(670, 712);
            this.executionProgressUI1.TabIndex = 42;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel1.Controls.Add(this.gbLoadChecks);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.executionProgressUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1376, 716);
            this.splitContainer1.SplitterDistance = 698;
            this.splitContainer1.TabIndex = 47;
            // 
            // gbLoadChecks
            // 
            this.gbLoadChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLoadChecks.Controls.Add(this.btnRerunChecks);
            this.gbLoadChecks.Controls.Add(this.dqePreRunCheckerUI1);
            this.gbLoadChecks.Location = new System.Drawing.Point(9, 3);
            this.gbLoadChecks.Name = "gbLoadChecks";
            this.gbLoadChecks.Size = new System.Drawing.Size(682, 507);
            this.gbLoadChecks.TabIndex = 10;
            this.gbLoadChecks.TabStop = false;
            this.gbLoadChecks.Text = "Pre run checks";
            // 
            // btnRerunChecks
            // 
            this.btnRerunChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRerunChecks.Location = new System.Drawing.Point(583, 448);
            this.btnRerunChecks.Name = "btnRerunChecks";
            this.btnRerunChecks.Size = new System.Drawing.Size(93, 23);
            this.btnRerunChecks.TabIndex = 1;
            this.btnRerunChecks.Text = "Re-run Checks";
            this.btnRerunChecks.UseVisualStyleBackColor = true;
            this.btnRerunChecks.Click += new System.EventHandler(this.btnRerunChecks_Click);
            // 
            // dqePreRunCheckerUI1
            // 
            this.dqePreRunCheckerUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dqePreRunCheckerUI1.Location = new System.Drawing.Point(3, 16);
            this.dqePreRunCheckerUI1.Name = "dqePreRunCheckerUI1";
            this.dqePreRunCheckerUI1.Size = new System.Drawing.Size(676, 488);
            this.dqePreRunCheckerUI1.TabIndex = 0;
            // 
            // btnExecute
            // 
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(6, 19);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(137, 23);
            this.btnExecute.TabIndex = 49;
            this.btnExecute.Text = "Start Execution";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnExecute);
            this.groupBox3.Location = new System.Drawing.Point(10, 527);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(681, 67);
            this.groupBox3.TabIndex = 54;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Execute";
            // 
            // DQEExecutionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DQEExecutionControl";
            this.Size = new System.Drawing.Size(1376, 716);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.gbLoadChecks.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ReusableUIComponents.Progress.ProgressUI executionProgressUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox gbLoadChecks;
        private System.Windows.Forms.Button btnRerunChecks;
        private ReusableUIComponents.ChecksUI.ChecksUI dqePreRunCheckerUI1;
        private System.Windows.Forms.OpenFileDialog dlgFilePicker;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnExecute;

    }
}
