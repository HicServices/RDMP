namespace Rdmp.UI.SubComponents
{
    partial class CreateExternalDatasetDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox3 = new System.Windows.Forms.GroupBox();
            cbPovider = new System.Windows.Forms.ComboBox();
            button1 = new System.Windows.Forms.Button();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cbPovider);
            groupBox3.Location = new System.Drawing.Point(12, 8);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(200, 60);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Dataset Provider";
            // 
            // cbPovider
            // 
            cbPovider.FormattingEnabled = true;
            cbPovider.Location = new System.Drawing.Point(6, 22);
            cbPovider.Name = "cbPovider";
            cbPovider.Size = new System.Drawing.Size(188, 23);
            cbPovider.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(123, 74);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(89, 23);
            button1.TabIndex = 3;
            button1.Text = "Create";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // CreateExternalDatasetDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(303, 149);
            Controls.Add(button1);
            Controls.Add(groupBox3);
            Name = "CreateExternalDatasetDialog";
            Text = "Create Dataset";
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cbPovider;
        private System.Windows.Forms.Button button1;
    }
}