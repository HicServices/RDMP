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
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvDataExport
            // 
            this.tlvDataExport.AllColumns.Add(this.olvName);
            this.tlvDataExport.AllColumns.Add(this.olvProjectNumber);
            this.tlvDataExport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvDataExport.CellEditUseWholeCell = false;
            this.tlvDataExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvProjectNumber});
            this.tlvDataExport.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDataExport.FullRowSelect = true;
            this.tlvDataExport.HideSelection = false;
            this.tlvDataExport.Location = new System.Drawing.Point(0, 0);
            this.tlvDataExport.Name = "tlvDataExport";
            this.tlvDataExport.ShowGroups = false;
            this.tlvDataExport.Size = new System.Drawing.Size(382, 664);
            this.tlvDataExport.TabIndex = 0;
            this.tlvDataExport.UseCompatibleStateImageBehavior = false;
            this.tlvDataExport.View = System.Windows.Forms.View.Details;
            this.tlvDataExport.VirtualMode = true;
            this.tlvDataExport.ItemActivate += new System.EventHandler(this.tlvDataExport_ItemActivate);
            this.tlvDataExport.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tlvDataExport_KeyUp);
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
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(327, 670);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(55, 21);
            this.btnExpandOrCollapse.TabIndex = 171;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // DataExportCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExpandOrCollapse);
            this.Controls.Add(this.tlvDataExport);
            this.Name = "DataExportCollectionUI";
            this.Size = new System.Drawing.Size(385, 694);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvDataExport;
        private OLVColumn olvName;
        private System.Windows.Forms.Button btnExpandOrCollapse;
        private OLVColumn olvProjectNumber;
    }
}
