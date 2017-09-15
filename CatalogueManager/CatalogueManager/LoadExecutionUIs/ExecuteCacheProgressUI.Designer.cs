namespace CatalogueManager.LoadExecutionUIs
{
    partial class ExecuteCacheProgressUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecuteCacheProgressUI));
            this.label2 = new System.Windows.Forms.Label();
            this.dtpStartDateToRetrieve = new System.Windows.Forms.DateTimePicker();
            this.cbIgnorePermissionWindow = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dtpEndDateToRetrieve = new System.Windows.Forms.DateTimePicker();
            this.btnViewFailures = new System.Windows.Forms.Button();
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.btnAbortLoad = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.ragChecks = new ReusableUIComponents.RAGSmiley();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.rbNextDates = new System.Windows.Forms.RadioButton();
            this.lblNextDateToLoad = new System.Windows.Forms.Label();
            this.rbRetryFailures = new System.Windows.Forms.RadioButton();
            this.rbSpecificDates = new System.Windows.Forms.RadioButton();
            this.checksUI = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.progressUI = new ReusableUIComponents.Progress.ProgressUI();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.gbLoad = new System.Windows.Forms.GroupBox();
            this.gbControls.SuspendLayout();
            this.gbLoad.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(500, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Start date:";
            // 
            // dtpStartDateToRetrieve
            // 
            this.dtpStartDateToRetrieve.Location = new System.Drawing.Point(562, 47);
            this.dtpStartDateToRetrieve.Name = "dtpStartDateToRetrieve";
            this.dtpStartDateToRetrieve.Size = new System.Drawing.Size(152, 20);
            this.dtpStartDateToRetrieve.TabIndex = 8;
            this.dtpStartDateToRetrieve.ValueChanged += new System.EventHandler(this.dtpStartDateToRetrieve_ValueChanged);
            // 
            // cbIgnorePermissionWindow
            // 
            this.cbIgnorePermissionWindow.AutoSize = true;
            this.cbIgnorePermissionWindow.Location = new System.Drawing.Point(562, 99);
            this.cbIgnorePermissionWindow.Name = "cbIgnorePermissionWindow";
            this.cbIgnorePermissionWindow.Size = new System.Drawing.Size(151, 17);
            this.cbIgnorePermissionWindow.TabIndex = 13;
            this.cbIgnorePermissionWindow.Text = "Ignore Permission Window";
            this.cbIgnorePermissionWindow.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(503, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "End date:";
            // 
            // dtpEndDateToRetrieve
            // 
            this.dtpEndDateToRetrieve.Location = new System.Drawing.Point(562, 73);
            this.dtpEndDateToRetrieve.Name = "dtpEndDateToRetrieve";
            this.dtpEndDateToRetrieve.Size = new System.Drawing.Size(152, 20);
            this.dtpEndDateToRetrieve.TabIndex = 12;
            this.dtpEndDateToRetrieve.ValueChanged += new System.EventHandler(this.dtpEndDateToRetrieve_ValueChanged);
            // 
            // btnViewFailures
            // 
            this.btnViewFailures.Location = new System.Drawing.Point(367, 48);
            this.btnViewFailures.Name = "btnViewFailures";
            this.btnViewFailures.Size = new System.Drawing.Size(81, 23);
            this.btnViewFailures.TabIndex = 0;
            this.btnViewFailures.Text = "View Failures";
            this.btnViewFailures.UseVisualStyleBackColor = true;
            this.btnViewFailures.Click += new System.EventHandler(this.btnViewFailures_Click);
            // 
            // gbControls
            // 
            this.gbControls.Controls.Add(this.btnAbortLoad);
            this.gbControls.Controls.Add(this.btnExecute);
            this.gbControls.Controls.Add(this.ragChecks);
            this.gbControls.Controls.Add(this.label1);
            this.gbControls.Controls.Add(this.label3);
            this.gbControls.Controls.Add(this.label4);
            this.gbControls.Controls.Add(this.btnRunChecks);
            this.gbControls.Location = new System.Drawing.Point(3, 25);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new System.Drawing.Size(187, 90);
            this.gbControls.TabIndex = 57;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "Controls";
            // 
            // btnAbortLoad
            // 
            this.btnAbortLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnAbortLoad.Image")));
            this.btnAbortLoad.Location = new System.Drawing.Point(148, 42);
            this.btnAbortLoad.Name = "btnAbortLoad";
            this.btnAbortLoad.Size = new System.Drawing.Size(29, 23);
            this.btnAbortLoad.TabIndex = 58;
            this.btnAbortLoad.UseVisualStyleBackColor = true;
            this.btnAbortLoad.Click += new System.EventHandler(this.btnAbortLoad_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(56, 42);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(86, 23);
            this.btnExecute.TabIndex = 58;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // ragChecks
            // 
            this.ragChecks.AlwaysShowHandCursor = false;
            this.ragChecks.BackColor = System.Drawing.Color.Transparent;
            this.ragChecks.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragChecks.Location = new System.Drawing.Point(22, 16);
            this.ragChecks.Name = "ragChecks";
            this.ragChecks.Size = new System.Drawing.Size(25, 25);
            this.ragChecks.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Abort";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(75, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Execute";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Checks";
            // 
            // btnRunChecks
            // 
            this.btnRunChecks.Image = ((System.Drawing.Image)(resources.GetObject("btnRunChecks.Image")));
            this.btnRunChecks.Location = new System.Drawing.Point(20, 42);
            this.btnRunChecks.Name = "btnRunChecks";
            this.btnRunChecks.Size = new System.Drawing.Size(30, 23);
            this.btnRunChecks.TabIndex = 1;
            this.btnRunChecks.UseVisualStyleBackColor = true;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);
            // 
            // rbNextDates
            // 
            this.rbNextDates.AutoSize = true;
            this.rbNextDates.Checked = true;
            this.rbNextDates.Location = new System.Drawing.Point(217, 28);
            this.rbNextDates.Name = "rbNextDates";
            this.rbNextDates.Size = new System.Drawing.Size(78, 17);
            this.rbNextDates.TabIndex = 58;
            this.rbNextDates.TabStop = true;
            this.rbNextDates.Text = "Next Dates";
            this.rbNextDates.UseVisualStyleBackColor = true;
            // 
            // lblNextDateToLoad
            // 
            this.lblNextDateToLoad.AutoSize = true;
            this.lblNextDateToLoad.Location = new System.Drawing.Point(238, 48);
            this.lblNextDateToLoad.Name = "lblNextDateToLoad";
            this.lblNextDateToLoad.Size = new System.Drawing.Size(75, 13);
            this.lblNextDateToLoad.TabIndex = 59;
            this.lblNextDateToLoad.Text = "Next Date is X";
            // 
            // rbRetryFailures
            // 
            this.rbRetryFailures.AutoSize = true;
            this.rbRetryFailures.Location = new System.Drawing.Point(336, 28);
            this.rbRetryFailures.Name = "rbRetryFailures";
            this.rbRetryFailures.Size = new System.Drawing.Size(112, 17);
            this.rbRetryFailures.TabIndex = 58;
            this.rbRetryFailures.Text = "Retry Failed Dates";
            this.rbRetryFailures.UseVisualStyleBackColor = true;
            // 
            // rbSpecificDates
            // 
            this.rbSpecificDates.AutoSize = true;
            this.rbSpecificDates.Location = new System.Drawing.Point(479, 28);
            this.rbSpecificDates.Name = "rbSpecificDates";
            this.rbSpecificDates.Size = new System.Drawing.Size(94, 17);
            this.rbSpecificDates.TabIndex = 58;
            this.rbSpecificDates.Text = "Specific Dates";
            this.rbSpecificDates.UseVisualStyleBackColor = true;
            // 
            // checksUI
            // 
            this.checksUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI.Location = new System.Drawing.Point(3, 16);
            this.checksUI.Name = "checksUI";
            this.checksUI.Size = new System.Drawing.Size(888, 594);
            this.checksUI.TabIndex = 3;
            // 
            // progressUI
            // 
            this.progressUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressUI.Location = new System.Drawing.Point(3, 16);
            this.progressUI.Name = "progressUI";
            this.progressUI.Size = new System.Drawing.Size(888, 594);
            this.progressUI.TabIndex = 4;
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(0, 0);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(900, 22);
            this.rdmpObjectsRibbonUI1.TabIndex = 58;
            // 
            // gbLoad
            // 
            this.gbLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLoad.Controls.Add(this.progressUI);
            this.gbLoad.Controls.Add(this.checksUI);
            this.gbLoad.Location = new System.Drawing.Point(3, 131);
            this.gbLoad.Name = "gbLoad";
            this.gbLoad.Size = new System.Drawing.Size(894, 613);
            this.gbLoad.TabIndex = 59;
            this.gbLoad.TabStop = false;
            this.gbLoad.Text = "Load Execution";
            // 
            // ExecuteCacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rbNextDates);
            this.Controls.Add(this.lblNextDateToLoad);
            this.Controls.Add(this.rbRetryFailures);
            this.Controls.Add(this.btnViewFailures);
            this.Controls.Add(this.rbSpecificDates);
            this.Controls.Add(this.dtpStartDateToRetrieve);
            this.Controls.Add(this.gbControls);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpEndDateToRetrieve);
            this.Controls.Add(this.gbLoad);
            this.Controls.Add(this.cbIgnorePermissionWindow);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Name = "ExecuteCacheProgressUI";
            this.Size = new System.Drawing.Size(900, 747);
            this.gbControls.ResumeLayout(false);
            this.gbControls.PerformLayout();
            this.gbLoad.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ReusableUIComponents.ChecksUI.ChecksUI checksUI;
        private ReusableUIComponents.Progress.ProgressUI progressUI;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpStartDateToRetrieve;
        private System.Windows.Forms.Button btnViewFailures;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dtpEndDateToRetrieve;
        private System.Windows.Forms.CheckBox cbIgnorePermissionWindow;
        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.Button btnAbortLoad;
        private System.Windows.Forms.Button btnExecute;
        private ReusableUIComponents.RAGSmiley ragChecks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnRunChecks;
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private System.Windows.Forms.RadioButton rbNextDates;
        private System.Windows.Forms.RadioButton rbSpecificDates;
        private System.Windows.Forms.RadioButton rbRetryFailures;
        private System.Windows.Forms.GroupBox gbLoad;
        private System.Windows.Forms.Label lblNextDateToLoad;
    }
}

