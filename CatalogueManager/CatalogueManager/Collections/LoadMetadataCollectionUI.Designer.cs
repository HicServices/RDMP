using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Collections
{
    partial class LoadMetadataCollectionUI : ILifetimeSubscriber
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
            this.tlvLoadMetadata = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadMetadata)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvLoadMetadata
            // 
            this.tlvLoadMetadata.AllColumns.Add(this.olvName);
            this.tlvLoadMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvLoadMetadata.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.tlvLoadMetadata.CellEditUseWholeCell = false;
            this.tlvLoadMetadata.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvLoadMetadata.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvLoadMetadata.Location = new System.Drawing.Point(0, 3);
            this.tlvLoadMetadata.Name = "tlvLoadMetadata";
            this.tlvLoadMetadata.ShowGroups = false;
            this.tlvLoadMetadata.Size = new System.Drawing.Size(494, 566);
            this.tlvLoadMetadata.TabIndex = 0;
            this.tlvLoadMetadata.UseCompatibleStateImageBehavior = false;
            this.tlvLoadMetadata.View = System.Windows.Forms.View.Details;
            this.tlvLoadMetadata.VirtualMode = true;
            this.tlvLoadMetadata.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.otvLoadMetadata_CellRightClick);
            this.tlvLoadMetadata.ItemActivate += new System.EventHandler(this.otvLoadMetadata_ItemActivate);
            this.tlvLoadMetadata.KeyUp += new System.Windows.Forms.KeyEventHandler(this.otvLoadMetadata_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Load Metadata";
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(443, 575);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(51, 22);
            this.btnExpandOrCollapse.TabIndex = 171;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // LoadMetadataCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExpandOrCollapse);
            this.Controls.Add(this.tlvLoadMetadata);
            this.Name = "LoadMetadataCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadMetadata)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvLoadMetadata;
        private OLVColumn olvName;
        private System.Windows.Forms.Button btnExpandOrCollapse;
    }
}
