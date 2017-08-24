namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
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
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.SuspendLayout();
            // 
            // cbxValue
            // 
            this.cbxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxValue.FormattingEnabled = true;
            this.cbxValue.Location = new System.Drawing.Point(3, 3);
            this.cbxValue.Name = "cbxValue";
            this.cbxValue.Size = new System.Drawing.Size(510, 21);
            this.cbxValue.TabIndex = 18;
            this.cbxValue.SelectedIndexChanged += new System.EventHandler(this.cbxValue_SelectedIndexChanged);
            this.cbxValue.TextChanged += new System.EventHandler(this.cbxValue_TextChanged);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(519, 0);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 19;
            // 
            // ArgumentValueComboBoxUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.cbxValue);
            this.Name = "ArgumentValueComboBoxUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxValue;
        private ReusableUIComponents.RAGSmiley ragSmiley1;


    }
}
