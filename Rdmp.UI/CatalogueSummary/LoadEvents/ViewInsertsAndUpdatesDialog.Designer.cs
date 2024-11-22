using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.UI.CatalogueSummary.LoadEvents
{
    sealed partial class ViewInsertsAndUpdatesDialog : ICheckNotifier
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewInsertsAndUpdatesDialog));
            this.btnFetchData = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpCheck = new System.Windows.Forms.TabPage();
            this.checksUI1 = new ChecksUI.ChecksUI();
            this.tpViewInserts = new System.Windows.Forms.TabPage();
            this.dgInserts = new System.Windows.Forms.DataGridView();
            this.tpViewUpdates = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.diffDataTables1 = new DiffDataTables();
            this.label1 = new System.Windows.Forms.Label();
            this.tbBatchSizeToGet = new System.Windows.Forms.TextBox();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tpCheck.SuspendLayout();
            this.tpViewInserts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgInserts)).BeginInit();
            this.tpViewUpdates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnFetchData
            // 
            this.btnFetchData.Location = new System.Drawing.Point(12, 12);
            this.btnFetchData.Name = "btnFetchData";
            this.btnFetchData.Size = new System.Drawing.Size(171, 23);
            this.btnFetchData.TabIndex = 1;
            this.btnFetchData.Text = "Try To Fetch Appropriate Data";
            this.btnFetchData.UseVisualStyleBackColor = true;
            this.btnFetchData.Click += new System.EventHandler(this.btnFetchData_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpCheck);
            this.tabControl1.Controls.Add(this.tpViewInserts);
            this.tabControl1.Controls.Add(this.tpViewUpdates);
            this.tabControl1.Location = new System.Drawing.Point(12, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1220, 682);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpCheck
            // 
            this.tpCheck.Controls.Add(this.checksUI1);
            this.tpCheck.Location = new System.Drawing.Point(4, 22);
            this.tpCheck.Name = "tpCheck";
            this.tpCheck.Padding = new System.Windows.Forms.Padding(3);
            this.tpCheck.Size = new System.Drawing.Size(1212, 656);
            this.tpCheck.TabIndex = 0;
            this.tpCheck.Text = "Fetch Data";
            this.tpCheck.UseVisualStyleBackColor = true;
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(3, 3);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1206, 650);
            this.checksUI1.TabIndex = 0;
            // 
            // tpViewInserts
            // 
            this.tpViewInserts.Controls.Add(this.dgInserts);
            this.tpViewInserts.Location = new System.Drawing.Point(4, 22);
            this.tpViewInserts.Name = "tpViewInserts";
            this.tpViewInserts.Padding = new System.Windows.Forms.Padding(3);
            this.tpViewInserts.Size = new System.Drawing.Size(1212, 656);
            this.tpViewInserts.TabIndex = 1;
            this.tpViewInserts.Text = "View Inserts";
            this.tpViewInserts.UseVisualStyleBackColor = true;
            // 
            // dgInserts
            // 
            this.dgInserts.AllowUserToAddRows = false;
            this.dgInserts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgInserts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgInserts.Location = new System.Drawing.Point(3, 3);
            this.dgInserts.Name = "dgInserts";
            this.dgInserts.ReadOnly = true;
            this.dgInserts.Size = new System.Drawing.Size(1206, 650);
            this.dgInserts.TabIndex = 0;
            // 
            // tpViewUpdates
            // 
            this.tpViewUpdates.Controls.Add(this.label4);
            this.tpViewUpdates.Controls.Add(this.label3);
            this.tpViewUpdates.Controls.Add(this.pictureBox1);
            this.tpViewUpdates.Controls.Add(this.diffDataTables1);
            this.tpViewUpdates.Location = new System.Drawing.Point(4, 22);
            this.tpViewUpdates.Name = "tpViewUpdates";
            this.tpViewUpdates.Padding = new System.Windows.Forms.Padding(3);
            this.tpViewUpdates.Size = new System.Drawing.Size(1212, 656);
            this.tpViewUpdates.TabIndex = 2;
            this.tpViewUpdates.Text = "ViewUpdates";
            this.tpViewUpdates.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(207, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Understanding UPDATE / _Archive";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(7, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(706, 204);
            this.label3.TabIndex = 2;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(719, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(487, 226);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // diffDataTables1
            // 
            this.diffDataTables1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.diffDataTables1.Location = new System.Drawing.Point(3, 237);
            this.diffDataTables1.Name = "diffDataTables1";
            this.diffDataTables1.Size = new System.Drawing.Size(1206, 416);
            this.diffDataTables1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(189, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Batch Size To Get:";
            // 
            // tbBatchSizeToGet
            // 
            this.tbBatchSizeToGet.Location = new System.Drawing.Point(292, 14);
            this.tbBatchSizeToGet.Name = "tbBatchSizeToGet";
            this.tbBatchSizeToGet.Size = new System.Drawing.Size(344, 20);
            this.tbBatchSizeToGet.TabIndex = 4;
            this.tbBatchSizeToGet.TextChanged += new System.EventHandler(this.tbBatchSizeToGet_TextChanged);
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(705, 14);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(168, 20);
            this.tbTimeout.TabIndex = 6;
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(651, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Timeout:";
            // 
            // ViewInsertsAndUpdatesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 735);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbBatchSizeToGet);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnFetchData);
            this.Name = "ViewInsertsAndUpdatesDialog";
            this.Text = "Inserts And Updates";
            this.tabControl1.ResumeLayout(false);
            this.tpCheck.ResumeLayout(false);
            this.tpViewInserts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgInserts)).EndInit();
            this.tpViewUpdates.ResumeLayout(false);
            this.tpViewUpdates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnFetchData;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpCheck;
        private System.Windows.Forms.TabPage tpViewInserts;
        private System.Windows.Forms.TabPage tpViewUpdates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbBatchSizeToGet;
        private System.Windows.Forms.DataGridView dgInserts;
        private DiffDataTables diffDataTables1;
        private System.Windows.Forms.TextBox tbTimeout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
    }
}