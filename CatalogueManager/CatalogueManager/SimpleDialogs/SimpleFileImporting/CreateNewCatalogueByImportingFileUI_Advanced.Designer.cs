using RDMPObjectVisualisation.Pipelines;

namespace CatalogueManager.SimpleDialogs.SimpleFileImporting
{
    partial class CreateNewCatalogueByImportingFileUI_Advanced
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewCatalogueByImportingFileUI_Advanced));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.configureAndExecutePipeline1 = new RDMPObjectVisualisation.Pipelines.ConfigureAndExecutePipeline();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Database");
            this.imageList1.Images.SetKeyName(1, "Table");
            // 
            // configureAndExecutePipeline1
            // 
            this.configureAndExecutePipeline1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configureAndExecutePipeline1.Location = new System.Drawing.Point(0, 0);
            this.configureAndExecutePipeline1.Name = "configureAndExecutePipeline1";
            this.configureAndExecutePipeline1.Size = new System.Drawing.Size(979, 894);
            this.configureAndExecutePipeline1.TabIndex = 14;
            this.configureAndExecutePipeline1.TaskDescription = resources.GetString("configureAndExecutePipeline1.TaskDescription");
            // 
            // CreateNewCatalogueByImportingFileUI_Advanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.configureAndExecutePipeline1);
            this.Name = "CreateNewCatalogueByImportingFileUI_Advanced";
            this.Size = new System.Drawing.Size(979, 894);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private ConfigureAndExecutePipeline configureAndExecutePipeline1;
    }
}