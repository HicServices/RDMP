using BrightIdeasSoftware;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataVersionsUIs
{
    partial class LoadMetadataVersionsUI
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
            panel1 = new System.Windows.Forms.Panel();
            tlvLoadMetadata = new BrightIdeasSoftware.TreeListView();
            olvName = new OLVColumn();
            olvValue = new OLVColumn();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tlvLoadMetadata).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(tlvLoadMetadata);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1111, 276);
            panel1.TabIndex = 11;
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
            // LoadMetadataVersionsUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LoadMetadataVersionsUI";
            Size = new System.Drawing.Size(1111, 276);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tlvLoadMetadata).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private BrightIdeasSoftware.TreeListView tlvLoadMetadata;
        private OLVColumn olvName;
        private OLVColumn olvValue;
    }
}