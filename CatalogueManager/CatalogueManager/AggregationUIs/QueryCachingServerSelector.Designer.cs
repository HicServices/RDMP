namespace CatalogueManager.AggregationUIs
{
    partial class QueryCachingServerSelector
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
            this.btnCreateNewQueryCachingDatabase = new System.Windows.Forms.Button();
            this.ddSelectQueryCachingDatabase = new System.Windows.Forms.ComboBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCreateNewQueryCachingDatabase
            // 
            this.btnCreateNewQueryCachingDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewQueryCachingDatabase.Location = new System.Drawing.Point(602, 0);
            this.btnCreateNewQueryCachingDatabase.Name = "btnCreateNewQueryCachingDatabase";
            this.btnCreateNewQueryCachingDatabase.Size = new System.Drawing.Size(85, 21);
            this.btnCreateNewQueryCachingDatabase.TabIndex = 0;
            this.btnCreateNewQueryCachingDatabase.Text = "CreateNew...";
            this.btnCreateNewQueryCachingDatabase.UseVisualStyleBackColor = true;
            this.btnCreateNewQueryCachingDatabase.Click += new System.EventHandler(this.btnCreateNewQueryCachingDatabase_Click);
            // 
            // ddSelectQueryCachingDatabase
            // 
            this.ddSelectQueryCachingDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddSelectQueryCachingDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddSelectQueryCachingDatabase.FormattingEnabled = true;
            this.ddSelectQueryCachingDatabase.Location = new System.Drawing.Point(81, 0);
            this.ddSelectQueryCachingDatabase.Name = "ddSelectQueryCachingDatabase";
            this.ddSelectQueryCachingDatabase.Size = new System.Drawing.Size(440, 21);
            this.ddSelectQueryCachingDatabase.TabIndex = 1;
            this.ddSelectQueryCachingDatabase.SelectedIndexChanged += new System.EventHandler(this.ddSelectQueryCachingDatabase_SelectedIndexChanged);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(549, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(54, 21);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Query Cache:";
            // 
            // QueryCachingServerSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddSelectQueryCachingDatabase);
            this.Controls.Add(this.btnCreateNewQueryCachingDatabase);
            this.Controls.Add(this.btnClear);
            this.Name = "QueryCachingServerSelector";
            this.Size = new System.Drawing.Size(687, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreateNewQueryCachingDatabase;
        private System.Windows.Forms.ComboBox ddSelectQueryCachingDatabase;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label1;
    }
}
