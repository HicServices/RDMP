using BrightIdeasSoftware;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections
{
    partial class CohortIdentificationCollectionUI : ILifetimeSubscriber
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
            this.tlvCohortIdentificationConfigurations = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvFrozen = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortIdentificationConfigurations)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvCohortIdentificationConfigurations
            // 
            this.tlvCohortIdentificationConfigurations.AllColumns.Add(this.olvName);
            this.tlvCohortIdentificationConfigurations.AllColumns.Add(this.olvFrozen);
            this.tlvCohortIdentificationConfigurations.CellEditUseWholeCell = false;
            this.tlvCohortIdentificationConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,olvFrozen});
            this.tlvCohortIdentificationConfigurations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCohortIdentificationConfigurations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvCohortIdentificationConfigurations.Location = new System.Drawing.Point(0, 0);
            this.tlvCohortIdentificationConfigurations.Name = "tlvCohortIdentificationConfigurations";
            this.tlvCohortIdentificationConfigurations.ShowGroups = false;
            this.tlvCohortIdentificationConfigurations.Size = new System.Drawing.Size(500, 600);
            this.tlvCohortIdentificationConfigurations.TabIndex = 0;
            this.tlvCohortIdentificationConfigurations.UseCompatibleStateImageBehavior = false;
            this.tlvCohortIdentificationConfigurations.View = System.Windows.Forms.View.Details;
            this.tlvCohortIdentificationConfigurations.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.Sortable = false;
            this.olvName.Text = "Cohort Identification Configurations";
            this.olvName.MinimumWidth = 100;
            // 
            // olvFrozen
            //
            // 
            this.olvFrozen.Sortable = true;
            this.olvFrozen.Text = "Frozen";
            this.olvFrozen.IsEditable = false;
            // CohortIdentificationCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvCohortIdentificationConfigurations);
            this.Name = "CohortIdentificationCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortIdentificationConfigurations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvCohortIdentificationConfigurations;
        private OLVColumn olvName;
        private OLVColumn olvFrozen;
    }
}
