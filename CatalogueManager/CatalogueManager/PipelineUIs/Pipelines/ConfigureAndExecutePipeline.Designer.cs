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
            this.label1 = new System.Windows.Forms.Label();
            this.pPipelineSelection = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel_pipelineDiagram1 = new System.Windows.Forms.Panel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tpConfigure = new System.Windows.Forms.TabPage();
            this.lblTask = new System.Windows.Forms.Label();
            this.lblContext = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPreviewSource = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.tpExecute = new System.Windows.Forms.TabPage();
            this.progressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.tabControl2.SuspendLayout();
            this.tpConfigure.SuspendLayout();
            this.tpExecute.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 333);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input Objects";
            // 
            // pPipelineSelection
            // 
            this.pPipelineSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pPipelineSelection.Location = new System.Drawing.Point(7, 146);
            this.pPipelineSelection.Name = "pPipelineSelection";
            this.pPipelineSelection.Size = new System.Drawing.Size(995, 165);
            this.pPipelineSelection.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 314);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Pipeline Diagram:";
            // 
            // panel_pipelineDiagram1
            // 
            this.panel_pipelineDiagram1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_pipelineDiagram1.Location = new System.Drawing.Point(3, 373);
            this.panel_pipelineDiagram1.Name = "panel_pipelineDiagram1";
            this.panel_pipelineDiagram1.Size = new System.Drawing.Size(996, 110);
            this.panel_pipelineDiagram1.TabIndex = 6;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tpConfigure);
            this.tabControl2.Controls.Add(this.tpExecute);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1016, 544);
            this.tabControl2.TabIndex = 8;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            // 
            // tpConfigure
            // 
            this.tpConfigure.AutoScroll = true;
            this.tpConfigure.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.tpConfigure.Controls.Add(this.lblTask);
            this.tpConfigure.Controls.Add(this.lblContext);
            this.tpConfigure.Controls.Add(this.label5);
            this.tpConfigure.Controls.Add(this.label3);
            this.tpConfigure.Controls.Add(this.btnPreviewSource);
            this.tpConfigure.Controls.Add(this.btnExecute);
            this.tpConfigure.Controls.Add(this.panel_pipelineDiagram1);
            this.tpConfigure.Controls.Add(this.label1);
            this.tpConfigure.Controls.Add(this.label2);
            this.tpConfigure.Controls.Add(this.pPipelineSelection);
            this.tpConfigure.Location = new System.Drawing.Point(4, 22);
            this.tpConfigure.Name = "tpConfigure";
            this.tpConfigure.Padding = new System.Windows.Forms.Padding(3);
            this.tpConfigure.Size = new System.Drawing.Size(1008, 518);
            this.tpConfigure.TabIndex = 0;
            this.tpConfigure.Text = "Configure";
            this.tpConfigure.UseVisualStyleBackColor = true;
            this.tpConfigure.Click += new System.EventHandler(this.tpConfigure_Click);
            // 
            // lblTask
            // 
            this.lblTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTask.Location = new System.Drawing.Point(59, 17);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(943, 97);
            this.lblTask.TabIndex = 10;
            // 
            // lblContext
            // 
            this.lblContext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblContext.Location = new System.Drawing.Point(59, 114);
            this.lblContext.Name = "lblContext";
            this.lblContext.Size = new System.Drawing.Size(943, 29);
            this.lblContext.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Context:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Task:";
            // 
            // btnPreviewSource
            // 
            this.btnPreviewSource.Location = new System.Drawing.Point(3, 489);
            this.btnPreviewSource.Name = "btnPreviewSource";
            this.btnPreviewSource.Size = new System.Drawing.Size(75, 23);
            this.btnPreviewSource.TabIndex = 8;
            this.btnPreviewSource.Text = "Preview";
            this.btnPreviewSource.UseVisualStyleBackColor = true;
            this.btnPreviewSource.Click += new System.EventHandler(this.btnPreviewSource_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(84, 489);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 9;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // tpExecute
            // 
            this.tpExecute.Controls.Add(this.progressUI1);
            this.tpExecute.Location = new System.Drawing.Point(4, 22);
            this.tpExecute.Name = "tpExecute";
            this.tpExecute.Padding = new System.Windows.Forms.Padding(3);
            this.tpExecute.Size = new System.Drawing.Size(1008, 586);
            this.tpExecute.TabIndex = 1;
            this.tpExecute.Text = "Execute / Preview";
            this.tpExecute.UseVisualStyleBackColor = true;
            // 
            // progressUI1
            // 
            this.progressUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressUI1.Location = new System.Drawing.Point(3, 3);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(1002, 580);
            this.progressUI1.TabIndex = 2;
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(6, 346);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(999, 26);
            this.rdmpObjectsRibbonUI1.TabIndex = 0;
            // 
            // ConfigureAndExecutePipeline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl2);
            this.Name = "ConfigureAndExecutePipeline";
            this.Size = new System.Drawing.Size(1016, 544);
            this.Load += new System.EventHandler(this.ConfigureAndExecutePipeline_Load);
            this.tabControl2.ResumeLayout(false);
            this.tpConfigure.ResumeLayout(false);
            this.tpConfigure.PerformLayout();
            this.tpExecute.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pPipelineSelection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel_pipelineDiagram1;
        private TabControl tabControl2;
        private TabPage tpConfigure;
        private TabPage tpExecute;
        private Button btnPreviewSource;
        private Button btnExecute;
        private ReusableUIComponents.Progress.ProgressUI progressUI1;
        private Label label5;
        private Label label3;
        private Label lblContext;
        private Label lblTask;
        private CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
    }
}
