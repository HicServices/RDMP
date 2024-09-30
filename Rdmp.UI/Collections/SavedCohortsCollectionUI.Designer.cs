namespace Rdmp.UI.Collections
{
    partial class SavedCohortsCollectionUI
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
            tlvSavedCohorts = new BrightIdeasSoftware.TreeListView();
            olvName = new BrightIdeasSoftware.OLVColumn();
            olvProjectNumber = new BrightIdeasSoftware.OLVColumn();
            olvVersion = new BrightIdeasSoftware.OLVColumn();
            tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvSavedCohorts).BeginInit();
            SuspendLayout();
            // 
            // tlvSavedCohorts
            // 
            tlvSavedCohorts.AllColumns.Add(olvName);
            tlvSavedCohorts.AllColumns.Add(olvProjectNumber);
            tlvSavedCohorts.AllColumns.Add(olvVersion);
            tlvSavedCohorts.CellEditUseWholeCell = false;
            tlvSavedCohorts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName, olvProjectNumber, olvVersion });
            tlvSavedCohorts.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvSavedCohorts.FullRowSelect = true;
            tlvSavedCohorts.Location = new System.Drawing.Point(0, 0);
            tlvSavedCohorts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvSavedCohorts.Name = "tlvSavedCohorts";
            tlvSavedCohorts.ShowGroups = false;
            tlvSavedCohorts.Size = new System.Drawing.Size(462, 747);
            tlvSavedCohorts.TabIndex = 1;
            tlvSavedCohorts.UseCompatibleStateImageBehavior = false;
            tlvSavedCohorts.View = System.Windows.Forms.View.Details;
            tlvSavedCohorts.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.CellEditUseWholeCell = true;
            olvName.MinimumWidth = 100;
            olvName.Text = "Saved Cohorts";
            olvName.Width = 100;
            // 
            // olvProjectNumber
            // 
            olvProjectNumber.AspectName = "";
            olvProjectNumber.Text = "Project Number";
            // 
            // olvVersion
            // 
            olvVersion.AspectName = "";
            olvVersion.Text = "Version";
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 724);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(462, 23);
            tbFilter.TabIndex = 2;
            // 
            // SavedCohortsCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvSavedCohorts);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SavedCohortsCollectionUI";
            Size = new System.Drawing.Size(462, 747);
            ((System.ComponentModel.ISupportInitialize)tlvSavedCohorts).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvSavedCohorts;
        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvProjectNumber;
        private BrightIdeasSoftware.OLVColumn olvVersion;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
