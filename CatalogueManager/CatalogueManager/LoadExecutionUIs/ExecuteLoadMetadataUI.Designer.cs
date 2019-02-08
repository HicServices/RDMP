using CatalogueManager.SimpleControls;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Progress;

namespace CatalogueManager.LoadExecutionUIs
{
    partial class ExecuteLoadMetadataUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecuteLoadMetadataUI));
            this.ddLoadProgress = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.udDaysPerJob = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.gbLoadProgresses = new System.Windows.Forms.GroupBox();
            this.btnRefreshLoadProgresses = new System.Windows.Forms.Button();
            this.helpIconAbortShouldCancel = new ReusableUIComponents.HelpIcon();
            this.helpIconRunRepeatedly = new ReusableUIComponents.HelpIcon();
            this.cbRunIteratively = new System.Windows.Forms.CheckBox();
            this.cbAbortShouldActuallyCancelInstead = new System.Windows.Forms.CheckBox();
            this.flpControls = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            ((System.ComponentModel.ISupportInitialize)(this.udDaysPerJob)).BeginInit();
            this.gbLoadProgresses.SuspendLayout();
            this.flpControls.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddLoadProgress
            // 
            this.ddLoadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddLoadProgress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLoadProgress.FormattingEnabled = true;
            this.ddLoadProgress.Location = new System.Drawing.Point(88, 14);
            this.ddLoadProgress.Name = "ddLoadProgress";
            this.ddLoadProgress.Size = new System.Drawing.Size(327, 21);
            this.ddLoadProgress.TabIndex = 48;
            this.ddLoadProgress.SelectedIndexChanged += new System.EventHandler(this.ddLoadProgress_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 50;
            this.label4.Text = "Load Progress";
            // 
            // udDaysPerJob
            // 
            this.udDaysPerJob.Location = new System.Drawing.Point(88, 38);
            this.udDaysPerJob.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udDaysPerJob.Name = "udDaysPerJob";
            this.udDaysPerJob.Size = new System.Drawing.Size(54, 20);
            this.udDaysPerJob.TabIndex = 48;
            this.udDaysPerJob.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 46;
            this.label3.Text = "Days per job";
            // 
            // gbLoadProgresses
            // 
            this.gbLoadProgresses.Controls.Add(this.btnRefreshLoadProgresses);
            this.gbLoadProgresses.Controls.Add(this.helpIconAbortShouldCancel);
            this.gbLoadProgresses.Controls.Add(this.helpIconRunRepeatedly);
            this.gbLoadProgresses.Controls.Add(this.cbRunIteratively);
            this.gbLoadProgresses.Controls.Add(this.cbAbortShouldActuallyCancelInstead);
            this.gbLoadProgresses.Controls.Add(this.ddLoadProgress);
            this.gbLoadProgresses.Controls.Add(this.udDaysPerJob);
            this.gbLoadProgresses.Controls.Add(this.label3);
            this.gbLoadProgresses.Controls.Add(this.label4);
            this.gbLoadProgresses.Location = new System.Drawing.Point(3, 3);
            this.gbLoadProgresses.Name = "gbLoadProgresses";
            this.gbLoadProgresses.Size = new System.Drawing.Size(457, 62);
            this.gbLoadProgresses.TabIndex = 57;
            this.gbLoadProgresses.TabStop = false;
            this.gbLoadProgresses.Text = "Progressable To Execute";
            // 
            // btnRefreshLoadProgresses
            // 
            this.btnRefreshLoadProgresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshLoadProgresses.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshLoadProgresses.Image")));
            this.btnRefreshLoadProgresses.Location = new System.Drawing.Point(418, 13);
            this.btnRefreshLoadProgresses.Name = "btnRefreshLoadProgresses";
            this.btnRefreshLoadProgresses.Size = new System.Drawing.Size(30, 23);
            this.btnRefreshLoadProgresses.TabIndex = 59;
            this.btnRefreshLoadProgresses.UseVisualStyleBackColor = true;
            this.btnRefreshLoadProgresses.Click += new System.EventHandler(this.btnRefreshLoadProgresses_Click);
            // 
            // helpIconAbortShouldCancel
            // 
            this.helpIconAbortShouldCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconAbortShouldCancel.BackgroundImage")));
            this.helpIconAbortShouldCancel.Location = new System.Drawing.Point(422, 36);
            this.helpIconAbortShouldCancel.Name = "helpIconAbortShouldCancel";
            this.helpIconAbortShouldCancel.Size = new System.Drawing.Size(19, 19);
            this.helpIconAbortShouldCancel.TabIndex = 52;
            // 
            // helpIconRunRepeatedly
            // 
            this.helpIconRunRepeatedly.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIconRunRepeatedly.BackgroundImage")));
            this.helpIconRunRepeatedly.Location = new System.Drawing.Point(246, 37);
            this.helpIconRunRepeatedly.Name = "helpIconRunRepeatedly";
            this.helpIconRunRepeatedly.Size = new System.Drawing.Size(19, 19);
            this.helpIconRunRepeatedly.TabIndex = 52;
            // 
            // cbRunIteratively
            // 
            this.cbRunIteratively.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRunIteratively.AutoSize = true;
            this.cbRunIteratively.Location = new System.Drawing.Point(148, 39);
            this.cbRunIteratively.Name = "cbRunIteratively";
            this.cbRunIteratively.Size = new System.Drawing.Size(103, 17);
            this.cbRunIteratively.TabIndex = 51;
            this.cbRunIteratively.Text = "Run Repeatedly";
            this.cbRunIteratively.UseVisualStyleBackColor = true;
            this.cbRunIteratively.CheckedChanged += new System.EventHandler(this.cbRunIteratively_CheckedChanged);
            // 
            // cbAbortShouldActuallyCancelInstead
            // 
            this.cbAbortShouldActuallyCancelInstead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAbortShouldActuallyCancelInstead.AutoSize = true;
            this.cbAbortShouldActuallyCancelInstead.Enabled = false;
            this.cbAbortShouldActuallyCancelInstead.Location = new System.Drawing.Point(271, 39);
            this.cbAbortShouldActuallyCancelInstead.Name = "cbAbortShouldActuallyCancelInstead";
            this.cbAbortShouldActuallyCancelInstead.Size = new System.Drawing.Size(152, 17);
            this.cbAbortShouldActuallyCancelInstead.TabIndex = 11;
            this.cbAbortShouldActuallyCancelInstead.Text = "Clicking Abort Should Wait";
            this.cbAbortShouldActuallyCancelInstead.UseVisualStyleBackColor = true;
            // 
            // flpControls
            // 
            this.flpControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpControls.Controls.Add(this.gbLoadProgresses);
            this.flpControls.Location = new System.Drawing.Point(198, 3);
            this.flpControls.Name = "flpControls";
            this.flpControls.Size = new System.Drawing.Size(721, 70);
            this.flpControls.TabIndex = 59;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.flpControls);
            this.panel2.Controls.Add(this.checkAndExecuteUI1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(922, 733);
            this.panel2.TabIndex = 60;
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(922, 733);
            this.checkAndExecuteUI1.TabIndex = 58;
            // 
            // ExecuteLoadMetadataUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Name = "ExecuteLoadMetadataUI";
            this.Size = new System.Drawing.Size(922, 733);
            ((System.ComponentModel.ISupportInitialize)(this.udDaysPerJob)).EndInit();
            this.gbLoadProgresses.ResumeLayout(false);
            this.gbLoadProgresses.PerformLayout();
            this.flpControls.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox ddLoadProgress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown udDaysPerJob;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbLoadProgresses;
        private ReusableUIComponents.HelpIcon helpIconRunRepeatedly;
        private System.Windows.Forms.CheckBox cbRunIteratively;
        private System.Windows.Forms.CheckBox cbAbortShouldActuallyCancelInstead;
        private ReusableUIComponents.HelpIcon helpIconAbortShouldCancel;
        private System.Windows.Forms.Button btnRefreshLoadProgresses;
        private CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.FlowLayoutPanel flpControls;
        private System.Windows.Forms.Panel panel2;
    }
}

