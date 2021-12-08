using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.SimpleDialogs.ForwardEngineering
{
    partial class ConfigureCatalogueExtractabilityUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureCatalogueExtractabilityUI));
            this.olvColumnExtractability = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnInfoName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExtractionCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIsExtractionIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIsHashOnRelease = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddToExisting = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ddCategoriseMany = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.pChangeAll = new System.Windows.Forms.Panel();
            this.pFilter = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbTableName = new System.Windows.Forms.TextBox();
            this.tbCatalogueName = new System.Windows.Forms.TextBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.pbProject = new System.Windows.Forms.PictureBox();
            this.lblProject = new System.Windows.Forms.Label();
            this.btnPickProject = new System.Windows.Forms.Button();
            this.gbProjectSpecific = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.ddIsExtractionIdentifier = new System.Windows.Forms.ComboBox();
            this.tbAcronym = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRenameTableInfo = new System.Windows.Forms.Button();
            this.helpIcon1 = new HelpIcon();
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).BeginInit();
            this.pChangeAll.SuspendLayout();
            this.pFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).BeginInit();
            this.gbProjectSpecific.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvColumnExtractability
            // 
            this.olvColumnExtractability.AllColumns.Add(this.olvColumnInfoName);
            this.olvColumnExtractability.AllColumns.Add(this.olvExtractionCategory);
            this.olvColumnExtractability.AllColumns.Add(this.olvIsExtractionIdentifier);
            this.olvColumnExtractability.AllColumns.Add(this.olvIsHashOnRelease);
            this.olvColumnExtractability.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvColumnExtractability.CellEditUseWholeCell = false;
            this.olvColumnExtractability.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnInfoName,
            this.olvExtractionCategory});
            this.olvColumnExtractability.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvColumnExtractability.FullRowSelect = true;
            this.olvColumnExtractability.Location = new System.Drawing.Point(10, 164);
            this.olvColumnExtractability.Name = "olvColumnExtractability";
            this.olvColumnExtractability.RowHeight = 19;
            this.olvColumnExtractability.ShowGroups = false;
            this.olvColumnExtractability.Size = new System.Drawing.Size(795, 298);
            this.olvColumnExtractability.TabIndex = 7;
            this.olvColumnExtractability.UseCompatibleStateImageBehavior = false;
            this.olvColumnExtractability.UseFiltering = true;
            this.olvColumnExtractability.View = System.Windows.Forms.View.Details;
            // 
            // olvColumnInfoName
            // 
            this.olvColumnInfoName.AspectName = "ToString";
            this.olvColumnInfoName.Groupable = false;
            this.olvColumnInfoName.Text = "Name";
            this.olvColumnInfoName.MinimumWidth = 100;
            // 
            // olvExtractionCategory
            // 
            this.olvExtractionCategory.Text = "Is Extractable";
            this.olvExtractionCategory.Width = 100;
            // 
            // olvIsExtractionIdentifier
            // 
            this.olvIsExtractionIdentifier.CheckBoxes = true;
            this.olvIsExtractionIdentifier.DisplayIndex = 3;
            this.olvIsExtractionIdentifier.IsVisible = false;
            this.olvIsExtractionIdentifier.Text = "IsExtractionIdentifier";
            this.olvIsExtractionIdentifier.Width = 106;
            // 
            // olvIsHashOnRelease
            // 
            this.olvIsHashOnRelease.CheckBoxes = true;
            this.olvIsHashOnRelease.DisplayIndex = 2;
            this.olvIsHashOnRelease.IsVisible = false;
            this.olvIsHashOnRelease.Text = "Hash On Release";
            this.olvIsHashOnRelease.Width = 106;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(295, 528);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(207, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel (Do not create a Catalogue)";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddToExisting
            // 
            this.btnAddToExisting.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAddToExisting.Location = new System.Drawing.Point(508, 528);
            this.btnAddToExisting.Name = "btnAddToExisting";
            this.btnAddToExisting.Size = new System.Drawing.Size(161, 23);
            this.btnAddToExisting.TabIndex = 7;
            this.btnAddToExisting.Text = "Add to existing Catalogue";
            this.btnAddToExisting.UseVisualStyleBackColor = true;
            this.btnAddToExisting.Click += new System.EventHandler(this.btnAddToExisting_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(170, 528);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(119, 23);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Set All:";
            // 
            // ddCategoriseMany
            // 
            this.ddCategoriseMany.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddCategoriseMany.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCategoriseMany.FormattingEnabled = true;
            this.ddCategoriseMany.Location = new System.Drawing.Point(50, 3);
            this.ddCategoriseMany.Name = "ddCategoriseMany";
            this.ddCategoriseMany.Size = new System.Drawing.Size(207, 21);
            this.ddCategoriseMany.TabIndex = 5;
            this.ddCategoriseMany.SelectedIndexChanged += new System.EventHandler(this.ddCategoriseMany_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(485, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // pChangeAll
            // 
            this.pChangeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pChangeAll.Controls.Add(this.ddCategoriseMany);
            this.pChangeAll.Controls.Add(this.label3);
            this.pChangeAll.Location = new System.Drawing.Point(545, 495);
            this.pChangeAll.Name = "pChangeAll";
            this.pChangeAll.Size = new System.Drawing.Size(260, 26);
            this.pChangeAll.TabIndex = 8;
            // 
            // pFilter
            // 
            this.pFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pFilter.Controls.Add(this.label6);
            this.pFilter.Controls.Add(this.tbFilter);
            this.pFilter.Location = new System.Drawing.Point(10, 495);
            this.pFilter.Name = "pFilter";
            this.pFilter.Size = new System.Drawing.Size(529, 27);
            this.pFilter.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 148);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Choose which columns are extractable";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Catalogue Name:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Description:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(394, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Table Name:";
            // 
            // tbTableName
            // 
            this.tbTableName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTableName.Location = new System.Drawing.Point(468, 6);
            this.tbTableName.Name = "tbTableName";
            this.tbTableName.ReadOnly = true;
            this.tbTableName.Size = new System.Drawing.Size(334, 20);
            this.tbTableName.TabIndex = 12;
            // 
            // tbCatalogueName
            // 
            this.tbCatalogueName.Location = new System.Drawing.Point(111, 6);
            this.tbCatalogueName.Name = "tbCatalogueName";
            this.tbCatalogueName.Size = new System.Drawing.Size(277, 20);
            this.tbCatalogueName.TabIndex = 12;
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(111, 57);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(694, 48);
            this.tbDescription.TabIndex = 12;
            // 
            // pbProject
            // 
            this.pbProject.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pbProject.Location = new System.Drawing.Point(3, 3);
            this.pbProject.Name = "pbProject";
            this.pbProject.Size = new System.Drawing.Size(26, 26);
            this.pbProject.TabIndex = 19;
            this.pbProject.TabStop = false;
            // 
            // lblProject
            // 
            this.lblProject.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProject.AutoSize = true;
            this.lblProject.Location = new System.Drawing.Point(35, 9);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(27, 13);
            this.lblProject.TabIndex = 18;
            this.lblProject.Text = "blah";
            // 
            // btnPickProject
            // 
            this.btnPickProject.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnPickProject.Location = new System.Drawing.Point(68, 4);
            this.btnPickProject.Name = "btnPickProject";
            this.btnPickProject.Size = new System.Drawing.Size(67, 23);
            this.btnPickProject.TabIndex = 21;
            this.btnPickProject.Text = "Existing...";
            this.btnPickProject.UseVisualStyleBackColor = true;
            this.btnPickProject.Click += new System.EventHandler(this.btnPickProject_Click);
            // 
            // gbProjectSpecific
            // 
            this.gbProjectSpecific.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbProjectSpecific.Controls.Add(this.flowLayoutPanel1);
            this.gbProjectSpecific.Location = new System.Drawing.Point(262, 108);
            this.gbProjectSpecific.Name = "gbProjectSpecific";
            this.gbProjectSpecific.Size = new System.Drawing.Size(543, 52);
            this.gbProjectSpecific.TabIndex = 22;
            this.gbProjectSpecific.TabStop = false;
            this.gbProjectSpecific.Text = "Catalogue can only be used with Project:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.pbProject);
            this.flowLayoutPanel1.Controls.Add(this.lblProject);
            this.flowLayoutPanel1.Controls.Add(this.btnPickProject);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(537, 33);
            this.flowLayoutPanel1.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 465);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 26);
            this.label7.TabIndex = 23;
            this.label7.Text = "Patient Identifier Column:\r\n(IsExtractionIdentifier)";
            // 
            // ddIsExtractionIdentifier
            // 
            this.ddIsExtractionIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddIsExtractionIdentifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddIsExtractionIdentifier.FormattingEnabled = true;
            this.ddIsExtractionIdentifier.Location = new System.Drawing.Point(137, 468);
            this.ddIsExtractionIdentifier.Name = "ddIsExtractionIdentifier";
            this.ddIsExtractionIdentifier.Size = new System.Drawing.Size(362, 21);
            this.ddIsExtractionIdentifier.TabIndex = 24;
            this.ddIsExtractionIdentifier.SelectedIndexChanged += new System.EventHandler(this.ddIsExtractionIdentifier_SelectedIndexChanged);
            // 
            // tbAcronym
            // 
            this.tbAcronym.Location = new System.Drawing.Point(111, 32);
            this.tbAcronym.Name = "tbAcronym";
            this.tbAcronym.Size = new System.Drawing.Size(160, 20);
            this.tbAcronym.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(54, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Acronym:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnRenameTableInfo);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tbAcronym);
            this.panel1.Controls.Add(this.olvColumnExtractability);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.pFilter);
            this.panel1.Controls.Add(this.ddIsExtractionIdentifier);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.helpIcon1);
            this.panel1.Controls.Add(this.gbProjectSpecific);
            this.panel1.Controls.Add(this.btnAddToExisting);
            this.panel1.Controls.Add(this.tbDescription);
            this.panel1.Controls.Add(this.pChangeAll);
            this.panel1.Controls.Add(this.tbCatalogueName);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.tbTableName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(819, 568);
            this.panel1.TabIndex = 27;
            // 
            // btnRenameTableInfo
            // 
            this.btnRenameTableInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRenameTableInfo.Location = new System.Drawing.Point(727, 30);
            this.btnRenameTableInfo.Name = "btnRenameTableInfo";
            this.btnRenameTableInfo.Size = new System.Drawing.Size(75, 23);
            this.btnRenameTableInfo.TabIndex = 27;
            this.btnRenameTableInfo.Text = "Rename...";
            this.btnRenameTableInfo.UseVisualStyleBackColor = true;
            this.btnRenameTableInfo.Click += new System.EventHandler(this.BtnRenameTableInfo_Click);
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(207, 144);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.SuppressClick = false;
            this.helpIcon1.TabIndex = 8;
            // 
            // ConfigureCatalogueExtractabilityUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 568);
            this.Controls.Add(this.panel1);
            this.Name = "ConfigureCatalogueExtractabilityUI";
            this.Text = "Configure Extractability";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigureCatalogueExtractabilityUI_FormClosing);
            this.Load += new System.EventHandler(this.ConfigureCatalogueExtractabilityUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumnExtractability)).EndInit();
            this.pChangeAll.ResumeLayout(false);
            this.pChangeAll.PerformLayout();
            this.pFilter.ResumeLayout(false);
            this.pFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).EndInit();
            this.gbProjectSpecific.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilter;
        private BrightIdeasSoftware.ObjectListView olvColumnExtractability;
        private BrightIdeasSoftware.OLVColumn olvColumnInfoName;
        private BrightIdeasSoftware.OLVColumn olvExtractionCategory;
        private System.Windows.Forms.ComboBox ddCategoriseMany;
        private System.Windows.Forms.Label label3;
        private BrightIdeasSoftware.OLVColumn olvIsExtractionIdentifier;
        private BrightIdeasSoftware.OLVColumn olvIsHashOnRelease;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnAddToExisting;
        private System.Windows.Forms.Panel pChangeAll;
        private System.Windows.Forms.Panel pFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbTableName;
        private System.Windows.Forms.TextBox tbCatalogueName;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.PictureBox pbProject;
        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Button btnPickProject;
        private System.Windows.Forms.GroupBox gbProjectSpecific;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ddIsExtractionIdentifier;
        private System.Windows.Forms.TextBox tbAcronym;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel1;
        private HelpIcon helpIcon1;
        private System.Windows.Forms.Button btnRenameTableInfo;
    }
}
