

namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    partial class HomeUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeUI));
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.boxCatalogue = new ResearchDataManagementPlatform.WindowManagement.HomePane.HomeBoxUI();
            this.boxCohort = new ResearchDataManagementPlatform.WindowManagement.HomePane.HomeBoxUI();
            this.boxProject = new ResearchDataManagementPlatform.WindowManagement.HomePane.HomeBoxUI();
            this.boxDataLoad = new ResearchDataManagementPlatform.WindowManagement.HomePane.HomeBoxUI();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label2.Location = new System.Drawing.Point(94, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(599, 39);
            this.label2.TabIndex = 0;
            this.label2.Text = "Research Data Management Platform";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox10.Image")));
            this.pictureBox10.Location = new System.Drawing.Point(18, 10);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(80, 80);
            this.pictureBox10.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox10.TabIndex = 8;
            this.pictureBox10.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.boxCatalogue);
            this.flowLayoutPanel1.Controls.Add(this.boxCohort);
            this.flowLayoutPanel1.Controls.Add(this.boxProject);
            this.flowLayoutPanel1.Controls.Add(this.boxDataLoad);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(18, 94);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1269, 603);
            this.flowLayoutPanel1.TabIndex = 17;
            // 
            // boxCatalogue
            // 
            this.boxCatalogue.Location = new System.Drawing.Point(3, 3);
            this.boxCatalogue.Name = "boxCatalogue";
            this.boxCatalogue.Size = new System.Drawing.Size(400, 315);
            this.boxCatalogue.TabIndex = 21;
            // 
            // boxCohort
            // 
            this.boxCohort.Location = new System.Drawing.Point(409, 3);
            this.boxCohort.Name = "boxCohort";
            this.boxCohort.Size = new System.Drawing.Size(400, 315);
            this.boxCohort.TabIndex = 22;
            // 
            // boxProject
            // 
            this.boxProject.Location = new System.Drawing.Point(815, 3);
            this.boxProject.Name = "boxProject";
            this.boxProject.Size = new System.Drawing.Size(400, 315);
            this.boxProject.TabIndex = 23;
            // 
            // boxDataLoad
            // 
            this.boxDataLoad.Location = new System.Drawing.Point(3, 324);
            this.boxDataLoad.Name = "boxDataLoad";
            this.boxDataLoad.Size = new System.Drawing.Size(400, 315);
            this.boxDataLoad.TabIndex = 24;
            // 
            // HomeUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.pictureBox10);
            this.Controls.Add(this.label2);
            this.Name = "HomeUI";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.Size = new System.Drawing.Size(1287, 697);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox10;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private HomeBoxUI boxCatalogue;
        private HomeBoxUI boxCohort;
        private HomeBoxUI boxProject;
        private HomeBoxUI boxDataLoad;
    }
}
