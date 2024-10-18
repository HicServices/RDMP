using BrightIdeasSoftware;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections
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
            components = new System.ComponentModel.Container();
            tlvLoadMetadata = new TreeListView();
            olvName = new OLVColumn();
            olvValue = new OLVColumn();
            tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvLoadMetadata).BeginInit();
            SuspendLayout();
            // 
            // tlvLoadMetadata
            // 
            tlvLoadMetadata.AllColumns.Add(olvName);
            tlvLoadMetadata.AllColumns.Add(olvValue);
            tlvLoadMetadata.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            tlvLoadMetadata.CellEditUseWholeCell = false;
            tlvLoadMetadata.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName, olvValue });
            tlvLoadMetadata.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvLoadMetadata.Location = new System.Drawing.Point(0, 0);
            tlvLoadMetadata.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvLoadMetadata.Name = "tlvLoadMetadata";
            tlvLoadMetadata.ShowGroups = false;
            tlvLoadMetadata.Size = new System.Drawing.Size(583, 692);
            tlvLoadMetadata.TabIndex = 0;
            tlvLoadMetadata.UseCompatibleStateImageBehavior = false;
            tlvLoadMetadata.View = System.Windows.Forms.View.Details;
            tlvLoadMetadata.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.MinimumWidth = 100;
            olvName.Text = "Load Metadata";
            olvName.Width = 100;
            // 
            // olvValue
            // 
            olvValue.AspectName = "Value";
            olvValue.IsEditable = false;
            olvValue.Text = "Value";
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 669);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(583, 23);
            tbFilter.TabIndex = 1;
            // 
            // LoadMetadataCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvLoadMetadata);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LoadMetadataCollectionUI";
            Size = new System.Drawing.Size(583, 692);
            ((System.ComponentModel.ISupportInitialize)tlvLoadMetadata).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeListView tlvLoadMetadata;
        private OLVColumn olvName;
        private OLVColumn olvValue;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
