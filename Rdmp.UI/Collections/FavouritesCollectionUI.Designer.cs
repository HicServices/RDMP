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
            components = new System.ComponentModel.Container();
            tlvFavourites = new BrightIdeasSoftware.TreeListView();
            olvName = new BrightIdeasSoftware.OLVColumn();
            tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvFavourites).BeginInit();
            SuspendLayout();
            // 
            // tlvFavourites
            // 
            tlvFavourites.AllColumns.Add(olvName);
            tlvFavourites.CellEditUseWholeCell = false;
            tlvFavourites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName });
            tlvFavourites.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvFavourites.FullRowSelect = true;
            tlvFavourites.Location = new System.Drawing.Point(0, 0);
            tlvFavourites.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvFavourites.Name = "tlvFavourites";
            tlvFavourites.ShowGroups = false;
            tlvFavourites.Size = new System.Drawing.Size(376, 643);
            tlvFavourites.TabIndex = 2;
            tlvFavourites.UseCompatibleStateImageBehavior = false;
            tlvFavourites.View = System.Windows.Forms.View.Details;
            tlvFavourites.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.CellEditUseWholeCell = true;
            olvName.MinimumWidth = 100;
            olvName.Text = "Favourites";
            olvName.Width = 100;
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 620);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(376, 23);
            tbFilter.TabIndex = 3;
            // 
            // FavouritesCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvFavourites);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FavouritesCollectionUI";
            Size = new System.Drawing.Size(376, 643);
            ((System.ComponentModel.ISupportInitialize)tlvFavourites).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvFavourites;
        private BrightIdeasSoftware.OLVColumn olvName;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
