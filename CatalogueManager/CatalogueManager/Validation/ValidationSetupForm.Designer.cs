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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnConfigureStandardRegex = new System.Windows.Forms.ToolStripButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
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
            this.olvColumns.Size = new System.Drawing.Size(240, 507);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(922, 436);
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
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoEllipsis = true;
            this.label5.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label5.Location = new System.Drawing.Point(3, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(1180, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Column Validation";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(6, 44);
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
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.ddConsequence);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.ddSecondaryConstraints);
            this.splitContainer1.Panel2.Controls.Add(this.ddPrimaryConstraints);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddSecondaryConstraint);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(1177, 537);
            this.splitContainer1.SplitterDistance = 242;
            this.splitContainer1.TabIndex = 9;
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 513);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(196, 20);
            this.tbFilter.TabIndex = 2;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 516);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Filter:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnConfigureStandardRegex});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1183, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnConfigureStandardRegex
            // 
            this.btnConfigureStandardRegex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConfigureStandardRegex.Image = ((System.Drawing.Image)(resources.GetObject("btnConfigureStandardRegex.Image")));
            this.btnConfigureStandardRegex.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConfigureStandardRegex.Name = "btnConfigureStandardRegex";
            this.btnConfigureStandardRegex.Size = new System.Drawing.Size(23, 22);
            this.btnConfigureStandardRegex.Text = "Configure Standard Regexes";
            this.btnConfigureStandardRegex.Click += new System.EventHandler(this.btnConfigureStandardRegex_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(38, 1);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 11;
            this.objectSaverButton1.Text = "objectSaverButton1";
            this.objectSaverButton1.UseVisualStyleBackColor = true;
            // 
            // ValidationSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label5);
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
        private System.Windows.Forms.ToolStripButton btnConfigureStandardRegex;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}

