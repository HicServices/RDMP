using CatalogueManager.PipelineUIs.DemandsInitializationUIs;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs
{
    partial class ArgumentCollection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArgumentCollection));
            this.lblClassName = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnViewSourceCode = new System.Windows.Forms.Button();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.lblNoArguments = new System.Windows.Forms.Label();
            this.pArguments = new System.Windows.Forms.Panel();
            this.lblTypeUnloadable = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblClassName
            // 
            this.lblClassName.AutoSize = true;
            this.lblClassName.Location = new System.Drawing.Point(22, 2);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(60, 13);
            this.lblClassName.TabIndex = 3;
            this.lblClassName.Text = "ClassName";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(4, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // btnViewSourceCode
            // 
            this.btnViewSourceCode.Location = new System.Drawing.Point(235, -1);
            this.btnViewSourceCode.Name = "btnViewSourceCode";
            this.btnViewSourceCode.Size = new System.Drawing.Size(52, 19);
            this.btnViewSourceCode.TabIndex = 6;
            this.btnViewSourceCode.Text = "Source";
            this.btnViewSourceCode.UseVisualStyleBackColor = true;
            this.btnViewSourceCode.Click += new System.EventHandler(this.btnViewSourceCode_Click);
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(210, 0);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 8;
            // 
            // lblNoArguments
            // 
            this.lblNoArguments.AutoSize = true;
            this.lblNoArguments.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoArguments.Location = new System.Drawing.Point(276, 41);
            this.lblNoArguments.Name = "lblNoArguments";
            this.lblNoArguments.Size = new System.Drawing.Size(107, 13);
            this.lblNoArguments.TabIndex = 0;
            this.lblNoArguments.Text = "No Arguments Found";
            this.lblNoArguments.Visible = false;
            // 
            // pArguments
            // 
            this.pArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pArguments.AutoScroll = true;
            this.pArguments.Location = new System.Drawing.Point(4, 22);
            this.pArguments.Name = "pArguments";
            this.pArguments.Size = new System.Drawing.Size(713, 438);
            this.pArguments.TabIndex = 9;
            // 
            // lblTypeUnloadable
            // 
            this.lblTypeUnloadable.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblTypeUnloadable.AutoSize = true;
            this.lblTypeUnloadable.ForeColor = System.Drawing.Color.Red;
            this.lblTypeUnloadable.Location = new System.Drawing.Point(273, 70);
            this.lblTypeUnloadable.Name = "lblTypeUnloadable";
            this.lblTypeUnloadable.Size = new System.Drawing.Size(141, 13);
            this.lblTypeUnloadable.TabIndex = 10;
            this.lblTypeUnloadable.Text = "Component Type Not Found";
            // 
            // ArgumentCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.lblTypeUnloadable);
            this.Controls.Add(this.lblNoArguments);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.btnViewSourceCode);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblClassName);
            this.Controls.Add(this.pArguments);
            this.Name = "ArgumentCollection";
            this.Size = new System.Drawing.Size(720, 463);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblClassName;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnViewSourceCode;
        private ReusableUIComponents.HelpIcon helpIcon1;
        private System.Windows.Forms.Label lblNoArguments;
        private System.Windows.Forms.Panel pArguments;
        private System.Windows.Forms.Label lblTypeUnloadable;
    }
}
