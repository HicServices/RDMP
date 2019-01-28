namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{
    partial class SqlProcessTaskUI
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.lblPath = new System.Windows.Forms.Label();
            this.pbFile = new System.Windows.Forms.PictureBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblID = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.loadStageIconUI1 = new CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadStageIconUI();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(3, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(888, 638);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Editor";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(110, 2);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 2;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(25, 7);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(39, 13);
            this.lblPath.TabIndex = 3;
            this.lblPath.Text = "lblPath";
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(0, 4);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(19, 19);
            this.pbFile.TabIndex = 4;
            this.pbFile.TabStop = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(141, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(222, 7);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(21, 13);
            this.lblID.TabIndex = 6;
            this.lblID.Text = "ID:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(249, 4);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 7;
            // 
            // loadStageIconUI1
            // 
            this.loadStageIconUI1.Location = new System.Drawing.Point(355, 5);
            this.loadStageIconUI1.Name = "loadStageIconUI1";
            this.loadStageIconUI1.Size = new System.Drawing.Size(232, 19);
            this.loadStageIconUI1.TabIndex = 8;
            // 
            // SqlProcessTaskUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.loadStageIconUI1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.pbFile);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.groupBox1);
            this.Name = "SqlProcessTaskUI";
            this.Size = new System.Drawing.Size(894, 703);
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.PictureBox pbFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox tbID;
        private LoadStageIconUI loadStageIconUI1;
    }
}
