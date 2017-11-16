using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Collections
{
    partial class TableInfoCollectionUI 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableInfoCollectionUI));
            this.tlvTableInfos = new BrightIdeasSoftware.TreeListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfos)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvTableInfos
            // 
            this.tlvTableInfos.AllColumns.Add(this.olvColumn1);
            this.tlvTableInfos.AllColumns.Add(this.olvColumn2);
            this.tlvTableInfos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvTableInfos.CellEditUseWholeCell = false;
            this.tlvTableInfos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2});
            this.tlvTableInfos.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvTableInfos.Location = new System.Drawing.Point(3, 3);
            this.tlvTableInfos.Name = "tlvTableInfos";
            this.tlvTableInfos.ShowGroups = false;
            this.tlvTableInfos.Size = new System.Drawing.Size(494, 566);
            this.tlvTableInfos.TabIndex = 160;
            this.tlvTableInfos.UseCompatibleStateImageBehavior = false;
            this.tlvTableInfos.UseFiltering = true;
            this.tlvTableInfos.View = System.Windows.Forms.View.Details;
            this.tlvTableInfos.VirtualMode = true;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Text = "Name";
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "";
            this.olvColumn2.Text = "DataType";
            this.olvColumn2.Width = 110;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "goldkey");
            this.imageList1.Images.SetKeyName(1, "anonymous");
            this.imageList1.Images.SetKeyName(2, "sync");
            this.imageList1.Images.SetKeyName(3, "backup");
            this.imageList1.Images.SetKeyName(4, "parameters");
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(449, 575);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(51, 22);
            this.btnExpandOrCollapse.TabIndex = 170;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // TableInfoCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExpandOrCollapse);
            this.Controls.Add(this.tlvTableInfos);
            this.Name = "TableInfoCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfos)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvTableInfos;
        private OLVColumn olvColumn1;
        private System.Windows.Forms.ImageList imageList1;
        private OLVColumn olvColumn2;
        private System.Windows.Forms.Button btnExpandOrCollapse;
    }
}
