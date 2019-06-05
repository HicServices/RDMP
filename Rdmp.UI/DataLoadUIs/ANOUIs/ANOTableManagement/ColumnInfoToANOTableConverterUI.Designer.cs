namespace Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement
{
    partial class ColumnInfoToANOTableConverterUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbSuffix = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.gbConfigureANOTable = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.ddExternalDatabaseServer = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbANOTableName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDataType = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblPreviewDataIsFictional = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dgPreview = new System.Windows.Forms.DataGridView();
            this.gbANOTable = new System.Windows.Forms.GroupBox();
            this.btnFinalise = new System.Windows.Forms.Button();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.label11 = new System.Windows.Forms.Label();
            this.ddANOTables = new System.Windows.Forms.ComboBox();
            this.btnCreateNewANOTable = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.gbCreateNewANOTable = new System.Windows.Forms.GroupBox();
            this.lblDoNotIncludeUnderscore = new System.Windows.Forms.Label();
            this.lblNoDefaultANOStore = new System.Windows.Forms.Label();
            this.lblMustStartWithANO = new System.Windows.Forms.Label();
            this.gbSelectExistingANOTable = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.lblRowcount = new System.Windows.Forms.Label();
            this.ntimeout = new System.Windows.Forms.NumericUpDown();
            this.gbConfigureANOTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPreview)).BeginInit();
            this.gbANOTable.SuspendLayout();
            this.gbCreateNewANOTable.SuspendLayout();
            this.gbSelectExistingANOTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ColumnInfo Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Suffix:";
            // 
            // tbSuffix
            // 
            this.tbSuffix.Location = new System.Drawing.Point(96, 96);
            this.tbSuffix.Name = "tbSuffix";
            this.tbSuffix.Size = new System.Drawing.Size(221, 20);
            this.tbSuffix.TabIndex = 1;
            this.tbSuffix.TextChanged += new System.EventHandler(this.tbSuffix_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "ColumnInfo DataType:";
            // 
            // gbConfigureANOTable
            // 
            this.gbConfigureANOTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbConfigureANOTable.Controls.Add(this.textBox1);
            this.gbConfigureANOTable.Controls.Add(this.label12);
            this.gbConfigureANOTable.Controls.Add(this.label10);
            this.gbConfigureANOTable.Controls.Add(this.label9);
            this.gbConfigureANOTable.Controls.Add(this.numericUpDown2);
            this.gbConfigureANOTable.Controls.Add(this.numericUpDown1);
            this.gbConfigureANOTable.Location = new System.Drawing.Point(6, 19);
            this.gbConfigureANOTable.Name = "gbConfigureANOTable";
            this.gbConfigureANOTable.Size = new System.Drawing.Size(1139, 95);
            this.gbConfigureANOTable.TabIndex = 4;
            this.gbConfigureANOTable.TabStop = false;
            this.gbConfigureANOTable.Text = "Configure Parameters";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(117, 16);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(99, 20);
            this.textBox1.TabIndex = 14;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(84, 19);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "ID:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(245, 70);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "NumberOLetters:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "NumberOfIntegers:";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(334, 68);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown2.TabIndex = 10;
            this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(119, 68);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 10;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // ddExternalDatabaseServer
            // 
            this.ddExternalDatabaseServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExternalDatabaseServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExternalDatabaseServer.FormattingEnabled = true;
            this.ddExternalDatabaseServer.Location = new System.Drawing.Point(96, 19);
            this.ddExternalDatabaseServer.Name = "ddExternalDatabaseServer";
            this.ddExternalDatabaseServer.Size = new System.Drawing.Size(837, 21);
            this.ddExternalDatabaseServer.Sorted = true;
            this.ddExternalDatabaseServer.TabIndex = 9;
            this.ddExternalDatabaseServer.SelectedIndexChanged += new System.EventHandler(this.ddExternalDatabaseServer_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 52);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(108, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Timeout (in seconds):";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "ANOServer:";
            // 
            // tbANOTableName
            // 
            this.tbANOTableName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbANOTableName.Location = new System.Drawing.Point(96, 72);
            this.tbANOTableName.Name = "tbANOTableName";
            this.tbANOTableName.Size = new System.Drawing.Size(663, 20);
            this.tbANOTableName.TabIndex = 1;
            this.tbANOTableName.TextChanged += new System.EventHandler(this.tbANOTableName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "ANOTableName:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(126, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(10, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "-";
            // 
            // lblDataType
            // 
            this.lblDataType.AutoSize = true;
            this.lblDataType.Location = new System.Drawing.Point(126, 33);
            this.lblDataType.Name = "lblDataType";
            this.lblDataType.Size = new System.Drawing.Size(10, 13);
            this.lblDataType.TabIndex = 0;
            this.lblDataType.Text = "-";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 231);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblPreviewDataIsFictional);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.dgPreview);
            this.splitContainer1.Panel1.Controls.Add(this.gbANOTable);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.checksUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1163, 565);
            this.splitContainer1.SplitterDistance = 340;
            this.splitContainer1.TabIndex = 6;
            // 
            // lblPreviewDataIsFictional
            // 
            this.lblPreviewDataIsFictional.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPreviewDataIsFictional.AutoSize = true;
            this.lblPreviewDataIsFictional.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.lblPreviewDataIsFictional.ForeColor = System.Drawing.Color.Red;
            this.lblPreviewDataIsFictional.Location = new System.Drawing.Point(18, 321);
            this.lblPreviewDataIsFictional.Name = "lblPreviewDataIsFictional";
            this.lblPreviewDataIsFictional.Size = new System.Drawing.Size(253, 13);
            this.lblPreviewDataIsFictional.TabIndex = 16;
            this.lblPreviewDataIsFictional.Text = "(You DataTable is empty, the above data is fictional)";
            this.lblPreviewDataIsFictional.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Preview:";
            // 
            // dgPreview
            // 
            this.dgPreview.AllowUserToAddRows = false;
            this.dgPreview.AllowUserToDeleteRows = false;
            this.dgPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgPreview.Location = new System.Drawing.Point(12, 172);
            this.dgPreview.Name = "dgPreview";
            this.dgPreview.ReadOnly = true;
            this.dgPreview.Size = new System.Drawing.Size(1148, 162);
            this.dgPreview.TabIndex = 14;
            // 
            // gbANOTable
            // 
            this.gbANOTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbANOTable.Controls.Add(this.gbConfigureANOTable);
            this.gbANOTable.Controls.Add(this.btnFinalise);
            this.gbANOTable.Enabled = false;
            this.gbANOTable.Location = new System.Drawing.Point(6, 3);
            this.gbANOTable.Name = "gbANOTable";
            this.gbANOTable.Size = new System.Drawing.Size(1151, 142);
            this.gbANOTable.TabIndex = 13;
            this.gbANOTable.TabStop = false;
            this.gbANOTable.Text = "ANOTable";
            // 
            // btnFinalise
            // 
            this.btnFinalise.Location = new System.Drawing.Point(6, 120);
            this.btnFinalise.Name = "btnFinalise";
            this.btnFinalise.Size = new System.Drawing.Size(205, 23);
            this.btnFinalise.TabIndex = 12;
            this.btnFinalise.Text = "Finalise and Push ANOTable";
            this.btnFinalise.UseVisualStyleBackColor = true;
            this.btnFinalise.Click += new System.EventHandler(this.btnFinalise_Click);
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(0, 0);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1163, 221);
            this.checksUI1.TabIndex = 5;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 22);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "ANOTable:";
            // 
            // ddANOTables
            // 
            this.ddANOTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddANOTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddANOTables.FormattingEnabled = true;
            this.ddANOTables.Location = new System.Drawing.Point(73, 19);
            this.ddANOTables.Name = "ddANOTables";
            this.ddANOTables.Size = new System.Drawing.Size(837, 21);
            this.ddANOTables.Sorted = true;
            this.ddANOTables.TabIndex = 9;
            this.ddANOTables.SelectedIndexChanged += new System.EventHandler(this.ddANOTables_SelectedIndexChanged);
            // 
            // btnCreateNewANOTable
            // 
            this.btnCreateNewANOTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewANOTable.Location = new System.Drawing.Point(96, 122);
            this.btnCreateNewANOTable.Name = "btnCreateNewANOTable";
            this.btnCreateNewANOTable.Size = new System.Drawing.Size(134, 23);
            this.btnCreateNewANOTable.TabIndex = 12;
            this.btnCreateNewANOTable.Text = "Create New ANOTable";
            this.btnCreateNewANOTable.UseVisualStyleBackColor = true;
            this.btnCreateNewANOTable.Click += new System.EventHandler(this.btnCreateNewANOTable_Click);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(711, 57);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(47, 13);
            this.label13.TabIndex = 14;
            this.label13.Text = "--- OR ---";
            // 
            // gbCreateNewANOTable
            // 
            this.gbCreateNewANOTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbCreateNewANOTable.Controls.Add(this.lblDoNotIncludeUnderscore);
            this.gbCreateNewANOTable.Controls.Add(this.lblNoDefaultANOStore);
            this.gbCreateNewANOTable.Controls.Add(this.lblMustStartWithANO);
            this.gbCreateNewANOTable.Controls.Add(this.ddExternalDatabaseServer);
            this.gbCreateNewANOTable.Controls.Add(this.label5);
            this.gbCreateNewANOTable.Controls.Add(this.tbANOTableName);
            this.gbCreateNewANOTable.Controls.Add(this.btnCreateNewANOTable);
            this.gbCreateNewANOTable.Controls.Add(this.label6);
            this.gbCreateNewANOTable.Controls.Add(this.tbSuffix);
            this.gbCreateNewANOTable.Controls.Add(this.label3);
            this.gbCreateNewANOTable.Location = new System.Drawing.Point(230, 73);
            this.gbCreateNewANOTable.Name = "gbCreateNewANOTable";
            this.gbCreateNewANOTable.Size = new System.Drawing.Size(939, 152);
            this.gbCreateNewANOTable.TabIndex = 15;
            this.gbCreateNewANOTable.TabStop = false;
            this.gbCreateNewANOTable.Text = "Create New ANOTable";
            // 
            // lblDoNotIncludeUnderscore
            // 
            this.lblDoNotIncludeUnderscore.AutoSize = true;
            this.lblDoNotIncludeUnderscore.Location = new System.Drawing.Point(323, 99);
            this.lblDoNotIncludeUnderscore.Name = "lblDoNotIncludeUnderscore";
            this.lblDoNotIncludeUnderscore.Size = new System.Drawing.Size(202, 13);
            this.lblDoNotIncludeUnderscore.TabIndex = 13;
            this.lblDoNotIncludeUnderscore.Text = "(Suffixes cannot start with an underscore)";
            // 
            // lblNoDefaultANOStore
            // 
            this.lblNoDefaultANOStore.AutoSize = true;
            this.lblNoDefaultANOStore.ForeColor = System.Drawing.Color.Red;
            this.lblNoDefaultANOStore.Location = new System.Drawing.Point(16, 43);
            this.lblNoDefaultANOStore.Name = "lblNoDefaultANOStore";
            this.lblNoDefaultANOStore.Size = new System.Drawing.Size(925, 13);
            this.lblNoDefaultANOStore.TabIndex = 13;
            this.lblNoDefaultANOStore.Text = "(You do not yet have a default ANOServer configured, set one up in \'Locations=>Ma" +
    "nage External Servers\' (in main CatalogueManager screen) or select the correct o" +
    "ne here (if you have multiple))";
            this.lblNoDefaultANOStore.Visible = false;
            // 
            // lblMustStartWithANO
            // 
            this.lblMustStartWithANO.AutoSize = true;
            this.lblMustStartWithANO.Location = new System.Drawing.Point(652, 96);
            this.lblMustStartWithANO.Name = "lblMustStartWithANO";
            this.lblMustStartWithANO.Size = new System.Drawing.Size(107, 13);
            this.lblMustStartWithANO.TabIndex = 13;
            this.lblMustStartWithANO.Text = "(Must start with ANO)";
            // 
            // gbSelectExistingANOTable
            // 
            this.gbSelectExistingANOTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSelectExistingANOTable.Controls.Add(this.ddANOTables);
            this.gbSelectExistingANOTable.Controls.Add(this.label11);
            this.gbSelectExistingANOTable.Location = new System.Drawing.Point(253, 9);
            this.gbSelectExistingANOTable.Name = "gbSelectExistingANOTable";
            this.gbSelectExistingANOTable.Size = new System.Drawing.Size(916, 45);
            this.gbSelectExistingANOTable.TabIndex = 15;
            this.gbSelectExistingANOTable.TabStop = false;
            this.gbSelectExistingANOTable.Text = "Select Existing ANOTable";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(60, 73);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Row Count:";
            // 
            // lblRowcount
            // 
            this.lblRowcount.AutoSize = true;
            this.lblRowcount.Location = new System.Drawing.Point(131, 73);
            this.lblRowcount.Name = "lblRowcount";
            this.lblRowcount.Size = new System.Drawing.Size(10, 13);
            this.lblRowcount.TabIndex = 0;
            this.lblRowcount.Text = "-";
            // 
            // ntimeout
            // 
            this.ntimeout.Location = new System.Drawing.Point(129, 50);
            this.ntimeout.Name = "ntimeout";
            this.ntimeout.Size = new System.Drawing.Size(87, 20);
            this.ntimeout.TabIndex = 16;
            this.ntimeout.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // ConvertColumnInfoIntoANOColumnInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1187, 796);
            this.Controls.Add(this.ntimeout);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.gbSelectExistingANOTable);
            this.Controls.Add(this.gbCreateNewANOTable);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.lblRowcount);
            this.Controls.Add(this.lblDataType);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Name = "ConvertColumnInfoIntoANOColumnInfo";
            this.Text = "CreateNEWANOTableConfiguration";
            this.gbConfigureANOTable.ResumeLayout(false);
            this.gbConfigureANOTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPreview)).EndInit();
            this.gbANOTable.ResumeLayout(false);
            this.gbCreateNewANOTable.ResumeLayout(false);
            this.gbCreateNewANOTable.PerformLayout();
            this.gbSelectExistingANOTable.ResumeLayout(false);
            this.gbSelectExistingANOTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbSuffix;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox gbConfigureANOTable;
        private System.Windows.Forms.TextBox tbANOTableName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDataType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ddExternalDatabaseServer;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox ddANOTables;
        private System.Windows.Forms.Button btnCreateNewANOTable;
        private System.Windows.Forms.GroupBox gbANOTable;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox gbCreateNewANOTable;
        private System.Windows.Forms.GroupBox gbSelectExistingANOTable;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblRowcount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgPreview;
        private System.Windows.Forms.NumericUpDown ntimeout;
        private System.Windows.Forms.Button btnFinalise;
        private System.Windows.Forms.Label lblMustStartWithANO;
        private System.Windows.Forms.Label lblPreviewDataIsFictional;
        private System.Windows.Forms.Label lblDoNotIncludeUnderscore;
        private System.Windows.Forms.Label lblNoDefaultANOStore;
    }
}