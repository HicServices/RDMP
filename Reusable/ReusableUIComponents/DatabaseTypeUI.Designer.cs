namespace ReusableUIComponents
{
    partial class DatabaseTypeUI
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
            this.pbDatabaseProvider = new System.Windows.Forms.PictureBox();
            this.ddDatabaseType = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // pbDatabaseProvider
            // 
            this.pbDatabaseProvider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbDatabaseProvider.Location = new System.Drawing.Point(128, 3);
            this.pbDatabaseProvider.Name = "pbDatabaseProvider";
            this.pbDatabaseProvider.Size = new System.Drawing.Size(19, 19);
            this.pbDatabaseProvider.TabIndex = 170;
            this.pbDatabaseProvider.TabStop = false;
            // 
            // ddDatabaseType
            // 
            this.ddDatabaseType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseType.FormattingEnabled = true;
            this.ddDatabaseType.Location = new System.Drawing.Point(3, 3);
            this.ddDatabaseType.Name = "ddDatabaseType";
            this.ddDatabaseType.Size = new System.Drawing.Size(121, 21);
            this.ddDatabaseType.TabIndex = 169;
            this.ddDatabaseType.SelectedIndexChanged += new System.EventHandler(this.ddDatabaseType_SelectedIndexChanged);
            // 
            // DatabaseTypeUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbDatabaseProvider);
            this.Controls.Add(this.ddDatabaseType);
            this.Name = "DatabaseTypeUI";
            this.Size = new System.Drawing.Size(150, 28);
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbDatabaseProvider;
        private System.Windows.Forms.ComboBox ddDatabaseType;
    }
}
