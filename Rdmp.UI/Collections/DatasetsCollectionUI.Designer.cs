using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    partial class DatasetsCollectionUI
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


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tlvDatasets = new BrightIdeasSoftware.TreeListView();
            olvName = new BrightIdeasSoftware.OLVColumn();
            tbFilter = new TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvDatasets).BeginInit();
            SuspendLayout();
            // 
            // tlvDatasets
            // 
            tlvDatasets.AllColumns.Add(olvName);
            tlvDatasets.CellEditUseWholeCell = false;
            tlvDatasets.Columns.AddRange(new ColumnHeader[] { olvName });
            tlvDatasets.Dock = DockStyle.Fill;
            tlvDatasets.FullRowSelect = true;
            tlvDatasets.Location = new System.Drawing.Point(0, 0);
            tlvDatasets.Margin = new Padding(4, 3, 4, 3);
            tlvDatasets.Name = "tlvDatasets";
            tlvDatasets.ShowGroups = false;
            tlvDatasets.Size = new System.Drawing.Size(376, 643);
            tlvDatasets.TabIndex = 2;
            tlvDatasets.UseCompatibleStateImageBehavior = false;
            tlvDatasets.View = View.Details;
            tlvDatasets.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.CellEditUseWholeCell = true;
            olvName.MinimumWidth = 100;
            olvName.Text = "Datasets";
            olvName.Width = 100;
            // 
            // tbFilter
            // 
            tbFilter.Dock = DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 620);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(376, 23);
            tbFilter.TabIndex = 3;
            // 
            // DatasetsCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvDatasets);
            Margin = new Padding(4, 3, 4, 3);
            Name = "DatasetsCollectionUI";
            Size = new System.Drawing.Size(376, 643);
            ((System.ComponentModel.ISupportInitialize)tlvDatasets).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvDatasets;
        private BrightIdeasSoftware.OLVColumn olvName;
        private TextBox tbFilter;
    }
}