namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    partial class ConfigureCatalogueExtractabilityUI
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
            this.gbMarkAllExtractable = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.ddExtractionIdentifier = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbMakeAllColumnsExtractable = new System.Windows.Forms.CheckBox();
            this.gbMarkAllExtractable.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMarkAllExtractable
            // 
            this.gbMarkAllExtractable.Controls.Add(this.btnClear);
            this.gbMarkAllExtractable.Controls.Add(this.ddExtractionIdentifier);
            this.gbMarkAllExtractable.Controls.Add(this.label2);
            this.gbMarkAllExtractable.Controls.Add(this.cbMakeAllColumnsExtractable);
            this.gbMarkAllExtractable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMarkAllExtractable.Location = new System.Drawing.Point(0, 0);
            this.gbMarkAllExtractable.Name = "gbMarkAllExtractable";
            this.gbMarkAllExtractable.Size = new System.Drawing.Size(708, 45);
            this.gbMarkAllExtractable.TabIndex = 7;
            this.gbMarkAllExtractable.TabStop = false;
            this.gbMarkAllExtractable.Text = "Extractability";
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(668, 16);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(40, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // ddExtractionIdentifier
            // 
            this.ddExtractionIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExtractionIdentifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionIdentifier.Enabled = false;
            this.ddExtractionIdentifier.FormattingEnabled = true;
            this.ddExtractionIdentifier.Location = new System.Drawing.Point(280, 17);
            this.ddExtractionIdentifier.Name = "ddExtractionIdentifier";
            this.ddExtractionIdentifier.Size = new System.Drawing.Size(382, 21);
            this.ddExtractionIdentifier.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(176, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Extraction Identifier:";
            // 
            // cbMakeAllColumnsExtractable
            // 
            this.cbMakeAllColumnsExtractable.AutoSize = true;
            this.cbMakeAllColumnsExtractable.Location = new System.Drawing.Point(7, 20);
            this.cbMakeAllColumnsExtractable.Name = "cbMakeAllColumnsExtractable";
            this.cbMakeAllColumnsExtractable.Size = new System.Drawing.Size(163, 17);
            this.cbMakeAllColumnsExtractable.TabIndex = 0;
            this.cbMakeAllColumnsExtractable.Text = "Make all columns extractable";
            this.cbMakeAllColumnsExtractable.UseVisualStyleBackColor = true;
            this.cbMakeAllColumnsExtractable.CheckedChanged += new System.EventHandler(this.cbMakeAllColumnsExtractable_CheckedChanged);
            // 
            // ConfigureCatalogueExtractabilityUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbMarkAllExtractable);
            this.Name = "ConfigureCatalogueExtractabilityUI";
            this.Size = new System.Drawing.Size(708, 45);
            this.gbMarkAllExtractable.ResumeLayout(false);
            this.gbMarkAllExtractable.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbMarkAllExtractable;
        private System.Windows.Forms.ComboBox ddExtractionIdentifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbMakeAllColumnsExtractable;
        private System.Windows.Forms.Button btnClear;
    }
}
