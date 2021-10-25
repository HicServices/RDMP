using BrightIdeasSoftware;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.CohortUI
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
            tbID = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            lblDescription = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            tbOverrideReleaseIdentifierSQL = new System.Windows.Forms.TextBox();
            pDescription = new System.Windows.Forms.Panel();
            helpIcon1 = new HelpIcon();
            tlvCohortUsage = new BrightIdeasSoftware.TreeListView();
            olvUsedIn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            tlvPreviousVersions = new BrightIdeasSoftware.TreeListView();
            olvOtherVersions = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            olvVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            label2 = new System.Windows.Forms.Label();
            tbVersion = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbProjectNumber = new System.Windows.Forms.TextBox();
            btnShowProject = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            tbOriginId = new System.Windows.Forms.TextBox();
            panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(tlvCohortUsage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(tlvPreviousVersions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbID
            // 
            tbID.Location = new System.Drawing.Point(157, 4);
            tbID.Name = "tbID";
            tbID.ReadOnly = true;
            tbID.Size = new System.Drawing.Size(100, 20);
            tbID.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(130, 7);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(21, 13);
            label5.TabIndex = 11;
            label5.Text = "ID:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(96, 108);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(55, 13);
            lblDescription.TabIndex = 13;
            lblDescription.Text = "Audit Log:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(1, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(150, 13);
            label1.TabIndex = 11;
            label1.Text = "OverrideReleaseIdentifierSQL:";
            // 
            // tbOverrideReleaseIdentifierSQL
            // 
            tbOverrideReleaseIdentifierSQL.Location = new System.Drawing.Point(157, 30);
            tbOverrideReleaseIdentifierSQL.Name = "tbOverrideReleaseIdentifierSQL";
            tbOverrideReleaseIdentifierSQL.Size = new System.Drawing.Size(468, 20);
            tbOverrideReleaseIdentifierSQL.TabIndex = 12;
            tbOverrideReleaseIdentifierSQL.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // pDescription
            // 
            pDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            pDescription.Location = new System.Drawing.Point(157, 108);
            pDescription.Name = "pDescription";
            pDescription.Size = new System.Drawing.Size(888, 161);
            pDescription.TabIndex = 15;
            // 
            // helpIcon1
            // 
            helpIcon1.Location = new System.Drawing.Point(631, 30);
            helpIcon1.Name = "helpIcon1";
            helpIcon1.Size = new System.Drawing.Size(19, 19);
            helpIcon1.TabIndex = 17;
            // 
            // tlvCohortUsage
            // 
            tlvCohortUsage.AllColumns.Add(olvUsedIn);
            tlvCohortUsage.CellEditUseWholeCell = false;
            tlvCohortUsage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            olvUsedIn});
            tlvCohortUsage.Cursor = System.Windows.Forms.Cursors.Default;
            tlvCohortUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvCohortUsage.Location = new System.Drawing.Point(0, 0);
            tlvCohortUsage.Name = "tlvCohortUsage";
            tlvCohortUsage.ShowGroups = false;
            tlvCohortUsage.Size = new System.Drawing.Size(539, 276);
            tlvCohortUsage.TabIndex = 18;
            tlvCohortUsage.UseCompatibleStateImageBehavior = false;
            tlvCohortUsage.View = System.Windows.Forms.View.Details;
            tlvCohortUsage.VirtualMode = true;
            // 
            // olvUsedIn
            // 
            olvUsedIn.AspectName = "ToString";
            olvUsedIn.Text = "Used In";
            olvUsedIn.MinimumWidth = 100;
            // 
            // tlvPreviousVersions
            // 
            tlvPreviousVersions.AllColumns.Add(olvOtherVersions);
            tlvPreviousVersions.AllColumns.Add(olvVersion);
            tlvPreviousVersions.CellEditUseWholeCell = false;
            tlvPreviousVersions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            olvOtherVersions,
            olvVersion});
            tlvPreviousVersions.Cursor = System.Windows.Forms.Cursors.Default;
            tlvPreviousVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvPreviousVersions.Location = new System.Drawing.Point(0, 0);
            tlvPreviousVersions.Name = "tlvPreviousVersions";
            tlvPreviousVersions.ShowGroups = false;
            tlvPreviousVersions.Size = new System.Drawing.Size(496, 276);
            tlvPreviousVersions.TabIndex = 19;
            tlvPreviousVersions.UseCompatibleStateImageBehavior = false;
            tlvPreviousVersions.View = System.Windows.Forms.View.Details;
            tlvPreviousVersions.VirtualMode = true;
            // 
            // olvOtherVersions
            // 
            olvOtherVersions.AspectName = "ToString";
            olvOtherVersions.Text = "Other Versions";
            olvOtherVersions.MinimumWidth = 100;
            // 
            // olvVersion
            // 
            olvVersion.AspectName = "ExternalVersion";
            olvVersion.Text = "Version";
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            splitContainer1.Location = new System.Drawing.Point(4, 275);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tlvCohortUsage);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tlvPreviousVersions);
            splitContainer1.Size = new System.Drawing.Size(1039, 276);
            splitContainer1.SplitterDistance = 539;
            splitContainer1.TabIndex = 20;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(106, 56);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(45, 13);
            label2.TabIndex = 11;
            label2.Text = "Version:";
            // 
            // tbVersion
            // 
            tbVersion.Location = new System.Drawing.Point(157, 56);
            tbVersion.Name = "tbVersion";
            tbVersion.ReadOnly = true;
            tbVersion.Size = new System.Drawing.Size(159, 20);
            tbVersion.TabIndex = 12;
            tbVersion.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(68, 85);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(83, 13);
            label3.TabIndex = 11;
            label3.Text = "Project Number:";
            // 
            // tbProjectNumber
            // 
            tbProjectNumber.Location = new System.Drawing.Point(157, 82);
            tbProjectNumber.Name = "tbProjectNumber";
            tbProjectNumber.ReadOnly = true;
            tbProjectNumber.Size = new System.Drawing.Size(159, 20);
            tbProjectNumber.TabIndex = 12;
            tbProjectNumber.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // btnShowProject
            // 
            btnShowProject.Location = new System.Drawing.Point(322, 80);
            btnShowProject.Name = "btnShowProject";
            btnShowProject.Size = new System.Drawing.Size(45, 23);
            btnShowProject.TabIndex = 21;
            btnShowProject.Text = "Show";
            btnShowProject.UseVisualStyleBackColor = true;
            btnShowProject.Click += new System.EventHandler(this.btnShowProject_Click);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(264, 7);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(51, 13);
            label4.TabIndex = 11;
            label4.Text = "Origin ID:";
            // 
            // tbOriginId
            // 
            tbOriginId.Location = new System.Drawing.Point(321, 4);
            tbOriginId.Name = "tbOriginId";
            tbOriginId.ReadOnly = true;
            tbOriginId.Size = new System.Drawing.Size(100, 20);
            tbOriginId.TabIndex = 12;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbID);
            panel1.Controls.Add(splitContainer1);
            panel1.Controls.Add(btnShowProject);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(helpIcon1);
            panel1.Controls.Add(tbOriginId);
            panel1.Controls.Add(pDescription);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(lblDescription);
            panel1.Controls.Add(tbOverrideReleaseIdentifierSQL);
            panel1.Controls.Add(tbProjectNumber);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(tbVersion);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1046, 563);
            panel1.TabIndex = 22;
            // 
            // ExtractableCohortUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(panel1);
            this.Name = "ExtractableCohortUI";
            this.Size = new System.Drawing.Size(1046, 563);
            ((System.ComponentModel.ISupportInitialize)(tlvCohortUsage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(tlvPreviousVersions)).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbOverrideReleaseIdentifierSQL;
        private System.Windows.Forms.Panel pDescription;
        private HelpIcon helpIcon1;
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
