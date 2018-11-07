using BrightIdeasSoftware;

namespace CatalogueManager.Validation
{
    partial class ValidationSetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValidationSetupForm));
            this.olvColumns = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ddPrimaryConstraints = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ddConsequence = new System.Windows.Forms.ComboBox();
            this.ddSecondaryConstraints = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAddSecondaryConstraint = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cbxTimePeriodColumn = new System.Windows.Forms.ToolStripComboBox();
            this.lblPickTimePeriodColumn = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cbxPivotColumn = new System.Windows.Forms.ToolStripComboBox();
            this.lblPickPivotColumn = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvColumns
            // 
            this.olvColumns.AllColumns.Add(this.olvName);
            this.olvColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvColumns.CellEditUseWholeCell = false;
            this.olvColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvColumns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvColumns.FullRowSelect = true;
            this.olvColumns.HideSelection = false;
            this.olvColumns.Location = new System.Drawing.Point(0, 0);
            this.olvColumns.Name = "olvColumns";
            this.olvColumns.Size = new System.Drawing.Size(246, 477);
            this.olvColumns.TabIndex = 0;
            this.olvColumns.UseCompatibleStateImageBehavior = false;
            this.olvColumns.View = System.Windows.Forms.View.Details;
            this.olvColumns.SelectedIndexChanged += new System.EventHandler(this.lbColumns_SelectedIndexChanged);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Groupable = false;
            this.olvName.Text = "Select column to configure validation";
            // 
            // ddPrimaryConstraints
            // 
            this.ddPrimaryConstraints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPrimaryConstraints.FormattingEnabled = true;
            this.ddPrimaryConstraints.Location = new System.Drawing.Point(4, 17);
            this.ddPrimaryConstraints.Name = "ddPrimaryConstraints";
            this.ddPrimaryConstraints.Size = new System.Drawing.Size(190, 21);
            this.ddPrimaryConstraints.Sorted = true;
            this.ddPrimaryConstraints.TabIndex = 2;
            this.ddPrimaryConstraints.SelectedIndexChanged += new System.EventHandler(this.ddPrimaryConstraints_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Primary Constraint:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(200, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Consequence:";
            // 
            // ddConsequence
            // 
            this.ddConsequence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddConsequence.FormattingEnabled = true;
            this.ddConsequence.Location = new System.Drawing.Point(200, 17);
            this.ddConsequence.Name = "ddConsequence";
            this.ddConsequence.Size = new System.Drawing.Size(121, 21);
            this.ddConsequence.TabIndex = 8;
            this.ddConsequence.SelectedIndexChanged += new System.EventHandler(this.ddConsequence_SelectedIndexChanged);
            // 
            // ddSecondaryConstraints
            // 
            this.ddSecondaryConstraints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddSecondaryConstraints.FormattingEnabled = true;
            this.ddSecondaryConstraints.Location = new System.Drawing.Point(6, 66);
            this.ddSecondaryConstraints.Name = "ddSecondaryConstraints";
            this.ddSecondaryConstraints.Size = new System.Drawing.Size(223, 21);
            this.ddSecondaryConstraints.Sorted = true;
            this.ddSecondaryConstraints.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Secondary Constraints:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 93);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(965, 406);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // btnAddSecondaryConstraint
            // 
            this.btnAddSecondaryConstraint.Image = ((System.Drawing.Image)(resources.GetObject("btnAddSecondaryConstraint.Image")));
            this.btnAddSecondaryConstraint.Location = new System.Drawing.Point(233, 63);
            this.btnAddSecondaryConstraint.Name = "btnAddSecondaryConstraint";
            this.btnAddSecondaryConstraint.Size = new System.Drawing.Size(27, 27);
            this.btnAddSecondaryConstraint.TabIndex = 4;
            this.btnAddSecondaryConstraint.UseVisualStyleBackColor = true;
            this.btnAddSecondaryConstraint.Click += new System.EventHandler(this.btnAddSecondaryConstraint_Click);
            // 
            // label5
            // 
            this.label5.AutoEllipsis = true;
            this.label5.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label5.Dock = System.Windows.Forms.DockStyle.Top;
            this.label5.Location = new System.Drawing.Point(0, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(1183, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Column Validation Rules";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 45);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.olvColumns);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.objectSaverButton1);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.ddConsequence);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.ddSecondaryConstraints);
            this.splitContainer1.Panel2.Controls.Add(this.ddPrimaryConstraints);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddSecondaryConstraint);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(1183, 539);
            this.splitContainer1.SplitterDistance = 251;
            this.splitContainer1.TabIndex = 9;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 483);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(205, 20);
            this.tbFilter.TabIndex = 2;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 486);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Filter:";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(265, 62);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(56, 28);
            this.objectSaverButton1.TabIndex = 11;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cbxTimePeriodColumn,
            this.lblPickTimePeriodColumn,
            this.toolStripSeparator1,
            this.toolStripLabel2,
            this.cbxPivotColumn,
            this.lblPickPivotColumn,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1183, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(117, 22);
            this.toolStripLabel1.Text = "Time Period Column";
            // 
            // cbxTimePeriodColumn
            // 
            this.cbxTimePeriodColumn.Name = "cbxTimePeriodColumn";
            this.cbxTimePeriodColumn.Size = new System.Drawing.Size(200, 25);
            this.cbxTimePeriodColumn.SelectedIndexChanged += new System.EventHandler(this.cbxTimePeriodColumn_SelectedIndexChanged);
            // 
            // lblPickTimePeriodColumn
            // 
            this.lblPickTimePeriodColumn.BackColor = System.Drawing.Color.White;
            this.lblPickTimePeriodColumn.Name = "lblPickTimePeriodColumn";
            this.lblPickTimePeriodColumn.Size = new System.Drawing.Size(16, 22);
            this.lblPickTimePeriodColumn.Text = "...";
            this.lblPickTimePeriodColumn.Click += new System.EventHandler(this.lblPickTimePeriodColumn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(80, 22);
            this.toolStripLabel2.Text = "Pivot Column";
            // 
            // cbxPivotColumn
            // 
            this.cbxPivotColumn.Name = "cbxPivotColumn";
            this.cbxPivotColumn.Size = new System.Drawing.Size(200, 25);
            this.cbxPivotColumn.SelectedIndexChanged += new System.EventHandler(this.cbxPivotColumn_SelectedIndexChanged);
            // 
            // lblPickPivotColumn
            // 
            this.lblPickPivotColumn.BackColor = System.Drawing.Color.White;
            this.lblPickPivotColumn.Name = "lblPickPivotColumn";
            this.lblPickPivotColumn.Size = new System.Drawing.Size(16, 22);
            this.lblPickPivotColumn.Text = "...";
            this.lblPickPivotColumn.Click += new System.EventHandler(this.lblPickPivotColumn_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            // 
            // ValidationSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ValidationSetupForm";
            this.Size = new System.Drawing.Size(1183, 584);
            ((System.ComponentModel.ISupportInitialize)(this.olvColumns)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvColumns;
        private System.Windows.Forms.ComboBox ddPrimaryConstraints;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddSecondaryConstraint;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox ddSecondaryConstraints;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddConsequence;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private OLVColumn olvName;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cbxTimePeriodColumn;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cbxPivotColumn;
        private System.Windows.Forms.ToolStripLabel lblPickTimePeriodColumn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel lblPickPivotColumn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

