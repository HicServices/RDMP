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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeywordHelpTextListbox));
            this.olvHelpSections = new BrightIdeasSoftware.ObjectListView();
            this.olvKeyword = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvHelpText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.olvHelpSections)).BeginInit();
            this.SuspendLayout();
            // 
            // olvHelpSections
            // 
            this.olvHelpSections.AllColumns.Add(this.olvKeyword);
            this.olvHelpSections.AllColumns.Add(this.olvHelpText);
            this.olvHelpSections.CellEditUseWholeCell = false;
            this.olvHelpSections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvKeyword,
            this.olvHelpText});
            this.olvHelpSections.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvHelpSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvHelpSections.Location = new System.Drawing.Point(0, 0);
            this.olvHelpSections.Name = "olvHelpSections";
            this.olvHelpSections.ShowGroups = false;
            this.olvHelpSections.Size = new System.Drawing.Size(756, 326);
            this.olvHelpSections.TabIndex = 6;
            this.olvHelpSections.UseCompatibleStateImageBehavior = false;
            this.olvHelpSections.View = System.Windows.Forms.View.Details;
            this.olvHelpSections.ItemActivate += new System.EventHandler(this.olvHelpSections_ItemActivate);
            // 
            // olvKeyword
            // 
            this.olvKeyword.AspectName = "Keyword";
            this.olvKeyword.Text = "Keyword";
            this.olvKeyword.Width = 120;
            // 
            // olvHelpText
            // 
            this.olvHelpText.AspectName = "HelpText";
            this.olvHelpText.FillsFreeSpace = true;
            this.olvHelpText.Text = "HelpText (Double Click for popup)";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Information.ico");
            // 
            // KeywordHelpTextListbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private System.Windows.Forms.ImageList imageList1;
    }
}
