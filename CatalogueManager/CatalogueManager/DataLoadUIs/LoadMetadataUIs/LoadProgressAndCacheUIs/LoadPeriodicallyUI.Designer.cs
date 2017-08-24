namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    partial class LoadPeriodicallyUI
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
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.tbLastLoaded = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDaysToWaitBetweenLoads = new System.Windows.Forms.TextBox();
            this.tbNextLoadWillBe = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ddOnsuccessfulLoadLaunch = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnNone = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(431, 8);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(431, 36);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tbLastLoaded
            // 
            this.tbLastLoaded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLastLoaded.Location = new System.Drawing.Point(75, 10);
            this.tbLastLoaded.Name = "tbLastLoaded";
            this.tbLastLoaded.Size = new System.Drawing.Size(350, 20);
            this.tbLastLoaded.TabIndex = 1;
            this.tbLastLoaded.TextChanged += new System.EventHandler(this.tbLastLoaded_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Last Loaded";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "DaysToWaitBetweenLoads";
            // 
            // tbDaysToWaitBetweenLoads
            // 
            this.tbDaysToWaitBetweenLoads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDaysToWaitBetweenLoads.Location = new System.Drawing.Point(146, 33);
            this.tbDaysToWaitBetweenLoads.Name = "tbDaysToWaitBetweenLoads";
            this.tbDaysToWaitBetweenLoads.Size = new System.Drawing.Size(279, 20);
            this.tbDaysToWaitBetweenLoads.TabIndex = 1;
            this.tbDaysToWaitBetweenLoads.TextChanged += new System.EventHandler(this.tbDaysToWaitBetweenLoads_TextChanged);
            // 
            // tbNextLoadWillBe
            // 
            this.tbNextLoadWillBe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNextLoadWillBe.Location = new System.Drawing.Point(104, 59);
            this.tbNextLoadWillBe.Name = "tbNextLoadWillBe";
            this.tbNextLoadWillBe.ReadOnly = true;
            this.tbNextLoadWillBe.Size = new System.Drawing.Size(321, 20);
            this.tbNextLoadWillBe.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Next Load Will Be ";
            // 
            // ddOnsuccessfulLoadLaunch
            // 
            this.ddOnsuccessfulLoadLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddOnsuccessfulLoadLaunch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOnsuccessfulLoadLaunch.FormattingEnabled = true;
            this.ddOnsuccessfulLoadLaunch.Location = new System.Drawing.Point(146, 86);
            this.ddOnsuccessfulLoadLaunch.Name = "ddOnsuccessfulLoadLaunch";
            this.ddOnsuccessfulLoadLaunch.Size = new System.Drawing.Size(306, 21);
            this.ddOnsuccessfulLoadLaunch.Sorted = true;
            this.ddOnsuccessfulLoadLaunch.TabIndex = 3;
            this.ddOnsuccessfulLoadLaunch.SelectedIndexChanged += new System.EventHandler(this.ddOnsuccessfulLoadLaunch_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(145, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "On Successful Load Launch:";
            // 
            // btnNone
            // 
            this.btnNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNone.Location = new System.Drawing.Point(458, 84);
            this.btnNone.Name = "btnNone";
            this.btnNone.Size = new System.Drawing.Size(45, 23);
            this.btnNone.TabIndex = 0;
            this.btnNone.Text = "None";
            this.btnNone.UseVisualStyleBackColor = true;
            this.btnNone.Click += new System.EventHandler(this.btnNone_Click);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(143, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(309, 29);
            this.label5.TabIndex = 4;
            this.label5.Text = "(Optional, use this to select another load that comes automatically after this on" +
    "e e.g. MyLoadPart2)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LoadPeriodicallyUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ddOnsuccessfulLoadLaunch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbNextLoadWillBe);
            this.Controls.Add(this.tbDaysToWaitBetweenLoads);
            this.Controls.Add(this.tbLastLoaded);
            this.Controls.Add(this.btnNone);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCreate);
            this.Name = "LoadPeriodicallyUI";
            this.Size = new System.Drawing.Size(510, 180);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TextBox tbLastLoaded;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDaysToWaitBetweenLoads;
        private System.Windows.Forms.TextBox tbNextLoadWillBe;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ddOnsuccessfulLoadLaunch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnNone;
        private System.Windows.Forms.Label label5;
    }
}
