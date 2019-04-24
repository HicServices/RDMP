namespace CohortManager.Wizard
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
            this.pbParameter.Location = new System.Drawing.Point(2, 0);
            this.pbParameter.Name = "pbParameter";
            this.pbParameter.Size = new System.Drawing.Size(20, 20);
            this.pbParameter.TabIndex = 4;
            this.pbParameter.TabStop = false;
            // 
            // lblParameterName
            // 
            this.lblParameterName.AutoSize = true;
            this.lblParameterName.Location = new System.Drawing.Point(28, 4);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(83, 13);
            this.lblParameterName.TabIndex = 3;
            this.lblParameterName.Text = "ParameterName";
            // 
            // tbValue
            // 
            this.tbValue.Location = new System.Drawing.Point(167, 1);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(100, 20);
            this.tbValue.TabIndex = 5;
            this.tbValue.TextChanged += new System.EventHandler(this.tbValue_TextChanged);
            // 
            // SimpleParameterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.pbParameter);
            this.Controls.Add(this.lblParameterName);
            this.Name = "SimpleParameterUI";
            this.Size = new System.Drawing.Size(270, 21);
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
