using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace CohortManager.Collections
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
            this.btnExpandOrCollapse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortIdentificationConfigurations)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvCohortIdentificationConfigurations
            // 
            this.tlvCohortIdentificationConfigurations.AllColumns.Add(this.olvName);
            this.tlvCohortIdentificationConfigurations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvCohortIdentificationConfigurations.CellEditUseWholeCell = false;
            this.tlvCohortIdentificationConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvCohortIdentificationConfigurations.Location = new System.Drawing.Point(3, 3);
            this.tlvCohortIdentificationConfigurations.Name = "tlvCohortIdentificationConfigurations";
            this.tlvCohortIdentificationConfigurations.ShowGroups = false;
            this.tlvCohortIdentificationConfigurations.Size = new System.Drawing.Size(497, 566);
            this.tlvCohortIdentificationConfigurations.TabIndex = 0;
            this.tlvCohortIdentificationConfigurations.UseCompatibleStateImageBehavior = false;
            this.tlvCohortIdentificationConfigurations.View = System.Windows.Forms.View.Details;
            this.tlvCohortIdentificationConfigurations.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Sortable = false;
            this.olvName.Text = "Cohort Identification Configurations";
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(446, 575);
            this.btnExpandOrCollapse.Name = "btnExpandOrCollapse";
            this.btnExpandOrCollapse.Size = new System.Drawing.Size(51, 22);
            this.btnExpandOrCollapse.TabIndex = 172;
            this.btnExpandOrCollapse.Text = "Expand";
            this.btnExpandOrCollapse.UseVisualStyleBackColor = true;
            this.btnExpandOrCollapse.Click += new System.EventHandler(this.btnExpandOrCollapse_Click);
            // 
            // CohortIdentificationCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExpandOrCollapse);
            this.Controls.Add(this.tlvCohortIdentificationConfigurations);
            this.Name = "CohortIdentificationCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortIdentificationConfigurations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvCohortIdentificationConfigurations;
        private OLVColumn olvName;
        private System.Windows.Forms.Button btnExpandOrCollapse;
    }
}
