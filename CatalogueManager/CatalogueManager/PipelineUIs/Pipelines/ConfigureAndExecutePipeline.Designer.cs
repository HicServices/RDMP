using System.Windows.Forms;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    partial class ConfigureAndExecutePipeline
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
            this.pPipelineSelection = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel_pipelineDiagram1 = new System.Windows.Forms.Panel();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnPreviewSource = new System.Windows.Forms.Button();
            this.lblTask = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.progressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.SuspendLayout();
            // 
            // pPipelineSelection
            // 
            this.pPipelineSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pPipelineSelection.Location = new System.Drawing.Point(75, 23);
            this.pPipelineSelection.Name = "pPipelineSelection";
            this.pPipelineSelection.Size = new System.Drawing.Size(702, 32);
            this.pPipelineSelection.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input Objects";
            // 
            // panel_pipelineDiagram1
            // 
            this.panel_pipelineDiagram1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_pipelineDiagram1.Location = new System.Drawing.Point(0, 99);
            this.panel_pipelineDiagram1.Name = "panel_pipelineDiagram1";
            this.panel_pipelineDiagram1.Size = new System.Drawing.Size(777, 110);
            this.panel_pipelineDiagram1.TabIndex = 6;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(84, 215);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 9;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnPreviewSource
            // 
            this.btnPreviewSource.Location = new System.Drawing.Point(3, 215);
            this.btnPreviewSource.Name = "btnPreviewSource";
            this.btnPreviewSource.Size = new System.Drawing.Size(75, 23);
            this.btnPreviewSource.TabIndex = 8;
            this.btnPreviewSource.Text = "Preview";
            this.btnPreviewSource.UseVisualStyleBackColor = true;
            this.btnPreviewSource.Click += new System.EventHandler(this.btnPreviewSource_Click);
            // 
            // lblTask
            // 
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(0, 1);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(34, 13);
            this.lblTask.TabIndex = 10;
            this.lblTask.Text = "Task:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Pipeline:";
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(6, 70);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(763, 26);
            this.rdmpObjectsRibbonUI1.TabIndex = 0;
            // 
            // progressUI1
            // 
            this.progressUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI1.Location = new System.Drawing.Point(0, 244);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(780, 553);
            this.progressUI1.TabIndex = 12;
            // 
            // ConfigureAndExecutePipeline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressUI1);
            this.Controls.Add(this.btnPreviewSource);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.pPipelineSelection);
            this.Controls.Add(this.panel_pipelineDiagram1);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(0, 600);
            this.Name = "ConfigureAndExecutePipeline";
            this.Size = new System.Drawing.Size(780, 800);
            this.Load += new System.EventHandler(this.ConfigureAndExecutePipeline_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Panel pPipelineSelection;
        private Label label1;
        private Panel panel_pipelineDiagram1;
        private Button btnExecute;
        private Button btnPreviewSource;
        private Label lblTask;
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private Label label4;
        private ReusableUIComponents.Progress.ProgressUI progressUI1;

    }
}
