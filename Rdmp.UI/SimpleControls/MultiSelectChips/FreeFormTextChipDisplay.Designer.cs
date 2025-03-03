namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    partial class FreeFormTextChipDisplay
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
            textBox1 = new System.Windows.Forms.TextBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            textBox1.Location = new System.Drawing.Point(0, 0);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(300, 23);
            textBox1.TabIndex = 0;
            textBox1.KeyPress += textBox1_KeyPressed;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 23);
            flowLayoutPanel1.MaximumSize = new System.Drawing.Size(350, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(300, 0);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // FreeFormTextChipDisplay
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            AutoSize = true;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(textBox1);
            Location = new System.Drawing.Point(0, 40);
            MaximumSize = new System.Drawing.Size(300, 0);
            MinimumSize = new System.Drawing.Size(100, 20);
            Name = "FreeFormTextChipDisplay";
            Size = new System.Drawing.Size(300, 26);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
