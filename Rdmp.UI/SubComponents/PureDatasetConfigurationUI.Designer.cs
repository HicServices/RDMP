namespace Rdmp.UI.SubComponents
{
    partial class PureDatasetConfigurationUI
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
            label1 = new System.Windows.Forms.Label();
            columnButtonRenderer1 = new BrightIdeasSoftware.ColumnButtonRenderer();
            btnViewOnPure = new System.Windows.Forms.Button();
            lblUrl = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(42, 80);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(542, 15);
            label1.TabIndex = 0;
            label1.Text = "Pure Datasets are not currently managable via RDMP. Please visit the following link to edit this dataset";
            label1.Click += label1_Click;
            // 
            // columnButtonRenderer1
            // 
            columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
            // 
            // btnViewOnPure
            // 
            btnViewOnPure.Enabled = false;
            btnViewOnPure.Location = new System.Drawing.Point(42, 40);
            btnViewOnPure.Name = "btnViewOnPure";
            btnViewOnPure.Size = new System.Drawing.Size(121, 23);
            btnViewOnPure.TabIndex = 1;
            btnViewOnPure.Text = "View on Pure";
            btnViewOnPure.UseVisualStyleBackColor = true;
            btnViewOnPure.Click += btnViewOnPure_Click;
            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new System.Drawing.Point(47, 104);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new System.Drawing.Size(0, 15);
            lblUrl.TabIndex = 2;
            // 
            // PureDatasetConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblUrl);
            Controls.Add(btnViewOnPure);
            Controls.Add(label1);
            Name = "PureDatasetConfigurationUI";
            Size = new System.Drawing.Size(955, 513);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
        private System.Windows.Forms.Button btnViewOnPure;
        private System.Windows.Forms.Label lblUrl;
    }
}
