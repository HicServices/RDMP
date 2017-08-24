namespace ReusableUIComponents
{
    partial class RAGSmiley
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RAGSmiley));
            this.pbGreen = new System.Windows.Forms.PictureBox();
            this.pbYellow = new System.Windows.Forms.PictureBox();
            this.pbRed = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbYellow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).BeginInit();
            this.SuspendLayout();
            // 
            // pbGreen
            // 
            this.pbGreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbGreen.BackColor = System.Drawing.Color.Transparent;
            this.pbGreen.Image = ((System.Drawing.Image)(resources.GetObject("pbGreen.Image")));
            this.pbGreen.Location = new System.Drawing.Point(0, 0);
            this.pbGreen.Name = "pbGreen";
            this.pbGreen.Size = new System.Drawing.Size(25, 25);
            this.pbGreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbGreen.TabIndex = 0;
            this.pbGreen.TabStop = false;
            this.pbGreen.Click += new System.EventHandler(this.pb_Click);
            // 
            // pbYellow
            // 
            this.pbYellow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbYellow.BackColor = System.Drawing.Color.Transparent;
            this.pbYellow.Image = ((System.Drawing.Image)(resources.GetObject("pbYellow.Image")));
            this.pbYellow.Location = new System.Drawing.Point(0, 0);
            this.pbYellow.Name = "pbYellow";
            this.pbYellow.Size = new System.Drawing.Size(25, 25);
            this.pbYellow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbYellow.TabIndex = 0;
            this.pbYellow.TabStop = false;
            this.pbYellow.Click += new System.EventHandler(this.pb_Click);
            // 
            // pbRed
            // 
            this.pbRed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbRed.BackColor = System.Drawing.Color.Transparent;
            this.pbRed.Image = ((System.Drawing.Image)(resources.GetObject("pbRed.Image")));
            this.pbRed.Location = new System.Drawing.Point(0, 0);
            this.pbRed.Name = "pbRed";
            this.pbRed.Size = new System.Drawing.Size(25, 25);
            this.pbRed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbRed.TabIndex = 0;
            this.pbRed.TabStop = false;
            this.pbRed.Click += new System.EventHandler(this.pb_Click);
            // 
            // RAGSmiley
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbRed);
            this.Controls.Add(this.pbYellow);
            this.Controls.Add(this.pbGreen);
            this.Name = "RAGSmiley";
            this.Size = new System.Drawing.Size(25, 25);
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbYellow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pbGreen;
        private System.Windows.Forms.PictureBox pbYellow;
        private System.Windows.Forms.PictureBox pbRed;
    }
}
