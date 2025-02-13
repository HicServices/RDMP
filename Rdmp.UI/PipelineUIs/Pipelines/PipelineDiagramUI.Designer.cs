namespace Rdmp.UI.PipelineUIs.Pipelines
{
    sealed partial class PipelineDiagramUI
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
            this.flpPipelineDiagram = new System.Windows.Forms.FlowLayoutPanel();
            this.pInitializationObjects = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flpPipelineDiagram
            // 
            this.flpPipelineDiagram.AutoScroll = true;
            this.flpPipelineDiagram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPipelineDiagram.Location = new System.Drawing.Point(0, 0);
            this.flpPipelineDiagram.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.flpPipelineDiagram.Name = "flpPipelineDiagram";
            this.flpPipelineDiagram.Size = new System.Drawing.Size(1165, 97);
            this.flpPipelineDiagram.TabIndex = 5;
            // 
            // pInitializationObjects
            // 
            this.pInitializationObjects.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pInitializationObjects.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pInitializationObjects.Location = new System.Drawing.Point(0, 97);
            this.pInitializationObjects.Margin = new System.Windows.Forms.Padding(0);
            this.pInitializationObjects.Name = "pInitializationObjects";
            this.pInitializationObjects.Size = new System.Drawing.Size(1165, 28);
            this.pInitializationObjects.TabIndex = 6;
            // 
            // PipelineDiagramUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.flpPipelineDiagram);
            this.Controls.Add(this.pInitializationObjects);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(0, 127);
            this.Name = "PipelineDiagramUI";
            this.Size = new System.Drawing.Size(1165, 125);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpPipelineDiagram;
        private System.Windows.Forms.FlowLayoutPanel pInitializationObjects;
    }
}
