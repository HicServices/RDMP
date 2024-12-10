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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewCatalogueOverviewUI));
            panel1 = new System.Windows.Forms.Panel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            btnGenerate = new System.Windows.Forms.Button();
            btnSettings = new System.Windows.Forms.Button();
            lblDescription = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            cbTimeColumns = new System.Windows.Forms.ComboBox();
            panel3 = new System.Windows.Forms.Panel();
            lblLatestExtraction = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            lblLastDataLoad = new System.Windows.Forms.Label();
            panel2 = new System.Windows.Forms.Panel();
            panel7 = new System.Windows.Forms.Panel();
            label6 = new System.Windows.Forms.Label();
            lblPeople = new System.Windows.Forms.Label();
            panel6 = new System.Windows.Forms.Panel();
            label4 = new System.Windows.Forms.Label();
            lblDateRange = new System.Windows.Forms.Label();
            panel5 = new System.Windows.Forms.Panel();
            label3 = new System.Windows.Forms.Label();
            lblRecords = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel7.SuspendLayout();
            panel6.SuspendLayout();
            panel5.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(btnGenerate);
            splitContainer1.Panel1.Controls.Add(btnSettings);
            splitContainer1.Panel1.Controls.Add(lblDescription);
            splitContainer1.Panel1.Controls.Add(lblName);
            splitContainer1.Panel1.Controls.Add(cbTimeColumns);
            splitContainer1.Size = new System.Drawing.Size(1342, 400);
            splitContainer1.SplitterDistance = 259;
            splitContainer1.TabIndex = 0;
            // 
            // btnGenerate
            // 
            btnGenerate.Image = (System.Drawing.Image)resources.GetObject("btnGenerate.Image");
            btnGenerate.Location = new System.Drawing.Point(198, 3);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(26, 23);
            btnGenerate.TabIndex = 5;
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
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
            lblDescription.Location = new System.Drawing.Point(16, 108);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(240, 285);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "label1";
            // 
            // lblName
            // 
            lblName.Font = new System.Drawing.Font("Segoe UI", 16F);
            lblName.Location = new System.Drawing.Point(16, 29);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(240, 79);
            lblName.TabIndex = 2;
            lblName.Text = "lblName";
            // 
            // cbTimeColumns
            // 
            cbTimeColumns.FormattingEnabled = true;
            cbTimeColumns.Location = new System.Drawing.Point(0, 3);
            cbTimeColumns.Name = "cbTimeColumns";
            cbTimeColumns.Size = new System.Drawing.Size(196, 23);
            cbTimeColumns.TabIndex = 4;
            cbTimeColumns.SelectedIndexChanged += cbTimeColumns_SelectedIndexChanged;
            // 
            // panel3
            // 
            panel3.BackColor = System.Drawing.SystemColors.ControlDark;
            panel3.Controls.Add(lblLatestExtraction);
            panel3.Controls.Add(label2);
            panel3.Controls.Add(label1);
            panel3.Controls.Add(lblLastDataLoad);
            panel3.Location = new System.Drawing.Point(16, 17);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(190, 148);
            panel3.TabIndex = 4;
            // 
            // lblLatestExtraction
            // 
            lblLatestExtraction.BackColor = System.Drawing.SystemColors.ControlDark;
            lblLatestExtraction.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblLatestExtraction.Location = new System.Drawing.Point(13, 116);
            lblLatestExtraction.Name = "lblLatestExtraction";
            lblLatestExtraction.Size = new System.Drawing.Size(166, 23);
            lblLatestExtraction.TabIndex = 3;
            lblLatestExtraction.Text = "label2";
            lblLatestExtraction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 16F);
            label2.Location = new System.Drawing.Point(13, 77);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(169, 30);
            label2.TabIndex = 2;
            label2.Text = "Latest Extraction";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 16F);
            label1.Location = new System.Drawing.Point(3, 1);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(181, 30);
            label1.TabIndex = 0;
            label1.Text = "Lastest Data Load";
            // 
            // lblLastDataLoad
            // 
            lblLastDataLoad.BackColor = System.Drawing.SystemColors.ControlDark;
            lblLastDataLoad.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblLastDataLoad.Location = new System.Drawing.Point(13, 42);
            lblLastDataLoad.Name = "lblLastDataLoad";
            lblLastDataLoad.Size = new System.Drawing.Size(166, 25);
            lblLastDataLoad.TabIndex = 1;
            lblLastDataLoad.Text = "label2";
            lblLastDataLoad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(panel7);
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(panel5);
            panel2.Location = new System.Drawing.Point(0, 424);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1342, 246);
            panel2.TabIndex = 1;
            // 
            // panel7
            // 
            panel7.BackColor = System.Drawing.SystemColors.ControlDark;
            panel7.Controls.Add(label6);
            panel7.Controls.Add(lblPeople);
            panel7.Location = new System.Drawing.Point(705, 17);
            panel7.Name = "panel7";
            panel7.Size = new System.Drawing.Size(182, 108);
            panel7.TabIndex = 7;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Segoe UI", 16F);
            label6.Location = new System.Drawing.Point(34, 4);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(98, 30);
            label6.TabIndex = 2;
            label6.Text = "# People";
            // 
            // lblPeople
            // 
            lblPeople.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblPeople.Location = new System.Drawing.Point(3, 63);
            lblPeople.Name = "lblPeople";
            lblPeople.Size = new System.Drawing.Size(176, 23);
            lblPeople.TabIndex = 3;
            lblPeople.Text = "label2";
            lblPeople.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            panel6.AutoSize = true;
            panel6.BackColor = System.Drawing.SystemColors.ControlDark;
            panel6.Controls.Add(label4);
            panel6.Controls.Add(lblDateRange);
            panel6.Location = new System.Drawing.Point(1069, 17);
            panel6.Name = "panel6";
            panel6.Size = new System.Drawing.Size(202, 108);
            panel6.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = System.Windows.Forms.DockStyle.Fill;
            label4.Font = new System.Drawing.Font("Segoe UI", 16F);
            label4.Location = new System.Drawing.Point(0, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(125, 30);
            label4.TabIndex = 2;
            label4.Text = "Date Range";
            label4.Click += label4_Click;
            // 
            // lblDateRange
            // 
            lblDateRange.AutoSize = true;
            lblDateRange.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblDateRange.Location = new System.Drawing.Point(16, 63);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Size = new System.Drawing.Size(52, 21);
            lblDateRange.TabIndex = 3;
            lblDateRange.Text = "label2";
            lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel5
            // 
            panel5.BackColor = System.Drawing.SystemColors.ControlDark;
            panel5.Controls.Add(label3);
            panel5.Controls.Add(lblRecords);
            panel5.Location = new System.Drawing.Point(310, 17);
            panel5.Name = "panel5";
            panel5.Size = new System.Drawing.Size(182, 108);
            panel5.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 16F);
            label3.Location = new System.Drawing.Point(34, 4);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(109, 30);
            label3.TabIndex = 2;
            label3.Text = "# Records";
            // 
            // lblRecords
            // 
            lblRecords.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblRecords.Location = new System.Drawing.Point(3, 63);
            lblRecords.Name = "lblRecords";
            lblRecords.Size = new System.Drawing.Size(176, 23);
            lblRecords.TabIndex = 3;
            lblRecords.Text = "label2";
            lblRecords.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblLastDataLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblLatestExtraction;
        private System.Windows.Forms.ComboBox cbTimeColumns;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblDateRange;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblRecords;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblPeople;
        private System.Windows.Forms.Button btnGenerate;
    }
}