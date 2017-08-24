using ReusableLibraryCode.Checks;
using ReusableUIComponents.LinkLabels;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    partial class MEFStartupUI : ICheckNotifier
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MEFStartupUI));
            this.pbFolder = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbDosBox = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pbDownloadArrow = new System.Windows.Forms.PictureBox();
            this.lblFilesDownloaded = new System.Windows.Forms.Label();
            this.pbBigArrowPointingRight = new System.Windows.Forms.PictureBox();
            this.ragSmiley2 = new ReusableUIComponents.RAGSmiley();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.pathLinkLabel1 = new ReusableUIComponents.LinkLabels.PathLinkLabel();
            this.lblFilesLoaded = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDosBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadArrow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBigArrowPointingRight)).BeginInit();
            this.SuspendLayout();
            // 
            // pbFolder
            // 
            this.pbFolder.Image = ((System.Drawing.Image)(resources.GetObject("pbFolder.Image")));
            this.pbFolder.Location = new System.Drawing.Point(0, 7);
            this.pbFolder.Name = "pbFolder";
            this.pbFolder.Size = new System.Drawing.Size(87, 73);
            this.pbFolder.TabIndex = 0;
            this.pbFolder.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(150, 100);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(46, 50);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 153);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Catalogue (LoadModuleAssembly Table)";
            // 
            // pbDosBox
            // 
            this.pbDosBox.Image = ((System.Drawing.Image)(resources.GetObject("pbDosBox.Image")));
            this.pbDosBox.Location = new System.Drawing.Point(206, 0);
            this.pbDosBox.Name = "pbDosBox";
            this.pbDosBox.Size = new System.Drawing.Size(96, 80);
            this.pbDosBox.TabIndex = 4;
            this.pbDosBox.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(204, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Loaded Assemblies";
            // 
            // pbDownloadArrow
            // 
            this.pbDownloadArrow.Image = ((System.Drawing.Image)(resources.GetObject("pbDownloadArrow.Image")));
            this.pbDownloadArrow.Location = new System.Drawing.Point(6, 99);
            this.pbDownloadArrow.Name = "pbDownloadArrow";
            this.pbDownloadArrow.Size = new System.Drawing.Size(138, 50);
            this.pbDownloadArrow.TabIndex = 7;
            this.pbDownloadArrow.TabStop = false;
            // 
            // lblFilesDownloaded
            // 
            this.lblFilesDownloaded.AutoSize = true;
            this.lblFilesDownloaded.BackColor = System.Drawing.Color.Transparent;
            this.lblFilesDownloaded.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilesDownloaded.ForeColor = System.Drawing.Color.Yellow;
            this.lblFilesDownloaded.Location = new System.Drawing.Point(52, 121);
            this.lblFilesDownloaded.Name = "lblFilesDownloaded";
            this.lblFilesDownloaded.Size = new System.Drawing.Size(45, 13);
            this.lblFilesDownloaded.TabIndex = 8;
            this.lblFilesDownloaded.Text = "X Files";
            // 
            // pbBigArrowPointingRight
            // 
            this.pbBigArrowPointingRight.Image = ((System.Drawing.Image)(resources.GetObject("pbBigArrowPointingRight.Image")));
            this.pbBigArrowPointingRight.Location = new System.Drawing.Point(86, 21);
            this.pbBigArrowPointingRight.Name = "pbBigArrowPointingRight";
            this.pbBigArrowPointingRight.Size = new System.Drawing.Size(117, 51);
            this.pbBigArrowPointingRight.TabIndex = 9;
            this.pbBigArrowPointingRight.TabStop = false;
            // 
            // ragSmiley2
            // 
            this.ragSmiley2.AlwaysShowHandCursor = false;
            this.ragSmiley2.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley2.Location = new System.Drawing.Point(235, 25);
            this.ragSmiley2.Name = "ragSmiley2";
            this.ragSmiley2.Size = new System.Drawing.Size(34, 34);
            this.ragSmiley2.TabIndex = 5;
            this.ragSmiley2.Click += new System.EventHandler(this.ragSmiley2_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(22, 28);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(34, 34);
            this.ragSmiley1.TabIndex = 5;
            // 
            // pathLinkLabel1
            // 
            this.pathLinkLabel1.Location = new System.Drawing.Point(3, 83);
            this.pathLinkLabel1.Name = "pathLinkLabel1";
            this.pathLinkLabel1.Size = new System.Drawing.Size(141, 13);
            this.pathLinkLabel1.TabIndex = 3;
            this.pathLinkLabel1.TabStop = true;
            this.pathLinkLabel1.Text = "pathLinkLabel1";
            // 
            // lblFilesLoaded
            // 
            this.lblFilesLoaded.AutoSize = true;
            this.lblFilesLoaded.BackColor = System.Drawing.Color.Transparent;
            this.lblFilesLoaded.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilesLoaded.ForeColor = System.Drawing.Color.Yellow;
            this.lblFilesLoaded.Location = new System.Drawing.Point(93, 36);
            this.lblFilesLoaded.Name = "lblFilesLoaded";
            this.lblFilesLoaded.Size = new System.Drawing.Size(45, 13);
            this.lblFilesLoaded.TabIndex = 10;
            this.lblFilesLoaded.Text = "X Files";
            // 
            // MEFStartupUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblFilesLoaded);
            this.Controls.Add(this.pbBigArrowPointingRight);
            this.Controls.Add(this.lblFilesDownloaded);
            this.Controls.Add(this.pbDownloadArrow);
            this.Controls.Add(this.ragSmiley2);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.pbDosBox);
            this.Controls.Add(this.pathLinkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pbFolder);
            this.Name = "MEFStartupUI";
            this.Size = new System.Drawing.Size(300, 166);
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDosBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDownloadArrow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBigArrowPointingRight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbFolder;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private PathLinkLabel pathLinkLabel1;
        private System.Windows.Forms.PictureBox pbDosBox;
        private System.Windows.Forms.Label label3;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private ReusableUIComponents.RAGSmiley ragSmiley2;
        private System.Windows.Forms.PictureBox pbDownloadArrow;
        private System.Windows.Forms.Label lblFilesDownloaded;
        private System.Windows.Forms.PictureBox pbBigArrowPointingRight;
        private System.Windows.Forms.Label lblFilesLoaded;
    }
}
