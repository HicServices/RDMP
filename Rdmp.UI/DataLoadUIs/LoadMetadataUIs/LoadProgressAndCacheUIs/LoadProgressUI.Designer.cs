﻿using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    partial class LoadProgressUI
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
            this.nDefaultNumberOfDaysToLoadEachTime = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tbOriginDate = new System.Windows.Forms.TextBox();
            this.btnEditLoadProgress = new System.Windows.Forms.Button();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbDataLoadProgress = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.loadProgressDiagram1 = new LoadProgressDiagramUI();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.nDefaultNumberOfDaysToLoadEachTime)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nDefaultNumberOfDaysToLoadEachTime
            // 
            this.nDefaultNumberOfDaysToLoadEachTime.Location = new System.Drawing.Point(140, 108);
            this.nDefaultNumberOfDaysToLoadEachTime.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nDefaultNumberOfDaysToLoadEachTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nDefaultNumberOfDaysToLoadEachTime.Name = "nDefaultNumberOfDaysToLoadEachTime";
            this.nDefaultNumberOfDaysToLoadEachTime.Size = new System.Drawing.Size(120, 20);
            this.nDefaultNumberOfDaysToLoadEachTime.TabIndex = 0;
            this.nDefaultNumberOfDaysToLoadEachTime.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Number Of Days To Load:";
            // 
            // tbOriginDate
            // 
            this.tbOriginDate.Location = new System.Drawing.Point(140, 56);
            this.tbOriginDate.Name = "tbOriginDate";
            this.tbOriginDate.Size = new System.Drawing.Size(162, 20);
            this.tbOriginDate.TabIndex = 1;
            this.tbOriginDate.TextChanged += new System.EventHandler(this.tbOriginDate_TextChanged);
            // 
            // btnEditLoadProgress
            // 
            this.btnEditLoadProgress.Location = new System.Drawing.Point(276, 80);
            this.btnEditLoadProgress.Name = "btnEditLoadProgress";
            this.btnEditLoadProgress.Size = new System.Drawing.Size(40, 23);
            this.btnEditLoadProgress.TabIndex = 5;
            this.btnEditLoadProgress.Text = "Edit";
            this.btnEditLoadProgress.UseVisualStyleBackColor = true;
            this.btnEditLoadProgress.Click += new System.EventHandler(this.btnEditLoadProgress_Click);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(140, 4);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(97, 20);
            this.tbID.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(113, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "ID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(83, 85);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Progress:";
            // 
            // tbDataLoadProgress
            // 
            this.tbDataLoadProgress.Location = new System.Drawing.Point(140, 82);
            this.tbDataLoadProgress.Name = "tbDataLoadProgress";
            this.tbDataLoadProgress.ReadOnly = true;
            this.tbDataLoadProgress.Size = new System.Drawing.Size(130, 20);
            this.tbDataLoadProgress.TabIndex = 4;
            this.tbDataLoadProgress.TextChanged += new System.EventHandler(this.tbDataLoadProgress_TextChanged);
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(140, 30);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(350, 20);
            this.tbName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(71, 59);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "Origin Date:";
            // 
            // loadProgressDiagram1
            // 
            this.loadProgressDiagram1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadProgressDiagram1.Location = new System.Drawing.Point(6, 134);
            this.loadProgressDiagram1.Name = "loadProgressDiagram1";
            this.loadProgressDiagram1.Size = new System.Drawing.Size(930, 681);
            this.loadProgressDiagram1.TabIndex = 26;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.nDefaultNumberOfDaysToLoadEachTime);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.loadProgressDiagram1);
            this.panel1.Controls.Add(this.btnEditLoadProgress);
            this.panel1.Controls.Add(this.tbOriginDate);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.tbDataLoadProgress);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(939, 818);
            this.panel1.TabIndex = 28;
            // 
            // LoadProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "LoadProgressUI";
            this.Size = new System.Drawing.Size(939, 818);
            ((System.ComponentModel.ISupportInitialize)(this.nDefaultNumberOfDaysToLoadEachTime)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.TextBox tbDataLoadProgress;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.NumericUpDown nDefaultNumberOfDaysToLoadEachTime;
        private System.Windows.Forms.Button btnEditLoadProgress;
        private System.Windows.Forms.TextBox tbOriginDate;
        private Diagrams.LoadProgressDiagramUI loadProgressDiagram1;
        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Panel panel1;
    }
}
