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
            components = new System.ComponentModel.Container();
            tlvCohortIdentificationConfigurations = new TreeListView();
            olvName = new OLVColumn();
            olvFrozen = new OLVColumn();
            tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)tlvCohortIdentificationConfigurations).BeginInit();
            SuspendLayout();
            // 
            // tlvCohortIdentificationConfigurations
            // 
            tlvCohortIdentificationConfigurations.AllColumns.Add(olvName);
            tlvCohortIdentificationConfigurations.AllColumns.Add(olvFrozen);
            tlvCohortIdentificationConfigurations.CellEditUseWholeCell = false;
            tlvCohortIdentificationConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName, olvFrozen });
            tlvCohortIdentificationConfigurations.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvCohortIdentificationConfigurations.Location = new System.Drawing.Point(0, 0);
            tlvCohortIdentificationConfigurations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvCohortIdentificationConfigurations.Name = "tlvCohortIdentificationConfigurations";
            tlvCohortIdentificationConfigurations.ShowGroups = false;
            tlvCohortIdentificationConfigurations.Size = new System.Drawing.Size(583, 692);
            tlvCohortIdentificationConfigurations.TabIndex = 0;
            tlvCohortIdentificationConfigurations.UseCompatibleStateImageBehavior = false;
            tlvCohortIdentificationConfigurations.View = System.Windows.Forms.View.Details;
            tlvCohortIdentificationConfigurations.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.MinimumWidth = 100;
            olvName.Sortable = false;
            olvName.Text = "Cohort Identification Configurations";
            olvName.Width = 100;
            // 
            // olvFrozen
            // 
            olvFrozen.IsEditable = false;
            olvFrozen.Text = "Frozen";
            // 
            // tbFilter
            // 
            tbFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            tbFilter.Location = new System.Drawing.Point(0, 669);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(583, 23);
            tbFilter.TabIndex = 1;
            // 
            // CohortIdentificationCollectionUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbFilter);
            Controls.Add(tlvCohortIdentificationConfigurations);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CohortIdentificationCollectionUI";
            Size = new System.Drawing.Size(583, 692);
            ((System.ComponentModel.ISupportInitialize)tlvCohortIdentificationConfigurations).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeListView tlvCohortIdentificationConfigurations;
        private OLVColumn olvName;
        private OLVColumn olvFrozen;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
