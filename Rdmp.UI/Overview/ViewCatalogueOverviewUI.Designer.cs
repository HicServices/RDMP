using BrightIdeasSoftware;
using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.Overview
{
    partial class ViewCatalogueOverviewUI
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewCatalogueOverviewUI));
            panel1 = new System.Windows.Forms.Panel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            btnSettings = new System.Windows.Forms.Button();
            lblDescription = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            panel4 = new System.Windows.Forms.Panel();
            pictureBox5 = new System.Windows.Forms.PictureBox();
            lblLatestExtraction = new System.Windows.Forms.Label();
            panel7 = new System.Windows.Forms.Panel();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            lblPeople = new System.Windows.Forms.Label();
            panel3 = new System.Windows.Forms.Panel();
            pictureBox4 = new System.Windows.Forms.PictureBox();
            lblLastDataLoad = new System.Windows.Forms.Label();
            panel5 = new System.Windows.Forms.Panel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lblRecords = new System.Windows.Forms.Label();
            panel6 = new System.Windows.Forms.Panel();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            lblDateRange = new System.Windows.Forms.Label();
            btnGenerate = new System.Windows.Forms.Button();
            cbTimeColumns = new System.Windows.Forms.ComboBox();
            panel2 = new System.Windows.Forms.Panel();
            panel8 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            toolTip2 = new System.Windows.Forms.ToolTip(components);
            viewSourceCodeToolTip1 = new SimpleDialogs.ViewSourceCodeToolTip();
            toolTip3 = new System.Windows.Forms.ToolTip(components);
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            panel2.SuspendLayout();
            panel8.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Location = new System.Drawing.Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1342, 400);
            panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(btnSettings);
            splitContainer1.Panel1.Controls.Add(lblDescription);
            splitContainer1.Panel1.Controls.Add(lblName);
            splitContainer1.Size = new System.Drawing.Size(1342, 400);
            splitContainer1.SplitterDistance = 259;
            splitContainer1.TabIndex = 0;
            // 
            // btnSettings
            // 
            btnSettings.Image = (System.Drawing.Image)resources.GetObject("btnSettings.Image");
            btnSettings.Location = new System.Drawing.Point(230, 3);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(26, 23);
            btnSettings.TabIndex = 4;
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // lblDescription
            // 
            lblDescription.Location = new System.Drawing.Point(3, 79);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(253, 285);
            lblDescription.TabIndex = 3;
            // 
            // lblName
            // 
            lblName.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblName.Location = new System.Drawing.Point(3, 0);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(221, 79);
            lblName.TabIndex = 2;
            lblName.Text = "Catalogue Name";
            lblName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel4
            // 
            panel4.BackColor = System.Drawing.SystemColors.Control;
            panel4.Controls.Add(pictureBox5);
            panel4.Controls.Add(lblLatestExtraction);
            panel4.Location = new System.Drawing.Point(512, 45);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(357, 39);
            panel4.TabIndex = 13;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = (System.Drawing.Image)resources.GetObject("pictureBox5.Image");
            pictureBox5.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox5.InitialImage");
            pictureBox5.Location = new System.Drawing.Point(3, 4);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new System.Drawing.Size(28, 28);
            pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox5.TabIndex = 12;
            pictureBox5.TabStop = false;
            toolTip2.SetToolTip(pictureBox5, "Latest Extraction");
            // 
            // lblLatestExtraction
            // 
            lblLatestExtraction.BackColor = System.Drawing.SystemColors.Control;
            lblLatestExtraction.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblLatestExtraction.Location = new System.Drawing.Point(37, 5);
            lblLatestExtraction.Name = "lblLatestExtraction";
            lblLatestExtraction.Size = new System.Drawing.Size(317, 28);
            lblLatestExtraction.TabIndex = 3;
            lblLatestExtraction.Text = "No Extractions";
            lblLatestExtraction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel7
            // 
            panel7.BackColor = System.Drawing.SystemColors.Control;
            panel7.Controls.Add(pictureBox2);
            panel7.Controls.Add(lblPeople);
            panel7.Location = new System.Drawing.Point(512, 7);
            panel7.Name = "panel7";
            panel7.Size = new System.Drawing.Size(241, 32);
            panel7.TabIndex = 7;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (System.Drawing.Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox2.InitialImage");
            pictureBox2.Location = new System.Drawing.Point(3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(28, 28);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 9;
            pictureBox2.TabStop = false;
            // 
            // lblPeople
            // 
            lblPeople.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblPeople.Location = new System.Drawing.Point(37, 4);
            lblPeople.Name = "lblPeople";
            lblPeople.Size = new System.Drawing.Size(201, 27);
            lblPeople.TabIndex = 3;
            lblPeople.Text = "- People";
            lblPeople.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            panel3.BackColor = System.Drawing.SystemColors.Control;
            panel3.Controls.Add(pictureBox4);
            panel3.Controls.Add(lblLastDataLoad);
            panel3.Location = new System.Drawing.Point(268, 45);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(241, 38);
            panel3.TabIndex = 4;
            // 
            // pictureBox4
            // 
            pictureBox4.Image = (System.Drawing.Image)resources.GetObject("pictureBox4.Image");
            pictureBox4.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox4.InitialImage");
            pictureBox4.Location = new System.Drawing.Point(3, 3);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new System.Drawing.Size(28, 28);
            pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox4.TabIndex = 11;
            pictureBox4.TabStop = false;
            toolTip1.SetToolTip(pictureBox4, "Latest Data Load");
            // 
            // lblLastDataLoad
            // 
            lblLastDataLoad.BackColor = System.Drawing.SystemColors.Control;
            lblLastDataLoad.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblLastDataLoad.Location = new System.Drawing.Point(33, 2);
            lblLastDataLoad.Name = "lblLastDataLoad";
            lblLastDataLoad.Size = new System.Drawing.Size(186, 31);
            lblLastDataLoad.TabIndex = 1;
            lblLastDataLoad.Text = "No Data Loads";
            lblLastDataLoad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel5
            // 
            panel5.BackColor = System.Drawing.SystemColors.Control;
            panel5.Controls.Add(pictureBox1);
            panel5.Controls.Add(lblRecords);
            panel5.Location = new System.Drawing.Point(268, 7);
            panel5.Name = "panel5";
            panel5.Size = new System.Drawing.Size(241, 32);
            panel5.TabIndex = 6;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox1.InitialImage");
            pictureBox1.Location = new System.Drawing.Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(28, 28);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // lblRecords
            // 
            lblRecords.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblRecords.Location = new System.Drawing.Point(37, 3);
            lblRecords.Name = "lblRecords";
            lblRecords.Size = new System.Drawing.Size(201, 28);
            lblRecords.TabIndex = 3;
            lblRecords.Text = "- Records";
            lblRecords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel6
            // 
            panel6.AutoSize = true;
            panel6.BackColor = System.Drawing.SystemColors.Control;
            panel6.Controls.Add(pictureBox3);
            panel6.Controls.Add(lblDateRange);
            panel6.Location = new System.Drawing.Point(759, 7);
            panel6.Name = "panel6";
            panel6.Size = new System.Drawing.Size(455, 34);
            panel6.TabIndex = 7;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (System.Drawing.Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox3.InitialImage");
            pictureBox3.Location = new System.Drawing.Point(3, 3);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(28, 28);
            pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox3.TabIndex = 10;
            pictureBox3.TabStop = false;
            toolTip3.SetToolTip(pictureBox3, "Date Range");
            // 
            // lblDateRange
            // 
            lblDateRange.AutoSize = true;
            lblDateRange.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblDateRange.Location = new System.Drawing.Point(37, 5);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Size = new System.Drawing.Size(122, 21);
            lblDateRange.TabIndex = 3;
            lblDateRange.Text = "No Dates Found";
            lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnGenerate
            // 
            btnGenerate.Image = (System.Drawing.Image)resources.GetObject("btnGenerate.Image");
            btnGenerate.Location = new System.Drawing.Point(205, 31);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(26, 23);
            btnGenerate.TabIndex = 5;
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // cbTimeColumns
            // 
            cbTimeColumns.FormattingEnabled = true;
            cbTimeColumns.Location = new System.Drawing.Point(3, 31);
            cbTimeColumns.Name = "cbTimeColumns";
            cbTimeColumns.Size = new System.Drawing.Size(196, 23);
            cbTimeColumns.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Controls.Add(panel8);
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(panel5);
            panel2.Location = new System.Drawing.Point(0, 424);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1342, 405);
            panel2.TabIndex = 1;
            // 
            // panel8
            // 
            panel8.Controls.Add(label1);
            panel8.Controls.Add(cbTimeColumns);
            panel8.Controls.Add(btnGenerate);
            panel8.Location = new System.Drawing.Point(3, 4);
            panel8.Name = "panel8";
            panel8.Size = new System.Drawing.Size(253, 73);
            panel8.TabIndex = 14;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 12F);
            label1.Location = new System.Drawing.Point(3, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(133, 21);
            label1.TabIndex = 6;
            label1.Text = "Rebuild Overview";
            // 
            // viewSourceCodeToolTip1
            // 
            viewSourceCodeToolTip1.OwnerDraw = true;
            // 
            // ViewCatalogueOverviewUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ViewCatalogueOverviewUI";
            Size = new System.Drawing.Size(1342, 955);
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblLastDataLoad;
        private System.Windows.Forms.Label lblLatestExtraction;
        private System.Windows.Forms.ComboBox cbTimeColumns;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label lblDateRange;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label lblRecords;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label lblPeople;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Label label1;
        private SimpleDialogs.ViewSourceCodeToolTip viewSourceCodeToolTip1;
        private System.Windows.Forms.ToolTip toolTip3;
    }
}