namespace ReusableUIComponents
{
    partial class KeywordHelpTextListbox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeywordHelpTextListbox));
            this.olvHelpSections = new BrightIdeasSoftware.ObjectListView();
            this.olvKeyword = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvHelpText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.olvHelpSections)).BeginInit();
            this.SuspendLayout();
            // 
            // olvHelpSections
            // 
            this.olvHelpSections.AllColumns.Add(this.olvKeyword);
            this.olvHelpSections.AllColumns.Add(this.olvHelpText);
            this.olvHelpSections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvHelpSections.BackColor = System.Drawing.SystemColors.Info;
            this.olvHelpSections.CellEditUseWholeCell = false;
            this.olvHelpSections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvKeyword,
            this.olvHelpText});
            this.olvHelpSections.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvHelpSections.Location = new System.Drawing.Point(0, 0);
            this.olvHelpSections.Name = "olvHelpSections";
            this.olvHelpSections.ShowGroups = false;
            this.olvHelpSections.Size = new System.Drawing.Size(756, 291);
            this.olvHelpSections.TabIndex = 6;
            this.olvHelpSections.UseCompatibleStateImageBehavior = false;
            this.olvHelpSections.View = System.Windows.Forms.View.Details;
            this.olvHelpSections.ItemActivate += new System.EventHandler(this.olvHelpSections_ItemActivate);
            // 
            // olvKeyword
            // 
            this.olvKeyword.AspectName = "Keyword";
            this.olvKeyword.Text = "Helpful Keywords*";
            this.olvKeyword.Width = 120;
            // 
            // olvHelpText
            // 
            this.olvHelpText.AspectName = "HelpText";
            this.olvHelpText.FillsFreeSpace = true;
            this.olvHelpText.Text = "HelpText (Double Click for popup)";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.SystemColors.Info;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 294);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(750, 32);
            this.label1.TabIndex = 7;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // KeywordHelpTextListbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.olvHelpSections);
            this.Name = "KeywordHelpTextListbox";
            this.Size = new System.Drawing.Size(756, 326);
            ((System.ComponentModel.ISupportInitialize)(this.olvHelpSections)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView olvHelpSections;
        private BrightIdeasSoftware.OLVColumn olvKeyword;
        private BrightIdeasSoftware.OLVColumn olvHelpText;
        private System.Windows.Forms.Label label1;
    }
}
