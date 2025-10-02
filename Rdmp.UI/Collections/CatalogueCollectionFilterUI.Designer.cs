namespace Rdmp.UI.Collections
{
    partial class CatalogueCollectionFilterUI
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
            cbProjectSpecific = new System.Windows.Forms.CheckBox();
            cbShowDeprecated = new System.Windows.Forms.CheckBox();
            cbShowInternal = new System.Windows.Forms.CheckBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // cbProjectSpecific
            // 
            cbProjectSpecific.Image = Properties.Resources.project_icon_pink;
            cbProjectSpecific.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            cbProjectSpecific.Location = new System.Drawing.Point(197, 3);
            cbProjectSpecific.Name = "cbProjectSpecific";
            cbProjectSpecific.Size = new System.Drawing.Size(122, 30);
            cbProjectSpecific.TabIndex = 8;
            cbProjectSpecific.Text = "     Project Specific";
            cbProjectSpecific.UseVisualStyleBackColor = true;
            cbProjectSpecific.CheckedChanged += OnCheckboxChanged;
            // 
            // cbShowDeprecated
            // 
            cbShowDeprecated.Image = Properties.Resources.deprecated_icon;
            cbShowDeprecated.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            cbShowDeprecated.Location = new System.Drawing.Point(90, 3);
            cbShowDeprecated.Name = "cbShowDeprecated";
            cbShowDeprecated.Size = new System.Drawing.Size(101, 30);
            cbShowDeprecated.TabIndex = 6;
            cbShowDeprecated.Text = "     Deprecated";
            cbShowDeprecated.UseVisualStyleBackColor = true;
            cbShowDeprecated.CheckedChanged += OnCheckboxChanged;
            // 
            // cbShowInternal
            // 
            cbShowInternal.Image = Properties.Resources.internal_icon;
            cbShowInternal.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            cbShowInternal.Location = new System.Drawing.Point(3, 3);
            cbShowInternal.Name = "cbShowInternal";
            cbShowInternal.Size = new System.Drawing.Size(81, 30);
            cbShowInternal.TabIndex = 7;
            cbShowInternal.Text = "     Internal";
            cbShowInternal.UseVisualStyleBackColor = true;
            cbShowInternal.CheckedChanged += OnCheckboxChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(cbShowInternal);
            flowLayoutPanel1.Controls.Add(cbShowDeprecated);
            flowLayoutPanel1.Controls.Add(cbProjectSpecific);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(323, 80);
            flowLayoutPanel1.TabIndex = 10;
            // 
            // CatalogueCollectionFilterUI
            // 
            Controls.Add(flowLayoutPanel1);
            Name = "CatalogueCollectionFilterUI";
            Size = new System.Drawing.Size(323, 80);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbShowDeprecated;
        private System.Windows.Forms.CheckBox cbShowInternal;
        private System.Windows.Forms.CheckBox cbProjectSpecific;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
