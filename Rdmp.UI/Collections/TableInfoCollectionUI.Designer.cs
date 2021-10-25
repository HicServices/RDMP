using BrightIdeasSoftware;

namespace Rdmp.UI.Collections
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
            this.olvDataType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tlvTableInfos)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvTableInfos
            // 
            this.tlvTableInfos.AllColumns.Add(this.olvColumn1);
            this.tlvTableInfos.AllColumns.Add(this.olvDataType);
            this.tlvTableInfos.AllColumns.Add(this.olvValue);            
            this.tlvTableInfos.CellEditUseWholeCell = false;
            this.tlvTableInfos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvDataType,
            this.olvValue});
            this.tlvTableInfos.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvTableInfos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvTableInfos.Location = new System.Drawing.Point(0, 0);
            this.tlvTableInfos.Name = "tlvTableInfos";
            this.tlvTableInfos.ShowGroups = false;
            this.tlvTableInfos.Size = new System.Drawing.Size(500, 600);
            this.tlvTableInfos.TabIndex = 160;
            this.tlvTableInfos.UseCompatibleStateImageBehavior = false;
            this.tlvTableInfos.UseFiltering = true;
            this.tlvTableInfos.View = System.Windows.Forms.View.Details;
            this.tlvTableInfos.VirtualMode = true;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.Text = "Name";
            this.olvColumn1.MinimumWidth = 100;
            // 
            // olvColumn2
            // 
            this.olvDataType.AspectName = "";
            this.olvDataType.IsEditable = false;
            this.olvDataType.Text = "DataType";
            this.olvDataType.Width = 110;

            // 
            // olvValue
            // 
            this.olvValue.AspectName = "Value";
            this.olvValue.IsEditable = false;
            this.olvValue.Text = "Value";
            this.olvValue.Width = 110;
            
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
            // TableInfoCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private OLVColumn olvDataType;
        private OLVColumn olvValue;
    }
}
