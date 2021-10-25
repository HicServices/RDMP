namespace Rdmp.UI.Collections
{
    partial class FavouritesCollectionUI
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
            this.tlvFavourites = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvFavourites)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvFavourites
            // 
            this.tlvFavourites.AllColumns.Add(this.olvName);
            this.tlvFavourites.CellEditUseWholeCell = false;
            this.tlvFavourites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvFavourites.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvFavourites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvFavourites.FullRowSelect = true;
            this.tlvFavourites.HideSelection = false;
            this.tlvFavourites.Location = new System.Drawing.Point(0, 0);
            this.tlvFavourites.Name = "tlvFavourites";
            this.tlvFavourites.ShowGroups = false;
            this.tlvFavourites.Size = new System.Drawing.Size(322, 557);
            this.tlvFavourites.TabIndex = 2;
            this.tlvFavourites.UseCompatibleStateImageBehavior = false;
            this.tlvFavourites.View = System.Windows.Forms.View.Details;
            this.tlvFavourites.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.CellEditUseWholeCell = true;
            this.olvName.Text = "Favourites";
            this.olvName.MinimumWidth = 100;
            // 
            // FavouritesCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvFavourites);
            this.Name = "FavouritesCollectionUI";
            this.Size = new System.Drawing.Size(322, 557);
            ((System.ComponentModel.ISupportInitialize)(this.tlvFavourites)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvFavourites;
        private BrightIdeasSoftware.OLVColumn olvName;
    }
}
