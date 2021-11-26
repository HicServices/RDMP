namespace Rdmp.UI.PipelineUIs.Pipelines.Models
{
    partial class DividerLineControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DividerLineControl));
            this.pbDividerLine = new System.Windows.Forms.PictureBox();
            this.pbDropPrompt = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDividerLine)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDropPrompt)).BeginInit();
            this.SuspendLayout();
            // 
            // pbDividerLine
            // 
            this.pbDividerLine.Image = ((System.Drawing.Image)(resources.GetObject("pbDividerLine.Image")));
            this.pbDividerLine.InitialImage = null;
            this.pbDividerLine.Location = new System.Drawing.Point(6, 5);
            this.pbDividerLine.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbDividerLine.Name = "pbDividerLine";
            this.pbDividerLine.Size = new System.Drawing.Size(12, 56);
            this.pbDividerLine.TabIndex = 0;
            this.pbDividerLine.TabStop = false;
            // 
            // pbDropPrompt
            // 
            this.pbDropPrompt.Image = ((System.Drawing.Image)(resources.GetObject("pbDropPrompt.Image")));
            this.pbDropPrompt.InitialImage = null;
            this.pbDropPrompt.Location = new System.Drawing.Point(5, 4);
            this.pbDropPrompt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbDropPrompt.Name = "pbDropPrompt";
            this.pbDropPrompt.Size = new System.Drawing.Size(12, 52);
            this.pbDropPrompt.TabIndex = 1;
            this.pbDropPrompt.TabStop = false;
            // 
            // DividerLineControl
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbDropPrompt);
            this.Controls.Add(this.pbDividerLine);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DividerLineControl";
            this.Size = new System.Drawing.Size(22, 58);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DividerLineControl_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.DividerLineControl_DragOver);
            this.DragLeave += new System.EventHandler(this.DividerLineControl_DragLeave);
            ((System.ComponentModel.ISupportInitialize)(this.pbDividerLine)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDropPrompt)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbDividerLine;
        private System.Windows.Forms.PictureBox pbDropPrompt;


    }
}
