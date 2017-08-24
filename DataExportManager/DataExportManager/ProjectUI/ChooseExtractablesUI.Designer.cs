using BrightIdeasSoftware;

namespace DataExportManager.ProjectUI
{
    partial class ChooseExtractablesUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseExtractablesUI));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Not Launched", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Awaiting Execution", 8);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Waiting for SQL Server", 3);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Writing to File", 4);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Crashed", 1);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("User Aborted", 2);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Completed", "tick.bmp");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Warning", "warning.bmp");
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Writing Metadata", "word.bmp");
            System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("Supporting SQL/Lookups", 9);
            System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("Supporting Document", "supportingdocument.bmp");
            System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("Bundle", 12);
            System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("Extractable Dataset", 13);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvKey = new System.Windows.Forms.ListView();
            this.tlvDatasets = new BrightIdeasSoftware.TreeListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "none.bmp");
            this.imageList1.Images.SetKeyName(1, "failed.bmp");
            this.imageList1.Images.SetKeyName(2, "stopped.bmp");
            this.imageList1.Images.SetKeyName(3, "talkingtoSQL.bmp");
            this.imageList1.Images.SetKeyName(4, "writing.bmp");
            this.imageList1.Images.SetKeyName(5, "tick.bmp");
            this.imageList1.Images.SetKeyName(6, "warning.bmp");
            this.imageList1.Images.SetKeyName(7, "word.bmp");
            this.imageList1.Images.SetKeyName(8, "sleeping.bmp");
            this.imageList1.Images.SetKeyName(9, "sql.bmp");
            this.imageList1.Images.SetKeyName(10, "supportingdocument.bmp");
            this.imageList1.Images.SetKeyName(11, "folder.bmp");
            this.imageList1.Images.SetKeyName(12, "bundle.bmp");
            this.imageList1.Images.SetKeyName(13, "extractabledataset.bmp");
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lvKey);
            this.groupBox1.Location = new System.Drawing.Point(3, 625);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(430, 122);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Key";
            // 
            // lvKey
            // 
            this.lvKey.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewItem1.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.StateImageIndex = 0;
            listViewItem5.StateImageIndex = 0;
            listViewItem6.StateImageIndex = 0;
            listViewItem7.StateImageIndex = 0;
            listViewItem8.StateImageIndex = 0;
            listViewItem9.StateImageIndex = 0;
            this.lvKey.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12,
            listViewItem13});
            this.lvKey.Location = new System.Drawing.Point(3, 16);
            this.lvKey.Name = "lvKey";
            this.lvKey.Size = new System.Drawing.Size(424, 103);
            this.lvKey.SmallImageList = this.imageList1;
            this.lvKey.TabIndex = 22;
            this.lvKey.UseCompatibleStateImageBehavior = false;
            this.lvKey.View = System.Windows.Forms.View.SmallIcon;
            // 
            // tlvDatasets
            // 
            this.tlvDatasets.AllColumns.Add(this.olvColumn1);
            this.tlvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvDatasets.CellEditUseWholeCell = false;
            this.tlvDatasets.CheckBoxes = true;
            this.tlvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.tlvDatasets.HierarchicalCheckboxes = true;
            this.tlvDatasets.Location = new System.Drawing.Point(3, 3);
            this.tlvDatasets.Name = "tlvDatasets";
            this.tlvDatasets.ShowGroups = false;
            this.tlvDatasets.ShowImagesOnSubItems = true;
            this.tlvDatasets.Size = new System.Drawing.Size(427, 616);
            this.tlvDatasets.SmallImageList = this.imageList1;
            this.tlvDatasets.TabIndex = 25;
            this.tlvDatasets.Text = "label1";
            this.tlvDatasets.UseCompatibleStateImageBehavior = false;
            this.tlvDatasets.View = System.Windows.Forms.View.Details;
            this.tlvDatasets.VirtualMode = true;
            this.tlvDatasets.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.tlvDatasets_ItemChecked);
            this.tlvDatasets.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.tlvDatasets_ItemSelectionChanged);
            this.tlvDatasets.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvDatasets_MouseDoubleClick);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Text = "Extractables";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ChooseExtractablesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tlvDatasets);
            this.Controls.Add(this.groupBox1);
            this.Name = "ChooseExtractablesUI";
            this.Size = new System.Drawing.Size(436, 750);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDatasets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lvKey;
        private TreeListView tlvDatasets;
        private OLVColumn olvColumn1;
        private System.Windows.Forms.Timer timer1;
    }
}
