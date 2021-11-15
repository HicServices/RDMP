using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.CohortUI.ImportCustomData
{
    partial class CohortCreationRequestUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortCreationRequestUI));
            this.label1 = new System.Windows.Forms.Label();
            this.ddExistingCohort = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblExternalCohortTable = new System.Windows.Forms.Label();
            this.gbRevisedCohort = new System.Windows.Forms.GroupBox();
            this.existingHelpIcon = new Rdmp.UI.SimpleControls.HelpIcon();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbExistingCohortSource = new System.Windows.Forms.TextBox();
            this.tbExistingVersion = new System.Windows.Forms.TextBox();
            this.lblNewVersionNumber = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.rbNewCohort = new System.Windows.Forms.RadioButton();
            this.rbRevisedCohort = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.gbNewCohort = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.btnNewProject = new System.Windows.Forms.Button();
            this.gbChooseCohortType = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.lblProject = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnClearProject = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.gbDescription = new System.Windows.Forms.GroupBox();
            this.pbProject = new System.Windows.Forms.PictureBox();
            this.pbCohortSource = new System.Windows.Forms.PictureBox();
            this.btnExisting = new System.Windows.Forms.Button();
            this.lblErrorNoProjectNumber = new System.Windows.Forms.Label();
            this.tbSetProjectNumber = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.gbRevisedCohort.SuspendLayout();
            this.gbNewCohort.SuspendLayout();
            this.gbChooseCohortType.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(335, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "This form lets you describe the cohort you are trying to import";
            // 
            // ddExistingCohort
            // 
            this.ddExistingCohort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExistingCohort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExistingCohort.FormattingEnabled = true;
            this.ddExistingCohort.Location = new System.Drawing.Point(172, 18);
            this.ddExistingCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddExistingCohort.Name = "ddExistingCohort";
            this.ddExistingCohort.Size = new System.Drawing.Size(510, 23);
            this.ddExistingCohort.Sorted = true;
            this.ddExistingCohort.TabIndex = 1;
            this.ddExistingCohort.SelectedIndexChanged += new System.EventHandler(this.ddExistingCohort_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 32);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "ExternalCohortTable:";
            // 
            // lblExternalCohortTable
            // 
            this.lblExternalCohortTable.AutoSize = true;
            this.lblExternalCohortTable.Location = new System.Drawing.Point(182, 32);
            this.lblExternalCohortTable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExternalCohortTable.Name = "lblExternalCohortTable";
            this.lblExternalCohortTable.Size = new System.Drawing.Size(30, 15);
            this.lblExternalCohortTable.TabIndex = 2;
            this.lblExternalCohortTable.Text = "blah";
            // 
            // gbRevisedCohort
            // 
            this.gbRevisedCohort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbRevisedCohort.Controls.Add(this.existingHelpIcon);
            this.gbRevisedCohort.Controls.Add(this.label8);
            this.gbRevisedCohort.Controls.Add(this.label2);
            this.gbRevisedCohort.Controls.Add(this.tbExistingCohortSource);
            this.gbRevisedCohort.Controls.Add(this.tbExistingVersion);
            this.gbRevisedCohort.Controls.Add(this.lblNewVersionNumber);
            this.gbRevisedCohort.Controls.Add(this.label6);
            this.gbRevisedCohort.Controls.Add(this.label5);
            this.gbRevisedCohort.Controls.Add(this.ddExistingCohort);
            this.gbRevisedCohort.Enabled = false;
            this.gbRevisedCohort.Location = new System.Drawing.Point(13, 85);
            this.gbRevisedCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbRevisedCohort.Name = "gbRevisedCohort";
            this.gbRevisedCohort.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbRevisedCohort.Size = new System.Drawing.Size(950, 78);
            this.gbRevisedCohort.TabIndex = 1;
            this.gbRevisedCohort.TabStop = false;
            this.gbRevisedCohort.Text = "Revised Cohort";
            // 
            // existingHelpIcon
            // 
            this.existingHelpIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.existingHelpIcon.BackColor = System.Drawing.Color.Transparent;
            this.existingHelpIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("existingHelpIcon.BackgroundImage")));
            this.existingHelpIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.existingHelpIcon.Location = new System.Drawing.Point(919, 20);
            this.existingHelpIcon.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.existingHelpIcon.MaximumSize = new System.Drawing.Size(26, 25);
            this.existingHelpIcon.MinimumSize = new System.Drawing.Size(26, 25);
            this.existingHelpIcon.Name = "existingHelpIcon";
            this.existingHelpIcon.Size = new System.Drawing.Size(26, 25);
            this.existingHelpIcon.SuppressClick = false;
            this.existingHelpIcon.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(832, 45);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 15);
            this.label8.TabIndex = 5;
            this.label8.Text = "(Source)";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(690, 45);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "(Current Version)";
            // 
            // tbExistingCohortSource
            // 
            this.tbExistingCohortSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExistingCohortSource.Location = new System.Drawing.Point(796, 18);
            this.tbExistingCohortSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbExistingCohortSource.Name = "tbExistingCohortSource";
            this.tbExistingCohortSource.ReadOnly = true;
            this.tbExistingCohortSource.Size = new System.Drawing.Size(116, 23);
            this.tbExistingCohortSource.TabIndex = 4;
            // 
            // tbExistingVersion
            // 
            this.tbExistingVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExistingVersion.Location = new System.Drawing.Point(690, 18);
            this.tbExistingVersion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbExistingVersion.Name = "tbExistingVersion";
            this.tbExistingVersion.ReadOnly = true;
            this.tbExistingVersion.Size = new System.Drawing.Size(98, 23);
            this.tbExistingVersion.TabIndex = 4;
            // 
            // lblNewVersionNumber
            // 
            this.lblNewVersionNumber.AutoSize = true;
            this.lblNewVersionNumber.Location = new System.Drawing.Point(176, 46);
            this.lblNewVersionNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNewVersionNumber.Name = "lblNewVersionNumber";
            this.lblNewVersionNumber.Size = new System.Drawing.Size(18, 15);
            this.lblNewVersionNumber.TabIndex = 3;
            this.lblNewVersionNumber.Text = "-1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 47);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(157, 15);
            this.label6.TabIndex = 2;
            this.label6.Text = "New version number will be:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(71, 22);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "Existing Cohort:";
            // 
            // rbNewCohort
            // 
            this.rbNewCohort.AutoSize = true;
            this.rbNewCohort.Location = new System.Drawing.Point(80, 20);
            this.rbNewCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbNewCohort.Name = "rbNewCohort";
            this.rbNewCohort.Size = new System.Drawing.Size(92, 19);
            this.rbNewCohort.TabIndex = 1;
            this.rbNewCohort.TabStop = true;
            this.rbNewCohort.Text = "New Cohort ";
            this.rbNewCohort.UseVisualStyleBackColor = true;
            this.rbNewCohort.CheckedChanged += new System.EventHandler(this.rbNewCohort_CheckedChanged);
            // 
            // rbRevisedCohort
            // 
            this.rbRevisedCohort.AutoSize = true;
            this.rbRevisedCohort.Location = new System.Drawing.Point(186, 20);
            this.rbRevisedCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbRevisedCohort.Name = "rbRevisedCohort";
            this.rbRevisedCohort.Size = new System.Drawing.Size(207, 19);
            this.rbRevisedCohort.TabIndex = 2;
            this.rbRevisedCohort.TabStop = true;
            this.rbRevisedCohort.Text = "Revised version of existing Cohort ";
            this.rbRevisedCohort.UseVisualStyleBackColor = true;
            this.rbRevisedCohort.CheckedChanged += new System.EventHandler(this.rbRevisedCohort_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "This is a:";
            // 
            // gbNewCohort
            // 
            this.gbNewCohort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbNewCohort.Controls.Add(this.label7);
            this.gbNewCohort.Controls.Add(this.tbName);
            this.gbNewCohort.Enabled = false;
            this.gbNewCohort.Location = new System.Drawing.Point(13, 24);
            this.gbNewCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbNewCohort.Name = "gbNewCohort";
            this.gbNewCohort.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbNewCohort.Size = new System.Drawing.Size(950, 54);
            this.gbNewCohort.TabIndex = 0;
            this.gbNewCohort.TabStop = false;
            this.gbNewCohort.Text = "New Cohort";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 25);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(58, 22);
            this.tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(884, 23);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // btnNewProject
            // 
            this.btnNewProject.Location = new System.Drawing.Point(383, 63);
            this.btnNewProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnNewProject.Name = "btnNewProject";
            this.btnNewProject.Size = new System.Drawing.Size(54, 27);
            this.btnNewProject.TabIndex = 5;
            this.btnNewProject.Text = "New...";
            this.btnNewProject.UseVisualStyleBackColor = true;
            this.btnNewProject.Click += new System.EventHandler(this.btnNewProject_Click);
            // 
            // gbChooseCohortType
            // 
            this.gbChooseCohortType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbChooseCohortType.Controls.Add(this.rbRevisedCohort);
            this.gbChooseCohortType.Controls.Add(this.label4);
            this.gbChooseCohortType.Controls.Add(this.rbNewCohort);
            this.gbChooseCohortType.Location = new System.Drawing.Point(31, 125);
            this.gbChooseCohortType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbChooseCohortType.Name = "gbChooseCohortType";
            this.gbChooseCohortType.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbChooseCohortType.Size = new System.Drawing.Size(993, 48);
            this.gbChooseCohortType.TabIndex = 9;
            this.gbChooseCohortType.TabStop = false;
            this.gbChooseCohortType.Text = "2. Choose Cohort Type";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.gbNewCohort);
            this.groupBox3.Controls.Add(this.gbRevisedCohort);
            this.groupBox3.Location = new System.Drawing.Point(31, 180);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(993, 178);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "3. Configure Cohort (doesn\'t exist yet, next screen will actually create it)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 24);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Comment:";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(71, 24);
            this.tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(914, 67);
            this.tbDescription.TabIndex = 3;
            // 
            // lblProject
            // 
            this.lblProject.AutoSize = true;
            this.lblProject.Location = new System.Drawing.Point(182, 68);
            this.lblProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(30, 15);
            this.lblProject.TabIndex = 4;
            this.lblProject.Text = "blah";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(88, 68);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 15);
            this.label11.TabIndex = 3;
            this.label11.Text = "Project:";
            // 
            // btnClearProject
            // 
            this.btnClearProject.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClearProject.Location = new System.Drawing.Point(486, 474);
            this.btnClearProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearProject.Name = "btnClearProject";
            this.btnClearProject.Size = new System.Drawing.Size(141, 27);
            this.btnClearProject.TabIndex = 14;
            this.btnClearProject.Text = "Cancel";
            this.btnClearProject.UseVisualStyleBackColor = true;
            this.btnClearProject.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(330, 474);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(149, 27);
            this.btnOk.TabIndex = 13;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(293, 471);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(30, 30);
            this.ragSmiley1.TabIndex = 12;
            // 
            // gbDescription
            // 
            this.gbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDescription.Controls.Add(this.label9);
            this.gbDescription.Controls.Add(this.tbDescription);
            this.gbDescription.Location = new System.Drawing.Point(31, 365);
            this.gbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbDescription.Name = "gbDescription";
            this.gbDescription.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbDescription.Size = new System.Drawing.Size(993, 99);
            this.gbDescription.TabIndex = 11;
            this.gbDescription.TabStop = false;
            this.gbDescription.Text = "4. Enter Description Of Cohort";
            // 
            // pbProject
            // 
            this.pbProject.Location = new System.Drawing.Point(145, 60);
            this.pbProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbProject.Name = "pbProject";
            this.pbProject.Size = new System.Drawing.Size(30, 30);
            this.pbProject.TabIndex = 17;
            this.pbProject.TabStop = false;
            // 
            // pbCohortSource
            // 
            this.pbCohortSource.Location = new System.Drawing.Point(145, 29);
            this.pbCohortSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbCohortSource.Name = "pbCohortSource";
            this.pbCohortSource.Size = new System.Drawing.Size(30, 30);
            this.pbCohortSource.TabIndex = 18;
            this.pbCohortSource.TabStop = false;
            // 
            // btnExisting
            // 
            this.btnExisting.Location = new System.Drawing.Point(443, 63);
            this.btnExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExisting.Name = "btnExisting";
            this.btnExisting.Size = new System.Drawing.Size(78, 27);
            this.btnExisting.TabIndex = 6;
            this.btnExisting.Text = "Existing...";
            this.btnExisting.UseVisualStyleBackColor = true;
            this.btnExisting.Click += new System.EventHandler(this.btnExisting_Click);
            // 
            // lblErrorNoProjectNumber
            // 
            this.lblErrorNoProjectNumber.AutoSize = true;
            this.lblErrorNoProjectNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblErrorNoProjectNumber.ForeColor = System.Drawing.Color.Red;
            this.lblErrorNoProjectNumber.Location = new System.Drawing.Point(173, 97);
            this.lblErrorNoProjectNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblErrorNoProjectNumber.Name = "lblErrorNoProjectNumber";
            this.lblErrorNoProjectNumber.Size = new System.Drawing.Size(244, 13);
            this.lblErrorNoProjectNumber.TabIndex = 7;
            this.lblErrorNoProjectNumber.Text = "Selected Project does not have a Project Number:";
            this.lblErrorNoProjectNumber.Visible = false;
            // 
            // tbSetProjectNumber
            // 
            this.tbSetProjectNumber.Location = new System.Drawing.Point(461, 93);
            this.tbSetProjectNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbSetProjectNumber.Name = "tbSetProjectNumber";
            this.tbSetProjectNumber.Size = new System.Drawing.Size(138, 23);
            this.tbSetProjectNumber.TabIndex = 8;
            this.tbSetProjectNumber.Visible = false;
            this.tbSetProjectNumber.TextChanged += new System.EventHandler(this.tbSetProjectNumber_TextChanged);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(528, 63);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(50, 27);
            this.btnClear.TabIndex = 6;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // CohortCreationRequestUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 515);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.tbSetProjectNumber);
            this.Controls.Add(this.lblErrorNoProjectNumber);
            this.Controls.Add(this.btnExisting);
            this.Controls.Add(this.pbCohortSource);
            this.Controls.Add(this.btnNewProject);
            this.Controls.Add(this.pbProject);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.gbDescription);
            this.Controls.Add(this.btnClearProject);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblProject);
            this.Controls.Add(this.lblExternalCohortTable);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gbChooseCohortType);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CohortCreationRequestUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Cohort";
            this.Load += new System.EventHandler(this.CohortCreationRequestUI_Load);
            this.gbRevisedCohort.ResumeLayout(false);
            this.gbRevisedCohort.PerformLayout();
            this.gbNewCohort.ResumeLayout(false);
            this.gbNewCohort.PerformLayout();
            this.gbChooseCohortType.ResumeLayout(false);
            this.gbChooseCohortType.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.gbDescription.ResumeLayout(false);
            this.gbDescription.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddExistingCohort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblExternalCohortTable;
        private System.Windows.Forms.GroupBox gbRevisedCohort;
        private System.Windows.Forms.RadioButton rbNewCohort;
        private System.Windows.Forms.RadioButton rbRevisedCohort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblNewVersionNumber;
        private System.Windows.Forms.GroupBox gbNewCohort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnNewProject;
        private System.Windows.Forms.GroupBox gbChooseCohortType;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnClearProject;
        private System.Windows.Forms.Button btnOk;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.PictureBox pbProject;
        private System.Windows.Forms.PictureBox pbCohortSource;
        private System.Windows.Forms.Button btnExisting;
        private System.Windows.Forms.Label lblErrorNoProjectNumber;
        private System.Windows.Forms.TextBox tbSetProjectNumber;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbExistingCohortSource;
        private System.Windows.Forms.TextBox tbExistingVersion;
        private HelpIcon existingHelpIcon;
    }
}