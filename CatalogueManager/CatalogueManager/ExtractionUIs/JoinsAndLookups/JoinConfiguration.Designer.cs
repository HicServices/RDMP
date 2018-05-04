namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    partial class JoinConfiguration
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
            this.label3 = new System.Windows.Forms.Label();
            this.olvLeftColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvLeftColumnNames = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.olvRightColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvRightColumnNames = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnCreateJoinInfo = new System.Windows.Forms.Button();
            this.tbCollation = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnChooseRightTableInfo = new System.Windows.Forms.Button();
            this.tbLeftTableInfo = new System.Windows.Forms.TextBox();
            this.tbRightTableInfo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbFilterLeft = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tbFilterRight = new System.Windows.Forms.TextBox();
            this.rbAllLeftHandTableRecords = new System.Windows.Forms.RadioButton();
            this.rbAllRightHandTableRecords = new System.Windows.Forms.RadioButton();
            this.rbJoinInner = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pk1 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.pk3 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.pk2 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.fk1 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.fk3 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            this.fk2 = new CatalogueManager.ExtractionUIs.JoinsAndLookups.KeyDropLocationUI();
            ((System.ComponentModel.ISupportInitialize)(this.olvLeftColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvRightColumns)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(184, 264);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 163;
            this.label3.Text = "(Columns)";
            // 
            // olvLeftColumns
            // 
            this.olvLeftColumns.AllColumns.Add(this.olvLeftColumnNames);
            this.olvLeftColumns.AllowDrop = true;
            this.olvLeftColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.olvLeftColumns.CellEditUseWholeCell = false;
            this.olvLeftColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvLeftColumnNames});
            this.olvLeftColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvLeftColumns.IsSimpleDragSource = true;
            this.olvLeftColumns.Location = new System.Drawing.Point(7, 29);
            this.olvLeftColumns.Name = "olvLeftColumns";
            this.olvLeftColumns.Size = new System.Drawing.Size(461, 232);
            this.olvLeftColumns.TabIndex = 3;
            this.olvLeftColumns.UseCompatibleStateImageBehavior = false;
            this.olvLeftColumns.View = System.Windows.Forms.View.Details;
            // 
            // olvLeftColumnNames
            // 
            this.olvLeftColumnNames.AspectName = "ToString";
            this.olvLeftColumnNames.FillsFreeSpace = true;
            this.olvLeftColumnNames.Groupable = false;
            this.olvLeftColumnNames.Text = "ColumnInfos";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(474, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(13, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "=";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(474, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "=";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(474, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 166;
            this.label4.Text = "=";
            // 
            // olvRightColumns
            // 
            this.olvRightColumns.AllColumns.Add(this.olvRightColumnNames);
            this.olvRightColumns.AllowDrop = true;
            this.olvRightColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.olvRightColumns.CellEditUseWholeCell = false;
            this.olvRightColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvRightColumnNames});
            this.olvRightColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvRightColumns.IsSimpleDragSource = true;
            this.olvRightColumns.Location = new System.Drawing.Point(493, 29);
            this.olvRightColumns.Name = "olvRightColumns";
            this.olvRightColumns.Size = new System.Drawing.Size(461, 232);
            this.olvRightColumns.TabIndex = 4;
            this.olvRightColumns.UseCompatibleStateImageBehavior = false;
            this.olvRightColumns.View = System.Windows.Forms.View.Details;
            this.olvRightColumns.DragDrop += new System.Windows.Forms.DragEventHandler(this.olvRightColumns_DragDrop);
            this.olvRightColumns.DragEnter += new System.Windows.Forms.DragEventHandler(this.olvRightColumns_DragEnter);
            // 
            // olvRightColumnNames
            // 
            this.olvRightColumnNames.AspectName = "ToString";
            this.olvRightColumnNames.FillsFreeSpace = true;
            this.olvRightColumnNames.Groupable = false;
            this.olvRightColumnNames.Text = "ColumnInfos";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(402, 192);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 24;
            // 
            // btnCreateJoinInfo
            // 
            this.btnCreateJoinInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateJoinInfo.Enabled = false;
            this.btnCreateJoinInfo.Location = new System.Drawing.Point(433, 194);
            this.btnCreateJoinInfo.Name = "btnCreateJoinInfo";
            this.btnCreateJoinInfo.Size = new System.Drawing.Size(109, 23);
            this.btnCreateJoinInfo.TabIndex = 25;
            this.btnCreateJoinInfo.Text = "Create Join";
            this.btnCreateJoinInfo.UseVisualStyleBackColor = true;
            this.btnCreateJoinInfo.Click += new System.EventHandler(this.btnCreateJoinInfo_Click);
            // 
            // tbCollation
            // 
            this.tbCollation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbCollation.Location = new System.Drawing.Point(433, 168);
            this.tbCollation.Name = "tbCollation";
            this.tbCollation.Size = new System.Drawing.Size(229, 20);
            this.tbCollation.TabIndex = 23;
            this.tbCollation.Leave += new System.EventHandler(this.tbCollation_Leave);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(377, 172);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 13);
            this.label13.TabIndex = 22;
            this.label13.Text = "Collation:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(694, 264);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 163;
            this.label2.Text = "(Columns)";
            // 
            // btnChooseRightTableInfo
            // 
            this.btnChooseRightTableInfo.Location = new System.Drawing.Point(496, 0);
            this.btnChooseRightTableInfo.Name = "btnChooseRightTableInfo";
            this.btnChooseRightTableInfo.Size = new System.Drawing.Size(26, 26);
            this.btnChooseRightTableInfo.TabIndex = 1;
            this.btnChooseRightTableInfo.UseVisualStyleBackColor = true;
            this.btnChooseRightTableInfo.Click += new System.EventHandler(this.btnChooseRightTableInfo_Click);
            // 
            // tbLeftTableInfo
            // 
            this.tbLeftTableInfo.Location = new System.Drawing.Point(7, 3);
            this.tbLeftTableInfo.Name = "tbLeftTableInfo";
            this.tbLeftTableInfo.ReadOnly = true;
            this.tbLeftTableInfo.Size = new System.Drawing.Size(397, 20);
            this.tbLeftTableInfo.TabIndex = 0;
            // 
            // tbRightTableInfo
            // 
            this.tbRightTableInfo.Location = new System.Drawing.Point(531, 3);
            this.tbRightTableInfo.Name = "tbRightTableInfo";
            this.tbRightTableInfo.ReadOnly = true;
            this.tbRightTableInfo.Size = new System.Drawing.Size(397, 20);
            this.tbRightTableInfo.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(74, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(162, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Drag Columns Into Here>>>>>>>";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(725, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(206, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "<<<<<<<<<<Drag Matching Columns Here";
            // 
            // tbFilterLeft
            // 
            this.tbFilterLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilterLeft.Location = new System.Drawing.Point(41, 280);
            this.tbFilterLeft.Name = "tbFilterLeft";
            this.tbFilterLeft.Size = new System.Drawing.Size(423, 20);
            this.tbFilterLeft.TabIndex = 6;
            this.tbFilterLeft.TextChanged += new System.EventHandler(this.tbFilterLeft_TextChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 283);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Filter:";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(490, 283);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Filter:";
            // 
            // tbFilterRight
            // 
            this.tbFilterRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilterRight.Location = new System.Drawing.Point(528, 280);
            this.tbFilterRight.Name = "tbFilterRight";
            this.tbFilterRight.Size = new System.Drawing.Size(422, 20);
            this.tbFilterRight.TabIndex = 8;
            this.tbFilterRight.TextChanged += new System.EventHandler(this.tbFilterRight_TextChanged);
            // 
            // rbAllLeftHandTableRecords
            // 
            this.rbAllLeftHandTableRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbAllLeftHandTableRecords.AutoSize = true;
            this.rbAllLeftHandTableRecords.Location = new System.Drawing.Point(181, 134);
            this.rbAllLeftHandTableRecords.Name = "rbAllLeftHandTableRecords";
            this.rbAllLeftHandTableRecords.Size = new System.Drawing.Size(224, 17);
            this.rbAllLeftHandTableRecords.TabIndex = 19;
            this.rbAllLeftHandTableRecords.TabStop = true;
            this.rbAllLeftHandTableRecords.Text = "Always Show All Records From This Table";
            this.rbAllLeftHandTableRecords.UseVisualStyleBackColor = true;
            this.rbAllLeftHandTableRecords.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbAllRightHandTableRecords
            // 
            this.rbAllRightHandTableRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbAllRightHandTableRecords.AutoSize = true;
            this.rbAllRightHandTableRecords.Location = new System.Drawing.Point(569, 134);
            this.rbAllRightHandTableRecords.Name = "rbAllRightHandTableRecords";
            this.rbAllRightHandTableRecords.Size = new System.Drawing.Size(224, 17);
            this.rbAllRightHandTableRecords.TabIndex = 21;
            this.rbAllRightHandTableRecords.TabStop = true;
            this.rbAllRightHandTableRecords.Text = "Always Show All Records From This Table";
            this.rbAllRightHandTableRecords.UseVisualStyleBackColor = true;
            this.rbAllRightHandTableRecords.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbJoinInner
            // 
            this.rbJoinInner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbJoinInner.AutoSize = true;
            this.rbJoinInner.Location = new System.Drawing.Point(423, 134);
            this.rbJoinInner.Name = "rbJoinInner";
            this.rbJoinInner.Size = new System.Drawing.Size(120, 17);
            this.rbJoinInner.TabIndex = 20;
            this.rbJoinInner.TabStop = true;
            this.rbJoinInner.Text = "Exact Matches Only";
            this.rbJoinInner.UseVisualStyleBackColor = true;
            this.rbJoinInner.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbJoinInner);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.rbAllRightHandTableRecords);
            this.panel1.Controls.Add(this.tbCollation);
            this.panel1.Controls.Add(this.rbAllLeftHandTableRecords);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.btnCreateJoinInfo);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.ragSmiley1);
            this.panel1.Controls.Add(this.pk1);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.pk3);
            this.panel1.Controls.Add(this.pk2);
            this.panel1.Controls.Add(this.fk1);
            this.panel1.Controls.Add(this.fk3);
            this.panel1.Controls.Add(this.fk2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(965, 224);
            this.panel1.TabIndex = 167;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.tbLeftTableInfo);
            this.panel2.Controls.Add(this.tbRightTableInfo);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.btnChooseRightTableInfo);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.tbFilterRight);
            this.panel2.Controls.Add(this.olvRightColumns);
            this.panel2.Controls.Add(this.tbFilterLeft);
            this.panel2.Controls.Add(this.olvLeftColumns);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(965, 305);
            this.panel2.TabIndex = 168;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2MinSize = 240;
            this.splitContainer1.Size = new System.Drawing.Size(965, 533);
            this.splitContainer1.SplitterDistance = 305;
            this.splitContainer1.TabIndex = 169;
            // 
            // pk1
            // 
            this.pk1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pk1.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk1.Location = new System.Drawing.Point(242, 13);
            this.pk1.Name = "pk1";
            this.pk1.Size = new System.Drawing.Size(226, 35);
            this.pk1.TabIndex = 10;
            this.pk1.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // pk3
            // 
            this.pk3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pk3.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk3.Location = new System.Drawing.Point(242, 97);
            this.pk3.Name = "pk3";
            this.pk3.Size = new System.Drawing.Size(226, 35);
            this.pk3.TabIndex = 16;
            this.pk3.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // pk2
            // 
            this.pk2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pk2.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.pk2.Location = new System.Drawing.Point(242, 56);
            this.pk2.Name = "pk2";
            this.pk2.Size = new System.Drawing.Size(226, 35);
            this.pk2.TabIndex = 13;
            this.pk2.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // fk1
            // 
            this.fk1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fk1.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk1.Location = new System.Drawing.Point(493, 13);
            this.fk1.Name = "fk1";
            this.fk1.Size = new System.Drawing.Size(226, 35);
            this.fk1.TabIndex = 11;
            this.fk1.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // fk3
            // 
            this.fk3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fk3.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk3.Location = new System.Drawing.Point(493, 97);
            this.fk3.Name = "fk3";
            this.fk3.Size = new System.Drawing.Size(226, 35);
            this.fk3.TabIndex = 18;
            this.fk3.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // fk2
            // 
            this.fk2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.fk2.KeyType = CatalogueManager.ExtractionUIs.JoinsAndLookups.JoinKeyType.PrimaryKey;
            this.fk2.Location = new System.Drawing.Point(493, 56);
            this.fk2.Name = "fk2";
            this.fk2.Size = new System.Drawing.Size(226, 35);
            this.fk2.TabIndex = 15;
            this.fk2.SelectedColumnChanged += new System.Action(this.k_SelectedColumnChanged);
            // 
            // JoinConfiguration
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.Name = "JoinConfiguration";
            this.Size = new System.Drawing.Size(965, 533);
            ((System.ComponentModel.ISupportInitialize)(this.olvLeftColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvRightColumns)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private BrightIdeasSoftware.ObjectListView olvLeftColumns;
        private BrightIdeasSoftware.OLVColumn olvLeftColumnNames;
        private KeyDropLocationUI fk3;
        private KeyDropLocationUI fk2;
        private KeyDropLocationUI fk1;
        private KeyDropLocationUI pk2;
        private KeyDropLocationUI pk3;
        private KeyDropLocationUI pk1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private BrightIdeasSoftware.ObjectListView olvRightColumns;
        private BrightIdeasSoftware.OLVColumn olvRightColumnNames;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnCreateJoinInfo;
        private System.Windows.Forms.TextBox tbCollation;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnChooseRightTableInfo;
        private System.Windows.Forms.TextBox tbLeftTableInfo;
        private System.Windows.Forms.TextBox tbRightTableInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbFilterLeft;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbFilterRight;
        private System.Windows.Forms.RadioButton rbAllLeftHandTableRecords;
        private System.Windows.Forms.RadioButton rbAllRightHandTableRecords;
        private System.Windows.Forms.RadioButton rbJoinInner;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
