namespace Rdmp.UI.DataLoadUIs.ModuleUIs
{
    partial class DatabaseColumnRequestUI
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
            this.lblColumnName = new System.Windows.Forms.Label();
            this.tbExplicitDbType = new System.Windows.Forms.TextBox();
            this.ddManagedType = new System.Windows.Forms.ComboBox();
            this.nLength = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nAfterDecimal = new System.Windows.Forms.NumericUpDown();
            this.nBeforeDecimal = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nLength)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nAfterDecimal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nBeforeDecimal)).BeginInit();
            this.SuspendLayout();
            // 
            // lblColumnName
            // 
            this.lblColumnName.Location = new System.Drawing.Point(3, 1);
            this.lblColumnName.Name = "lblColumnName";
            this.lblColumnName.Size = new System.Drawing.Size(145, 20);
            this.lblColumnName.TabIndex = 0;
            this.lblColumnName.Text = "lblColumnName";
            this.lblColumnName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbExplicitDbType
            // 
            this.tbExplicitDbType.Location = new System.Drawing.Point(3, 3);
            this.tbExplicitDbType.Name = "tbExplicitDbType";
            this.tbExplicitDbType.Size = new System.Drawing.Size(120, 20);
            this.tbExplicitDbType.TabIndex = 1;
            this.tbExplicitDbType.TextChanged += new System.EventHandler(this.tbExplicitDbType_TextChanged);
            // 
            // ddManagedType
            // 
            this.ddManagedType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddManagedType.FormattingEnabled = true;
            this.ddManagedType.Location = new System.Drawing.Point(129, 2);
            this.ddManagedType.Name = "ddManagedType";
            this.ddManagedType.Size = new System.Drawing.Size(150, 21);
            this.ddManagedType.TabIndex = 2;
            this.ddManagedType.SelectedIndexChanged += new System.EventHandler(this.ddManagedType_SelectedIndexChanged);
            // 
            // nLength
            // 
            this.nLength.Location = new System.Drawing.Point(285, 4);
            this.nLength.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nLength.Name = "nLength";
            this.nLength.Size = new System.Drawing.Size(120, 20);
            this.nLength.TabIndex = 3;
            this.nLength.ValueChanged += new System.EventHandler(this.n_ValueChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.nAfterDecimal);
            this.panel1.Controls.Add(this.nBeforeDecimal);
            this.panel1.Controls.Add(this.nLength);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.ddManagedType);
            this.panel1.Controls.Add(this.tbExplicitDbType);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(151, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(591, 43);
            this.panel1.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(380, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "After Decimal";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(283, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Before Decimal";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(291, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Max Length if String";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(162, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Managed Type";
            // 
            // nAfterDecimal
            // 
            this.nAfterDecimal.Location = new System.Drawing.Point(376, 4);
            this.nAfterDecimal.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nAfterDecimal.Name = "nAfterDecimal";
            this.nAfterDecimal.Size = new System.Drawing.Size(85, 20);
            this.nAfterDecimal.TabIndex = 3;
            this.nAfterDecimal.ValueChanged += new System.EventHandler(this.n_ValueChanged);
            // 
            // nBeforeDecimal
            // 
            this.nBeforeDecimal.Location = new System.Drawing.Point(285, 4);
            this.nBeforeDecimal.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nBeforeDecimal.Name = "nBeforeDecimal";
            this.nBeforeDecimal.Size = new System.Drawing.Size(85, 20);
            this.nBeforeDecimal.TabIndex = 3;
            this.nBeforeDecimal.ValueChanged += new System.EventHandler(this.n_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Explicit DbType";
            // 
            // DatabaseColumnRequestUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblColumnName);
            this.Name = "DatabaseColumnRequestUI";
            this.Size = new System.Drawing.Size(742, 43);
            ((System.ComponentModel.ISupportInitialize)(this.nLength)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nAfterDecimal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nBeforeDecimal)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblColumnName;
        private System.Windows.Forms.TextBox tbExplicitDbType;
        private System.Windows.Forms.ComboBox ddManagedType;
        private System.Windows.Forms.NumericUpDown nLength;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nAfterDecimal;
        private System.Windows.Forms.NumericUpDown nBeforeDecimal;
    }
}
