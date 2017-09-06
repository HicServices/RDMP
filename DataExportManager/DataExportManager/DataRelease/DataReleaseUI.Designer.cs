namespace DataExportManager.DataRelease
{
    partial class DataReleaseUI
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
            this.scLeft = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblLoading = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnFinalise = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dataReleaseKeyUI1 = new DataExportManager.DataRelease.DataReleaseKeyUI();
            this.doReleaseAndAuditUI1 = new DataExportManager.DataRelease.DoReleaseAndAuditUI();
            ((System.ComponentModel.ISupportInitialize)(this.scLeft)).BeginInit();
            this.scLeft.Panel1.SuspendLayout();
            this.scLeft.Panel2.SuspendLayout();
            this.scLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scLeft
            // 
            this.scLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scLeft.Location = new System.Drawing.Point(0, 0);
            this.scLeft.Name = "scLeft";
            this.scLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scLeft.Panel1
            // 
            this.scLeft.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // scLeft.Panel2
            // 
            this.scLeft.Panel2.Controls.Add(this.btnRefresh);
            this.scLeft.Panel2.Controls.Add(this.lblLoading);
            this.scLeft.Panel2.Controls.Add(this.label2);
            this.scLeft.Panel2.Controls.Add(this.dataReleaseKeyUI1);
            this.scLeft.Size = new System.Drawing.Size(569, 562);
            this.scLeft.SplitterDistance = 413;
            this.scLeft.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(565, 409);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.Resize += new System.EventHandler(this.flowLayoutPanel1_Resize);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(5, 118);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(53, 23);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Reload";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoading.Location = new System.Drawing.Point(32, 34);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(391, 108);
            this.lblLoading.TabIndex = 2;
            this.lblLoading.Text = "Loading";
            this.lblLoading.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Key:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.scLeft);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.doReleaseAndAuditUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1074, 562);
            this.splitContainer1.SplitterDistance = 569;
            this.splitContainer1.TabIndex = 2;
            // 
            // btnFinalise
            // 
            this.btnFinalise.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnFinalise.Location = new System.Drawing.Point(314, 579);
            this.btnFinalise.Name = "btnFinalise";
            this.btnFinalise.Size = new System.Drawing.Size(186, 23);
            this.btnFinalise.TabIndex = 3;
            this.btnFinalise.Text = "Finalise Selection";
            this.btnFinalise.UseVisualStyleBackColor = true;
            this.btnFinalise.Click += new System.EventHandler(this.btnFinalise_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(520, 579);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(186, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // dataReleaseKeyUI1
            // 
            this.dataReleaseKeyUI1.AutoScroll = true;
            this.dataReleaseKeyUI1.Location = new System.Drawing.Point(3, 21);
            this.dataReleaseKeyUI1.Name = "dataReleaseKeyUI1";
            this.dataReleaseKeyUI1.Size = new System.Drawing.Size(432, 144);
            this.dataReleaseKeyUI1.TabIndex = 0;
            // 
            // doReleaseAndAuditUI1
            // 
            this.doReleaseAndAuditUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doReleaseAndAuditUI1.Location = new System.Drawing.Point(0, 0);
            this.doReleaseAndAuditUI1.Name = "doReleaseAndAuditUI1";
            this.doReleaseAndAuditUI1.Size = new System.Drawing.Size(497, 558);
            this.doReleaseAndAuditUI1.TabIndex = 0;
            // 
            // DataReleaseUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 614);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnFinalise);
            this.Controls.Add(this.splitContainer1);
            this.Name = "DataReleaseUI";
            this.Load += new System.EventHandler(this.DataReleaseManagementUI_Load);
            this.scLeft.Panel1.ResumeLayout(false);
            this.scLeft.Panel2.ResumeLayout(false);
            this.scLeft.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scLeft)).EndInit();
            this.scLeft.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scLeft;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.Label label2;
        private DataReleaseKeyUI dataReleaseKeyUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DoReleaseAndAuditUI doReleaseAndAuditUI1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnFinalise;
        private System.Windows.Forms.Button btnCancel;

    }
}
