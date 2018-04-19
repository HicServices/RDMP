using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace DataExportManager.Collections
{
    partial class DataExportCollectionUI : ILifetimeSubscriber
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
            this.tlvDataExport = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvProjectNumber = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvDataExport
            // 
            this.tlvDataExport.AllColumns.Add(this.olvName);
            this.tlvDataExport.AllColumns.Add(this.olvProjectNumber);
            this.tlvDataExport.CellEditUseWholeCell = false;
            this.tlvDataExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvProjectNumber});
            this.tlvDataExport.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDataExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvDataExport.FullRowSelect = true;
            this.tlvDataExport.HideSelection = false;
            this.tlvDataExport.Location = new System.Drawing.Point(0, 0);
            this.tlvDataExport.Name = "tlvDataExport";
            this.tlvDataExport.ShowGroups = false;
            this.tlvDataExport.Size = new System.Drawing.Size(385, 694);
            this.tlvDataExport.TabIndex = 0;
            this.tlvDataExport.UseCompatibleStateImageBehavior = false;
            this.tlvDataExport.View = System.Windows.Forms.View.Details;
            this.tlvDataExport.VirtualMode = true;
            this.tlvDataExport.ItemActivate += new System.EventHandler(this.tlvDataExport_ItemActivate);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.CellEditUseWholeCell = true;
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Data Export";
            // 
            // olvProjectNumber
            // 
            this.olvProjectNumber.Text = "ProjectNumber";
            this.olvProjectNumber.Width = 89;
            // 
            // DataExportCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvDataExport);
            this.Name = "DataExportCollectionUI";
            this.Size = new System.Drawing.Size(385, 694);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvDataExport;
        private OLVColumn olvName;
        private OLVColumn olvProjectNumber;
    }
}
