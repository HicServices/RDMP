namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class MockarooFileGeneratorUI
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.label3 = new System.Windows.Forms.Label();
            this.tbAPIKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbNumberOfFiles = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbNumberOfHeadersStart = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbNumberOfHeadersEnd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNumberOfRecordsStart = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbNumberOfRecordsEnd = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbDirectory = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.progressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnJumpToUpload = new System.Windows.Forms.Button();
            this.lbFilesToAdd = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.progressUI1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1265, 919);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(3, 19);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(20, 20);
            this.ragSmiley1.TabIndex = 41;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(29, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "API Key";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbAPIKey
            // 
            this.tbAPIKey.Location = new System.Drawing.Point(80, 19);
            this.tbAPIKey.Name = "tbAPIKey";
            this.tbAPIKey.Size = new System.Drawing.Size(100, 20);
            this.tbAPIKey.TabIndex = 1;
            this.tbAPIKey.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(186, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Generate";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbNumberOfFiles
            // 
            this.tbNumberOfFiles.Location = new System.Drawing.Point(259, 19);
            this.tbNumberOfFiles.Name = "tbNumberOfFiles";
            this.tbNumberOfFiles.Size = new System.Drawing.Size(100, 20);
            this.tbNumberOfFiles.TabIndex = 3;
            this.tbNumberOfFiles.Text = "1";
            this.tbNumberOfFiles.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(365, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 20);
            this.label9.TabIndex = 4;
            this.label9.Text = "files.  With between";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbNumberOfHeadersStart
            // 
            this.tbNumberOfHeadersStart.Location = new System.Drawing.Point(483, 19);
            this.tbNumberOfHeadersStart.Name = "tbNumberOfHeadersStart";
            this.tbNumberOfHeadersStart.Size = new System.Drawing.Size(100, 20);
            this.tbNumberOfHeadersStart.TabIndex = 5;
            this.tbNumberOfHeadersStart.Text = "1";
            this.tbNumberOfHeadersStart.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(589, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "and";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbNumberOfHeadersEnd
            // 
            this.tbNumberOfHeadersEnd.Location = new System.Drawing.Point(634, 19);
            this.tbNumberOfHeadersEnd.Name = "tbNumberOfHeadersEnd";
            this.tbNumberOfHeadersEnd.Size = new System.Drawing.Size(100, 20);
            this.tbNumberOfHeadersEnd.TabIndex = 7;
            this.tbNumberOfHeadersEnd.Text = "10";
            this.tbNumberOfHeadersEnd.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(740, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "headers and between";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbNumberOfRecordsStart
            // 
            this.tbNumberOfRecordsStart.Location = new System.Drawing.Point(863, 19);
            this.tbNumberOfRecordsStart.Name = "tbNumberOfRecordsStart";
            this.tbNumberOfRecordsStart.Size = new System.Drawing.Size(100, 20);
            this.tbNumberOfRecordsStart.TabIndex = 9;
            this.tbNumberOfRecordsStart.Text = "1";
            this.tbNumberOfRecordsStart.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(969, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "and";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbNumberOfRecordsEnd
            // 
            this.tbNumberOfRecordsEnd.Location = new System.Drawing.Point(1014, 19);
            this.tbNumberOfRecordsEnd.Name = "tbNumberOfRecordsEnd";
            this.tbNumberOfRecordsEnd.Size = new System.Drawing.Size(100, 20);
            this.tbNumberOfRecordsEnd.TabIndex = 11;
            this.tbNumberOfRecordsEnd.Text = "5";
            this.tbNumberOfRecordsEnd.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(3, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(152, 20);
            this.label6.TabIndex = 12;
            this.label6.Text = "rows (per file).  Into directory:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbDirectory
            // 
            this.tbDirectory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.tbDirectory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbDirectory.Location = new System.Drawing.Point(161, 45);
            this.tbDirectory.Name = "tbDirectory";
            this.tbDirectory.Size = new System.Drawing.Size(462, 20);
            this.tbDirectory.TabIndex = 13;
            this.tbDirectory.Text = "c:\\temp\\";
            this.tbDirectory.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(629, 45);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(138, 23);
            this.btnGo.TabIndex = 16;
            this.btnGo.Text = "Generate Mockaroo Files";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(11, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(747, 160);
            this.panel1.TabIndex = 18;
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(764, 19);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(471, 143);
            this.serverDatabaseTableSelector1.TabIndex = 19;
            this.serverDatabaseTableSelector1.Table = "";
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // progressUI1
            // 
            this.progressUI1.Location = new System.Drawing.Point(3, 467);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(1250, 572);
            this.progressUI1.TabIndex = 42;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lbFilesToAdd);
            this.groupBox1.Controls.Add(this.btnJumpToUpload);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.serverDatabaseTableSelector1);
            this.groupBox1.Location = new System.Drawing.Point(3, 80);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1250, 381);
            this.groupBox1.TabIndex = 43;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Upload Files";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ragSmiley1);
            this.groupBox2.Controls.Add(this.tbAPIKey);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnGo);
            this.groupBox2.Controls.Add(this.tbNumberOfFiles);
            this.groupBox2.Controls.Add(this.tbDirectory);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.tbNumberOfHeadersStart);
            this.groupBox2.Controls.Add(this.tbNumberOfRecordsEnd);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbNumberOfHeadersEnd);
            this.groupBox2.Controls.Add(this.tbNumberOfRecordsStart);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1134, 71);
            this.groupBox2.TabIndex = 43;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "GenerateFiles";
            // 
            // btnJumpToUpload
            // 
            this.btnJumpToUpload.Location = new System.Drawing.Point(6, 352);
            this.btnJumpToUpload.Name = "btnJumpToUpload";
            this.btnJumpToUpload.Size = new System.Drawing.Size(471, 23);
            this.btnJumpToUpload.TabIndex = 17;
            this.btnJumpToUpload.Text = "Upload Files As Catalogues (Use first column as extarction identifier)";
            this.btnJumpToUpload.UseVisualStyleBackColor = true;
            this.btnJumpToUpload.Click += new System.EventHandler(this.btnJumpToUpload_Click);
            // 
            // lbFilesToAdd
            // 
            this.lbFilesToAdd.AllowDrop = true;
            this.lbFilesToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFilesToAdd.FormattingEnabled = true;
            this.lbFilesToAdd.Location = new System.Drawing.Point(9, 199);
            this.lbFilesToAdd.Name = "lbFilesToAdd";
            this.lbFilesToAdd.Size = new System.Drawing.Size(1235, 147);
            this.lbFilesToAdd.TabIndex = 20;
            this.lbFilesToAdd.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbFilesToAdd_DragDrop);
            this.lbFilesToAdd.DragEnter += new System.Windows.Forms.DragEventHandler(this.lbFilesToAdd_DragEnter);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 185);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Files:";
            // 
            // MockarooFileGeneratorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1265, 919);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "MockarooFileGeneratorUI";
            this.Text = "MockarooFileGeneratorUI";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbAPIKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbNumberOfFiles;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbNumberOfHeadersStart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbNumberOfHeadersEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbNumberOfRecordsStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbNumberOfRecordsEnd;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbDirectory;
        private System.Windows.Forms.Button btnGo;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Panel panel1;
        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private ReusableUIComponents.Progress.ProgressUI progressUI1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnJumpToUpload;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox lbFilesToAdd;

    }
}