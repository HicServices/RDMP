using BrightIdeasSoftware;

namespace Rdmp.UI.Collections
{
    partial class DataExportCollectionUI
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
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvProjectNumber = new BrightIdeasSoftware.OLVColumn();
            this.olvCohortSource = new BrightIdeasSoftware.OLVColumn();
            this.olvCohortVersion = new BrightIdeasSoftware.OLVColumn();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvDataExport
            // 
            this.tlvDataExport.AllColumns.Add(this.olvName);
            this.tlvDataExport.AllColumns.Add(this.olvProjectNumber);
            this.tlvDataExport.AllColumns.Add(this.olvCohortSource);
            this.tlvDataExport.AllColumns.Add(this.olvCohortVersion);
            this.tlvDataExport.CellEditUseWholeCell = false;
            this.tlvDataExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvProjectNumber,
            this.olvCohortVersion});
            this.tlvDataExport.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvDataExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvDataExport.FullRowSelect = true;
            this.tlvDataExport.Location = new System.Drawing.Point(0, 0);
            this.tlvDataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tlvDataExport.Name = "tlvDataExport";
            this.tlvDataExport.ShowGroups = false;
            this.tlvDataExport.Size = new System.Drawing.Size(449, 801);
            this.tlvDataExport.TabIndex = 0;
            this.tlvDataExport.UseCompatibleStateImageBehavior = false;
            this.tlvDataExport.View = System.Windows.Forms.View.Details;
            this.tlvDataExport.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.CellEditUseWholeCell = true;
            this.olvName.MinimumWidth = 100;
            this.olvName.Text = "Projects";
            this.olvName.Width = 100;
            // 
            // olvProjectNumber
            // 
            this.olvProjectNumber.Text = "Project Number";
            this.olvProjectNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.olvProjectNumber.Width = 89;
            // 
            // olvCohortSource
            // 
            this.olvCohortSource.IsVisible = false;
            this.olvCohortSource.Text = "CohortSource";
            // 
            // olvCohortVersion
            // 
            this.olvCohortVersion.Text = "Version";
            // 
            // DataExportCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvDataExport);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DataExportCollectionUI";
            this.Size = new System.Drawing.Size(449, 801);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDataExport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvDataExport;
        private OLVColumn olvName;
        private OLVColumn olvProjectNumber;
        private OLVColumn olvCohortSource;
        private OLVColumn olvCohortVersion;
    }
}
