namespace Rdmp.UI.Wizard
{
    partial class SimpleParameterUI
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
            this.pbParameter = new System.Windows.Forms.PictureBox();
            this.lblParameterName = new System.Windows.Forms.Label();
            this.tbValue = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbParameter)).BeginInit();
            this.SuspendLayout();
            // 
            // pbParameter
            // 
            this.pbParameter.Location = new System.Drawing.Point(9, 3);
            this.pbParameter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbParameter.Name = "pbParameter";
            this.pbParameter.Size = new System.Drawing.Size(23, 23);
            this.pbParameter.TabIndex = 4;
            this.pbParameter.TabStop = false;
            // 
            // lblParameterName
            // 
            this.lblParameterName.AutoSize = true;
            this.lblParameterName.Location = new System.Drawing.Point(38, 6);
            this.lblParameterName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(93, 15);
            this.lblParameterName.TabIndex = 3;
            this.lblParameterName.Text = "ParameterName";
            // 
            // tbValue
            // 
            this.tbValue.Location = new System.Drawing.Point(199, 3);
            this.tbValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(116, 23);
            this.tbValue.TabIndex = 5;
            this.tbValue.TextChanged += new System.EventHandler(this.tbValue_TextChanged);
            // 
            // SimpleParameterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.pbParameter);
            this.Controls.Add(this.lblParameterName);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SimpleParameterUI";
            this.Size = new System.Drawing.Size(315, 31);
            ((System.ComponentModel.ISupportInitialize)(this.pbParameter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbParameter;
        private System.Windows.Forms.Label lblParameterName;
        public System.Windows.Forms.TextBox tbValue;
    }
}
