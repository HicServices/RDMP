using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Collections
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
            this.olvCheckResult = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList_RightClickIcons = new System.Windows.Forms.ImageList(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnCheckCatalogues = new System.Windows.Forms.Button();
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            this.lblHowToEdit = new System.Windows.Forms.Label();
            this.gbColdStorage = new System.Windows.Forms.GroupBox();
            this.rbColdStorage = new System.Windows.Forms.RadioButton();
            this.rbWarmStorage = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbNotInternal = new System.Windows.Forms.RadioButton();
            this.rbInternal = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbDeprecated = new System.Windows.Forms.RadioButton();
            this.rbLive = new System.Windows.Forms.RadioButton();
            this.label42 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnShowFlags = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).BeginInit();
            this.gbColdStorage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlvCatalogues
            // 
            this.tlvCatalogues.AllColumns.Add(this.olvColumn1);
            this.tlvCatalogues.AllColumns.Add(this.olvFilters);
            this.tlvCatalogues.AllColumns.Add(this.olvCheckResult);
            this.tlvCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvCatalogues.CellEditUseWholeCell = false;
            this.tlvCatalogues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvCheckResult});
            this.tlvCatalogues.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCatalogues.Location = new System.Drawing.Point(0, 3);
            this.tlvCatalogues.Name = "tlvCatalogues";
            this.tlvCatalogues.ShowGroups = false;
            this.tlvCatalogues.Size = new System.Drawing.Size(500, 528);
            this.tlvCatalogues.TabIndex = 0;
            this.tlvCatalogues.Text = "label1";
            this.tlvCatalogues.UseCompatibleStateImageBehavior = false;
            this.tlvCatalogues.UseFiltering = true;
            this.tlvCatalogues.View = System.Windows.Forms.View.Details;
            this.tlvCatalogues.VirtualMode = true;
            this.tlvCatalogues.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.tlvCatalogues_CellRightClick);
            this.tlvCatalogues.ItemActivate += new System.EventHandler(this.tlvCatalogues_ItemActivate);
            this.tlvCatalogues.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tlvCatalogues_KeyUp);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn1.Text = "Catalogues";
            this.olvColumn1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // olvFilters
            // 
            this.olvFilters.DisplayIndex = 1;
            this.olvFilters.IsVisible = false;
            this.olvFilters.Text = "Filters";
            // 
            // olvCheckResult
            // 
            this.olvCheckResult.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvCheckResult.Text = "Check";
            this.olvCheckResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvCheckResult.Width = 45;
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
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 549);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(408, 10);
            this.progressBar1.TabIndex = 166;
            this.progressBar1.Visible = false;
            // 
            // btnCheckCatalogues
            // 
            this.btnCheckCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckCatalogues.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnCheckCatalogues.Location = new System.Drawing.Point(460, 537);
            this.btnCheckCatalogues.Name = "btnCheckCatalogues";
            this.btnCheckCatalogues.Size = new System.Drawing.Size(40, 22);
            this.btnCheckCatalogues.TabIndex = 167;
            this.btnCheckCatalogues.Text = "Check";
            this.btnCheckCatalogues.UseVisualStyleBackColor = true;
            this.btnCheckCatalogues.Click += new System.EventHandler(this.btnCheckCatalogues_Click);
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(409, 537);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(51, 22);
            this.btnExpandOrCollapse.TabIndex = 168;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // lblHowToEdit
            // 
            this.lblHowToEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHowToEdit.AutoSize = true;
            this.lblHowToEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHowToEdit.Location = new System.Drawing.Point(0, 533);
            this.lblHowToEdit.Name = "lblHowToEdit";
            this.lblHowToEdit.Size = new System.Drawing.Size(98, 13);
            this.lblHowToEdit.TabIndex = 169;
            this.lblHowToEdit.Text = "Press F2 to rename";
            this.lblHowToEdit.Visible = false;
            // 
            // gbColdStorage
            // 
            this.gbColdStorage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbColdStorage.Controls.Add(this.rbColdStorage);
            this.gbColdStorage.Controls.Add(this.rbWarmStorage);
            this.gbColdStorage.Location = new System.Drawing.Point(3, -2);
            this.gbColdStorage.Name = "gbColdStorage";
            this.gbColdStorage.Size = new System.Drawing.Size(1004, 36);
            this.gbColdStorage.TabIndex = 174;
            this.gbColdStorage.TabStop = false;
            this.gbColdStorage.Text = "Show (Warm / Cold Storage)";
            // 
            // rbColdStorage
            // 
            this.rbColdStorage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbColdStorage.AutoSize = true;
            this.rbColdStorage.Location = new System.Drawing.Point(69, 16);
            this.rbColdStorage.Name = "rbColdStorage";
            this.rbColdStorage.Size = new System.Drawing.Size(86, 17);
            this.rbColdStorage.TabIndex = 156;
            this.rbColdStorage.Text = "Cold Storage";
            this.rbColdStorage.UseVisualStyleBackColor = true;
            this.rbColdStorage.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // rbWarmStorage
            // 
            this.rbWarmStorage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbWarmStorage.AutoSize = true;
            this.rbWarmStorage.Checked = true;
            this.rbWarmStorage.Location = new System.Drawing.Point(3, 16);
            this.rbWarmStorage.Name = "rbWarmStorage";
            this.rbWarmStorage.Size = new System.Drawing.Size(53, 17);
            this.rbWarmStorage.TabIndex = 155;
            this.rbWarmStorage.TabStop = true;
            this.rbWarmStorage.Text = "Warm";
            this.rbWarmStorage.UseVisualStyleBackColor = true;
            this.rbWarmStorage.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.rbNotInternal);
            this.groupBox2.Controls.Add(this.rbInternal);
            this.groupBox2.Location = new System.Drawing.Point(6, 82);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1004, 43);
            this.groupBox2.TabIndex = 173;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Show (Availablility)";
            // 
            // rbNotInternal
            // 
            this.rbNotInternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbNotInternal.AutoSize = true;
            this.rbNotInternal.Checked = true;
            this.rbNotInternal.Location = new System.Drawing.Point(4, 20);
            this.rbNotInternal.Name = "rbNotInternal";
            this.rbNotInternal.Size = new System.Drawing.Size(54, 17);
            this.rbNotInternal.TabIndex = 155;
            this.rbNotInternal.TabStop = true;
            this.rbNotInternal.Text = "Public";
            this.rbNotInternal.UseVisualStyleBackColor = true;
            this.rbNotInternal.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // rbInternal
            // 
            this.rbInternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbInternal.AutoSize = true;
            this.rbInternal.Location = new System.Drawing.Point(68, 20);
            this.rbInternal.Name = "rbInternal";
            this.rbInternal.Size = new System.Drawing.Size(84, 17);
            this.rbInternal.TabIndex = 156;
            this.rbInternal.Text = "Internal Only";
            this.rbInternal.UseVisualStyleBackColor = true;
            this.rbInternal.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbDeprecated);
            this.groupBox1.Controls.Add(this.rbLive);
            this.groupBox1.Location = new System.Drawing.Point(3, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1004, 36);
            this.groupBox1.TabIndex = 172;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Show (Deprecation)";
            // 
            // rbDeprecated
            // 
            this.rbDeprecated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbDeprecated.AutoSize = true;
            this.rbDeprecated.Location = new System.Drawing.Point(71, 16);
            this.rbDeprecated.Name = "rbDeprecated";
            this.rbDeprecated.Size = new System.Drawing.Size(105, 17);
            this.rbDeprecated.TabIndex = 156;
            this.rbDeprecated.Text = "Deprecated Only";
            this.rbDeprecated.UseVisualStyleBackColor = true;
            this.rbDeprecated.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // rbLive
            // 
            this.rbLive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbLive.AutoSize = true;
            this.rbLive.Checked = true;
            this.rbLive.Location = new System.Drawing.Point(4, 16);
            this.rbLive.Name = "rbLive";
            this.rbLive.Size = new System.Drawing.Size(69, 17);
            this.rbLive.TabIndex = 155;
            this.rbLive.TabStop = true;
            this.rbLive.Text = "Live Only";
            this.rbLive.UseVisualStyleBackColor = true;
            this.rbLive.CheckedChanged += new System.EventHandler(this.rbFlag_CheckedChanged);
            // 
            // label42
            // 
            this.label42.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(25, 560);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(32, 13);
            this.label42.TabIndex = 171;
            this.label42.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(25, 576);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(472, 20);
            this.tbFilter.TabIndex = 170;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnShowFlags);
            this.splitContainer1.Panel1.Controls.Add(this.tlvCatalogues);
            this.splitContainer1.Panel1.Controls.Add(this.lblHowToEdit);
            this.splitContainer1.Panel1.Controls.Add(this.btnExpandOrCollapse);
            this.splitContainer1.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer1.Panel1.Controls.Add(this.btnCheckCatalogues);
            this.splitContainer1.Panel1.Controls.Add(this.label42);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gbColdStorage);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(500, 600);
            this.splitContainer1.SplitterDistance = 465;
            this.splitContainer1.TabIndex = 175;
            // 
            // btnShowFlags
            // 
            this.btnShowFlags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnShowFlags.Location = new System.Drawing.Point(3, 574);
            this.btnShowFlags.Name = "btnShowFlags";
            this.btnShowFlags.Size = new System.Drawing.Size(16, 23);
            this.btnShowFlags.TabIndex = 172;
            this.btnShowFlags.Text = "+";
            this.btnShowFlags.UseVisualStyleBackColor = true;
            this.btnShowFlags.Click += new System.EventHandler(this.btnShowFlags_Click);
            // 
            // CatalogueCollectionUI
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "CatalogueCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCatalogues)).EndInit();
            this.gbColdStorage.ResumeLayout(false);
            this.gbColdStorage.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvCatalogues;
        private ImageList imageList_RightClickIcons;
        private OLVColumn olvColumn1;
        private ProgressBar progressBar1;
        private OLVColumn olvCheckResult;
        private Button btnCheckCatalogues;
        private OLVColumn olvFilters;
        private Button btnExpandOrCollapse;
        private Label lblHowToEdit;
        private GroupBox gbColdStorage;
        private RadioButton rbColdStorage;
        private RadioButton rbWarmStorage;
        private GroupBox groupBox2;
        private RadioButton rbNotInternal;
        private RadioButton rbInternal;
        private GroupBox groupBox1;
        private RadioButton rbDeprecated;
        private RadioButton rbLive;
        private Label label42;
        private TextBox tbFilter;
        private SplitContainer splitContainer1;
        private Button btnShowFlags;
    }
}
