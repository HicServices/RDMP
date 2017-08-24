namespace CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding
{
    partial class ConfigurePreLoadDiscardedColumns
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
            this.lblTableInfoName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbPreLoadDiscardedColumns = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.btnNewColumn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.preLoadDiscardedColumnUI1 = new CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding.PreLoadDiscardedColumnUI();
            this.btnCheck = new System.Windows.Forms.Button();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.ddIdentifierDump = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMustSpecifyIdentifierDump = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTableInfoName
            // 
            this.lblTableInfoName.AutoSize = true;
            this.lblTableInfoName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTableInfoName.Location = new System.Drawing.Point(4, 12);
            this.lblTableInfoName.Name = "lblTableInfoName";
            this.lblTableInfoName.Size = new System.Drawing.Size(157, 25);
            this.lblTableInfoName.TabIndex = 0;
            this.lblTableInfoName.Text = "TableInfoName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "PreLoadDiscardedColumns:";
            // 
            // lbPreLoadDiscardedColumns
            // 
            this.lbPreLoadDiscardedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPreLoadDiscardedColumns.FormattingEnabled = true;
            this.lbPreLoadDiscardedColumns.Location = new System.Drawing.Point(3, 6);
            this.lbPreLoadDiscardedColumns.Name = "lbPreLoadDiscardedColumns";
            this.lbPreLoadDiscardedColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbPreLoadDiscardedColumns.Size = new System.Drawing.Size(182, 485);
            this.lbPreLoadDiscardedColumns.TabIndex = 1;
            this.lbPreLoadDiscardedColumns.SelectedIndexChanged += new System.EventHandler(this.lbPreLoadDiscardedColumns_SelectedIndexChanged);
            this.lbPreLoadDiscardedColumns.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbPreLoadDiscardedColumns_KeyDown);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(2, 53);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnNewColumn);
            this.splitContainer1.Panel1.Controls.Add(this.lbPreLoadDiscardedColumns);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.btnCheck);
            this.splitContainer1.Panel2.Controls.Add(this.checksUI1);
            this.splitContainer1.Size = new System.Drawing.Size(853, 563);
            this.splitContainer1.SplitterDistance = 189;
            this.splitContainer1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1, 527);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "(or Ctrl + V paste in a list of columns)";
            // 
            // btnNewColumn
            // 
            this.btnNewColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewColumn.Location = new System.Drawing.Point(7, 501);
            this.btnNewColumn.Name = "btnNewColumn";
            this.btnNewColumn.Size = new System.Drawing.Size(177, 23);
            this.btnNewColumn.TabIndex = 2;
            this.btnNewColumn.Text = "New Column";
            this.btnNewColumn.UseVisualStyleBackColor = true;
            this.btnNewColumn.Click += new System.EventHandler(this.btnNewColumn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.preLoadDiscardedColumnUI1);
            this.groupBox1.Location = new System.Drawing.Point(3, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(654, 174);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Selected PreLoadDiscarded Column:";
            // 
            // preLoadDiscardedColumnUI1
            // 
            this.preLoadDiscardedColumnUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.preLoadDiscardedColumnUI1.Location = new System.Drawing.Point(3, 16);
            this.preLoadDiscardedColumnUI1.MultiSelectMode = false;
            this.preLoadDiscardedColumnUI1.Name = "preLoadDiscardedColumnUI1";
            this.preLoadDiscardedColumnUI1.PreLoadDiscardedColumn = null;
            this.preLoadDiscardedColumnUI1.Size = new System.Drawing.Size(648, 155);
            this.preLoadDiscardedColumnUI1.TabIndex = 0;
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(6, 183);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(75, 23);
            this.btnCheck.TabIndex = 2;
            this.btnCheck.Text = "Check";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // checksUI1
            // 
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(3, 212);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(643, 348);
            this.checksUI1.TabIndex = 1;
            // 
            // ddIdentifierDump
            // 
            this.ddIdentifierDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddIdentifierDump.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddIdentifierDump.FormattingEnabled = true;
            this.ddIdentifierDump.Location = new System.Drawing.Point(648, 9);
            this.ddIdentifierDump.Name = "ddIdentifierDump";
            this.ddIdentifierDump.Size = new System.Drawing.Size(193, 21);
            this.ddIdentifierDump.TabIndex = 3;
            this.ddIdentifierDump.SelectedIndexChanged += new System.EventHandler(this.ddIdentifierDump_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(527, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Identifier Dump Server:";
            // 
            // lblMustSpecifyIdentifierDump
            // 
            this.lblMustSpecifyIdentifierDump.AutoSize = true;
            this.lblMustSpecifyIdentifierDump.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMustSpecifyIdentifierDump.ForeColor = System.Drawing.Color.Red;
            this.lblMustSpecifyIdentifierDump.Location = new System.Drawing.Point(654, 33);
            this.lblMustSpecifyIdentifierDump.Name = "lblMustSpecifyIdentifierDump";
            this.lblMustSpecifyIdentifierDump.Size = new System.Drawing.Size(176, 13);
            this.lblMustSpecifyIdentifierDump.TabIndex = 5;
            this.lblMustSpecifyIdentifierDump.Text = "You must specify an Identifier Dump";
            this.lblMustSpecifyIdentifierDump.Visible = false;
            // 
            // ConfigurePreLoadDiscardedColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 616);
            this.Controls.Add(this.lblMustSpecifyIdentifierDump);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ddIdentifierDump);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lblTableInfoName);
            this.KeyPreview = true;
            this.Name = "ConfigurePreLoadDiscardedColumns";
            this.Text = "ConfigurePreLoadDiscardedColumns";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ConfigurePreLoadDiscardedColumns_KeyUp);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTableInfoName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbPreLoadDiscardedColumns;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnNewColumn;
        private PreLoadDiscardedColumnUI preLoadDiscardedColumnUI1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddIdentifierDump;
        private System.Windows.Forms.Label label3;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Label lblMustSpecifyIdentifierDump;
    }
}