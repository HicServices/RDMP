namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    partial class Screen2
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
            this.listView1 = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDataType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfTimesSeen = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnDiscoverExtractionIdentifiers = new System.Windows.Forms.Button();
            this.cohortSourceDiagram1 = new DataExportManager.CohortUI.CohortSourceDiagram();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbIWantToHackTheReleaseIdentifierMyself = new System.Windows.Forms.RadioButton();
            this.rbGuid = new System.Windows.Forms.RadioButton();
            this.rbAutoIncrementing = new System.Windows.Forms.RadioButton();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.listView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.AllColumns.Add(this.olvName);
            this.listView1.AllColumns.Add(this.olvDataType);
            this.listView1.AllColumns.Add(this.olvNumberOfTimesSeen);
            this.listView1.CellEditUseWholeCell = false;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvDataType,
            this.olvNumberOfTimesSeen});
            this.listView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 66);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(718, 309);
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // olvName
            // 
            this.olvName.AspectName = "RuntimeName";
            this.olvName.Text = "Column Name";
            this.olvName.Width = 117;
            // 
            // olvDataType
            // 
            this.olvDataType.AspectName = "DataType";
            this.olvDataType.Text = "Data Type";
            this.olvDataType.Width = 112;
            // 
            // olvNumberOfTimesSeen
            // 
            this.olvNumberOfTimesSeen.AspectName = "CountOfTimesSeen";
            this.olvNumberOfTimesSeen.Text = "Number of Times Seen";
            this.olvNumberOfTimesSeen.Width = 130;
            // 
            // btnDiscoverExtractionIdentifiers
            // 
            this.btnDiscoverExtractionIdentifiers.Location = new System.Drawing.Point(0, 37);
            this.btnDiscoverExtractionIdentifiers.Name = "btnDiscoverExtractionIdentifiers";
            this.btnDiscoverExtractionIdentifiers.Size = new System.Drawing.Size(379, 23);
            this.btnDiscoverExtractionIdentifiers.TabIndex = 3;
            this.btnDiscoverExtractionIdentifiers.Text = "1. Attempt to figure out what name/datatype my patient identifiers are";
            this.btnDiscoverExtractionIdentifiers.UseVisualStyleBackColor = true;
            this.btnDiscoverExtractionIdentifiers.Click += new System.EventHandler(this.btnDiscoverExtractionIdentifiers_Click);
            // 
            // cohortSourceDiagram1
            // 
            this.cohortSourceDiagram1.Location = new System.Drawing.Point(12, 533);
            this.cohortSourceDiagram1.Name = "cohortSourceDiagram1";
            this.cohortSourceDiagram1.Size = new System.Drawing.Size(706, 166);
            this.cohortSourceDiagram1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 506);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "What will be created:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 382);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(333, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "2. Select which identifier schema/name is correct (from listbox above)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbIWantToHackTheReleaseIdentifierMyself);
            this.groupBox1.Controls.Add(this.rbGuid);
            this.groupBox1.Controls.Add(this.rbAutoIncrementing);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(7, 407);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 96);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "3. Choose Release Identifier Strategy";
            // 
            // rbIWantToHackTheReleaseIdentifierMyself
            // 
            this.rbIWantToHackTheReleaseIdentifierMyself.AutoSize = true;
            this.rbIWantToHackTheReleaseIdentifierMyself.Location = new System.Drawing.Point(7, 66);
            this.rbIWantToHackTheReleaseIdentifierMyself.Name = "rbIWantToHackTheReleaseIdentifierMyself";
            this.rbIWantToHackTheReleaseIdentifierMyself.Size = new System.Drawing.Size(309, 17);
            this.rbIWantToHackTheReleaseIdentifierMyself.TabIndex = 0;
            this.rbIWantToHackTheReleaseIdentifierMyself.TabStop = true;
            this.rbIWantToHackTheReleaseIdentifierMyself.Text = "Leave Blank (I want to ALTER/implement it myself manually)";
            this.rbIWantToHackTheReleaseIdentifierMyself.UseVisualStyleBackColor = true;
            this.rbIWantToHackTheReleaseIdentifierMyself.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbGuid
            // 
            this.rbGuid.AutoSize = true;
            this.rbGuid.Location = new System.Drawing.Point(7, 43);
            this.rbGuid.Name = "rbGuid";
            this.rbGuid.Size = new System.Drawing.Size(47, 17);
            this.rbGuid.TabIndex = 0;
            this.rbGuid.TabStop = true;
            this.rbGuid.Text = "Guid";
            this.rbGuid.UseVisualStyleBackColor = true;
            this.rbGuid.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbAutoIncrementing
            // 
            this.rbAutoIncrementing.AutoSize = true;
            this.rbAutoIncrementing.Location = new System.Drawing.Point(7, 20);
            this.rbAutoIncrementing.Name = "rbAutoIncrementing";
            this.rbAutoIncrementing.Size = new System.Drawing.Size(199, 17);
            this.rbAutoIncrementing.TabIndex = 0;
            this.rbAutoIncrementing.TabStop = true;
            this.rbAutoIncrementing.Text = "Auto incrementing int (magic number)";
            this.rbAutoIncrementing.UseVisualStyleBackColor = true;
            this.rbAutoIncrementing.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // btnNext
            // 
            this.btnNext.Enabled = false;
            this.btnNext.Location = new System.Drawing.Point(12, 696);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(297, 23);
            this.btnNext.TabIndex = 9;
            this.btnNext.Text = "That\'s it! That\'s what I want, Create it for me now please";
            this.btnNext.UseVisualStyleBackColor = true;
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(3, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(58, 23);
            this.btnBack.TabIndex = 10;
            this.btnBack.Text = "<< Back";
            this.btnBack.UseVisualStyleBackColor = true;
            // 
            // Screen2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cohortSourceDiagram1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnDiscoverExtractionIdentifiers);
            this.Name = "Screen2";
            this.Size = new System.Drawing.Size(752, 742);
            ((System.ComponentModel.ISupportInitialize)(this.listView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView listView1;
        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvDataType;
        private BrightIdeasSoftware.OLVColumn olvNumberOfTimesSeen;
        private System.Windows.Forms.Button btnDiscoverExtractionIdentifiers;
        private CohortSourceDiagram cohortSourceDiagram1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbGuid;
        private System.Windows.Forms.RadioButton rbAutoIncrementing;
        public System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.RadioButton rbIWantToHackTheReleaseIdentifierMyself;
        public System.Windows.Forms.Button btnBack;
    }
}
