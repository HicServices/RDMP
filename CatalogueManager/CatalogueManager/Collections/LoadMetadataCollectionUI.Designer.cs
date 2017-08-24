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
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.lblHowToEdit = new System.Windows.Forms.Label();
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
            this.tlvLoadMetadata.Size = new System.Drawing.Size(494, 550);
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
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 577);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(53, 574);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(444, 20);
            this.tbFilter.TabIndex = 2;
            // 
            // lblHowToEdit
            // 
            this.lblHowToEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHowToEdit.AutoSize = true;
            this.lblHowToEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHowToEdit.Location = new System.Drawing.Point(3, 556);
            this.lblHowToEdit.Name = "lblHowToEdit";
            this.lblHowToEdit.Size = new System.Drawing.Size(98, 13);
            this.lblHowToEdit.TabIndex = 3;
            this.lblHowToEdit.Text = "Press F2 to rename";
            this.lblHowToEdit.Visible = false;
            // 
            // LoadMetadataCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblHowToEdit);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tlvLoadMetadata);
            this.Name = "LoadMetadataCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadMetadata)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvLoadMetadata;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private OLVColumn olvName;
        private System.Windows.Forms.Label lblHowToEdit;
    }
}
