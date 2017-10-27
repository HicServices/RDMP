namespace RDMPObjectVisualisation.DataObjects
{
    partial class UnknownObjectVisualisation
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
            this.lblType = new System.Windows.Forms.Label();
            this.lblToString = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(3, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(41, 13);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "lblType";
            // 
            // lblToString
            // 
            this.lblToString.AutoSize = true;
            this.lblToString.Location = new System.Drawing.Point(3, 47);
            this.lblToString.Name = "lblToString";
            this.lblToString.Size = new System.Drawing.Size(57, 13);
            this.lblToString.TabIndex = 1;
            this.lblToString.Text = "lblToString";
            // 
            // UnknownObjectVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lblToString);
            this.Controls.Add(this.lblType);
            this.Name = "UnknownObjectVisualisation";
            this.Size = new System.Drawing.Size(98, 98);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblToString;
    }
}
