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
            this.cbShowDeprecated = new System.Windows.Forms.CheckBox();
            this.cbShowInternal = new System.Windows.Forms.CheckBox();
            this.cbProjectSpecific = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbShowDeprecated
            // 
            this.cbShowDeprecated.AutoSize = true;
            this.cbShowDeprecated.Location = new System.Drawing.Point(163, 3);
            this.cbShowDeprecated.Name = "cbShowDeprecated";
            this.cbShowDeprecated.Size = new System.Drawing.Size(82, 17);
            this.cbShowDeprecated.TabIndex = 6;
            this.cbShowDeprecated.Text = "Deprecated";
            this.cbShowDeprecated.UseVisualStyleBackColor = true;
            this.cbShowDeprecated.CheckedChanged += new System.EventHandler(this.OnCheckboxChanged);
            // 
            // cbShowInternal
            // 
            this.cbShowInternal.AutoSize = true;
            this.cbShowInternal.Location = new System.Drawing.Point(96, 3);
            this.cbShowInternal.Name = "cbShowInternal";
            this.cbShowInternal.Size = new System.Drawing.Size(61, 17);
            this.cbShowInternal.TabIndex = 7;
            this.cbShowInternal.Text = "Internal";
            this.cbShowInternal.UseVisualStyleBackColor = true;
            this.cbShowInternal.CheckedChanged += new System.EventHandler(this.OnCheckboxChanged);
            // 
            // cbProjectSpecific
            // 
            this.cbProjectSpecific.AutoSize = true;
            this.cbProjectSpecific.Location = new System.Drawing.Point(3, 26);
            this.cbProjectSpecific.Name = "cbProjectSpecific";
            this.cbProjectSpecific.Size = new System.Drawing.Size(100, 17);
            this.cbProjectSpecific.TabIndex = 8;
            this.cbProjectSpecific.Text = "Project Specific";
            this.cbProjectSpecific.UseVisualStyleBackColor = true;
            this.cbProjectSpecific.CheckedChanged += new System.EventHandler(this.OnCheckboxChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cbShowInternal);
            this.flowLayoutPanel1.Controls.Add(this.cbShowDeprecated);
            this.flowLayoutPanel1.Controls.Add(this.cbProjectSpecific);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(256, 69);
            this.flowLayoutPanel1.TabIndex = 10;
            // 
            // CatalogueCollectionFilterUI
            // 
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "CatalogueCollectionFilterUI";
            this.Size = new System.Drawing.Size(256, 69);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbShowDeprecated;
        private System.Windows.Forms.CheckBox cbShowInternal;
        private System.Windows.Forms.CheckBox cbProjectSpecific;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
