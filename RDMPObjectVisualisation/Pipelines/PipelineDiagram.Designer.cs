namespace RDMPObjectVisualisation.Pipelines
{
    partial class PipelineDiagram<T>
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
            this.SuspendLayout();
            // 
            // flpPipelineDiagram
            // 
            this.flpPipelineDiagram.AutoScroll = true;
            this.flpPipelineDiagram.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpPipelineDiagram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPipelineDiagram.Location = new System.Drawing.Point(0, 0);
            this.flpPipelineDiagram.Name = "flpPipelineDiagram";
            this.flpPipelineDiagram.Size = new System.Drawing.Size(1000, 110);
            this.flpPipelineDiagram.TabIndex = 5;
            this.flpPipelineDiagram.Paint += new System.Windows.Forms.PaintEventHandler(this.flpPipelineDiagram_Paint);
            // 
            // PipelineDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flpPipelineDiagram);
            this.MinimumSize = new System.Drawing.Size(0, 110);
            this.Name = "PipelineDiagram";
            this.Size = new System.Drawing.Size(1000, 110);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpPipelineDiagram;
    }
}
