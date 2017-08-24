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
            this.btnDeleteCaching = new System.Windows.Forms.Button();
            this.btnDeletePermissionWindow = new System.Windows.Forms.Button();
            this.ddCacheLagDelayDurationType = new System.Windows.Forms.ComboBox();
            this.udCacheLagDelayPeriodDuration = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbPermissionWindowRequired = new System.Windows.Forms.CheckBox();
            this.btnEditPermissionWindow = new System.Windows.Forms.Button();
            this.btnAddNewPermissionWindow = new System.Windows.Forms.Button();
            this.ddPermissionWindow = new System.Windows.Forms.ComboBox();
            this.tbCacheProgress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnConfigureCachingPipeline = new System.Windows.Forms.Button();
            this.ddCacheLagDurationType = new System.Windows.Forms.ComboBox();
            this.udCacheLagDuration = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbChunkPeriod = new System.Windows.Forms.TextBox();
            this.tbCacheProgressID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.gbCacheProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // gbCacheProgress
            // 
            this.gbCacheProgress.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gbCacheProgress.Controls.Add(this.btnDeleteCaching);
            this.gbCacheProgress.Controls.Add(this.btnDeletePermissionWindow);
            this.gbCacheProgress.Controls.Add(this.ddCacheLagDelayDurationType);
            this.gbCacheProgress.Controls.Add(this.udCacheLagDelayPeriodDuration);
            this.gbCacheProgress.Controls.Add(this.label2);
            this.gbCacheProgress.Controls.Add(this.label1);
            this.gbCacheProgress.Controls.Add(this.cbPermissionWindowRequired);
            this.gbCacheProgress.Controls.Add(this.btnEditPermissionWindow);
            this.gbCacheProgress.Controls.Add(this.btnAddNewPermissionWindow);
            this.gbCacheProgress.Controls.Add(this.ddPermissionWindow);
            this.gbCacheProgress.Controls.Add(this.tbCacheProgress);
            this.gbCacheProgress.Controls.Add(this.label4);
            this.gbCacheProgress.Controls.Add(this.btnConfigureCachingPipeline);
            this.gbCacheProgress.Controls.Add(this.ddCacheLagDurationType);
            this.gbCacheProgress.Controls.Add(this.udCacheLagDuration);
            this.gbCacheProgress.Controls.Add(this.label10);
            this.gbCacheProgress.Controls.Add(this.label8);
            this.gbCacheProgress.Controls.Add(this.label13);
            this.gbCacheProgress.Controls.Add(this.tbChunkPeriod);
            this.gbCacheProgress.Controls.Add(this.tbCacheProgressID);
            this.gbCacheProgress.Controls.Add(this.label9);
            this.gbCacheProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCacheProgress.Location = new System.Drawing.Point(0, 0);
            this.gbCacheProgress.Name = "gbCacheProgress";
            this.gbCacheProgress.Size = new System.Drawing.Size(676, 218);
            this.gbCacheProgress.TabIndex = 32;
            this.gbCacheProgress.TabStop = false;
            this.gbCacheProgress.Text = "Cache Progress:";
            // 
            // btnDeleteCaching
            // 
            this.btnDeleteCaching.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteCaching.Location = new System.Drawing.Point(582, 10);
            this.btnDeleteCaching.Name = "btnDeleteCaching";
            this.btnDeleteCaching.Size = new System.Drawing.Size(91, 23);
            this.btnDeleteCaching.TabIndex = 1;
            this.btnDeleteCaching.Text = "Delete Caching";
            this.btnDeleteCaching.UseVisualStyleBackColor = true;
            this.btnDeleteCaching.Click += new System.EventHandler(this.btnDeleteCaching_Click);
            // 
            // btnDeletePermissionWindow
            // 
            this.btnDeletePermissionWindow.Enabled = false;
            this.btnDeletePermissionWindow.Location = new System.Drawing.Point(287, 192);
            this.btnDeletePermissionWindow.Name = "btnDeletePermissionWindow";
            this.btnDeletePermissionWindow.Size = new System.Drawing.Size(141, 23);
            this.btnDeletePermissionWindow.TabIndex = 15;
            this.btnDeletePermissionWindow.Text = "Delete Permission Window";
            this.btnDeletePermissionWindow.UseVisualStyleBackColor = true;
            this.btnDeletePermissionWindow.Click += new System.EventHandler(this.btnDeletePermissionWindow_Click);
            // 
            // ddCacheLagDelayDurationType
            // 
            this.ddCacheLagDelayDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDelayDurationType.FormattingEnabled = true;
            this.ddCacheLagDelayDurationType.Location = new System.Drawing.Point(340, 43);
            this.ddCacheLagDelayDurationType.Name = "ddCacheLagDelayDurationType";
            this.ddCacheLagDelayDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDelayDurationType.TabIndex = 6;
            this.ddCacheLagDelayDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDelayDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDelayPeriodDuration
            // 
            this.udCacheLagDelayPeriodDuration.Location = new System.Drawing.Point(296, 45);
            this.udCacheLagDelayPeriodDuration.Name = "udCacheLagDelayPeriodDuration";
            this.udCacheLagDelayPeriodDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDelayPeriodDuration.TabIndex = 5;
            this.udCacheLagDelayPeriodDuration.ValueChanged += new System.EventHandler(this.udCacheLagDelayPeriodDuration_ValueChanged);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(663, 42);
            this.label2.TabIndex = 43;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "Load Delay";
            // 
            // cbPermissionWindowRequired
            // 
            this.cbPermissionWindowRequired.AutoSize = true;
            this.cbPermissionWindowRequired.Location = new System.Drawing.Point(10, 142);
            this.cbPermissionWindowRequired.Name = "cbPermissionWindowRequired";
            this.cbPermissionWindowRequired.Size = new System.Drawing.Size(458, 17);
            this.cbPermissionWindowRequired.TabIndex = 11;
            this.cbPermissionWindowRequired.Text = "Use A PermissionWindow (Can only execute caching at certain times of day e.g. 10p" +
    "m-4am)";
            this.cbPermissionWindowRequired.UseVisualStyleBackColor = true;
            this.cbPermissionWindowRequired.CheckedChanged += new System.EventHandler(this.cbPermissionWindowRequired_CheckedChanged);
            // 
            // btnEditPermissionWindow
            // 
            this.btnEditPermissionWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditPermissionWindow.Enabled = false;
            this.btnEditPermissionWindow.Location = new System.Drawing.Point(538, 165);
            this.btnEditPermissionWindow.Name = "btnEditPermissionWindow";
            this.btnEditPermissionWindow.Size = new System.Drawing.Size(91, 23);
            this.btnEditPermissionWindow.TabIndex = 13;
            this.btnEditPermissionWindow.Text = "Edit...";
            this.btnEditPermissionWindow.UseVisualStyleBackColor = true;
            this.btnEditPermissionWindow.Click += new System.EventHandler(this.btnEditPermissionWindow_Click);
            // 
            // btnAddNewPermissionWindow
            // 
            this.btnAddNewPermissionWindow.Enabled = false;
            this.btnAddNewPermissionWindow.Location = new System.Drawing.Point(118, 192);
            this.btnAddNewPermissionWindow.Name = "btnAddNewPermissionWindow";
            this.btnAddNewPermissionWindow.Size = new System.Drawing.Size(163, 23);
            this.btnAddNewPermissionWindow.TabIndex = 14;
            this.btnAddNewPermissionWindow.Text = "Add New Permision Window...";
            this.btnAddNewPermissionWindow.UseVisualStyleBackColor = true;
            this.btnAddNewPermissionWindow.Click += new System.EventHandler(this.btnAddNewPermissionWindow_Click);
            // 
            // ddPermissionWindow
            // 
            this.ddPermissionWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddPermissionWindow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPermissionWindow.Enabled = false;
            this.ddPermissionWindow.FormattingEnabled = true;
            this.ddPermissionWindow.Location = new System.Drawing.Point(118, 165);
            this.ddPermissionWindow.Name = "ddPermissionWindow";
            this.ddPermissionWindow.Size = new System.Drawing.Size(414, 21);
            this.ddPermissionWindow.Sorted = true;
            this.ddPermissionWindow.TabIndex = 12;
            this.ddPermissionWindow.SelectedIndexChanged += new System.EventHandler(this.ddPermissionWindow_SelectedIndexChanged);
            // 
            // tbCacheProgress
            // 
            this.tbCacheProgress.Location = new System.Drawing.Point(260, 19);
            this.tbCacheProgress.Name = "tbCacheProgress";
            this.tbCacheProgress.ReadOnly = true;
            this.tbCacheProgress.Size = new System.Drawing.Size(130, 20);
            this.tbCacheProgress.TabIndex = 2;
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
            // btnConfigureCachingPipeline
            // 
            this.btnConfigureCachingPipeline.Location = new System.Drawing.Point(10, 113);
            this.btnConfigureCachingPipeline.Name = "btnConfigureCachingPipeline";
            this.btnConfigureCachingPipeline.Size = new System.Drawing.Size(156, 23);
            this.btnConfigureCachingPipeline.TabIndex = 10;
            this.btnConfigureCachingPipeline.Text = "Configure Caching Pipeline";
            this.btnConfigureCachingPipeline.UseVisualStyleBackColor = true;
            this.btnConfigureCachingPipeline.Click += new System.EventHandler(this.btnConfigureCachingPipeline_Click);
            // 
            // ddCacheLagDurationType
            // 
            this.ddCacheLagDurationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCacheLagDurationType.FormattingEnabled = true;
            this.ddCacheLagDurationType.Location = new System.Drawing.Point(152, 44);
            this.ddCacheLagDurationType.Name = "ddCacheLagDurationType";
            this.ddCacheLagDurationType.Size = new System.Drawing.Size(71, 21);
            this.ddCacheLagDurationType.TabIndex = 4;
            this.ddCacheLagDurationType.SelectedIndexChanged += new System.EventHandler(this.ddCacheLagDurationType_SelectedIndexChanged);
            // 
            // udCacheLagDuration
            // 
            this.udCacheLagDuration.Location = new System.Drawing.Point(108, 44);
            this.udCacheLagDuration.Name = "udCacheLagDuration";
            this.udCacheLagDuration.Size = new System.Drawing.Size(38, 20);
            this.udCacheLagDuration.TabIndex = 3;
            this.udCacheLagDuration.ValueChanged += new System.EventHandler(this.udCacheLagDuration_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 168);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Permission Window:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(417, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 30;
            this.label8.Text = "Chunk Period:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 48);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "Cache Lag Period:";
            // 
            // tbChunkPeriod
            // 
            this.tbChunkPeriod.Location = new System.Drawing.Point(497, 45);
            this.tbChunkPeriod.Name = "tbChunkPeriod";
            this.tbChunkPeriod.Size = new System.Drawing.Size(120, 20);
            this.tbChunkPeriod.TabIndex = 7;
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
            // CacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbCacheProgress);
            this.Name = "CacheProgressUI";
            this.Size = new System.Drawing.Size(676, 218);
            this.gbCacheProgress.ResumeLayout(false);
            this.gbCacheProgress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDelayPeriodDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCacheLagDuration)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbCacheProgress;
        private System.Windows.Forms.CheckBox cbPermissionWindowRequired;
        private System.Windows.Forms.Button btnEditPermissionWindow;
        private System.Windows.Forms.Button btnAddNewPermissionWindow;
        private System.Windows.Forms.ComboBox ddPermissionWindow;
        private System.Windows.Forms.TextBox tbCacheProgress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConfigureCachingPipeline;
        private System.Windows.Forms.ComboBox ddCacheLagDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDuration;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbChunkPeriod;
        private System.Windows.Forms.TextBox tbCacheProgressID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox ddCacheLagDelayDurationType;
        private System.Windows.Forms.NumericUpDown udCacheLagDelayPeriodDuration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDeletePermissionWindow;
        private System.Windows.Forms.Button btnDeleteCaching;
    }
}
