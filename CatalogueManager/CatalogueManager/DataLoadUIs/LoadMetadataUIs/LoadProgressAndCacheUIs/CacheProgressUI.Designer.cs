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
            this.gbCacheProgress = new System.Windows.Forms.GroupBox();
            this.pPipeline = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
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
            this.tbCacheProgressID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.ragSmiley1 = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.gbCacheProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // gbCacheProgress
            // 
            this.gbCacheProgress.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gbCacheProgress.Controls.Add(this.ragSmiley1);
            this.gbCacheProgress.Controls.Add(this.pPipeline);
            this.gbCacheProgress.Controls.Add(this.btnRefresh);
            this.gbCacheProgress.Controls.Add(this.btnEdit);
            this.gbCacheProgress.Controls.Add(this.ddCacheLagDelayDurationType);
            this.gbCacheProgress.Controls.Add(this.udCacheLagDelayPeriodDuration);
            this.gbCacheProgress.Controls.Add(this.label2);
            this.gbCacheProgress.Controls.Add(this.label1);
            this.gbCacheProgress.Controls.Add(this.tbCacheProgress);
            this.gbCacheProgress.Controls.Add(this.label4);
            this.gbCacheProgress.Controls.Add(this.ddCacheLagDurationType);
            this.gbCacheProgress.Controls.Add(this.udCacheLagDuration);
            this.gbCacheProgress.Controls.Add(this.label8);
            this.gbCacheProgress.Controls.Add(this.label13);
            this.gbCacheProgress.Controls.Add(this.tbChunkPeriod);
            this.gbCacheProgress.Controls.Add(this.tbCacheProgressID);
            this.gbCacheProgress.Controls.Add(this.label9);
            this.gbCacheProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCacheProgress.Location = new System.Drawing.Point(0, 0);
            this.gbCacheProgress.Name = "gbCacheProgress";
            this.gbCacheProgress.Size = new System.Drawing.Size(878, 334);
            this.gbCacheProgress.TabIndex = 32;
            this.gbCacheProgress.TabStop = false;
            this.gbCacheProgress.Text = "Cache Progress:";
            // 
            // pPipeline
            // 
            this.pPipeline.Location = new System.Drawing.Point(95, 159);
            this.pPipeline.Name = "pPipeline";
            this.pPipeline.Size = new System.Drawing.Size(777, 169);
            this.pPipeline.TabIndex = 46;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.Location = new System.Drawing.Point(396, 17);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(26, 26);
            this.btnRefresh.TabIndex = 45;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(260, 41);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(35, 20);
            this.btnEdit.TabIndex = 45;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // ddCacheLagDelayDurationType
            // 
            this.ddCacheLagDelayDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDelayDurationType.FormattingEnabled = true;
            this.ddCacheLagDelayDurationType.Location = new System.Drawing.Point(340, 89);
            this.ddCacheLagDelayDurationType.Name = "ddCacheLagDelayDurationType";
            this.ddCacheLagDelayDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDelayDurationType.TabIndex = 6;
            this.ddCacheLagDelayDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDelayDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDelayPeriodDuration
            // 
            this.udCacheLagDelayPeriodDuration.Location = new System.Drawing.Point(296, 91);
            this.udCacheLagDelayPeriodDuration.Name = "udCacheLagDelayPeriodDuration";
            this.udCacheLagDelayPeriodDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDelayPeriodDuration.TabIndex = 5;
            this.udCacheLagDelayPeriodDuration.ValueChanged += new System.EventHandler(this.udCacheLagDelayPeriodDuration_ValueChanged);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(663, 42);
            this.label2.TabIndex = 43;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Load Delay";
            // 
            // tbCacheProgress
            // 
            this.tbCacheProgress.Location = new System.Drawing.Point(260, 19);
            this.tbCacheProgress.Name = "tbCacheProgress";
            this.tbCacheProgress.ReadOnly = true;
            this.tbCacheProgress.Size = new System.Drawing.Size(130, 20);
            this.tbCacheProgress.TabIndex = 2;
            this.tbCacheProgress.TextChanged += new System.EventHandler(this.tbCacheProgress_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(169, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Cache Progress:";
            // 
            // ddCacheLagDurationType
            // 
            this.ddCacheLagDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDurationType.FormattingEnabled = true;
            this.ddCacheLagDurationType.Location = new System.Drawing.Point(152, 90);
            this.ddCacheLagDurationType.Name = "ddCacheLagDurationType";
            this.ddCacheLagDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDurationType.TabIndex = 4;
            this.ddCacheLagDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDuration
            // 
            this.udCacheLagDuration.Location = new System.Drawing.Point(108, 90);
            this.udCacheLagDuration.Name = "udCacheLagDuration";
            this.udCacheLagDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDuration.TabIndex = 3;
            this.udCacheLagDuration.ValueChanged += new System.EventHandler(this.udCacheLagDuration_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(417, 94);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 30;
            this.label8.Text = "Chunk Period:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 94);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "Cache Lag Period:";
            // 
            // tbChunkPeriod
            // 
            this.tbChunkPeriod.Location = new System.Drawing.Point(497, 91);
            this.tbChunkPeriod.Name = "tbChunkPeriod";
            this.tbChunkPeriod.Size = new System.Drawing.Size(120, 20);
            this.tbChunkPeriod.TabIndex = 7;
            this.tbChunkPeriod.TextChanged += new System.EventHandler(this.tbChunkPeriod_TextChanged);
            // 
            // tbCacheProgressID
            // 
            this.tbCacheProgressID.Location = new System.Drawing.Point(34, 19);
            this.tbCacheProgressID.Name = "tbCacheProgressID";
            this.tbCacheProgressID.ReadOnly = true;
            this.tbCacheProgressID.Size = new System.Drawing.Size(130, 20);
            this.tbCacheProgressID.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "ID:";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(65, 159);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 47;
            // 
            // CacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbCacheProgress);
            this.Name = "CacheProgressUI";
            this.Size = new System.Drawing.Size(878, 334);
            this.gbCacheProgress.ResumeLayout(false);
            this.gbCacheProgress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbCacheProgress;
        private System.Windows.Forms.TextBox tbCacheProgress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddCacheLagDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDuration;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbChunkPeriod;
        private System.Windows.Forms.TextBox tbCacheProgressID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox ddCacheLagDelayDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDelayPeriodDuration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Panel pPipeline;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley1;
    }
}
