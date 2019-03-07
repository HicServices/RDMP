namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class GenerateTestDataUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerateTestDataUI));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pOutputDirectory = new System.Windows.Forms.Panel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblDirectory = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ragSmileyDirectory = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.gbDemography = new System.Windows.Forms.GroupBox();
            this.sizeDemography = new CatalogueManager.SimpleDialogs.Reports.TestDataGenerator();
            this.gbPrescribing = new System.Windows.Forms.GroupBox();
            this.sizePrescribing = new CatalogueManager.SimpleDialogs.Reports.TestDataGenerator();
            this.gbBiochemistry = new System.Windows.Forms.GroupBox();
            this.sizeBiochemistry = new CatalogueManager.SimpleDialogs.Reports.TestDataGenerator();
            this.pPopulationSize = new System.Windows.Forms.Panel();
            this.tbPopulationSize = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.ragSmileyPopulation = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.groupBox1.SuspendLayout();
            this.pOutputDirectory.SuspendLayout();
            this.gbDemography.SuspendLayout();
            this.gbPrescribing.SuspendLayout();
            this.gbBiochemistry.SuspendLayout();
            this.pPopulationSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.helpIcon1);
            this.groupBox1.Controls.Add(this.pOutputDirectory);
            this.groupBox1.Controls.Add(this.gbDemography);
            this.groupBox1.Controls.Add(this.gbPrescribing);
            this.groupBox1.Controls.Add(this.gbBiochemistry);
            this.groupBox1.Controls.Add(this.pPopulationSize);
            this.groupBox1.Controls.Add(this.btnGenerate);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(863, 500);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Generate Test Data Files";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // pOutputDirectory
            // 
            this.pOutputDirectory.Controls.Add(this.btnBrowse);
            this.pOutputDirectory.Controls.Add(this.lblDirectory);
            this.pOutputDirectory.Controls.Add(this.label5);
            this.pOutputDirectory.Controls.Add(this.ragSmileyDirectory);
            this.pOutputDirectory.Location = new System.Drawing.Point(10, 432);
            this.pOutputDirectory.Name = "pOutputDirectory";
            this.pOutputDirectory.Size = new System.Drawing.Size(847, 32);
            this.pOutputDirectory.TabIndex = 4;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(159, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 10;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblDirectory
            // 
            this.lblDirectory.AutoSize = true;
            this.lblDirectory.Location = new System.Drawing.Point(136, 8);
            this.lblDirectory.Name = "lblDirectory";
            this.lblDirectory.Size = new System.Drawing.Size(59, 13);
            this.lblDirectory.TabIndex = 9;
            this.lblDirectory.Text = "lblDirectory";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Exercise Directory";
            // 
            // ragSmileyDirectory
            // 
            this.ragSmileyDirectory.AlwaysShowHandCursor = false;
            this.ragSmileyDirectory.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyDirectory.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyDirectory.Location = new System.Drawing.Point(255, 1);
            this.ragSmileyDirectory.Name = "ragSmileyDirectory";
            this.ragSmileyDirectory.Size = new System.Drawing.Size(26, 26);
            this.ragSmileyDirectory.TabIndex = 8;
            // 
            // gbDemography
            // 
            this.gbDemography.Controls.Add(this.sizeDemography);
            this.gbDemography.Location = new System.Drawing.Point(10, 267);
            this.gbDemography.Name = "gbDemography";
            this.gbDemography.Size = new System.Drawing.Size(833, 120);
            this.gbDemography.TabIndex = 2;
            this.gbDemography.TabStop = false;
            this.gbDemography.Text = "Demography";
            // 
            // sizeDemography
            // 
            this.sizeDemography.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sizeDemography.Generator = null;
            this.sizeDemography.Location = new System.Drawing.Point(3, 16);
            this.sizeDemography.Name = "sizeDemography";
            this.sizeDemography.Size = new System.Drawing.Size(827, 101);
            this.sizeDemography.TabIndex = 2;
            // 
            // gbPrescribing
            // 
            this.gbPrescribing.Controls.Add(this.sizePrescribing);
            this.gbPrescribing.Location = new System.Drawing.Point(10, 143);
            this.gbPrescribing.Name = "gbPrescribing";
            this.gbPrescribing.Size = new System.Drawing.Size(833, 118);
            this.gbPrescribing.TabIndex = 1;
            this.gbPrescribing.TabStop = false;
            this.gbPrescribing.Text = "Prescribing";
            // 
            // sizePrescribing
            // 
            this.sizePrescribing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sizePrescribing.Generator = null;
            this.sizePrescribing.Location = new System.Drawing.Point(3, 16);
            this.sizePrescribing.Name = "sizePrescribing";
            this.sizePrescribing.Size = new System.Drawing.Size(827, 99);
            this.sizePrescribing.TabIndex = 1;
            // 
            // gbBiochemistry
            // 
            this.gbBiochemistry.Controls.Add(this.sizeBiochemistry);
            this.gbBiochemistry.Location = new System.Drawing.Point(10, 19);
            this.gbBiochemistry.Name = "gbBiochemistry";
            this.gbBiochemistry.Size = new System.Drawing.Size(833, 118);
            this.gbBiochemistry.TabIndex = 0;
            this.gbBiochemistry.TabStop = false;
            this.gbBiochemistry.Text = "Biochemistry";
            // 
            // sizeBiochemistry
            // 
            this.sizeBiochemistry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sizeBiochemistry.Generator = null;
            this.sizeBiochemistry.Location = new System.Drawing.Point(3, 16);
            this.sizeBiochemistry.Name = "sizeBiochemistry";
            this.sizeBiochemistry.Size = new System.Drawing.Size(827, 99);
            this.sizeBiochemistry.TabIndex = 0;
            // 
            // pPopulationSize
            // 
            this.pPopulationSize.Controls.Add(this.tbPopulationSize);
            this.pPopulationSize.Controls.Add(this.label7);
            this.pPopulationSize.Controls.Add(this.ragSmileyPopulation);
            this.pPopulationSize.Location = new System.Drawing.Point(10, 393);
            this.pPopulationSize.Name = "pPopulationSize";
            this.pPopulationSize.Size = new System.Drawing.Size(269, 33);
            this.pPopulationSize.TabIndex = 3;
            // 
            // tbPopulationSize
            // 
            this.tbPopulationSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbPopulationSize.Location = new System.Drawing.Point(88, 6);
            this.tbPopulationSize.Name = "tbPopulationSize";
            this.tbPopulationSize.Size = new System.Drawing.Size(137, 20);
            this.tbPopulationSize.TabIndex = 3;
            this.tbPopulationSize.TextChanged += new System.EventHandler(this.tbPopulationSize_TextChanged);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "PopulationSize";
            // 
            // ragSmileyPopulation
            // 
            this.ragSmileyPopulation.AlwaysShowHandCursor = false;
            this.ragSmileyPopulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ragSmileyPopulation.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyPopulation.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyPopulation.Location = new System.Drawing.Point(231, 3);
            this.ragSmileyPopulation.Name = "ragSmileyPopulation";
            this.ragSmileyPopulation.Size = new System.Drawing.Size(26, 26);
            this.ragSmileyPopulation.TabIndex = 7;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenerate.Location = new System.Drawing.Point(9, 472);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(131, 23);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Generate Test Data";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(838, 8);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 6;
            // 
            // GenerateTestDataUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 521);
            this.Controls.Add(this.groupBox1);
            this.Name = "GenerateTestDataUI";
            this.Text = "GenerateTestDataUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserExercisesUI_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.pOutputDirectory.ResumeLayout(false);
            this.pOutputDirectory.PerformLayout();
            this.gbDemography.ResumeLayout(false);
            this.gbPrescribing.ResumeLayout(false);
            this.gbBiochemistry.ResumeLayout(false);
            this.pPopulationSize.ResumeLayout(false);
            this.pPopulationSize.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private TestDataGenerator sizePrescribing;
        private TestDataGenerator sizeBiochemistry;
        private TestDataGenerator sizeDemography;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbPopulationSize;
        private System.Windows.Forms.Label label7;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyPopulation;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyDirectory;
        private System.Windows.Forms.Panel pPopulationSize;
        private System.Windows.Forms.GroupBox gbBiochemistry;
        private System.Windows.Forms.GroupBox gbPrescribing;
        private System.Windows.Forms.GroupBox gbDemography;
        private System.Windows.Forms.Panel pOutputDirectory;
        private System.Windows.Forms.Label lblDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private ReusableUIComponents.HelpIcon helpIcon1;
    }
}