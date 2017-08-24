using DataLoadEngine.DataFlowPipeline;
using RDMPObjectVisualisation.Pipelines;

namespace DataExportManager.CohortUI.ImportCustomData
{
    partial class ImportCustomDataFileUI 
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportCustomDataFileUI));
            this.configureAndExecutePipeline1 = new ConfigureAndExecutePipeline();
            this.SuspendLayout();
            // 
            // configureAndExecutePipeline1
            // 
            this.configureAndExecutePipeline1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configureAndExecutePipeline1.Location = new System.Drawing.Point(0, 0);
            this.configureAndExecutePipeline1.Name = "configureAndExecutePipeline1";
            this.configureAndExecutePipeline1.Size = new System.Drawing.Size(1266, 872);
            this.configureAndExecutePipeline1.TabIndex = 0;
            this.configureAndExecutePipeline1.TaskDescription = resources.GetString("configureAndExecutePipeline1.TaskDescription");
            // 
            // ImportCustomDataFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 872);
            this.Controls.Add(this.configureAndExecutePipeline1);
            this.Name = "ImportCustomDataFileUI";
            this.Text = "ImportCustomDataFileUI";
            this.ResumeLayout(false);

        }

        #endregion

        private ConfigureAndExecutePipeline configureAndExecutePipeline1;

    }
}