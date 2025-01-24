namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    partial class DropdownOptionsChipDisplay
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
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            comboBox1 = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 29);
            flowLayoutPanel1.MaximumSize = new System.Drawing.Size(400, 400);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(283, 51);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(0, 0);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(283, 23);
            comboBox1.TabIndex = 2;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // DropdownOptionsChipDisplay
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(comboBox1);
            Controls.Add(flowLayoutPanel1);
            Location = new System.Drawing.Point(0, 40);
            MaximumSize = new System.Drawing.Size(400, 300);
            MinimumSize = new System.Drawing.Size(100, 20);
            Name = "DropdownOptionsChipDisplay";
            Size = new System.Drawing.Size(303, 83);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}
