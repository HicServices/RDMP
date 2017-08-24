namespace CatalogueManager.ExtractionUIs.JoinsAndLookups
{
    partial class KeyDropLocationUI
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
            this.label = new System.Windows.Forms.Label();
            this.tbPk1 = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(0, 22);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(226, 13);
            this.label.TabIndex = 153;
            this.label.Text = "(Primary Key)";
            this.label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tbPk1
            // 
            this.tbPk1.AllowDrop = true;
            this.tbPk1.Location = new System.Drawing.Point(0, 0);
            this.tbPk1.Name = "tbPk1";
            this.tbPk1.ReadOnly = true;
            this.tbPk1.Size = new System.Drawing.Size(194, 20);
            this.tbPk1.TabIndex = 152;
            this.tbPk1.DragDrop += new System.Windows.Forms.DragEventHandler(this.tbPk1_DragDrop);
            this.tbPk1.DragEnter += new System.Windows.Forms.DragEventHandler(this.tbPk1_DragEnter);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(196, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(26, 26);
            this.btnClear.TabIndex = 154;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // KeyDropLocationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.label);
            this.Controls.Add(this.tbPk1);
            this.Name = "KeyDropLocationUI";
            this.Size = new System.Drawing.Size(226, 35);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label;
        private System.Windows.Forms.TextBox tbPk1;
        private System.Windows.Forms.Button btnClear;
    }
}
