using BrightIdeasSoftware;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;
using System;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups
{
    partial class LookupConfigurationUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LookupConfigurationUI));
            lblTitle = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            cbSelectLookupTable = new System.Windows.Forms.ComboBox();
            btnAddAnotherRelation = new System.Windows.Forms.Button();
            gbAddRelation = new System.Windows.Forms.GroupBox();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            gbDescription = new System.Windows.Forms.GroupBox();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            btnAddDescription = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            tbCollation = new System.Windows.Forms.TextBox();
            btnCreateLookup = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            gbSubmit = new System.Windows.Forms.GroupBox();
            pictureBox5 = new System.Windows.Forms.PictureBox();
            pictureBox4 = new System.Windows.Forms.PictureBox();
            lblErrorText = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            gbAddRelation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            gbDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            groupBox1.SuspendLayout();
            gbSubmit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F);
            lblTitle.Location = new System.Drawing.Point(3, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(385, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Create Lookup For SOME_CATALOGUE";
            lblTitle.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(18, 66);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(114, 15);
            label2.TabIndex = 1;
            label2.Text = "Select Lookup Table:";
            label2.Click += label2_Click;
            // 
            // cbSelectLookupTable
            // 
            cbSelectLookupTable.FormattingEnabled = true;
            cbSelectLookupTable.Location = new System.Drawing.Point(138, 63);
            cbSelectLookupTable.Name = "cbSelectLookupTable";
            cbSelectLookupTable.Size = new System.Drawing.Size(281, 23);
            cbSelectLookupTable.TabIndex = 2;
            cbSelectLookupTable.SelectedIndexChanged += cbSelectLookupTable_SelectedIndexchanged;
            // 
            // btnAddAnotherRelation
            // 
            btnAddAnotherRelation.Enabled = false;
            btnAddAnotherRelation.Location = new System.Drawing.Point(0, 22);
            btnAddAnotherRelation.Name = "btnAddAnotherRelation";
            btnAddAnotherRelation.Size = new System.Drawing.Size(153, 23);
            btnAddAnotherRelation.TabIndex = 4;
            btnAddAnotherRelation.Text = "Add Another";
            btnAddAnotherRelation.UseVisualStyleBackColor = true;
            btnAddAnotherRelation.Click += btnAddAnotherRelation_Click;
            // 
            // gbAddRelation
            // 
            gbAddRelation.AutoSize = true;
            gbAddRelation.Controls.Add(pictureBox2);
            gbAddRelation.Controls.Add(btnAddAnotherRelation);
            gbAddRelation.Location = new System.Drawing.Point(15, 22);
            gbAddRelation.Name = "gbAddRelation";
            gbAddRelation.Size = new System.Drawing.Size(580, 72);
            gbAddRelation.TabIndex = 5;
            gbAddRelation.TabStop = false;
            gbAddRelation.Text = "Add Relation:";
            gbAddRelation.Enter += gbAddRelation_Enter;
            // 
            // pictureBox2
            // 
            pictureBox2.ErrorImage = (System.Drawing.Image)resources.GetObject("pictureBox2.ErrorImage");
            pictureBox2.Image = (System.Drawing.Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox2.InitialImage");
            pictureBox2.Location = new System.Drawing.Point(159, 22);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(21, 21);
            pictureBox2.TabIndex = 12;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // gbDescription
            // 
            gbDescription.AutoSize = true;
            gbDescription.Controls.Add(pictureBox3);
            gbDescription.Controls.Add(btnAddDescription);
            gbDescription.Location = new System.Drawing.Point(15, 109);
            gbDescription.Name = "gbDescription";
            gbDescription.Size = new System.Drawing.Size(318, 74);
            gbDescription.TabIndex = 6;
            gbDescription.TabStop = false;
            gbDescription.Text = "Add Description Column(s):";
            // 
            // pictureBox3
            // 
            pictureBox3.ErrorImage = (System.Drawing.Image)resources.GetObject("pictureBox3.ErrorImage");
            pictureBox3.Image = (System.Drawing.Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox3.InitialImage");
            pictureBox3.Location = new System.Drawing.Point(159, 24);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(21, 21);
            pictureBox3.TabIndex = 13;
            pictureBox3.TabStop = false;
            pictureBox3.Click += pictureBox3_Click;
            // 
            // btnAddDescription
            // 
            btnAddDescription.Enabled = false;
            btnAddDescription.Location = new System.Drawing.Point(0, 22);
            btnAddDescription.Name = "btnAddDescription";
            btnAddDescription.Size = new System.Drawing.Size(153, 23);
            btnAddDescription.TabIndex = 4;
            btnAddDescription.Text = "Add Another";
            btnAddDescription.UseVisualStyleBackColor = true;
            btnAddDescription.Click += btnAddDescription_Click;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 19);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 15);
            label1.TabIndex = 7;
            label1.Text = "Collation:";
            // 
            // tbCollation
            // 
            tbCollation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tbCollation.Location = new System.Drawing.Point(64, 16);
            tbCollation.Name = "tbCollation";
            tbCollation.Size = new System.Drawing.Size(254, 23);
            tbCollation.TabIndex = 8;
            // 
            // btnCreateLookup
            // 
            btnCreateLookup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnCreateLookup.Enabled = false;
            btnCreateLookup.Location = new System.Drawing.Point(6, 45);
            btnCreateLookup.Name = "btnCreateLookup";
            btnCreateLookup.Size = new System.Drawing.Size(114, 23);
            btnCreateLookup.TabIndex = 9;
            btnCreateLookup.Text = "Create Lookup";
            btnCreateLookup.UseVisualStyleBackColor = true;
            btnCreateLookup.Click += btnCreateLookup_Click;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(gbSubmit);
            groupBox1.Controls.Add(gbAddRelation);
            groupBox1.Controls.Add(gbDescription);
            groupBox1.Location = new System.Drawing.Point(18, 92);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(1141, 471);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            // 
            // gbSubmit
            // 
            gbSubmit.Controls.Add(pictureBox5);
            gbSubmit.Controls.Add(pictureBox4);
            gbSubmit.Controls.Add(lblErrorText);
            gbSubmit.Controls.Add(btnCreateLookup);
            gbSubmit.Controls.Add(tbCollation);
            gbSubmit.Controls.Add(label1);
            gbSubmit.Location = new System.Drawing.Point(15, 203);
            gbSubmit.Name = "gbSubmit";
            gbSubmit.Size = new System.Drawing.Size(1081, 123);
            gbSubmit.TabIndex = 10;
            gbSubmit.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.ErrorImage = (System.Drawing.Image)resources.GetObject("pictureBox5.ErrorImage");
            pictureBox5.Image = (System.Drawing.Image)resources.GetObject("pictureBox5.Image");
            pictureBox5.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox5.InitialImage");
            pictureBox5.Location = new System.Drawing.Point(132, 45);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new System.Drawing.Size(21, 21);
            pictureBox5.TabIndex = 15;
            pictureBox5.TabStop = false;
            pictureBox5.Click += pictureBox5_Click;
            // 
            // pictureBox4
            // 
            pictureBox4.ErrorImage = (System.Drawing.Image)resources.GetObject("pictureBox4.ErrorImage");
            pictureBox4.Image = (System.Drawing.Image)resources.GetObject("pictureBox4.Image");
            pictureBox4.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox4.InitialImage");
            pictureBox4.Location = new System.Drawing.Point(324, 18);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new System.Drawing.Size(21, 21);
            pictureBox4.TabIndex = 14;
            pictureBox4.TabStop = false;
            pictureBox4.Click += pictureBox4_Click;
            // 
            // lblErrorText
            // 
            lblErrorText.AutoSize = true;
            lblErrorText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblErrorText.ForeColor = System.Drawing.Color.Red;
            lblErrorText.Location = new System.Drawing.Point(7, 90);
            lblErrorText.Name = "lblErrorText";
            lblErrorText.Size = new System.Drawing.Size(57, 21);
            lblErrorText.TabIndex = 11;
            lblErrorText.Text = "label3";
            lblErrorText.Visible = false;
            // 
            // pictureBox1
            // 
            pictureBox1.ErrorImage = (System.Drawing.Image)resources.GetObject("pictureBox1.ErrorImage");
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = (System.Drawing.Image)resources.GetObject("pictureBox1.InitialImage");
            pictureBox1.Location = new System.Drawing.Point(425, 60);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(21, 21);
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // LookupConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(pictureBox1);
            Controls.Add(groupBox1);
            Controls.Add(cbSelectLookupTable);
            Controls.Add(label2);
            Controls.Add(lblTitle);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LookupConfigurationUI";
            Size = new System.Drawing.Size(1083, 789);
            gbAddRelation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            gbDescription.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            gbSubmit.ResumeLayout(false);
            gbSubmit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbSelectLookupTable;
        private System.Windows.Forms.Button btnAddAnotherRelation;
        private System.Windows.Forms.GroupBox gbAddRelation;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.Button btnAddDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbCollation;
        private System.Windows.Forms.Button btnCreateLookup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbSubmit;
        private System.Windows.Forms.Label lblErrorText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox4;
    }
}