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
            label1 = new System.Windows.Forms.Label();
            cbSelectDescription = new System.Windows.Forms.ComboBox();
            gbAddRelation.SuspendLayout();
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
            cbSelectLookupTable.Enabled = false;
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
            gbAddRelation.Location = new System.Drawing.Point(18, 139);
            gbAddRelation.Name = "gbAddRelation";
            gbAddRelation.Size = new System.Drawing.Size(300, 74);
            gbAddRelation.TabIndex = 5;
            gbAddRelation.TabStop = false;
            gbAddRelation.Text = "Add Relation:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(18, 251);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(172, 15);
            label1.TabIndex = 6;
            label1.Text = "Select The Description Column:";
            // 
            // cbSelectDescription
            // 
            cbSelectDescription.Enabled = false;
            cbSelectDescription.FormattingEnabled = true;
            cbSelectDescription.Location = new System.Drawing.Point(196, 248);
            cbSelectDescription.Name = "cbSelectDescription";
            cbSelectDescription.Size = new System.Drawing.Size(281, 23);
            cbSelectDescription.TabIndex = 7;
            // 
            // LookupConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(cbSelectDescription);
            Controls.Add(label1);
            Controls.Add(gbAddRelation);
            Controls.Add(cbSelectLookupTable);
            Controls.Add(label2);
            Controls.Add(lblTitle);
            Enabled = false;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LookupConfigurationUI";
            Size = new System.Drawing.Size(1287, 891);
            gbAddRelation.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbSelectLookupTable;
        private System.Windows.Forms.Button btnAddAnotherRelation;
        private System.Windows.Forms.GroupBox gbAddRelation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbSelectDescription;
    }
}