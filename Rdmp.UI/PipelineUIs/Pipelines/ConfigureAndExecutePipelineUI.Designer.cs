using System.Windows.Forms;
using Rdmp.UI.Progress;

namespace Rdmp.UI.PipelineUIs.Pipelines
{
    partial class ConfigureAndExecutePipelineUI
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
            this.panel_pipelineDiagram1 = new System.Windows.Forms.Panel();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnPreviewSource = new System.Windows.Forms.Button();
            this.lblTask = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressUI1 = new Rdmp.UI.Progress.ProgressUI();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pPipelineSelection
            // 
            this.pPipelineSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pPipelineSelection.Location = new System.Drawing.Point(74, 30);
            this.pPipelineSelection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pPipelineSelection.Name = "pPipelineSelection";
            this.pPipelineSelection.Size = new System.Drawing.Size(832, 30);
            this.pPipelineSelection.TabIndex = 3;
            // 
            // panel_pipelineDiagram1
            // 
            this.panel_pipelineDiagram1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_pipelineDiagram1.Location = new System.Drawing.Point(7, 66);
            this.panel_pipelineDiagram1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel_pipelineDiagram1.Name = "panel_pipelineDiagram1";
            this.panel_pipelineDiagram1.Size = new System.Drawing.Size(899, 127);
            this.panel_pipelineDiagram1.TabIndex = 6;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(103, 199);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(88, 27);
            this.btnExecute.TabIndex = 9;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnPreviewSource
            // 
            this.btnPreviewSource.Location = new System.Drawing.Point(7, 199);
            this.btnPreviewSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPreviewSource.Name = "btnPreviewSource";
            this.btnPreviewSource.Size = new System.Drawing.Size(88, 27);
            this.btnPreviewSource.TabIndex = 8;
            this.btnPreviewSource.Text = "Preview";
            this.btnPreviewSource.UseVisualStyleBackColor = true;
            this.btnPreviewSource.Click += new System.EventHandler(this.btnPreviewSource_Click);
            // 
            // lblTask
            // 
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(8, 10);
            this.lblTask.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(32, 15);
            this.lblTask.TabIndex = 10;
            this.lblTask.Text = "Task:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(8, 35);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Pipeline:";
            // 
            // progressUI1
            // 
            this.progressUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI1.Location = new System.Drawing.Point(7, 232);
            this.progressUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(899, 688);
            this.progressUI1.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblTask);
            this.panel1.Controls.Add(this.progressUI1);
            this.panel1.Controls.Add(this.pPipelineSelection);
            this.panel1.Controls.Add(this.btnPreviewSource);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.btnExecute);
            this.panel1.Controls.Add(this.panel_pipelineDiagram1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(910, 923);
            this.panel1.TabIndex = 13;
            // 
            // ConfigureAndExecutePipelineUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(0, 692);
            this.Name = "ConfigureAndExecutePipelineUI";
            this.Size = new System.Drawing.Size(910, 923);
            this.Load += new System.EventHandler(this.ConfigureAndExecutePipeline_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel pPipelineSelection;
        private Panel panel_pipelineDiagram1;
        private Button btnExecute;
        private Button btnPreviewSource;
        private Label lblTask;
        private Label label4;
        private ProgressUI progressUI1;
        private Panel panel1;

    }
}
