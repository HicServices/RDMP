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
            components = new System.ComponentModel.Container();
            tlvDataExport = new TreeListView();
            olvName = new OLVColumn();
            olvProjectNumber = new OLVColumn();
            olvCohortSource = new OLVColumn();
            olvCohortVersion = new OLVColumn();
            tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvDataExport).BeginInit();
            SuspendLayout();
            // 
            // tlvDataExport
            // 
            tlvDataExport.AllColumns.Add(olvName);
            tlvDataExport.AllColumns.Add(olvProjectNumber);
            tlvDataExport.AllColumns.Add(olvCohortSource);
            tlvDataExport.AllColumns.Add(olvCohortVersion);
            tlvDataExport.CellEditUseWholeCell = false;
            tlvDataExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName, olvProjectNumber, olvCohortVersion });
            tlvDataExport.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvDataExport.FullRowSelect = true;
            tlvDataExport.Location = new System.Drawing.Point(0, 0);
            tlvDataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvDataExport.Name = "tlvDataExport";
            tlvDataExport.ShowGroups = false;
            tlvDataExport.Size = new System.Drawing.Size(449, 801);
            tlvDataExport.TabIndex = 0;
            tlvDataExport.UseCompatibleStateImageBehavior = false;
            tlvDataExport.View = System.Windows.Forms.View.Details;
            tlvDataExport.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.CellEditUseWholeCell = true;
            olvName.MinimumWidth = 100;
            olvName.Text = "Projects";
            olvName.Width = 100;
            // 
            // olvProjectNumber
            // 
            olvProjectNumber.Text = "ProjectNumber";
            olvProjectNumber.Width = 89;
            // 
            // olvCohortSource
            // 
            olvCohortSource.IsVisible = false;
            olvCohortSource.Text = "CohortSource";
            // 
            // olvCohortVersion
            // 
            olvCohortVersion.Text = "Version";
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 778);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(449, 23);
            tbFilter.TabIndex = 1;
            // 
            // DataExportCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvDataExport);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DataExportCollectionUI";
            Size = new System.Drawing.Size(449, 801);
            ((System.ComponentModel.ISupportInitialize)tlvDataExport).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeListView tlvDataExport;
        private OLVColumn olvName;
        private OLVColumn olvProjectNumber;
        private OLVColumn olvCohortSource;
        private OLVColumn olvCohortVersion;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
