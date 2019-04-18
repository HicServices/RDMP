namespace CatalogueManager.Validation
{
    partial class ResolveMissingTargetPropertiesUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResolveMissingTargetPropertiesUI));
            this.lbMissingReferences = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbAvailableColumns = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbOperations = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbSelectAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbMissingReferences
            // 
            this.lbMissingReferences.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbMissingReferences.FormattingEnabled = true;
            this.lbMissingReferences.Location = new System.Drawing.Point(15, 111);
            this.lbMissingReferences.Name = "lbMissingReferences";
            this.lbMissingReferences.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lbMissingReferences.Size = new System.Drawing.Size(300, 537);
            this.lbMissingReferences.TabIndex = 0;
            this.lbMissingReferences.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbMissingReferences_KeyUp);
            this.lbMissingReferences.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbMissingReferences_MouseDown);
            this.lbMissingReferences.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbMissingReferences_MouseMove);
            this.lbMissingReferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbMissingReferences_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(12, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Missing References:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(666, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Unreferenced Columns";
            // 
            // lbAvailableColumns
            // 
            this.lbAvailableColumns.AllowDrop = true;
            this.lbAvailableColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAvailableColumns.FormattingEnabled = true;
            this.lbAvailableColumns.Location = new System.Drawing.Point(669, 111);
            this.lbAvailableColumns.Name = "lbAvailableColumns";
            this.lbAvailableColumns.Size = new System.Drawing.Size(300, 537);
            this.lbAvailableColumns.TabIndex = 2;
            this.lbAvailableColumns.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbAvailableColumns_DragDrop);
            this.lbAvailableColumns.DragEnter += new System.Windows.Forms.DragEventHandler(this.lbAvailableColumns_DragEnter);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(957, 72);
            this.label3.TabIndex = 4;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOk.Location = new System.Drawing.Point(321, 661);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(588, 661);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbOperations
            // 
            this.tbOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOperations.Location = new System.Drawing.Point(321, 111);
            this.tbOperations.Multiline = true;
            this.tbOperations.Name = "tbOperations";
            this.tbOperations.ReadOnly = true;
            this.tbOperations.Size = new System.Drawing.Size(342, 547);
            this.tbOperations.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(318, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Operations Performed:";
            // 
            // cbSelectAll
            // 
            this.cbSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectAll.AutoSize = true;
            this.cbSelectAll.Location = new System.Drawing.Point(214, 656);
            this.cbSelectAll.Name = "cbSelectAll";
            this.cbSelectAll.Size = new System.Drawing.Size(101, 17);
            this.cbSelectAll.TabIndex = 9;
            this.cbSelectAll.Text = "Select All/None";
            this.cbSelectAll.UseVisualStyleBackColor = true;
            this.cbSelectAll.CheckedChanged += new System.EventHandler(this.cbSelectAll_CheckedChanged);
            // 
            // ResolveMissingTargetPropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(972, 685);
            this.Controls.Add(this.cbSelectAll);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbOperations);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbAvailableColumns);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbMissingReferences);
            this.Name = "ResolveMissingTargetPropertiesForm";
            this.Text = "Resolve Missing Columns";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbMissingReferences;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbAvailableColumns;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbOperations;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbSelectAll;
    }
}