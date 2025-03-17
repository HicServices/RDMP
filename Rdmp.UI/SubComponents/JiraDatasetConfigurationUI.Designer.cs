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
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // columnButtonRenderer1
            // 
            columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(28, 24);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(247, 15);
            label1.TabIndex = 0;
            label1.Text = "This Jira Dataset is controlled via automation. ";
            // 
            // JiraDatasetConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(label1);
            Name = "JiraDatasetConfigurationUI";
            Size = new System.Drawing.Size(955, 648);
            Load += PureDatasetConfigurationUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
        private System.Windows.Forms.Label label1;
    }
}
