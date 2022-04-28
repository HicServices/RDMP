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
            this.suggestComboBox1 = new Rdmp.UI.SuggestComboBox();
            this.SuspendLayout();
            // 
            // lPick
            // 
            this.lPick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lPick.BackColor = System.Drawing.Color.White;
            this.lPick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lPick.Location = new System.Drawing.Point(440, 1);
            this.lPick.Margin = new System.Windows.Forms.Padding(0);
            this.lPick.Name = "lPick";
            this.lPick.Size = new System.Drawing.Size(23, 23);
            this.lPick.TabIndex = 11;
            this.lPick.Text = "...";
            this.lPick.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lPick.Click += new System.EventHandler(this.lPick_Click);
            // 
            // suggestComboBox1
            // 
            this.suggestComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.suggestComboBox1.FilterRule = null;
            this.suggestComboBox1.FormattingEnabled = true;
            this.suggestComboBox1.Location = new System.Drawing.Point(1, 1);
            this.suggestComboBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.suggestComboBox1.Name = "suggestComboBox1";
            this.suggestComboBox1.PropertySelector = null;
            this.suggestComboBox1.Size = new System.Drawing.Size(436, 23);
            this.suggestComboBox1.SuggestBoxHeight = 126;
            this.suggestComboBox1.SuggestListOrderRule = null;
            this.suggestComboBox1.TabIndex = 10;
            this.suggestComboBox1.TextUpdate += new System.EventHandler(this.suggestComboBox1_TextUpdate);
            // 
            // SelectIMapsDirectlyToDatabaseTableComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lPick);
            this.Controls.Add(this.suggestComboBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SelectIMapsDirectlyToDatabaseTableComboBox";
            this.Size = new System.Drawing.Size(464, 25);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lPick;
        private SuggestComboBox suggestComboBox1;
    }
}
