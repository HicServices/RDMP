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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ddSelectQueryCachingDatabase = new System.Windows.Forms.ComboBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCreateNewQueryCachingDatabase
            // 
            this.btnCreateNewQueryCachingDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewQueryCachingDatabase.Location = new System.Drawing.Point(625, 19);
            this.btnCreateNewQueryCachingDatabase.Name = "btnCreateNewQueryCachingDatabase";
            this.btnCreateNewQueryCachingDatabase.Size = new System.Drawing.Size(85, 23);
            this.btnCreateNewQueryCachingDatabase.TabIndex = 0;
            this.btnCreateNewQueryCachingDatabase.Text = "CreateNew...";
            this.btnCreateNewQueryCachingDatabase.UseVisualStyleBackColor = true;
            this.btnCreateNewQueryCachingDatabase.Click += new System.EventHandler(this.btnCreateNewQueryCachingDatabase_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ddSelectQueryCachingDatabase);
            this.groupBox1.Controls.Add(this.btnClear);
            this.groupBox1.Controls.Add(this.btnCreateNewQueryCachingDatabase);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(716, 77);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Query Caching Database (Must be on the same server as your data/aggregates)";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.ForeColor = System.Drawing.Color.Firebrick;
            this.label1.Location = new System.Drawing.Point(6, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(704, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "(The Cache will have sensitive data in it since it is used to cache aggregates an" +
    "d cohort identifiaction data so should have the same access controls as your mai" +
    "n repository)";
            // 
            // ddSelectQueryCachingDatabase
            // 
            this.ddSelectQueryCachingDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddSelectQueryCachingDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddSelectQueryCachingDatabase.FormattingEnabled = true;
            this.ddSelectQueryCachingDatabase.Location = new System.Drawing.Point(6, 21);
            this.ddSelectQueryCachingDatabase.Name = "ddSelectQueryCachingDatabase";
            this.ddSelectQueryCachingDatabase.Size = new System.Drawing.Size(555, 21);
            this.ddSelectQueryCachingDatabase.TabIndex = 1;
            this.ddSelectQueryCachingDatabase.SelectedIndexChanged += new System.EventHandler(this.ddSelectQueryCachingDatabase_SelectedIndexChanged);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(567, 19);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(54, 23);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // QueryCachingServerSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "QueryCachingServerSelector";
            this.Size = new System.Drawing.Size(716, 77);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateNewQueryCachingDatabase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox ddSelectQueryCachingDatabase;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label1;
    }
}
