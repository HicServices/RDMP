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
            ddExistingCohort = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            lblExternalCohortTable = new System.Windows.Forms.Label();
            gbRevisedCohort = new System.Windows.Forms.GroupBox();
            existingHelpIcon = new HelpIcon();
            label8 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tbExistingCohortSource = new System.Windows.Forms.TextBox();
            tbExistingVersion = new System.Windows.Forms.TextBox();
            lblNewVersionNumber = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            rbNewCohort = new System.Windows.Forms.RadioButton();
            rbRevisedCohort = new System.Windows.Forms.RadioButton();
            label4 = new System.Windows.Forms.Label();
            gbNewCohort = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            btnNewProject = new System.Windows.Forms.Button();
            gbChooseCohortType = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label9 = new System.Windows.Forms.Label();
            tbDescription = new System.Windows.Forms.TextBox();
            lblProject = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            btnClearProject = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            ragSmiley1 = new RAGSmiley();
            gbDescription = new System.Windows.Forms.GroupBox();
            pbProject = new System.Windows.Forms.PictureBox();
            pbCohortSource = new System.Windows.Forms.PictureBox();
            btnExisting = new System.Windows.Forms.Button();
            lblErrorNoProjectNumber = new System.Windows.Forms.Label();
            tbSetProjectNumber = new System.Windows.Forms.TextBox();
            btnClear = new System.Windows.Forms.Button();
            taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            groupBox1 = new System.Windows.Forms.GroupBox();
            cbCopyProjectSpecificCatalogues = new System.Windows.Forms.CheckBox();
            gbRevisedCohort.SuspendLayout();
            gbNewCohort.SuspendLayout();
            gbChooseCohortType.SuspendLayout();
            groupBox3.SuspendLayout();
            gbDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbProject).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbCohortSource).BeginInit();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // ddExistingCohort
            // 
            ddExistingCohort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ddExistingCohort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddExistingCohort.FormattingEnabled = true;
            ddExistingCohort.Location = new System.Drawing.Point(172, 18);
            ddExistingCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ddExistingCohort.Name = "ddExistingCohort";
            ddExistingCohort.Size = new System.Drawing.Size(565, 23);
            ddExistingCohort.Sorted = true;
            ddExistingCohort.TabIndex = 1;
            ddExistingCohort.SelectedIndexChanged += ddExistingCohort_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(18, 19);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(116, 15);
            label3.TabIndex = 1;
            label3.Text = "ExternalCohortTable:";
            // 
            // lblExternalCohortTable
            // 
            lblExternalCohortTable.AutoSize = true;
            lblExternalCohortTable.Location = new System.Drawing.Point(163, 19);
            lblExternalCohortTable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblExternalCohortTable.Name = "lblExternalCohortTable";
            lblExternalCohortTable.Size = new System.Drawing.Size(30, 15);
            lblExternalCohortTable.TabIndex = 2;
            lblExternalCohortTable.Text = "blah";
            // 
            // gbRevisedCohort
            // 
            gbRevisedCohort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbRevisedCohort.Controls.Add(existingHelpIcon);
            gbRevisedCohort.Controls.Add(label8);
            gbRevisedCohort.Controls.Add(label2);
            gbRevisedCohort.Controls.Add(tbExistingCohortSource);
            gbRevisedCohort.Controls.Add(tbExistingVersion);
            gbRevisedCohort.Controls.Add(lblNewVersionNumber);
            gbRevisedCohort.Controls.Add(label6);
            gbRevisedCohort.Controls.Add(label5);
            gbRevisedCohort.Controls.Add(ddExistingCohort);
            gbRevisedCohort.Enabled = false;
            gbRevisedCohort.Location = new System.Drawing.Point(13, 85);
            gbRevisedCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbRevisedCohort.Name = "gbRevisedCohort";
            gbRevisedCohort.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbRevisedCohort.Size = new System.Drawing.Size(1005, 78);
            gbRevisedCohort.TabIndex = 1;
            gbRevisedCohort.TabStop = false;
            gbRevisedCohort.Text = "Revised Cohort";
            // 
            // existingHelpIcon
            // 
            existingHelpIcon.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            existingHelpIcon.BackColor = System.Drawing.Color.Transparent;
            existingHelpIcon.BackgroundImage = (System.Drawing.Image)resources.GetObject("existingHelpIcon.BackgroundImage");
            existingHelpIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            existingHelpIcon.Location = new System.Drawing.Point(974, 20);
            existingHelpIcon.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            existingHelpIcon.MaximumSize = new System.Drawing.Size(26, 25);
            existingHelpIcon.MinimumSize = new System.Drawing.Size(26, 25);
            existingHelpIcon.Name = "existingHelpIcon";
            existingHelpIcon.Size = new System.Drawing.Size(26, 25);
            existingHelpIcon.TabIndex = 6;
            // 
            // label8
            // 
            label8.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(887, 45);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(51, 15);
            label8.TabIndex = 5;
            label8.Text = "(Source)";
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(745, 45);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(96, 15);
            label2.TabIndex = 5;
            label2.Text = "(Current Version)";
            // 
            // tbExistingCohortSource
            // 
            tbExistingCohortSource.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            tbExistingCohortSource.Location = new System.Drawing.Point(851, 18);
            tbExistingCohortSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbExistingCohortSource.Name = "tbExistingCohortSource";
            tbExistingCohortSource.ReadOnly = true;
            tbExistingCohortSource.Size = new System.Drawing.Size(116, 23);
            tbExistingCohortSource.TabIndex = 4;
            // 
            // tbExistingVersion
            // 
            tbExistingVersion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            tbExistingVersion.Location = new System.Drawing.Point(745, 18);
            tbExistingVersion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbExistingVersion.Name = "tbExistingVersion";
            tbExistingVersion.ReadOnly = true;
            tbExistingVersion.Size = new System.Drawing.Size(98, 23);
            tbExistingVersion.TabIndex = 4;
            // 
            // lblNewVersionNumber
            // 
            lblNewVersionNumber.AutoSize = true;
            lblNewVersionNumber.Location = new System.Drawing.Point(176, 46);
            lblNewVersionNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblNewVersionNumber.Name = "lblNewVersionNumber";
            lblNewVersionNumber.Size = new System.Drawing.Size(18, 15);
            lblNewVersionNumber.TabIndex = 3;
            lblNewVersionNumber.Text = "-1";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(7, 47);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(157, 15);
            label6.TabIndex = 2;
            label6.Text = "New version number will be:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(71, 22);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(91, 15);
            label5.TabIndex = 0;
            label5.Text = "Existing Cohort:";
            // 
            // rbNewCohort
            // 
            rbNewCohort.AutoSize = true;
            rbNewCohort.Location = new System.Drawing.Point(80, 20);
            rbNewCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbNewCohort.Name = "rbNewCohort";
            rbNewCohort.Size = new System.Drawing.Size(92, 19);
            rbNewCohort.TabIndex = 1;
            rbNewCohort.TabStop = true;
            rbNewCohort.Text = "New Cohort ";
            rbNewCohort.UseVisualStyleBackColor = true;
            rbNewCohort.CheckedChanged += rbNewCohort_CheckedChanged;
            // 
            // rbRevisedCohort
            // 
            rbRevisedCohort.AutoSize = true;
            rbRevisedCohort.Location = new System.Drawing.Point(186, 20);
            rbRevisedCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbRevisedCohort.Name = "rbRevisedCohort";
            rbRevisedCohort.Size = new System.Drawing.Size(207, 19);
            rbRevisedCohort.TabIndex = 2;
            rbRevisedCohort.TabStop = true;
            rbRevisedCohort.Text = "Revised version of existing Cohort ";
            rbRevisedCohort.UseVisualStyleBackColor = true;
            rbRevisedCohort.CheckedChanged += rbRevisedCohort_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(16, 22);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(51, 15);
            label4.TabIndex = 0;
            label4.Text = "This is a:";
            // 
            // gbNewCohort
            // 
            gbNewCohort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbNewCohort.Controls.Add(label7);
            gbNewCohort.Controls.Add(tbName);
            gbNewCohort.Enabled = false;
            gbNewCohort.Location = new System.Drawing.Point(13, 24);
            gbNewCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbNewCohort.Name = "gbNewCohort";
            gbNewCohort.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbNewCohort.Size = new System.Drawing.Size(1005, 54);
            gbNewCohort.TabIndex = 0;
            gbNewCohort.TabStop = false;
            gbNewCohort.Text = "New Cohort";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(7, 25);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(42, 15);
            label7.TabIndex = 0;
            label7.Text = "Name:";
            // 
            // tbName
            // 
            tbName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(58, 22);
            tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(939, 23);
            tbName.TabIndex = 1;
            tbName.TextChanged += tbName_TextChanged;
            // 
            // btnNewProject
            // 
            btnNewProject.Location = new System.Drawing.Point(373, 50);
            btnNewProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnNewProject.Name = "btnNewProject";
            btnNewProject.Size = new System.Drawing.Size(54, 25);
            btnNewProject.TabIndex = 5;
            btnNewProject.Text = "New...";
            btnNewProject.UseVisualStyleBackColor = true;
            btnNewProject.Click += btnNewProject_Click;
            // 
            // gbChooseCohortType
            // 
            gbChooseCohortType.Controls.Add(rbRevisedCohort);
            gbChooseCohortType.Controls.Add(label4);
            gbChooseCohortType.Controls.Add(rbNewCohort);
            gbChooseCohortType.Dock = System.Windows.Forms.DockStyle.Top;
            gbChooseCohortType.Location = new System.Drawing.Point(0, 139);
            gbChooseCohortType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbChooseCohortType.Name = "gbChooseCohortType";
            gbChooseCohortType.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbChooseCohortType.Size = new System.Drawing.Size(1048, 48);
            gbChooseCohortType.TabIndex = 9;
            gbChooseCohortType.TabStop = false;
            gbChooseCohortType.Text = "2. Choose Cohort Type";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(gbNewCohort);
            groupBox3.Controls.Add(gbRevisedCohort);
            groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox3.Location = new System.Drawing.Point(0, 187);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(1048, 178);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "3. Configure Cohort (doesn't exist yet, next screen will actually create it)";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(7, 24);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(64, 15);
            label9.TabIndex = 2;
            label9.Text = "Comment:";
            // 
            // tbDescription
            // 
            tbDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbDescription.Location = new System.Drawing.Point(71, 24);
            tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(969, 67);
            tbDescription.TabIndex = 3;
            // 
            // lblProject
            // 
            lblProject.AutoSize = true;
            lblProject.Location = new System.Drawing.Point(163, 55);
            lblProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblProject.Name = "lblProject";
            lblProject.Size = new System.Drawing.Size(30, 15);
            lblProject.TabIndex = 4;
            lblProject.Text = "blah";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(87, 55);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(47, 15);
            label11.TabIndex = 3;
            label11.Text = "Project:";
            // 
            // btnClearProject
            // 
            btnClearProject.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnClearProject.Location = new System.Drawing.Point(528, 7);
            btnClearProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClearProject.Name = "btnClearProject";
            btnClearProject.Size = new System.Drawing.Size(141, 27);
            btnClearProject.TabIndex = 14;
            btnClearProject.Text = "Cancel";
            btnClearProject.UseVisualStyleBackColor = true;
            btnClearProject.Click += btnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOk.Location = new System.Drawing.Point(372, 7);
            btnOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(149, 27);
            btnOk.TabIndex = 13;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // ragSmiley1
            // 
            ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(335, 4);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(30, 30);
            ragSmiley1.TabIndex = 12;
            // 
            // gbDescription
            // 
            gbDescription.Controls.Add(label9);
            gbDescription.Controls.Add(tbDescription);
            gbDescription.Dock = System.Windows.Forms.DockStyle.Top;
            gbDescription.Location = new System.Drawing.Point(0, 365);
            gbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbDescription.Name = "gbDescription";
            gbDescription.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbDescription.Size = new System.Drawing.Size(1048, 99);
            gbDescription.TabIndex = 11;
            gbDescription.TabStop = false;
            gbDescription.Text = "4. Enter Description Of Cohort";
            // 
            // pbProject
            // 
            pbProject.Location = new System.Drawing.Point(135, 51);
            pbProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbProject.Name = "pbProject";
            pbProject.Size = new System.Drawing.Size(30, 30);
            pbProject.TabIndex = 17;
            pbProject.TabStop = false;
            // 
            // pbCohortSource
            // 
            pbCohortSource.Location = new System.Drawing.Point(135, 16);
            pbCohortSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbCohortSource.Name = "pbCohortSource";
            pbCohortSource.Size = new System.Drawing.Size(30, 30);
            pbCohortSource.TabIndex = 18;
            pbCohortSource.TabStop = false;
            // 
            // btnExisting
            // 
            btnExisting.Location = new System.Drawing.Point(433, 50);
            btnExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnExisting.Name = "btnExisting";
            btnExisting.Size = new System.Drawing.Size(78, 25);
            btnExisting.TabIndex = 6;
            btnExisting.Text = "Existing...";
            btnExisting.UseVisualStyleBackColor = true;
            btnExisting.Click += btnExisting_Click;
            // 
            // lblErrorNoProjectNumber
            // 
            lblErrorNoProjectNumber.AutoSize = true;
            lblErrorNoProjectNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
            lblErrorNoProjectNumber.ForeColor = System.Drawing.Color.Red;
            lblErrorNoProjectNumber.Location = new System.Drawing.Point(135, 90);
            lblErrorNoProjectNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblErrorNoProjectNumber.Name = "lblErrorNoProjectNumber";
            lblErrorNoProjectNumber.Size = new System.Drawing.Size(244, 13);
            lblErrorNoProjectNumber.TabIndex = 7;
            lblErrorNoProjectNumber.Text = "Selected Project does not have a Project Number:";
            lblErrorNoProjectNumber.Visible = false;
            // 
            // tbSetProjectNumber
            // 
            tbSetProjectNumber.Location = new System.Drawing.Point(387, 86);
            tbSetProjectNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbSetProjectNumber.Name = "tbSetProjectNumber";
            tbSetProjectNumber.Size = new System.Drawing.Size(138, 23);
            tbSetProjectNumber.TabIndex = 8;
            tbSetProjectNumber.Visible = false;
            tbSetProjectNumber.TextChanged += tbSetProjectNumber_TextChanged;
            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(518, 50);
            btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(50, 25);
            btnClear.TabIndex = 6;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // taskDescriptionLabel1
            // 
            taskDescriptionLabel1.AutoSize = true;
            taskDescriptionLabel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
            taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            taskDescriptionLabel1.Size = new System.Drawing.Size(1048, 42);
            taskDescriptionLabel1.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(gbDescription);
            panel1.Controls.Add(groupBox3);
            panel1.Controls.Add(gbChooseCohortType);
            panel1.Controls.Add(groupBox1);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 42);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1048, 509);
            panel1.TabIndex = 20;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnOk);
            panel2.Controls.Add(ragSmiley1);
            panel2.Controls.Add(btnClearProject);
            panel2.Dock = System.Windows.Forms.DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 464);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1048, 55);
            panel2.TabIndex = 20;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cbCopyProjectSpecificCatalogues);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(pbProject);
            groupBox1.Controls.Add(btnNewProject);
            groupBox1.Controls.Add(btnClear);
            groupBox1.Controls.Add(pbCohortSource);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(tbSetProjectNumber);
            groupBox1.Controls.Add(btnExisting);
            groupBox1.Controls.Add(lblExternalCohortTable);
            groupBox1.Controls.Add(lblProject);
            groupBox1.Controls.Add(lblErrorNoProjectNumber);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(1048, 139);
            groupBox1.TabIndex = 19;
            groupBox1.TabStop = false;
            groupBox1.Text = "1. Project and Destination";
            // 
            // cbCopyProjectSpecificCatalogues
            // 
            cbCopyProjectSpecificCatalogues.AutoSize = true;
            cbCopyProjectSpecificCatalogues.Location = new System.Drawing.Point(135, 114);
            cbCopyProjectSpecificCatalogues.Name = "cbCopyProjectSpecificCatalogues";
            cbCopyProjectSpecificCatalogues.Size = new System.Drawing.Size(330, 19);
            cbCopyProjectSpecificCatalogues.TabIndex = 19;
            cbCopyProjectSpecificCatalogues.Text = "Copy any Project Specific Catalogues Required for Cohort";
            cbCopyProjectSpecificCatalogues.UseVisualStyleBackColor = true;
            cbCopyProjectSpecificCatalogues.Visible = false;
            // 
            // CohortCreationRequestUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1048, 551);
            Controls.Add(panel1);
            Controls.Add(taskDescriptionLabel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CohortCreationRequestUI";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Create Cohort";
            Load += CohortCreationRequestUI_Load;
            gbRevisedCohort.ResumeLayout(false);
            gbRevisedCohort.PerformLayout();
            gbNewCohort.ResumeLayout(false);
            gbNewCohort.PerformLayout();
            gbChooseCohortType.ResumeLayout(false);
            gbChooseCohortType.PerformLayout();
            groupBox3.ResumeLayout(false);
            gbDescription.ResumeLayout(false);
            gbDescription.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbProject).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbCohortSource).EndInit();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
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
        private SimpleDialogs.TaskDescriptionLabel taskDescriptionLabel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbCopyProjectSpecificCatalogues;
    }
}