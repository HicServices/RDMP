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
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.lblHowToEdit = new System.Windows.Forms.Label();
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvDataExport
            // 
            this.tlvDataExport.AllColumns.Add(this.olvName);
            
            this.tlvDataExport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvDataExport.CellEditUseWholeCell = false;
            this.tlvDataExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvDataExport.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDataExport.FullRowSelect = true;
            this.tlvDataExport.HideSelection = false;
            this.tlvDataExport.Location = new System.Drawing.Point(0, 0);
            this.tlvDataExport.Name = "tlvDataExport";
            this.tlvDataExport.ShowGroups = false;
            this.tlvDataExport.Size = new System.Drawing.Size(382, 650);
            this.tlvDataExport.TabIndex = 0;
            this.tlvDataExport.UseCompatibleStateImageBehavior = false;
            this.tlvDataExport.View = System.Windows.Forms.View.Details;
            this.tlvDataExport.VirtualMode = true;
            this.tlvDataExport.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.tlvDataExport_CellRightClick);
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
            // olvFavourite
            // 
            
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 674);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(41, 671);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(341, 20);
            this.tbFilter.TabIndex = 2;
            // 
            // lblHowToEdit
            // 
            this.lblHowToEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHowToEdit.AutoSize = true;
            this.lblHowToEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHowToEdit.Location = new System.Drawing.Point(3, 655);
            this.lblHowToEdit.Name = "lblHowToEdit";
            this.lblHowToEdit.Size = new System.Drawing.Size(98, 13);
            this.lblHowToEdit.TabIndex = 170;
            this.lblHowToEdit.Text = "Press F2 to rename";
            this.lblHowToEdit.Visible = false;
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(327, 650);
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
            this.Controls.Add(this.lblHowToEdit);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tlvDataExport);
            this.Name = "DataExportCollectionUI";
            this.Size = new System.Drawing.Size(385, 694);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvDataExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private OLVColumn olvName;
        private System.Windows.Forms.Label lblHowToEdit;
        private System.Windows.Forms.Button btnExpandOrCollapse;
    }
}
