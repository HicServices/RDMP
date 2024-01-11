namespace Rdmp.UI.Collections
{
    partial class DatasetsCollectionUI
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


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tlvDatasets = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvFavourites
            // 
            this.tlvDatasets.AllColumns.Add(this.olvName);
            this.tlvDatasets.CellEditUseWholeCell = false;
            this.tlvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvDatasets.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDatasets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvDatasets.FullRowSelect = true;
            this.tlvDatasets.HideSelection = false;
            this.tlvDatasets.Location = new System.Drawing.Point(0, 0);
            this.tlvDatasets.Name = "tlvFavourites";
            this.tlvDatasets.ShowGroups = false;
            this.tlvDatasets.Size = new System.Drawing.Size(322, 557);
            this.tlvDatasets.TabIndex = 2;
            this.tlvDatasets.UseCompatibleStateImageBehavior = false;
            this.tlvDatasets.View = System.Windows.Forms.View.Details;
            this.tlvDatasets.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.CellEditUseWholeCell = true;
            this.olvName.Text = "Datasets";
            this.olvName.MinimumWidth = 100;
            // 
            // FavouritesCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvDatasets);
            this.Name = "DatasetsCollectionUI";
            this.Size = new System.Drawing.Size(322, 557);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).EndInit();
            this.ResumeLayout(false);
            this.Text = "DatasetsCollectionUI";
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvDatasets;
        private BrightIdeasSoftware.OLVColumn olvName;
    }
}