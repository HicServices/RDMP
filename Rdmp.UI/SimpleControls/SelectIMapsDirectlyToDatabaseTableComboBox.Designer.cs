namespace Rdmp.UI.SimpleControls
{
    partial class SelectIMapsDirectlyToDatabaseTableComboBox
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
            this.lPick = new System.Windows.Forms.Label();
            this.suggestComboBox1 = new ReusableUIComponents.SuggestComboBox();
            this.SuspendLayout();
            // 
            // lPick
            // 
            this.lPick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lPick.BackColor = System.Drawing.Color.White;
            this.lPick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lPick.Location = new System.Drawing.Point(377, 1);
            this.lPick.Margin = new System.Windows.Forms.Padding(0);
            this.lPick.Name = "lPick";
            this.lPick.Size = new System.Drawing.Size(18, 21);
            this.lPick.TabIndex = 11;
            this.lPick.Text = "...";
            this.lPick.Click += new System.EventHandler(this.lPick_Click);
            // 
            // suggestComboBox1
            // 
            this.suggestComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.suggestComboBox1.FilterRule = null;
            this.suggestComboBox1.FormattingEnabled = true;
            this.suggestComboBox1.Location = new System.Drawing.Point(2, 1);
            this.suggestComboBox1.Name = "suggestComboBox1";
            this.suggestComboBox1.PropertySelector = null;
            this.suggestComboBox1.Size = new System.Drawing.Size(373, 21);
            this.suggestComboBox1.SuggestBoxHeight = 96;
            this.suggestComboBox1.SuggestListOrderRule = null;
            this.suggestComboBox1.TabIndex = 10;
            this.suggestComboBox1.TextUpdate += new System.EventHandler(this.suggestComboBox1_TextUpdate);
            // 
            // SelectIMapsDirectlyToDatabaseTableComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lPick);
            this.Controls.Add(this.suggestComboBox1);
            this.Name = "SelectIMapsDirectlyToDatabaseTableComboBox";
            this.Size = new System.Drawing.Size(397, 24);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lPick;
        private ReusableUIComponents.SuggestComboBox suggestComboBox1;
    }
}
