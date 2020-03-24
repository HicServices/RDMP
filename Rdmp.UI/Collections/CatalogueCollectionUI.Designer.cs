using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections
{
    partial class CatalogueCollectionUI : ILifetimeSubscriber
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CatalogueCollectionUI));
            this.tlvCatalogues = new BrightIdeasSoftware.TreeListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvFilters = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList_RightClickIcons = new System.Windows.Forms.ImageList(this.components);
            this.gbColdStorage = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.catalogueCollectionFilterUI1 = new Rdmp.UI.Collections.CatalogueCollectionFilterUI();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).BeginInit();
            this.gbColdStorage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlvCatalogues
            // 
            this.tlvCatalogues.AllColumns.Add(this.olvColumn1);
            this.tlvCatalogues.AllColumns.Add(this.olvFilters);
            this.tlvCatalogues.AllColumns.Add(this.olvOrder);
            this.tlvCatalogues.CellEditUseWholeCell = false;
            this.tlvCatalogues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.tlvCatalogues.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCatalogues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvCatalogues.HideSelection = false;
            this.tlvCatalogues.Location = new System.Drawing.Point(0, 0);
            this.tlvCatalogues.Name = "tlvCatalogues";
            this.tlvCatalogues.ShowGroups = false;
            this.tlvCatalogues.Size = new System.Drawing.Size(500, 405);
            this.tlvCatalogues.TabIndex = 0;
            this.tlvCatalogues.Text = "label1";
            this.tlvCatalogues.UseCompatibleStateImageBehavior = false;
            this.tlvCatalogues.UseFiltering = true;
            this.tlvCatalogues.View = System.Windows.Forms.View.Details;
            this.tlvCatalogues.VirtualMode = true;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn1.MinimumWidth = 100;
            this.olvColumn1.Text = "Catalogues";
            this.olvColumn1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn1.Width = 100;
            // 
            // olvFilters
            // 
            this.olvFilters.DisplayIndex = 1;
            this.olvFilters.IsVisible = false;
            this.olvFilters.Text = "Filters";
            // 
            // olvOrder
            // 
            this.olvOrder.IsEditable = false;
            this.olvOrder.IsVisible = false;
            this.olvOrder.Text = "Order";
            // 
            // imageList_RightClickIcons
            // 
            this.imageList_RightClickIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_RightClickIcons.ImageStream")));
            this.imageList_RightClickIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_RightClickIcons.Images.SetKeyName(0, "DLE");
            this.imageList_RightClickIcons.Images.SetKeyName(1, "DEM");
            this.imageList_RightClickIcons.Images.SetKeyName(2, "DQE");
            this.imageList_RightClickIcons.Images.SetKeyName(3, "LOG");
            this.imageList_RightClickIcons.Images.SetKeyName(4, "aggregates.png");
            // 
            // gbColdStorage
            // 
            this.gbColdStorage.Controls.Add(this.catalogueCollectionFilterUI1);
            this.gbColdStorage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbColdStorage.Location = new System.Drawing.Point(0, 414);
            this.gbColdStorage.Name = "gbColdStorage";
            this.gbColdStorage.Size = new System.Drawing.Size(500, 65);
            this.gbColdStorage.TabIndex = 1;
            this.gbColdStorage.TabStop = false;
            this.gbColdStorage.Text = "Show";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.tlvCatalogues);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 405);
            this.panel1.TabIndex = 0;
            // 
            // catalogueCollectionFilterUI1
            // 
            this.catalogueCollectionFilterUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.catalogueCollectionFilterUI1.Location = new System.Drawing.Point(3, 16);
            this.catalogueCollectionFilterUI1.Name = "catalogueCollectionFilterUI1";
            this.catalogueCollectionFilterUI1.Size = new System.Drawing.Size(494, 46);
            this.catalogueCollectionFilterUI1.TabIndex = 0;
            // 
            // CatalogueCollectionUI
            // 
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbColdStorage);
            this.Name = "CatalogueCollectionUI";
            this.Size = new System.Drawing.Size(500, 479);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).EndInit();
            this.gbColdStorage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvCatalogues;
        private ImageList imageList_RightClickIcons;
        private OLVColumn olvColumn1;
        private OLVColumn olvFilters;
        private GroupBox gbColdStorage;
        private Panel panel1;
        private OLVColumn olvOrder;
        private CatalogueCollectionFilterUI catalogueCollectionFilterUI1;
    }
}
