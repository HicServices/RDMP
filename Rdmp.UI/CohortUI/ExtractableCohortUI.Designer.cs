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
            helpIcon1 = new Rdmp.UI.SimpleControls.HelpIcon();
            tlvCohortUsage = new BrightIdeasSoftware.TreeListView();
            olvUsedIn = new BrightIdeasSoftware.OLVColumn();
            tlvPreviousVersions = new BrightIdeasSoftware.TreeListView();
            olvOtherVersions = new BrightIdeasSoftware.OLVColumn();
            olvVersion = new BrightIdeasSoftware.OLVColumn();
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
            tbID.Location = new System.Drawing.Point(183, 5);
            tbID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbID.Name = "tbID";
            tbID.ReadOnly = true;
            tbID.Size = new System.Drawing.Size(116, 23);
            tbID.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(158, 9);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(21, 15);
            label5.TabIndex = 11;
            label5.Text = "ID:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(117, 128);
            lblDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(62, 15);
            lblDescription.TabIndex = 13;
            lblDescription.Text = "Audit Log:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(17, 39);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(162, 15);
            label1.TabIndex = 11;
            label1.Text = "OverrideReleaseIdentifierSQL:";
            // 
            // tbOverrideReleaseIdentifierSQL
            // 
            tbOverrideReleaseIdentifierSQL.Location = new System.Drawing.Point(183, 35);
            tbOverrideReleaseIdentifierSQL.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbOverrideReleaseIdentifierSQL.Name = "tbOverrideReleaseIdentifierSQL";
            tbOverrideReleaseIdentifierSQL.Size = new System.Drawing.Size(545, 23);
            tbOverrideReleaseIdentifierSQL.TabIndex = 12;
            tbOverrideReleaseIdentifierSQL.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // pDescription
            // 
            pDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            pDescription.Location = new System.Drawing.Point(183, 125);
            pDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pDescription.Name = "pDescription";
            pDescription.Size = new System.Drawing.Size(1036, 186);
            pDescription.TabIndex = 15;
            // 
            // helpIcon1
            // 
            helpIcon1.BackColor = System.Drawing.Color.Transparent;
            helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            helpIcon1.Location = new System.Drawing.Point(729, 33);
            helpIcon1.Margin = new System.Windows.Forms.Padding(0);
            helpIcon1.MinimumSize = new System.Drawing.Size(26, 25);
            helpIcon1.Name = "helpIcon1";
            helpIcon1.Size = new System.Drawing.Size(26, 25);
            helpIcon1.SuppressClick = false;
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
            tlvCohortUsage.HideSelection = false;
            tlvCohortUsage.Location = new System.Drawing.Point(0, 0);
            tlvCohortUsage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvCohortUsage.Name = "tlvCohortUsage";
            tlvCohortUsage.ShowGroups = false;
            tlvCohortUsage.Size = new System.Drawing.Size(628, 318);
            tlvCohortUsage.TabIndex = 18;
            tlvCohortUsage.UseCompatibleStateImageBehavior = false;
            tlvCohortUsage.View = System.Windows.Forms.View.Details;
            tlvCohortUsage.VirtualMode = true;
            // 
            // olvUsedIn
            // 
            olvUsedIn.AspectName = "ToString";
            olvUsedIn.MinimumWidth = 100;
            olvUsedIn.Text = "Used In";
            olvUsedIn.Width = 100;
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
            tlvPreviousVersions.HideSelection = false;
            tlvPreviousVersions.Location = new System.Drawing.Point(0, 0);
            tlvPreviousVersions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvPreviousVersions.Name = "tlvPreviousVersions";
            tlvPreviousVersions.ShowGroups = false;
            tlvPreviousVersions.Size = new System.Drawing.Size(579, 318);
            tlvPreviousVersions.TabIndex = 19;
            tlvPreviousVersions.UseCompatibleStateImageBehavior = false;
            tlvPreviousVersions.View = System.Windows.Forms.View.Details;
            tlvPreviousVersions.VirtualMode = true;
            // 
            // olvOtherVersions
            // 
            olvOtherVersions.AspectName = "ToString";
            olvOtherVersions.MinimumWidth = 100;
            olvOtherVersions.Text = "Other Versions";
            olvOtherVersions.Width = 100;
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
            splitContainer1.Location = new System.Drawing.Point(5, 317);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tlvCohortUsage);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tlvPreviousVersions);
            splitContainer1.Size = new System.Drawing.Size(1212, 318);
            splitContainer1.SplitterDistance = 628;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 20;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(131, 69);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(48, 15);
            label2.TabIndex = 11;
            label2.Text = "Version:";
            // 
            // tbVersion
            // 
            tbVersion.Location = new System.Drawing.Point(183, 65);
            tbVersion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbVersion.Name = "tbVersion";
            tbVersion.ReadOnly = true;
            tbVersion.Size = new System.Drawing.Size(185, 23);
            tbVersion.TabIndex = 12;
            tbVersion.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(85, 99);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(94, 15);
            label3.TabIndex = 11;
            label3.Text = "Project Number:";
            // 
            // tbProjectNumber
            // 
            tbProjectNumber.Location = new System.Drawing.Point(183, 95);
            tbProjectNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbProjectNumber.Name = "tbProjectNumber";
            tbProjectNumber.ReadOnly = true;
            tbProjectNumber.Size = new System.Drawing.Size(185, 23);
            tbProjectNumber.TabIndex = 12;
            tbProjectNumber.TextChanged += new System.EventHandler(this.tbOverrideReleaseIdentifierSQL_TextChanged);
            // 
            // btnShowProject
            // 
            btnShowProject.Location = new System.Drawing.Point(372, 94);
            btnShowProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnShowProject.Name = "btnShowProject";
            btnShowProject.Size = new System.Drawing.Size(52, 25);
            btnShowProject.TabIndex = 21;
            btnShowProject.Text = "Show";
            btnShowProject.UseVisualStyleBackColor = true;
            btnShowProject.Click += new System.EventHandler(this.btnShowProject_Click);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(369, 9);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(57, 15);
            label4.TabIndex = 11;
            label4.Text = "Origin ID:";
            // 
            // tbOriginId
            // 
            tbOriginId.Location = new System.Drawing.Point(430, 5);
            tbOriginId.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbOriginId.Name = "tbOriginId";
            tbOriginId.ReadOnly = true;
            tbOriginId.Size = new System.Drawing.Size(117, 23);
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
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1220, 650);
            panel1.TabIndex = 22;
            // 
            // ExtractableCohortUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ExtractableCohortUI";
            this.Size = new System.Drawing.Size(1220, 650);
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
