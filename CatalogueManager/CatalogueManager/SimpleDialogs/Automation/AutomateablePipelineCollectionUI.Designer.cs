namespace CatalogueManager.SimpleDialogs.Automation
{
    partial class AutomateablePipelineCollectionUI
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
            this.lbAutomationPipelines = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.btnEditPipeline = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lbAutomationPipelines
            // 
            this.lbAutomationPipelines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbAutomationPipelines.FormattingEnabled = true;
            this.lbAutomationPipelines.Location = new System.Drawing.Point(3, 18);
            this.lbAutomationPipelines.Name = "lbAutomationPipelines";
            this.lbAutomationPipelines.Size = new System.Drawing.Size(274, 446);
            this.lbAutomationPipelines.TabIndex = 0;
            this.lbAutomationPipelines.SelectedIndexChanged += new System.EventHandler(this.lbAutomationPipelines_SelectedIndexChanged);
            this.lbAutomationPipelines.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbAutomationPipelines_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Automateable Pipelines (Plugins that use automation)";
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateNew.Location = new System.Drawing.Point(6, 478);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(271, 23);
            this.btnCreateNew.TabIndex = 2;
            this.btnCreateNew.Text = "Create New Automation Pipeline";
            this.btnCreateNew.UseVisualStyleBackColor = true;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // btnEditPipeline
            // 
            this.btnEditPipeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditPipeline.Enabled = false;
            this.btnEditPipeline.Location = new System.Drawing.Point(283, 478);
            this.btnEditPipeline.Name = "btnEditPipeline";
            this.btnEditPipeline.Size = new System.Drawing.Size(792, 23);
            this.btnEditPipeline.TabIndex = 3;
            this.btnEditPipeline.Text = "Edit Pipeline";
            this.btnEditPipeline.UseVisualStyleBackColor = true;
            this.btnEditPipeline.Click += new System.EventHandler(this.btnEditPipeline_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(287, 18);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(789, 446);
            this.panel1.TabIndex = 4;
            // 
            // AutomateablePipelineCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnEditPipeline);
            this.Controls.Add(this.btnCreateNew);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbAutomationPipelines);
            this.Name = "AutomateablePipelineCollectionUI";
            this.Size = new System.Drawing.Size(1079, 517);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbAutomationPipelines;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Button btnEditPipeline;
        private System.Windows.Forms.Panel panel1;
    }
}
