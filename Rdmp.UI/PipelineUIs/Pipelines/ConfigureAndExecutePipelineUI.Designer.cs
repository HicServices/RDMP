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
            this.label4 = new System.Windows.Forms.Label();
            this.progressUI1 = new Rdmp.UI.Progress.ProgressUI();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pPipelineSelection
            // 
            this.pPipelineSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.pPipelineSelection.Location = new System.Drawing.Point(68, 0);
            this.pPipelineSelection.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pPipelineSelection.Name = "pPipelineSelection";
            this.pPipelineSelection.Size = new System.Drawing.Size(836, 30);
            this.pPipelineSelection.TabIndex = 3;
            // 
            // panel_pipelineDiagram1
            // 
            this.panel_pipelineDiagram1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_pipelineDiagram1.Location = new System.Drawing.Point(3, 53);
            this.panel_pipelineDiagram1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel_pipelineDiagram1.Name = "panel_pipelineDiagram1";
            this.panel_pipelineDiagram1.Size = new System.Drawing.Size(904, 127);
            this.panel_pipelineDiagram1.TabIndex = 6;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(100, 3);
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
            this.btnPreviewSource.Location = new System.Drawing.Point(4, 3);
            this.btnPreviewSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPreviewSource.Name = "btnPreviewSource";
            this.btnPreviewSource.Size = new System.Drawing.Size(88, 27);
            this.btnPreviewSource.TabIndex = 8;
            this.btnPreviewSource.Text = "Preview";
            this.btnPreviewSource.UseVisualStyleBackColor = true;
            this.btnPreviewSource.Click += new System.EventHandler(this.btnPreviewSource_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Pipeline:";
            // 
            // progressUI1
            // 
            this.progressUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressUI1.Location = new System.Drawing.Point(3, 54);
            this.progressUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(904, 641);
            this.progressUI1.TabIndex = 12;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnPreviewSource);
            this.panel3.Controls.Add(this.btnExecute);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 19);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(904, 35);
            this.panel3.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pPipelineSelection);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(904, 34);
            this.panel2.TabIndex = 13;
            // 
            // taskDescriptionLabel1
            // 
            this.taskDescriptionLabel1.AutoSize = true;
            this.taskDescriptionLabel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
            this.taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            this.taskDescriptionLabel1.Size = new System.Drawing.Size(910, 42);
            this.taskDescriptionLabel1.TabIndex = 14;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel_pipelineDiagram1);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 42);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(910, 183);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "1. Select Pipeline";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.progressUI1);
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 225);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(910, 698);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "2. Execute Pipeline";
            // 
            // ConfigureAndExecutePipelineUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.taskDescriptionLabel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(0, 692);
            this.Name = "ConfigureAndExecutePipelineUI";
            this.Size = new System.Drawing.Size(910, 923);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Panel pPipelineSelection;
        private Panel panel_pipelineDiagram1;
        private Button btnExecute;
        private Button btnPreviewSource;
        private Label label4;
        private ProgressUI progressUI1;
        private SimpleDialogs.TaskDescriptionLabel taskDescriptionLabel1;
        private Panel panel3;
        private Panel panel2;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
    }
}
