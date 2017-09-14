namespace CatalogueManager.LoadExecutionUIs.CachingDashboard
{
    partial class CachingDashboardForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CachingDashboardForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblResourceIdentifier = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblLockInfo = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCacheFillProgress = new System.Windows.Forms.Label();
            this.lblCacheLagPeriod = new System.Windows.Forms.Label();
            this.lblChunkPeriod = new System.Windows.Forms.Label();
            this.btnShowPipeline = new System.Windows.Forms.Button();
            this.btnShowPermissionWindow = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.dgvCacheFetchErrors = new System.Windows.Forms.DataGridView();
            this.AttemptedOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Start = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.End = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExceptionText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblShortfall = new System.Windows.Forms.Label();
            this.btnForceUnlock = new System.Windows.Forms.Button();
            this.tvCacheItems = new System.Windows.Forms.TreeView();
            this.imageListForCacheTree = new System.Windows.Forms.ImageList(this.components);
            this.btnRefreshList = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCacheFetchErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(219, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Resource Identifier:";
            // 
            // lblResourceIdentifier
            // 
            this.lblResourceIdentifier.AutoSize = true;
            this.lblResourceIdentifier.Location = new System.Drawing.Point(324, 36);
            this.lblResourceIdentifier.Name = "lblResourceIdentifier";
            this.lblResourceIdentifier.Size = new System.Drawing.Size(58, 13);
            this.lblResourceIdentifier.TabIndex = 3;
            this.lblResourceIdentifier.Text = "<identifier>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(263, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Lock Info:";
            // 
            // lblLockInfo
            // 
            this.lblLockInfo.AutoSize = true;
            this.lblLockInfo.Location = new System.Drawing.Point(324, 146);
            this.lblLockInfo.Name = "lblLockInfo";
            this.lblLockInfo.Size = new System.Drawing.Size(36, 13);
            this.lblLockInfo.TabIndex = 5;
            this.lblLockInfo.Text = "<info>";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(233, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Cache Progress:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(223, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Cache Lag Period:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(244, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Chunk Period:";
            // 
            // lblCacheFillProgress
            // 
            this.lblCacheFillProgress.AutoSize = true;
            this.lblCacheFillProgress.Location = new System.Drawing.Point(324, 57);
            this.lblCacheFillProgress.Name = "lblCacheFillProgress";
            this.lblCacheFillProgress.Size = new System.Drawing.Size(36, 13);
            this.lblCacheFillProgress.TabIndex = 9;
            this.lblCacheFillProgress.Text = "<info>";
            // 
            // lblCacheLagPeriod
            // 
            this.lblCacheLagPeriod.AutoSize = true;
            this.lblCacheLagPeriod.Location = new System.Drawing.Point(324, 101);
            this.lblCacheLagPeriod.Name = "lblCacheLagPeriod";
            this.lblCacheLagPeriod.Size = new System.Drawing.Size(36, 13);
            this.lblCacheLagPeriod.TabIndex = 10;
            this.lblCacheLagPeriod.Text = "<info>";
            // 
            // lblChunkPeriod
            // 
            this.lblChunkPeriod.AutoSize = true;
            this.lblChunkPeriod.Location = new System.Drawing.Point(324, 79);
            this.lblChunkPeriod.Name = "lblChunkPeriod";
            this.lblChunkPeriod.Size = new System.Drawing.Size(36, 13);
            this.lblChunkPeriod.TabIndex = 11;
            this.lblChunkPeriod.Text = "<info>";
            // 
            // btnShowPipeline
            // 
            this.btnShowPipeline.Location = new System.Drawing.Point(222, 202);
            this.btnShowPipeline.Name = "btnShowPipeline";
            this.btnShowPipeline.Size = new System.Drawing.Size(128, 23);
            this.btnShowPipeline.TabIndex = 12;
            this.btnShowPipeline.Text = "Show Pipeline";
            this.btnShowPipeline.UseVisualStyleBackColor = true;
            this.btnShowPipeline.Click += new System.EventHandler(this.btnShowPipeline_Click);
            // 
            // btnShowPermissionWindow
            // 
            this.btnShowPermissionWindow.Location = new System.Drawing.Point(356, 202);
            this.btnShowPermissionWindow.Name = "btnShowPermissionWindow";
            this.btnShowPermissionWindow.Size = new System.Drawing.Size(164, 23);
            this.btnShowPermissionWindow.TabIndex = 13;
            this.btnShowPermissionWindow.Text = "Show Permission Window";
            this.btnShowPermissionWindow.UseVisualStyleBackColor = true;
            this.btnShowPermissionWindow.Click += new System.EventHandler(this.btnShowPermissionWindow_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 536);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(887, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(66, 17);
            this.statusLabel.Text = "statusLabel";
            // 
            // dgvCacheFetchErrors
            // 
            this.dgvCacheFetchErrors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCacheFetchErrors.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvCacheFetchErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCacheFetchErrors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.AttemptedOn,
            this.Start,
            this.End,
            this.ExceptionText});
            this.dgvCacheFetchErrors.Location = new System.Drawing.Point(222, 296);
            this.dgvCacheFetchErrors.Name = "dgvCacheFetchErrors";
            this.dgvCacheFetchErrors.Size = new System.Drawing.Size(653, 221);
            this.dgvCacheFetchErrors.TabIndex = 15;
            this.dgvCacheFetchErrors.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCacheFetchErrors_CellDoubleClick);
            // 
            // AttemptedOn
            // 
            this.AttemptedOn.HeaderText = "AttemptedOn";
            this.AttemptedOn.Name = "AttemptedOn";
            this.AttemptedOn.Width = 94;
            // 
            // Start
            // 
            this.Start.HeaderText = "Start";
            this.Start.Name = "Start";
            this.Start.Width = 54;
            // 
            // End
            // 
            this.End.HeaderText = "End";
            this.End.Name = "End";
            this.End.Width = 51;
            // 
            // ExceptionText
            // 
            this.ExceptionText.HeaderText = "ExceptionText";
            this.ExceptionText.Name = "ExceptionText";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(219, 279);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Fetch Request Errors:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(270, 123);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Shortfall:";
            // 
            // lblShortfall
            // 
            this.lblShortfall.AutoSize = true;
            this.lblShortfall.Location = new System.Drawing.Point(324, 123);
            this.lblShortfall.Name = "lblShortfall";
            this.lblShortfall.Size = new System.Drawing.Size(36, 13);
            this.lblShortfall.TabIndex = 18;
            this.lblShortfall.Text = "<info>";
            // 
            // btnForceUnlock
            // 
            this.btnForceUnlock.Location = new System.Drawing.Point(327, 164);
            this.btnForceUnlock.Name = "btnForceUnlock";
            this.btnForceUnlock.Size = new System.Drawing.Size(94, 23);
            this.btnForceUnlock.TabIndex = 19;
            this.btnForceUnlock.Text = "Force Unlock";
            this.btnForceUnlock.UseVisualStyleBackColor = true;
            this.btnForceUnlock.Click += new System.EventHandler(this.btnForceUnlock_Click);
            // 
            // tvCacheItems
            // 
            this.tvCacheItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvCacheItems.Location = new System.Drawing.Point(13, 27);
            this.tvCacheItems.Name = "tvCacheItems";
            this.tvCacheItems.Size = new System.Drawing.Size(200, 464);
            this.tvCacheItems.TabIndex = 20;
            // 
            // imageListForCacheTree
            // 
            this.imageListForCacheTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListForCacheTree.ImageStream")));
            this.imageListForCacheTree.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListForCacheTree.Images.SetKeyName(0, "blank.png");
            this.imageListForCacheTree.Images.SetKeyName(1, "Downloading");
            // 
            // btnRefreshList
            // 
            this.btnRefreshList.Location = new System.Drawing.Point(74, 497);
            this.btnRefreshList.Name = "btnRefreshList";
            this.btnRefreshList.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshList.TabIndex = 21;
            this.btnRefreshList.Text = "Refresh";
            this.btnRefreshList.UseVisualStyleBackColor = true;
            this.btnRefreshList.Click += new System.EventHandler(this.btnRefreshList_Click);
            // 
            // CachingDashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 558);
            this.Controls.Add(this.btnRefreshList);
            this.Controls.Add(this.tvCacheItems);
            this.Controls.Add(this.btnForceUnlock);
            this.Controls.Add(this.lblShortfall);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dgvCacheFetchErrors);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnShowPermissionWindow);
            this.Controls.Add(this.btnShowPipeline);
            this.Controls.Add(this.lblChunkPeriod);
            this.Controls.Add(this.lblCacheLagPeriod);
            this.Controls.Add(this.lblCacheFillProgress);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblLockInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblResourceIdentifier);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(600, 551);
            this.Name = "CachingDashboardForm";
            this.Text = "Caching Dashboard";
            this.Load += new System.EventHandler(this.CachingDashboardForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCacheFetchErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblResourceIdentifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblLockInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCacheFillProgress;
        private System.Windows.Forms.Label lblCacheLagPeriod;
        private System.Windows.Forms.Label lblChunkPeriod;
        private System.Windows.Forms.Button btnShowPipeline;
        private System.Windows.Forms.Button btnShowPermissionWindow;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.DataGridView dgvCacheFetchErrors;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttemptedOn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Start;
        private System.Windows.Forms.DataGridViewTextBoxColumn End;
        private System.Windows.Forms.DataGridViewTextBoxColumn ExceptionText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblShortfall;
        private System.Windows.Forms.Button btnForceUnlock;
        private System.Windows.Forms.TreeView tvCacheItems;
        private System.Windows.Forms.ImageList imageListForCacheTree;
        private System.Windows.Forms.Button btnRefreshList;
    }
}

