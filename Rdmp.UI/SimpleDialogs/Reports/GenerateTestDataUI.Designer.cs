namespace Rdmp.UI.SimpleDialogs.Reports
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
            this.pOutputDirectory = new System.Windows.Forms.Panel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblDirectory = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ragSmileyDirectory = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.pPopulationSize = new System.Windows.Forms.Panel();
            this.tbPopulationSize = new System.Windows.Forms.TextBox();
            this.cbLookups = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.ragSmileyPopulation = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pDatasets = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSeed = new System.Windows.Forms.TextBox();
            this.pOutputDirectory.SuspendLayout();
            this.pPopulationSize.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pOutputDirectory
            // 
            this.pOutputDirectory.Controls.Add(this.btnBrowse);
            this.pOutputDirectory.Controls.Add(this.lblDirectory);
            this.pOutputDirectory.Controls.Add(this.label5);
            this.pOutputDirectory.Controls.Add(this.ragSmileyDirectory);
            this.pOutputDirectory.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pOutputDirectory.Location = new System.Drawing.Point(0, 456);
            this.pOutputDirectory.Name = "pOutputDirectory";
            this.pOutputDirectory.Size = new System.Drawing.Size(887, 32);
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
            // pPopulationSize
            // 
            this.pPopulationSize.Controls.Add(this.tbSeed);
            this.pPopulationSize.Controls.Add(this.label1);
            this.pPopulationSize.Controls.Add(this.tbPopulationSize);
            this.pPopulationSize.Controls.Add(this.cbLookups);
            this.pPopulationSize.Controls.Add(this.label7);
            this.pPopulationSize.Controls.Add(this.ragSmileyPopulation);
            this.pPopulationSize.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pPopulationSize.Location = new System.Drawing.Point(0, 423);
            this.pPopulationSize.Name = "pPopulationSize";
            this.pPopulationSize.Size = new System.Drawing.Size(887, 33);
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
            // cbLookups
            // 
            this.cbLookups.AutoSize = true;
            this.cbLookups.Location = new System.Drawing.Point(263, 8);
            this.cbLookups.Name = "cbLookups";
            this.cbLookups.Size = new System.Drawing.Size(114, 17);
            this.cbLookups.TabIndex = 7;
            this.cbLookups.Text = "Generate Lookups";
            this.cbLookups.UseVisualStyleBackColor = true;
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
            this.btnGenerate.Location = new System.Drawing.Point(3, 6);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(131, 23);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Generate Test Data";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(865, 5);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.SuppressClick = false;
            this.helpIcon1.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnGenerate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 488);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(887, 33);
            this.panel1.TabIndex = 9;
            // 
            // pDatasets
            // 
            this.pDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pDatasets.AutoScroll = true;
            this.pDatasets.Location = new System.Drawing.Point(2, 4);
            this.pDatasets.Name = "pDatasets";
            this.pDatasets.Size = new System.Drawing.Size(882, 416);
            this.pDatasets.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pDatasets);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(887, 423);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(383, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Seed:";
            // 
            // tbSeed
            // 
            this.tbSeed.Location = new System.Drawing.Point(424, 7);
            this.tbSeed.Name = "tbSeed";
            this.tbSeed.Size = new System.Drawing.Size(100, 20);
            this.tbSeed.TabIndex = 9;
            this.tbSeed.TextChanged += new System.EventHandler(this.TbSeed_TextChanged);
            // 
            // GenerateTestDataUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 521);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pPopulationSize);
            this.Controls.Add(this.pOutputDirectory);
            this.Controls.Add(this.panel1);
            this.Name = "GenerateTestDataUI";
            this.Text = "Generate Test Data";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserExercisesUI_FormClosing);
            this.pOutputDirectory.ResumeLayout(false);
            this.pOutputDirectory.PerformLayout();
            this.pPopulationSize.ResumeLayout(false);
            this.pPopulationSize.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbPopulationSize;
        private System.Windows.Forms.Label label7;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyPopulation;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyDirectory;
        private System.Windows.Forms.Panel pPopulationSize;
        private System.Windows.Forms.Panel pOutputDirectory;
        private System.Windows.Forms.Label lblDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private ReusableUIComponents.HelpIcon helpIcon1;
        private System.Windows.Forms.CheckBox cbLookups;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pDatasets;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox tbSeed;
        private System.Windows.Forms.Label label1;
    }
}