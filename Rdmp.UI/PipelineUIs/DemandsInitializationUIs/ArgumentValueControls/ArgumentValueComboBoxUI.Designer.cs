namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueComboBoxUI
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
            this.cbxValue = new System.Windows.Forms.ComboBox();
            this.btnPick = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxValue
            // 
            this.cbxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxValue.FormattingEnabled = true;
            this.cbxValue.Location = new System.Drawing.Point(3, 3);
            this.cbxValue.Name = "cbxValue";
            this.cbxValue.Size = new System.Drawing.Size(490, 21);
            this.cbxValue.TabIndex = 18;
            this.cbxValue.TextChanged += new System.EventHandler(this.cbxValue_TextChanged);
            // 
            // btnPick
            // 
            this.btnPick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPick.Location = new System.Drawing.Point(499, 4);
            this.btnPick.Name = "btnPick";
            this.btnPick.Size = new System.Drawing.Size(45, 21);
            this.btnPick.TabIndex = 20;
            this.btnPick.Text = "Pick";
            this.btnPick.UseVisualStyleBackColor = true;
            this.btnPick.Click += new System.EventHandler(this.btnPick_Click);
            // 
            // ArgumentValueComboBoxUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnPick);
            this.Controls.Add(this.cbxValue);
            this.Name = "ArgumentValueComboBoxUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxValue;
        private System.Windows.Forms.Button btnPick;


    }
}
