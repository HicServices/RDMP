namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    partial class Chip
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Chip));
            lblText = new System.Windows.Forms.Label();
            btnClear = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lblText
            // 
            lblText.AutoSize = true;
            lblText.Location = new System.Drawing.Point(4, 4);
            lblText.Name = "lblText";
            lblText.Size = new System.Drawing.Size(38, 15);
            lblText.TabIndex = 0;
            lblText.Text = "label1";
            // 
            // btnClear
            // 
            btnClear.BackgroundImage = (System.Drawing.Image)resources.GetObject("btnClear.BackgroundImage");
            btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClear.Location = new System.Drawing.Point(46, 2);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(20, 20);
            btnClear.TabIndex = 1;
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // Chip
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlDark;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            Controls.Add(btnClear);
            Controls.Add(lblText);
            Name = "Chip";
            Size = new System.Drawing.Size(76, 24);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Button btnClear;
    }
}
