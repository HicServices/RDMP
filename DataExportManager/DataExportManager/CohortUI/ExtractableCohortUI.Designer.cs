using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace DataExportManager.CohortUI
{
    partial class ExtractableCohortUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractableCohortUI));
            this.tbID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbOverrideReleaseIdentifierSQL = new System.Windows.Forms.TextBox();
            this.pDescription = new System.Windows.Forms.Panel();
            
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.tlvCohortUsage = new BrightIdeasSoftware.TreeListView();
            this.olvUsedIn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tlvPreviousVersions = new BrightIdeasSoftware.TreeListView();
            this.olvOtherVersions = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.tbVersion = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbProjectNumber = new System.Windows.Forms.TextBox();
            this.btnShowProject = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbOriginId = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortUsage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tlvPreviousVersions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(161, 19);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(134, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "ID:";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(100, 123);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(55, 13);
            this.lblDescription.TabIndex = 13;
            this.lblDescription.Text = "Audit Log:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "OverrideReleaseIdentifierSQL:";
            // 
            // tbOverrideReleaseIdentifierSQL
            // 
            this.tbOverrideReleaseIdentifierSQL.Location = new System.Drawing.Point(161, 45);
            this.tbOverrideReleaseIdentifierSQL.Name = "tbOverrideReleaseIdentifierSQL";
            this.tbOverrideReleaseIdentifierSQL.Size = new System.Drawing.Size(468, 20);
            this.tbOverrideReleaseIdentifierSQL.TabIndex = 12;
            this.tbOverrideReleaseIdentifierSQL.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // pDescription
            // 
            this.pDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pDescription.Location = new System.Drawing.Point(161, 123);
            this.pDescription.Name = "pDescription";
            this.pDescription.Size = new System.Drawing.Size(885, 161);
            this.pDescription.TabIndex = 15;
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(635, 45);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 17;
            // 
            // tlvCohortUsage
            // 
            this.tlvCohortUsage.AllColumns.Add(this.olvUsedIn);
            this.tlvCohortUsage.CellEditUseWholeCell = false;
            this.tlvCohortUsage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvUsedIn});
            this.tlvCohortUsage.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCohortUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvCohortUsage.Location = new System.Drawing.Point(0, 0);
            this.tlvCohortUsage.Name = "tlvCohortUsage";
            this.tlvCohortUsage.ShowGroups = false;
            this.tlvCohortUsage.Size = new System.Drawing.Size(542, 276);
            this.tlvCohortUsage.TabIndex = 18;
            this.tlvCohortUsage.UseCompatibleStateImageBehavior = false;
            this.tlvCohortUsage.View = System.Windows.Forms.View.Details;
            this.tlvCohortUsage.VirtualMode = true;
            // 
            // olvUsedIn
            // 
            this.olvUsedIn.AspectName = "ToString";
            this.olvUsedIn.FillsFreeSpace = true;
            this.olvUsedIn.Text = "Used In";
            // 
            // tlvPreviousVersions
            // 
            this.tlvPreviousVersions.AllColumns.Add(this.olvOtherVersions);
            this.tlvPreviousVersions.AllColumns.Add(this.olvVersion);
            this.tlvPreviousVersions.CellEditUseWholeCell = false;
            this.tlvPreviousVersions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvOtherVersions,
            this.olvVersion});
            this.tlvPreviousVersions.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvPreviousVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvPreviousVersions.Location = new System.Drawing.Point(0, 0);
            this.tlvPreviousVersions.Name = "tlvPreviousVersions";
            this.tlvPreviousVersions.ShowGroups = false;
            this.tlvPreviousVersions.Size = new System.Drawing.Size(497, 276);
            this.tlvPreviousVersions.TabIndex = 19;
            this.tlvPreviousVersions.UseCompatibleStateImageBehavior = false;
            this.tlvPreviousVersions.View = System.Windows.Forms.View.Details;
            this.tlvPreviousVersions.VirtualMode = true;
            // 
            // olvOtherVersions
            // 
            this.olvOtherVersions.AspectName = "ToString";
            this.olvOtherVersions.FillsFreeSpace = true;
            this.olvOtherVersions.Text = "Other Versions";
            // 
            // olvVersion
            // 
            this.olvVersion.AspectName = "ExternalVersion";
            this.olvVersion.Text = "Version";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 290);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tlvCohortUsage);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tlvPreviousVersions);
            this.splitContainer1.Size = new System.Drawing.Size(1043, 276);
            this.splitContainer1.SplitterDistance = 542;
            this.splitContainer1.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(110, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Version:";
            // 
            // tbVersion
            // 
            this.tbVersion.Location = new System.Drawing.Point(161, 71);
            this.tbVersion.Name = "tbVersion";
            this.tbVersion.ReadOnly = true;
            this.tbVersion.Size = new System.Drawing.Size(159, 20);
            this.tbVersion.TabIndex = 12;
            this.tbVersion.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Project Number:";
            // 
            // tbProjectNumber
            // 
            this.tbProjectNumber.Location = new System.Drawing.Point(161, 97);
            this.tbProjectNumber.Name = "tbProjectNumber";
            this.tbProjectNumber.ReadOnly = true;
            this.tbProjectNumber.Size = new System.Drawing.Size(159, 20);
            this.tbProjectNumber.TabIndex = 12;
            this.tbProjectNumber.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // btnShowProject
            // 
            this.btnShowProject.Location = new System.Drawing.Point(326, 95);
            this.btnShowProject.Name = "btnShowProject";
            this.btnShowProject.Size = new System.Drawing.Size(45, 23);
            this.btnShowProject.TabIndex = 21;
            this.btnShowProject.Text = "Show";
            this.btnShowProject.UseVisualStyleBackColor = true;
            this.btnShowProject.Click += new System.EventHandler(this.btnShowProject_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(268, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Origin ID:";
            // 
            // tbOriginId
            // 
            this.tbOriginId.Location = new System.Drawing.Point(325, 19);
            this.tbOriginId.Name = "tbOriginId";
            this.tbOriginId.ReadOnly = true;
            this.tbOriginId.Size = new System.Drawing.Size(100, 20);
            this.tbOriginId.TabIndex = 12;
            // 
            // ExtractableCohortUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnShowProject);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.pDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.tbProjectNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbOverrideReleaseIdentifierSQL);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbOriginId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label5);
            this.Name = "ExtractableCohortUI";
            this.Size = new System.Drawing.Size(1046, 843);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortUsage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tlvPreviousVersions)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbOverrideReleaseIdentifierSQL;
        private System.Windows.Forms.Panel pDescription;
        private CatalogueManager.SimpleControls.ObjectSaverButton objectSaverButton1;
        private ReusableUIComponents.HelpIcon helpIcon1;
        private TreeListView tlvCohortUsage;
        private OLVColumn olvUsedIn;
        private TreeListView tlvPreviousVersions;
        private OLVColumn olvOtherVersions;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private OLVColumn olvVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbProjectNumber;
        private System.Windows.Forms.Button btnShowProject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbOriginId;
    }
}
