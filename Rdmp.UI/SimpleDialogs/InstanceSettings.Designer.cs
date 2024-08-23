namespace Rdmp.UI.SimpleDialogs
{
    partial class InstanceSettings
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
            components = new System.ComponentModel.Container();
            cbAutoSuggestProjectNumbers = new System.Windows.Forms.CheckBox();
            instanceSettingsToolTips = new System.Windows.Forms.ToolTip(components);
            label1 = new System.Windows.Forms.Label();
            cbCohortVersioningOnCommit = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // cbAutoSuggestProjectNumbers
            // 
            cbAutoSuggestProjectNumbers.AutoSize = true;
            cbAutoSuggestProjectNumbers.Location = new System.Drawing.Point(13, 40);
            cbAutoSuggestProjectNumbers.Name = "cbAutoSuggestProjectNumbers";
            cbAutoSuggestProjectNumbers.Size = new System.Drawing.Size(364, 19);
            cbAutoSuggestProjectNumbers.TabIndex = 0;
            cbAutoSuggestProjectNumbers.Text = "Automatically Suggest Project Numbers During Project Creation";
            cbAutoSuggestProjectNumbers.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 9);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(308, 15);
            label1.TabIndex = 2;
            label1.Text = "Settings will automatically be Saved as you change them ";
            // 
            // cbCohortVersioningOnCommit
            // 
            cbCohortVersioningOnCommit.AutoSize = true;
            cbCohortVersioningOnCommit.Location = new System.Drawing.Point(12, 65);
            cbCohortVersioningOnCommit.Name = "cbCohortVersioningOnCommit";
            cbCohortVersioningOnCommit.Size = new System.Drawing.Size(364, 19);
            cbCohortVersioningOnCommit.TabIndex = 3;
            cbCohortVersioningOnCommit.Text = "Prompt user to create a new version of the cohort before committing it";
            cbCohortVersioningOnCommit.UseVisualStyleBackColor = true;
            // 
            // InstanceSettings
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(cbCohortVersioningOnCommit);
            Controls.Add(label1);
            Controls.Add(cbAutoSuggestProjectNumbers);
            Name = "InstanceSettings";
            Text = "InstanceSettings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox cbAutoSuggestProjectNumbers;
        private System.Windows.Forms.ToolTip instanceSettingsToolTips;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbCohortVersioningOnCommit;
    }
}