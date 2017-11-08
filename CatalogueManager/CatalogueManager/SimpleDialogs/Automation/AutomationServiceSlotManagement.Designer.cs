using BrightIdeasSoftware;

namespace CatalogueManager.SimpleDialogs.Automation
{
    partial class AutomationServiceSlotManagement
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
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.automationServiceSlots = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.automationServiceSlotUI1 = new CatalogueManager.SimpleDialogs.Automation.AutomationServiceSlotUI();
            this.btnSaveToRemote = new System.Windows.Forms.Button();
            this.ddCredentials = new System.Windows.Forms.ComboBox();
            this.grpRemotes = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblCredentials = new System.Windows.Forms.Label();
            this.lblEndpoint = new System.Windows.Forms.Label();
            this.barRemoteSave = new System.Windows.Forms.ProgressBar();
            this.lblRemoteResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.automationServiceSlots)).BeginInit();
            this.grpRemotes.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(12, 517);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(294, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateNew.Location = new System.Drawing.Point(12, 488);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(294, 23);
            this.btnCreateNew.TabIndex = 1;
            this.btnCreateNew.Text = "Create New";
            this.btnCreateNew.UseVisualStyleBackColor = true;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // automationServiceSlots
            // 
            this.automationServiceSlots.AllColumns.Add(this.olvColumn1);
            this.automationServiceSlots.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.automationServiceSlots.CellEditUseWholeCell = false;
            this.automationServiceSlots.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.automationServiceSlots.Cursor = System.Windows.Forms.Cursors.Default;
            this.automationServiceSlots.FullRowSelect = true;
            this.automationServiceSlots.HideSelection = false;
            this.automationServiceSlots.Location = new System.Drawing.Point(12, 12);
            this.automationServiceSlots.Name = "automationServiceSlots";
            this.automationServiceSlots.Size = new System.Drawing.Size(294, 470);
            this.automationServiceSlots.TabIndex = 0;
            this.automationServiceSlots.UseCompatibleStateImageBehavior = false;
            this.automationServiceSlots.View = System.Windows.Forms.View.Details;
            this.automationServiceSlots.SelectedIndexChanged += new System.EventHandler(this.automationServiceSlots_SelectedIndexChanged);
            this.automationServiceSlots.KeyUp += new System.Windows.Forms.KeyEventHandler(this.automationServiceSlots_KeyUp);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "Automation Service Slots (you probably only want 1)";
            // 
            // automationServiceSlotUI1
            // 
            this.automationServiceSlotUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.automationServiceSlotUI1.AutomationServiceSlot = null;
            this.automationServiceSlotUI1.Location = new System.Drawing.Point(312, 12);
            this.automationServiceSlotUI1.Name = "automationServiceSlotUI1";
            this.automationServiceSlotUI1.Size = new System.Drawing.Size(1027, 686);
            this.automationServiceSlotUI1.TabIndex = 2;
            // 
            // btnSaveToRemote
            // 
            this.btnSaveToRemote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveToRemote.Location = new System.Drawing.Point(9, 78);
            this.btnSaveToRemote.Name = "btnSaveToRemote";
            this.btnSaveToRemote.Size = new System.Drawing.Size(279, 23);
            this.btnSaveToRemote.TabIndex = 3;
            this.btnSaveToRemote.Text = "Save to Remotes";
            this.btnSaveToRemote.UseVisualStyleBackColor = true;
            this.btnSaveToRemote.Click += new System.EventHandler(this.btnSaveToRemote_Click);
            // 
            // ddCredentials
            // 
            this.ddCredentials.FormattingEnabled = true;
            this.ddCredentials.Location = new System.Drawing.Point(68, 51);
            this.ddCredentials.Name = "ddCredentials";
            this.ddCredentials.Size = new System.Drawing.Size(220, 21);
            this.ddCredentials.TabIndex = 4;
            // 
            // grpRemotes
            // 
            this.grpRemotes.Controls.Add(this.lblRemoteResult);
            this.grpRemotes.Controls.Add(this.barRemoteSave);
            this.grpRemotes.Controls.Add(this.textBox1);
            this.grpRemotes.Controls.Add(this.lblCredentials);
            this.grpRemotes.Controls.Add(this.lblEndpoint);
            this.grpRemotes.Controls.Add(this.btnSaveToRemote);
            this.grpRemotes.Controls.Add(this.ddCredentials);
            this.grpRemotes.Location = new System.Drawing.Point(12, 546);
            this.grpRemotes.Name = "grpRemotes";
            this.grpRemotes.Size = new System.Drawing.Size(294, 157);
            this.grpRemotes.TabIndex = 5;
            this.grpRemotes.TabStop = false;
            this.grpRemotes.Text = "Remotes";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(68, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(220, 20);
            this.textBox1.TabIndex = 6;
            // 
            // lblCredentials
            // 
            this.lblCredentials.AutoSize = true;
            this.lblCredentials.Location = new System.Drawing.Point(6, 54);
            this.lblCredentials.Name = "lblCredentials";
            this.lblCredentials.Size = new System.Drawing.Size(59, 13);
            this.lblCredentials.TabIndex = 5;
            this.lblCredentials.Text = "Credentials";
            // 
            // lblEndpoint
            // 
            this.lblEndpoint.AutoSize = true;
            this.lblEndpoint.Location = new System.Drawing.Point(6, 25);
            this.lblEndpoint.Name = "lblEndpoint";
            this.lblEndpoint.Size = new System.Drawing.Size(49, 13);
            this.lblEndpoint.TabIndex = 5;
            this.lblEndpoint.Text = "Endpoint";
            // 
            // barRemoteSave
            // 
            this.barRemoteSave.Location = new System.Drawing.Point(9, 107);
            this.barRemoteSave.MarqueeAnimationSpeed = 0;
            this.barRemoteSave.Name = "barRemoteSave";
            this.barRemoteSave.Size = new System.Drawing.Size(279, 23);
            this.barRemoteSave.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.barRemoteSave.TabIndex = 7;
            // 
            // lblRemoteResult
            // 
            this.lblRemoteResult.AutoSize = true;
            this.lblRemoteResult.Location = new System.Drawing.Point(6, 137);
            this.lblRemoteResult.Name = "lblRemoteResult";
            this.lblRemoteResult.Size = new System.Drawing.Size(35, 13);
            this.lblRemoteResult.TabIndex = 8;
            this.lblRemoteResult.Text = "label1";
            // 
            // AutomationServiceSlotManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1346, 705);
            this.Controls.Add(this.grpRemotes);
            this.Controls.Add(this.automationServiceSlotUI1);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCreateNew);
            this.Controls.Add(this.automationServiceSlots);
            this.Name = "AutomationServiceSlotManagement";
            this.Text = "AutomationServiceSlotManagement";
            ((System.ComponentModel.ISupportInitialize)(this.automationServiceSlots)).EndInit();
            this.grpRemotes.ResumeLayout(false);
            this.grpRemotes.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ObjectListView automationServiceSlots;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Button btnDelete;
        private OLVColumn olvColumn1;
        private AutomationServiceSlotUI automationServiceSlotUI1;
        private System.Windows.Forms.Button btnSaveToRemote;
        private System.Windows.Forms.ComboBox ddCredentials;
        private System.Windows.Forms.GroupBox grpRemotes;
        private System.Windows.Forms.Label lblEndpoint;
        private System.Windows.Forms.Label lblCredentials;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblRemoteResult;
        private System.Windows.Forms.ProgressBar barRemoteSave;
    }
}