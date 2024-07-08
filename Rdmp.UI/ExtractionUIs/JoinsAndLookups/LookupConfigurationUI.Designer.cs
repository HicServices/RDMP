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
            lblTitle = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            cbSelectLookupTable = new System.Windows.Forms.ComboBox();
            btnAddAnotherRelation = new System.Windows.Forms.Button();
            gbAddRelation = new System.Windows.Forms.GroupBox();
            gbDescription = new System.Windows.Forms.GroupBox();
            btnAddDescription = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            tbCollation = new System.Windows.Forms.TextBox();
            btnCreateLookup = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            gbSubmit = new System.Windows.Forms.GroupBox();
            gbAddRelation.SuspendLayout();
            gbDescription.SuspendLayout();
            groupBox1.SuspendLayout();
            gbSubmit.SuspendLayout();
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
            gbAddRelation.Controls.Add(btnAddAnotherRelation);
            gbAddRelation.Location = new System.Drawing.Point(15, 22);
            gbAddRelation.Name = "gbAddRelation";
            gbAddRelation.Size = new System.Drawing.Size(580, 72);
            gbAddRelation.TabIndex = 5;
            gbAddRelation.TabStop = false;
            gbAddRelation.Text = "Add Relation:";
            gbAddRelation.Enter += gbAddRelation_Enter;
            // 
            // gbDescription
            // 
            gbDescription.AutoSize = true;
            gbDescription.Controls.Add(btnAddDescription);
            gbDescription.Location = new System.Drawing.Point(15, 109);
            gbDescription.Name = "gbDescription";
            gbDescription.Size = new System.Drawing.Size(318, 74);
            gbDescription.TabIndex = 6;
            gbDescription.TabStop = false;
            gbDescription.Text = "Add Description Column(s):";
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
            btnCreateLookup.Location = new System.Drawing.Point(6, 71);
            btnCreateLookup.Name = "btnCreateLookup";
            btnCreateLookup.Size = new System.Drawing.Size(114, 23);
            btnCreateLookup.TabIndex = 9;
            btnCreateLookup.Text = "Create Lookup";
            btnCreateLookup.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(gbSubmit);
            groupBox1.Controls.Add(gbAddRelation);
            groupBox1.Controls.Add(gbDescription);
            groupBox1.Location = new System.Drawing.Point(18, 92);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(1141, 871);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            // 
            // gbSubmit
            // 
            gbSubmit.Controls.Add(btnCreateLookup);
            gbSubmit.Controls.Add(tbCollation);
            gbSubmit.Controls.Add(label1);
            gbSubmit.Location = new System.Drawing.Point(15, 203);
            gbSubmit.Name = "gbSubmit";
            gbSubmit.Size = new System.Drawing.Size(439, 100);
            gbSubmit.TabIndex = 10;
            gbSubmit.TabStop = false;
            // 
            // LookupConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(groupBox1);
            Controls.Add(cbSelectLookupTable);
            Controls.Add(label2);
            Controls.Add(lblTitle);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LookupConfigurationUI";
            Size = new System.Drawing.Size(1236, 891);
            gbAddRelation.ResumeLayout(false);
            gbDescription.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            gbSubmit.ResumeLayout(false);
            gbSubmit.PerformLayout();
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
    }
}