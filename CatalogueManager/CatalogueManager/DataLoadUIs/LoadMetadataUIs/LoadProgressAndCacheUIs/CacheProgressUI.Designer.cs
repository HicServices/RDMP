namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    partial class CacheProgressUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CacheProgressUI));
            this.pPipeline = new System.Windows.Forms.Panel();
            this.btnEdit = new System.Windows.Forms.Button();
            this.ddCacheLagDelayDurationType = new System.Windows.Forms.ComboBox();
            this.udCacheLagDelayPeriodDuration = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbCacheProgress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ddCacheLagDurationType = new System.Windows.Forms.ComboBox();
            this.udCacheLagDuration = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbChunkPeriod = new System.Windows.Forms.TextBox();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.hiChunkPeriod = new ReusableUIComponents.HelpIcon();
            this.hiPipeline = new ReusableUIComponents.HelpIcon();
            this.hiCacheLagDelayPeriod = new ReusableUIComponents.HelpIcon();
            this.hiLagDuration = new ReusableUIComponents.HelpIcon();
            this.hiProgress = new ReusableUIComponents.HelpIcon();
            this.ragSmiley1 = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pPipeline
            // 
            this.pPipeline.Location = new System.Drawing.Point(42, 199);
            this.pPipeline.Name = "pPipeline";
            this.pPipeline.Size = new System.Drawing.Size(777, 169);
            this.pPipeline.TabIndex = 11;
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(216, 52);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(35, 20);
            this.btnEdit.TabIndex = 3;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // ddCacheLagDelayDurationType
            // 
            this.ddCacheLagDelayDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDelayDurationType.FormattingEnabled = true;
            this.ddCacheLagDelayDurationType.Location = new System.Drawing.Point(129, 104);
            this.ddCacheLagDelayDurationType.Name = "ddCacheLagDelayDurationType";
            this.ddCacheLagDelayDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDelayDurationType.TabIndex = 7;
            this.ddCacheLagDelayDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDelayDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDelayPeriodDuration
            // 
            this.udCacheLagDelayPeriodDuration.Location = new System.Drawing.Point(83, 105);
            this.udCacheLagDelayPeriodDuration.Name = "udCacheLagDelayPeriodDuration";
            this.udCacheLagDelayPeriodDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDelayPeriodDuration.TabIndex = 6;
            this.udCacheLagDelayPeriodDuration.ValueChanged += new System.EventHandler(this.udCacheLagDelayPeriodDuration_ValueChanged);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 154);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(663, 42);
            this.label2.TabIndex = 9;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Load Delay:";
            // 
            // tbCacheProgress
            // 
            this.tbCacheProgress.Location = new System.Drawing.Point(83, 52);
            this.tbCacheProgress.Name = "tbCacheProgress";
            this.tbCacheProgress.ReadOnly = true;
            this.tbCacheProgress.Size = new System.Drawing.Size(130, 20);
            this.tbCacheProgress.TabIndex = 2;
            this.tbCacheProgress.TextChanged += new System.EventHandler(this.tbCacheProgress_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Progress:";
            // 
            // ddCacheLagDurationType
            // 
            this.ddCacheLagDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDurationType.FormattingEnabled = true;
            this.ddCacheLagDurationType.Location = new System.Drawing.Point(129, 78);
            this.ddCacheLagDurationType.Name = "ddCacheLagDurationType";
            this.ddCacheLagDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDurationType.TabIndex = 5;
            this.ddCacheLagDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDuration
            // 
            this.udCacheLagDuration.Location = new System.Drawing.Point(83, 79);
            this.udCacheLagDuration.Name = "udCacheLagDuration";
            this.udCacheLagDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDuration.TabIndex = 4;
            this.udCacheLagDuration.ValueChanged += new System.EventHandler(this.udCacheLagDuration_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 30;
            this.label8.Text = "Chunk Period:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 81);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(61, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "Lag Period:";
            // 
            // tbChunkPeriod
            // 
            this.tbChunkPeriod.Location = new System.Drawing.Point(82, 131);
            this.tbChunkPeriod.Name = "tbChunkPeriod";
            this.tbChunkPeriod.Size = new System.Drawing.Size(120, 20);
            this.tbChunkPeriod.TabIndex = 8;
            this.tbChunkPeriod.TextChanged += new System.EventHandler(this.tbChunkPeriod_TextChanged);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(83, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(130, 20);
            this.tbID.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(56, 6);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "ID:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.hiChunkPeriod);
            this.panel1.Controls.Add(this.hiPipeline);
            this.panel1.Controls.Add(this.hiCacheLagDelayPeriod);
            this.panel1.Controls.Add(this.hiLagDuration);
            this.panel1.Controls.Add(this.hiProgress);
            this.panel1.Controls.Add(this.pPipeline);
            this.panel1.Controls.Add(this.ragSmiley1);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.ddCacheLagDelayDurationType);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.udCacheLagDelayPeriodDuration);
            this.panel1.Controls.Add(this.tbChunkPeriod);
            this.panel1.Controls.Add(this.btnEdit);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.ddCacheLagDurationType);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.udCacheLagDuration);
            this.panel1.Controls.Add(this.tbCacheProgress);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(878, 375);
            this.panel1.TabIndex = 33;
            // 
            // hiChunkPeriod
            // 
            this.hiChunkPeriod.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiChunkPeriod.BackgroundImage")));
            this.hiChunkPeriod.Location = new System.Drawing.Point(206, 130);
            this.hiChunkPeriod.Name = "hiChunkPeriod";
            this.hiChunkPeriod.Size = new System.Drawing.Size(19, 19);
            this.hiChunkPeriod.TabIndex = 48;
            // 
            // hiPipeline
            // 
            this.hiPipeline.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiPipeline.BackgroundImage")));
            this.hiPipeline.Location = new System.Drawing.Point(825, 199);
            this.hiPipeline.Name = "hiPipeline";
            this.hiPipeline.Size = new System.Drawing.Size(19, 19);
            this.hiPipeline.TabIndex = 47;
            // 
            // hiCacheLagDelayPeriod
            // 
            this.hiCacheLagDelayPeriod.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiCacheLagDelayPeriod.BackgroundImage")));
            this.hiCacheLagDelayPeriod.Location = new System.Drawing.Point(206, 105);
            this.hiCacheLagDelayPeriod.Name = "hiCacheLagDelayPeriod";
            this.hiCacheLagDelayPeriod.Size = new System.Drawing.Size(19, 19);
            this.hiCacheLagDelayPeriod.TabIndex = 46;
            // 
            // hiLagDuration
            // 
            this.hiLagDuration.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiLagDuration.BackgroundImage")));
            this.hiLagDuration.Location = new System.Drawing.Point(206, 79);
            this.hiLagDuration.Name = "hiLagDuration";
            this.hiLagDuration.Size = new System.Drawing.Size(19, 19);
            this.hiLagDuration.TabIndex = 45;
            // 
            // hiProgress
            // 
            this.hiProgress.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hiProgress.BackgroundImage")));
            this.hiProgress.Location = new System.Drawing.Point(254, 53);
            this.hiProgress.Name = "hiProgress";
            this.hiProgress.Size = new System.Drawing.Size(19, 19);
            this.hiProgress.TabIndex = 44;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(6, 199);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 10;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(83, 29);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(296, 20);
            this.tbName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Name:";
            // 
            // CacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "CacheProgressUI";
            this.Size = new System.Drawing.Size(878, 375);
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbCacheProgress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddCacheLagDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDuration;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbChunkPeriod;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox ddCacheLagDelayDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDelayPeriodDuration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Panel pPipeline;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label3;
        private ReusableUIComponents.HelpIcon hiCacheLagDelayPeriod;
        private ReusableUIComponents.HelpIcon hiLagDuration;
        private ReusableUIComponents.HelpIcon hiProgress;
        private ReusableUIComponents.HelpIcon hiPipeline;
        private ReusableUIComponents.HelpIcon hiChunkPeriod;
    }
}
