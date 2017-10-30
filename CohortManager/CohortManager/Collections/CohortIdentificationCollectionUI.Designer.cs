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
            this.label55 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
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
            this.tlvCohortIdentificationConfigurations.Size = new System.Drawing.Size(497, 545);
            this.tlvCohortIdentificationConfigurations.TabIndex = 0;
            this.tlvCohortIdentificationConfigurations.UseCompatibleStateImageBehavior = false;
            this.tlvCohortIdentificationConfigurations.View = System.Windows.Forms.View.Details;
            this.tlvCohortIdentificationConfigurations.VirtualMode = true;
            this.tlvCohortIdentificationConfigurations.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.tlvCohortIdentificationConfigurations_CellRightClick);
            this.tlvCohortIdentificationConfigurations.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tlvCohortIdentificationConfigurations_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Sortable = false;
            this.olvName.Text = "Cohort Identification Configurations";
            // 
            // label55
            // 
            this.label55.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(5, 580);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(32, 13);
            this.label55.TabIndex = 154;
            this.label55.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(43, 577);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(454, 20);
            this.tbFilter.TabIndex = 153;
            // 
            // btnExpandOrCollapse
            // 
            this.btnExpandOrCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpandOrCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            this.btnExpandOrCollapse.Location = new System.Drawing.Point(446, 551);
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
            this.Controls.Add(this.label55);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.tlvCohortIdentificationConfigurations);
            this.Name = "CohortIdentificationCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCohortIdentificationConfigurations)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvCohortIdentificationConfigurations;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.TextBox tbFilter;
        private OLVColumn olvName;
        private System.Windows.Forms.Button btnExpandOrCollapse;
    }
}
