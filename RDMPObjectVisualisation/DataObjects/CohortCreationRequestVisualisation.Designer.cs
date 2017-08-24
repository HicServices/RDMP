namespace RDMPObjectVisualisation.DataObjects
{
    partial class CohortCreationRequestVisualisation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortCreationRequestVisualisation));
            this.pbDefinition = new System.Windows.Forms.PictureBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.checksUIIconOnly1 = new ReusableUIComponents.ChecksUI.ChecksUIIconOnly();
            this.pbTarget = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefinition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTarget)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // pbDefinition
            // 
            this.pbDefinition.Image = ((System.Drawing.Image)(resources.GetObject("pbDefinition.Image")));
            this.pbDefinition.InitialImage = null;
            this.pbDefinition.Location = new System.Drawing.Point(15, 3);
            this.pbDefinition.Name = "pbDefinition";
            this.pbDefinition.Size = new System.Drawing.Size(54, 44);
            this.pbDefinition.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbDefinition.TabIndex = 3;
            this.pbDefinition.TabStop = false;
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(3, 50);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(192, 33);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = "Entity:";
            // 
            // lblType
            // 
            this.lblType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblType.Location = new System.Drawing.Point(-1, 83);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(200, 16);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "CohortCreationRequest";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checksUIIconOnly1
            // 
            this.checksUIIconOnly1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checksUIIconOnly1.Location = new System.Drawing.Point(175, 3);
            this.checksUIIconOnly1.Name = "checksUIIconOnly1";
            this.checksUIIconOnly1.Size = new System.Drawing.Size(20, 20);
            this.checksUIIconOnly1.TabIndex = 1;
            // 
            // pbTarget
            // 
            this.pbTarget.Image = ((System.Drawing.Image)(resources.GetObject("pbTarget.Image")));
            this.pbTarget.InitialImage = null;
            this.pbTarget.Location = new System.Drawing.Point(119, 3);
            this.pbTarget.Name = "pbTarget";
            this.pbTarget.Size = new System.Drawing.Size(44, 44);
            this.pbTarget.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbTarget.TabIndex = 4;
            this.pbTarget.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.InitialImage = null;
            this.pictureBox3.Location = new System.Drawing.Point(73, 15);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(44, 20);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 5;
            this.pictureBox3.TabStop = false;
            // 
            // ExternalCohortTableVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pbTarget);
            this.Controls.Add(this.pbDefinition);
            this.Controls.Add(this.checksUIIconOnly1);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblDescription);
            this.Name = "CohortCreationRequestVisualisation";
            this.Size = new System.Drawing.Size(198, 98);
            this.Load += new System.EventHandler(this.ExternalCohortTableVisualisation_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbDefinition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTarget)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ReusableUIComponents.ChecksUI.ChecksUIIconOnly checksUIIconOnly1;
        private System.Windows.Forms.PictureBox pbDefinition;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.PictureBox pbTarget;
        private System.Windows.Forms.PictureBox pictureBox3;
    }
}
