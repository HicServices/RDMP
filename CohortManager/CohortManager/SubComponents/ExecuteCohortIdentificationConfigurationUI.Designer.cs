using CatalogueManager.AggregationUIs;

namespace CohortManager.SubComponents
{
    partial class ExecuteCohortIdentificationConfigurationUI
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
            this.CohortCompilerUI1 = new CohortManager.SubComponents.CohortCompilerUI();
            this.queryCachingServerSelector = new CatalogueManager.AggregationUIs.QueryCachingServerSelector();
            this.SuspendLayout();
            // 
            // CohortCompilerUI1
            // 
            this.CohortCompilerUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CohortCompilerUI1.Location = new System.Drawing.Point(3, 102);
            this.CohortCompilerUI1.Name = "CohortCompilerUI1";
            this.CohortCompilerUI1.Size = new System.Drawing.Size(699, 589);
            this.CohortCompilerUI1.TabIndex = 0;
            // 
            // queryCachingServerSelector
            // 
            this.queryCachingServerSelector.AutoSize = true;
            this.queryCachingServerSelector.Location = new System.Drawing.Point(3, 3);
            this.queryCachingServerSelector.Name = "queryCachingServerSelector";
            this.queryCachingServerSelector.SelecteExternalDatabaseServer = null;
            this.queryCachingServerSelector.Size = new System.Drawing.Size(683, 93);
            this.queryCachingServerSelector.TabIndex = 1;
            // 
            // ExecuteCohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.queryCachingServerSelector);
            this.Controls.Add(this.CohortCompilerUI1);
            this.Name = "ExecuteCohortIdentificationConfigurationUI";
            this.Size = new System.Drawing.Size(705, 694);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CohortCompilerUI CohortCompilerUI1;
        private QueryCachingServerSelector queryCachingServerSelector;
    }
}
