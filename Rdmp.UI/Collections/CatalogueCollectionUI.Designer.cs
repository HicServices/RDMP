using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Refreshing;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    partial class CatalogueCollectionUI : ILifetimeSubscriber
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(CatalogueCollectionUI));
            tlvCatalogues = new TreeListView();
            olvColumn1 = new OLVColumn();
            olvColumn2 = new OLVColumn();
            attributesRenderer = new FlagRenderer();
            imageList_RightClickIcons = new ImageList(components);
            olvFilters = new OLVColumn();
            olvOrder = new OLVColumn();
            gbCatalogueFilters = new GroupBox();
            tbFilter = new TextBox();
            catalogueCollectionFilterUI1 = new CatalogueCollectionFilterUI();
            panel2 = new Panel();
            ((ISupportInitialize)tlvCatalogues).BeginInit();
            gbCatalogueFilters.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // tlvCatalogues
            // 
            tlvCatalogues.AllColumns.Add(olvColumn1);
            tlvCatalogues.AllColumns.Add(olvColumn2);
            tlvCatalogues.CellEditUseWholeCell = false;
            tlvCatalogues.Columns.AddRange(new ColumnHeader[] { olvColumn2, olvColumn1 });
            tlvCatalogues.Dock = DockStyle.Fill;
            tlvCatalogues.Location = new System.Drawing.Point(0, 0);
            tlvCatalogues.Name = "tlvCatalogues";
            tlvCatalogues.ShowGroups = false;
            tlvCatalogues.Size = new System.Drawing.Size(500, 404);
            tlvCatalogues.TabIndex = 0;
            tlvCatalogues.Text = "label1";
            tlvCatalogues.UseCompatibleStateImageBehavior = false;
            tlvCatalogues.UseFiltering = true;
            tlvCatalogues.View = View.Details;
            tlvCatalogues.VirtualMode = true;
            // 
            // olvColumn1
            // 
            olvColumn1.AspectName = "ToString";
            olvColumn1.HeaderTextAlign = HorizontalAlignment.Center;
            olvColumn1.MinimumWidth = 100;
            olvColumn1.Text = "Catalogues";
            olvColumn1.TextAlign = HorizontalAlignment.Center;
            olvColumn1.Width = 100;
            // 
            // olvColumn2
            // 
            olvColumn2.HeaderTextAlign = HorizontalAlignment.Center;
            olvColumn2.Renderer = attributesRenderer;
            olvColumn2.Text = "Status";
            olvColumn2.TextAlign = HorizontalAlignment.Center;
            // 
            // attributesRenderer
            // 
            attributesRenderer.ImageList = imageList_RightClickIcons;
            // 
            // imageList_RightClickIcons
            // 
            imageList_RightClickIcons.ColorDepth = ColorDepth.Depth8Bit;
            imageList_RightClickIcons.ImageStream = (ImageListStreamer)resources.GetObject("imageList_RightClickIcons.ImageStream");
            imageList_RightClickIcons.TransparentColor = System.Drawing.Color.Transparent;
            imageList_RightClickIcons.Images.SetKeyName(0, "DLE");
            imageList_RightClickIcons.Images.SetKeyName(1, "DEM");
            imageList_RightClickIcons.Images.SetKeyName(2, "DQE");
            imageList_RightClickIcons.Images.SetKeyName(3, "LOG");
            imageList_RightClickIcons.Images.SetKeyName(4, "aggregates.png");
            imageList_RightClickIcons.Images.SetKeyName(5, "deprecated");
            imageList_RightClickIcons.Images.SetKeyName(6, "projectSpecific");
            imageList_RightClickIcons.Images.SetKeyName(7, "internal");
            imageList_RightClickIcons.Images.SetKeyName(8, "primaryKey");
            imageList_RightClickIcons.Images.SetKeyName(9, "extactionIdentifer");
            // 
            // olvFilters
            // 
            olvFilters.DisplayIndex = 1;
            olvFilters.IsEditable = false;
            olvFilters.IsVisible = false;
            olvFilters.Text = "Filters";
            // 
            // olvOrder
            // 
            olvOrder.IsEditable = false;
            olvOrder.IsVisible = false;
            olvOrder.Text = "Order";
            // 
            // gbCatalogueFilters
            // 
            gbCatalogueFilters.Controls.Add(tbFilter);
            gbCatalogueFilters.Controls.Add(catalogueCollectionFilterUI1);
            gbCatalogueFilters.Dock = DockStyle.Bottom;
            gbCatalogueFilters.Location = new System.Drawing.Point(0, 404);
            gbCatalogueFilters.Margin = new Padding(0);
            gbCatalogueFilters.Name = "gbCatalogueFilters";
            gbCatalogueFilters.Size = new System.Drawing.Size(500, 75);
            gbCatalogueFilters.TabIndex = 1;
            gbCatalogueFilters.TabStop = false;
            // 
            // tbFilter
            // 
            tbFilter.Location = new System.Drawing.Point(0, 50);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(500, 23);
            tbFilter.TabIndex = 2;
            // 
            // catalogueCollectionFilterUI1
            // 
            catalogueCollectionFilterUI1.Dock = DockStyle.Fill;
            catalogueCollectionFilterUI1.Location = new System.Drawing.Point(3, 19);
            catalogueCollectionFilterUI1.Name = "catalogueCollectionFilterUI1";
            catalogueCollectionFilterUI1.Size = new System.Drawing.Size(494, 53);
            catalogueCollectionFilterUI1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Controls.Add(tlvCatalogues);
            panel2.Controls.Add(gbCatalogueFilters);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(500, 479);
            panel2.TabIndex = 2;
            // 
            // CatalogueCollectionUI
            // 
            Controls.Add(panel2);
            Name = "CatalogueCollectionUI";
            Size = new System.Drawing.Size(500, 479);
            ((ISupportInitialize)tlvCatalogues).EndInit();
            gbCatalogueFilters.ResumeLayout(false);
            gbCatalogueFilters.PerformLayout();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TreeListView tlvCatalogues;
        private ImageList imageList_RightClickIcons;
        private OLVColumn olvColumn1;
        private OLVColumn olvColumn2;
        private OLVColumn olvFilters;
        private GroupBox gbCatalogueFilters;
        private OLVColumn olvOrder;
        private CatalogueCollectionFilterUI catalogueCollectionFilterUI1;
        private Panel panel2;
        private TextBox tbFilter;
        private FlagRenderer attributesRenderer;
    }
}