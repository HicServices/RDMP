namespace CatalogueManager.LocationsMenu.LocationAdjustment
{
    partial class LocationAdjustableObjectUI
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
            this.tbPropertyValue = new System.Windows.Forms.TextBox();
            this.lblObjectNameAndID = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbPropertyValue
            // 
            this.tbPropertyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPropertyValue.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbPropertyValue.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbPropertyValue.Location = new System.Drawing.Point(226, 3);
            this.tbPropertyValue.Name = "tbPropertyValue";
            this.tbPropertyValue.Size = new System.Drawing.Size(668, 20);
            this.tbPropertyValue.TabIndex = 0;
            this.tbPropertyValue.TextChanged += new System.EventHandler(this.tbPropertyValue_TextChanged);
            // 
            // lblObjectNameAndID
            // 
            this.lblObjectNameAndID.Location = new System.Drawing.Point(3, 3);
            this.lblObjectNameAndID.Name = "lblObjectNameAndID";
            this.lblObjectNameAndID.Size = new System.Drawing.Size(217, 23);
            this.lblObjectNameAndID.TabIndex = 1;
            this.lblObjectNameAndID.Text = "label1";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(900, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // LocationAdjustableObjectUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblObjectNameAndID);
            this.Controls.Add(this.tbPropertyValue);
            this.Name = "LocationAdjustableObjectUI";
            this.Size = new System.Drawing.Size(978, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbPropertyValue;
        private System.Windows.Forms.Label lblObjectNameAndID;
        private System.Windows.Forms.Button btnSave;
    }
}
