namespace DataExportManager.DataRelease
{
    partial class ConfigurationReleasePotentialUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationReleasePotentialUI));
            this.lbConfigurationName = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.State = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Dataset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastExecution = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.lbCohortName = new System.Windows.Forms.Label();
            this.lblConfigurationInvalid = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRelease = new System.Windows.Forms.Button();
            this.ragSmileyEnvironment = new ReusableUIComponents.RAGSmiley();
            this.SuspendLayout();
            // 
            // lbConfigurationName
            // 
            this.lbConfigurationName.AutoSize = true;
            this.lbConfigurationName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConfigurationName.Location = new System.Drawing.Point(3, 11);
            this.lbConfigurationName.Name = "lbConfigurationName";
            this.lbConfigurationName.Size = new System.Drawing.Size(86, 13);
            this.lbConfigurationName.TabIndex = 0;
            this.lbConfigurationName.Text = "Configuration:";
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.State,
            this.Dataset,
            this.LastExecution});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(6, 73);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(414, 282);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // State
            // 
            this.State.Text = "State";
            this.State.Width = 58;
            // 
            // Dataset
            // 
            this.Dataset.Text = "Dataset";
            this.Dataset.Width = 150;
            // 
            // LastExecution
            // 
            this.LastExecution.Text = "Last Execution";
            this.LastExecution.Width = 170;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FileMissing");
            this.imageList1.Images.SetKeyName(1, "NeverBeenGenerated");
            this.imageList1.Images.SetKeyName(2, "OutOfSync");
            this.imageList1.Images.SetKeyName(3, "Releaseable");
            this.imageList1.Images.SetKeyName(4, "WrongCohort");
            this.imageList1.Images.SetKeyName(5, "Exception");
            this.imageList1.Images.SetKeyName(6, "DifferentFromCatalogue");
            // 
            // lbCohortName
            // 
            this.lbCohortName.AutoSize = true;
            this.lbCohortName.Location = new System.Drawing.Point(3, 33);
            this.lbCohortName.Name = "lbCohortName";
            this.lbCohortName.Size = new System.Drawing.Size(44, 13);
            this.lbCohortName.TabIndex = 0;
            this.lbCohortName.Text = " Cohort:";
            // 
            // lblConfigurationInvalid
            // 
            this.lblConfigurationInvalid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConfigurationInvalid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblConfigurationInvalid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblConfigurationInvalid.ForeColor = System.Drawing.Color.DarkRed;
            this.lblConfigurationInvalid.Location = new System.Drawing.Point(20, 371);
            this.lblConfigurationInvalid.Name = "lblConfigurationInvalid";
            this.lblConfigurationInvalid.Size = new System.Drawing.Size(382, 33);
            this.lblConfigurationInvalid.TabIndex = 3;
            this.lblConfigurationInvalid.Text = "Loading...";
            this.lblConfigurationInvalid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblConfigurationInvalid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lblConfigurationInvalid_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 358);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Environment State:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Datasets:";
            // 
            // btnRelease
            // 
            this.btnRelease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRelease.Location = new System.Drawing.Point(327, 3);
            this.btnRelease.Name = "btnRelease";
            this.btnRelease.Size = new System.Drawing.Size(93, 64);
            this.btnRelease.TabIndex = 6;
            this.btnRelease.Text = "Add To Release >>>";
            this.btnRelease.UseVisualStyleBackColor = true;
            this.btnRelease.Click += new System.EventHandler(this.btnRelease_Click);
            // 
            // ragSmileyEnvironment
            // 
            this.ragSmileyEnvironment.AlwaysShowHandCursor = false;
            this.ragSmileyEnvironment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmileyEnvironment.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyEnvironment.Location = new System.Drawing.Point(368, 369);
            this.ragSmileyEnvironment.Name = "ragSmileyEnvironment";
            this.ragSmileyEnvironment.Size = new System.Drawing.Size(37, 38);
            this.ragSmileyEnvironment.TabIndex = 7;
            this.ragSmileyEnvironment.Visible = false;
            // 
            // ConfigurationReleasePotentialUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.ragSmileyEnvironment);
            this.Controls.Add(this.btnRelease);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblConfigurationInvalid);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.lbCohortName);
            this.Controls.Add(this.lbConfigurationName);
            this.Name = "ConfigurationReleasePotentialUI";
            this.Size = new System.Drawing.Size(423, 416);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbConfigurationName;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Dataset;
        private System.Windows.Forms.ColumnHeader LastExecution;
        private System.Windows.Forms.ColumnHeader State;
        private System.Windows.Forms.Label lbCohortName;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label lblConfigurationInvalid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRelease;
        private ReusableUIComponents.RAGSmiley ragSmileyEnvironment;
    }
}
