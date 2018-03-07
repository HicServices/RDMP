namespace ReusableUIComponents.Settings
{
    partial class UserSettingsFileUI
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
            this.cbShowHomeOnStartup = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbEmphasiseOnTabChanged = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbShowHomeOnStartup
            // 
            this.cbShowHomeOnStartup.AutoSize = true;
            this.cbShowHomeOnStartup.Location = new System.Drawing.Point(39, 51);
            this.cbShowHomeOnStartup.Name = "cbShowHomeOnStartup";
            this.cbShowHomeOnStartup.Size = new System.Drawing.Size(138, 17);
            this.cbShowHomeOnStartup.TabIndex = 0;
            this.cbShowHomeOnStartup.Text = "Show Home On Startup";
            this.cbShowHomeOnStartup.UseVisualStyleBackColor = true;
            this.cbShowHomeOnStartup.CheckedChanged += new System.EventHandler(this.cbShowHomeOnStartup_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(274, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Settings will automatically be Saved as you change them";
            // 
            // cbEmphasiseOnTabChanged
            // 
            this.cbEmphasiseOnTabChanged.AutoSize = true;
            this.cbEmphasiseOnTabChanged.Location = new System.Drawing.Point(39, 74);
            this.cbEmphasiseOnTabChanged.Name = "cbEmphasiseOnTabChanged";
            this.cbEmphasiseOnTabChanged.Size = new System.Drawing.Size(215, 17);
            this.cbEmphasiseOnTabChanged.TabIndex = 2;
            this.cbEmphasiseOnTabChanged.Text = "Show Object Collection On Tab Change";
            this.cbEmphasiseOnTabChanged.UseVisualStyleBackColor = true;
            this.cbEmphasiseOnTabChanged.CheckedChanged += new System.EventHandler(this.cbEmphasiseOnTabChanged_CheckedChanged);
            // 
            // UserSettingsFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 565);
            this.Controls.Add(this.cbEmphasiseOnTabChanged);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbShowHomeOnStartup);
            this.Name = "UserSettingsFileUI";
            this.Text = "UserSettingsUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbShowHomeOnStartup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbEmphasiseOnTabChanged;
    }
}