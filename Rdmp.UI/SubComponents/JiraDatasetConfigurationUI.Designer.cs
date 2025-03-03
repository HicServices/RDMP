namespace Rdmp.UI.SubComponents
{
    partial class JiraDatasetConfigurationUI
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
            columnButtonRenderer1 = new BrightIdeasSoftware.ColumnButtonRenderer();
            btnViewOnJira = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // columnButtonRenderer1
            // 
            columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
            // 
            // btnViewOnJira
            // 
            btnViewOnJira.Enabled = false;
            btnViewOnJira.Location = new System.Drawing.Point(29, 26);
            btnViewOnJira.Name = "btnViewOnJira";
            btnViewOnJira.Size = new System.Drawing.Size(121, 23);
            btnViewOnJira.TabIndex = 1;
            btnViewOnJira.Text = "View on Jira";
            btnViewOnJira.UseVisualStyleBackColor = true;
            btnViewOnJira.Click += btnViewOnPure_Click;
            // 
            // JiraDatasetConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(btnViewOnJira);
            Name = "JiraDatasetConfigurationUI";
            Size = new System.Drawing.Size(955, 648);
            Load += PureDatasetConfigurationUI_Load;
            ResumeLayout(false);
        }

        #endregion
        private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
        private System.Windows.Forms.Button btnViewOnJira;
    }
}
