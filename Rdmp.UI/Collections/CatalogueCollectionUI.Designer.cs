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
            this.imageList_RightClickIcons = new System.Windows.Forms.ImageList(this.components);
            this.gbColdStorage = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cbShowColdStorage = new System.Windows.Forms.CheckBox();
            this.cbShowDeprecated = new System.Windows.Forms.CheckBox();
            this.cbShowInternal = new System.Windows.Forms.CheckBox();
            this.cbProjectSpecific = new System.Windows.Forms.CheckBox();
            this.cbShowNonExtractable = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).BeginInit();
            this.gbColdStorage.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlvCatalogues
            // 
            this.tlvCatalogues.AllColumns.Add(this.olvColumn1);
            this.tlvCatalogues.AllColumns.Add(this.olvFilters);
            this.tlvCatalogues.CellEditUseWholeCell = false;
            this.tlvCatalogues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.tlvCatalogues.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCatalogues.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.olvColumn1.Text = "Catalogues";
            this.olvColumn1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn1.MinimumWidth = 100;
            // 
            // olvFilters
            // 
            this.olvFilters.DisplayIndex = 1;
            this.olvFilters.IsVisible = false;
            this.olvFilters.Text = "Filters";
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
            this.gbColdStorage.Controls.Add(this.flowLayoutPanel1);
            this.gbColdStorage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gbColdStorage.Location = new System.Drawing.Point(0, 414);
            this.gbColdStorage.Name = "gbColdStorage";
            this.gbColdStorage.Size = new System.Drawing.Size(500, 65);
            this.gbColdStorage.TabIndex = 1;
            this.gbColdStorage.TabStop = false;
            this.gbColdStorage.Text = "Show";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cbShowColdStorage);
            this.flowLayoutPanel1.Controls.Add(this.cbShowDeprecated);
            this.flowLayoutPanel1.Controls.Add(this.cbShowInternal);
            this.flowLayoutPanel1.Controls.Add(this.cbProjectSpecific);
            this.flowLayoutPanel1.Controls.Add(this.cbShowNonExtractable);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(494, 46);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // cbShowColdStorage
            // 
            this.cbShowColdStorage.AutoSize = true;
            this.cbShowColdStorage.Location = new System.Drawing.Point(3, 3);
            this.cbShowColdStorage.Name = "cbShowColdStorage";
            this.cbShowColdStorage.Size = new System.Drawing.Size(87, 17);
            this.cbShowColdStorage.TabIndex = 0;
            this.cbShowColdStorage.Text = "Cold Storage";
            this.cbShowColdStorage.UseVisualStyleBackColor = true;
            this.cbShowColdStorage.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // cbShowDeprecated
            // 
            this.cbShowDeprecated.AutoSize = true;
            this.cbShowDeprecated.Location = new System.Drawing.Point(96, 3);
            this.cbShowDeprecated.Name = "cbShowDeprecated";
            this.cbShowDeprecated.Size = new System.Drawing.Size(82, 17);
            this.cbShowDeprecated.TabIndex = 1;
            this.cbShowDeprecated.Text = "Deprecated";
            this.cbShowDeprecated.UseVisualStyleBackColor = true;
            this.cbShowDeprecated.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // cbShowInternal
            // 
            this.cbShowInternal.AutoSize = true;
            this.cbShowInternal.Location = new System.Drawing.Point(184, 3);
            this.cbShowInternal.Name = "cbShowInternal";
            this.cbShowInternal.Size = new System.Drawing.Size(61, 17);
            this.cbShowInternal.TabIndex = 2;
            this.cbShowInternal.Text = "Internal";
            this.cbShowInternal.UseVisualStyleBackColor = true;
            this.cbShowInternal.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // cbProjectSpecific
            // 
            this.cbProjectSpecific.AutoSize = true;
            this.cbProjectSpecific.Location = new System.Drawing.Point(251, 3);
            this.cbProjectSpecific.Name = "cbProjectSpecific";
            this.cbProjectSpecific.Size = new System.Drawing.Size(100, 17);
            this.cbProjectSpecific.TabIndex = 3;
            this.cbProjectSpecific.Text = "Project Specific";
            this.cbProjectSpecific.UseVisualStyleBackColor = true;
            this.cbProjectSpecific.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // cbShowNonExtractable
            // 
            this.cbShowNonExtractable.AutoSize = true;
            this.cbShowNonExtractable.Location = new System.Drawing.Point(357, 3);
            this.cbShowNonExtractable.Name = "cbShowNonExtractable";
            this.cbShowNonExtractable.Size = new System.Drawing.Size(102, 17);
            this.cbShowNonExtractable.TabIndex = 4;
            this.cbShowNonExtractable.Text = "Non Extractable";
            this.cbShowNonExtractable.UseVisualStyleBackColor = true;
            this.cbShowNonExtractable.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
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
            // CatalogueCollectionUI
            // 
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbColdStorage);
            this.Name = "CatalogueCollectionUI";
            this.Size = new System.Drawing.Size(500, 479);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).EndInit();
            this.gbColdStorage.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvCatalogues;
        private ImageList imageList_RightClickIcons;
        private OLVColumn olvColumn1;
        private OLVColumn olvFilters;
        private GroupBox gbColdStorage;
        private CheckBox cbShowInternal;
        private CheckBox cbShowDeprecated;
        private CheckBox cbShowColdStorage;
        private Panel panel1;
        private CheckBox cbProjectSpecific;
        private CheckBox cbShowNonExtractable;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
