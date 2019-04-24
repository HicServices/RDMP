namespace CatalogueManager.Validation
{
    partial class SecondaryConstraintUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecondaryConstraintUI));
            this.lblType = new System.Windows.Forms.Label();
            this.lblConsequence = new System.Windows.Forms.Label();
            this.cbxConsequence = new System.Windows.Forms.ComboBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblException = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.Location = new System.Drawing.Point(36, 6);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(57, 16);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "TypeX ";
            // 
            // lblConsequence
            // 
            this.lblConsequence.AutoSize = true;
            this.lblConsequence.Location = new System.Drawing.Point(177, 8);
            this.lblConsequence.Name = "lblConsequence";
            this.lblConsequence.Size = new System.Drawing.Size(76, 13);
            this.lblConsequence.TabIndex = 2;
            this.lblConsequence.Text = "Consequence:";
            // 
            // cbxConsequence
            // 
            this.cbxConsequence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxConsequence.FormattingEnabled = true;
            this.cbxConsequence.Location = new System.Drawing.Point(259, 5);
            this.cbxConsequence.Name = "cbxConsequence";
            this.cbxConsequence.Size = new System.Drawing.Size(181, 21);
            this.cbxConsequence.TabIndex = 1;
            this.cbxConsequence.SelectedIndexChanged += new System.EventHandler(this.cbxConsequence_SelectedIndexChanged);
            // 
            // btnDelete
            // 
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(6, 1);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(27, 27);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblException
            // 
            this.lblException.AutoSize = true;
            this.lblException.ForeColor = System.Drawing.Color.Red;
            this.lblException.Location = new System.Drawing.Point(6, 65);
            this.lblException.Name = "lblException";
            this.lblException.Size = new System.Drawing.Size(0, 13);
            this.lblException.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(977, 36);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblType);
            this.panel1.Controls.Add(this.lblConsequence);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.cbxConsequence);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(971, 30);
            this.panel1.TabIndex = 5;
            // 
            // SecondaryConstraintUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.lblException);
            this.Name = "SecondaryConstraintUI";
            this.Size = new System.Drawing.Size(977, 36);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ComboBox cbxConsequence;
        private System.Windows.Forms.Label lblConsequence;
        private System.Windows.Forms.Label lblException;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
    }
}
