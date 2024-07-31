using BrightIdeasSoftware;
using System.Text.RegularExpressions;

namespace Rdmp.UI.SimpleDialogs
{
    partial class NewfindUI
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
            panel1 = new System.Windows.Forms.Panel();
            rbLocations = new System.Windows.Forms.RadioButton();
            rbSqlMode = new System.Windows.Forms.RadioButton();
            rbStandard = new System.Windows.Forms.RadioButton();
            cbCaseSensitive = new System.Windows.Forms.CheckBox();
            btnReplace = new System.Windows.Forms.Button();
            newFindToolStrip = new System.Windows.Forms.ToolStrip();
            cbRegex = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            tbReplace = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbFind = new System.Windows.Forms.TextBox();
            panel2 = new System.Windows.Forms.Panel();
            folv = new ObjectListView();
            olvID = new OLVColumn();
            olvName = new OLVColumn();
            olvHierarchy = new OLVColumn();
            olvObject = new OLVColumn();
            olvProperty = new OLVColumn();
            olvValue = new OLVColumn();
            panel3 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)folv).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.Controls.Add(rbLocations);
            panel1.Controls.Add(rbSqlMode);
            panel1.Controls.Add(rbStandard);
            panel1.Controls.Add(cbCaseSensitive);
            panel1.Controls.Add(btnReplace);
            panel1.Controls.Add(newFindToolStrip);
            panel1.Controls.Add(cbRegex);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(tbReplace);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(tbFind);
            panel1.Location = new System.Drawing.Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(776, 100);
            panel1.TabIndex = 0;
            // 
            // rbLocations
            // 
            rbLocations.AutoCheck = false;
            rbLocations.AutoSize = true;
            rbLocations.Location = new System.Drawing.Point(169, 28);
            rbLocations.Name = "rbLocations";
            rbLocations.Size = new System.Drawing.Size(122, 19);
            rbLocations.TabIndex = 10;
            rbLocations.Text = "Physical Locations";
            rbLocations.UseVisualStyleBackColor = true;
            rbLocations.Click += cbLocationMode_checkChanged;
            // 
            // rbSqlMode
            // 
            rbSqlMode.AutoCheck = false;
            rbSqlMode.AutoSize = true;
            rbSqlMode.Location = new System.Drawing.Point(88, 28);
            rbSqlMode.Name = "rbSqlMode";
            rbSqlMode.Size = new System.Drawing.Size(75, 19);
            rbSqlMode.TabIndex = 9;
            rbSqlMode.Text = "Sql Mode";
            rbSqlMode.UseVisualStyleBackColor = true;
            rbSqlMode.Click += cbSqlMode_checkChanged;
            // 
            // rbStandard
            // 
            rbStandard.AutoCheck = false;
            rbStandard.AutoSize = true;
            rbStandard.Checked = true;
            rbStandard.Location = new System.Drawing.Point(3, 28);
            rbStandard.Name = "rbStandard";
            rbStandard.Size = new System.Drawing.Size(72, 19);
            rbStandard.TabIndex = 8;
            rbStandard.TabStop = true;
            rbStandard.Text = "Standard";
            rbStandard.UseVisualStyleBackColor = true;
            rbStandard.Click += cbStandard_checkChanged;
            // 
            // cbCaseSensitive
            // 
            cbCaseSensitive.AutoCheck = false;
            cbCaseSensitive.AutoSize = true;
            cbCaseSensitive.Location = new System.Drawing.Point(592, 49);
            cbCaseSensitive.Name = "cbCaseSensitive";
            cbCaseSensitive.Size = new System.Drawing.Size(100, 19);
            cbCaseSensitive.TabIndex = 7;
            cbCaseSensitive.Text = "Case Sensitive";
            cbCaseSensitive.UseVisualStyleBackColor = true;
            cbCaseSensitive.Click += cbCaseSensitive_CheckedChanged;
            // 
            // btnReplace
            // 
            btnReplace.Location = new System.Drawing.Point(502, 77);
            btnReplace.Name = "btnReplace";
            btnReplace.Size = new System.Drawing.Size(75, 23);
            btnReplace.TabIndex = 6;
            btnReplace.Text = "Replace";
            btnReplace.UseVisualStyleBackColor = true;
            btnReplace.Click += btnReplace_Click;
            // 
            // newFindToolStrip
            // 
            newFindToolStrip.Location = new System.Drawing.Point(0, 0);
            newFindToolStrip.Name = "newFindToolStrip";
            newFindToolStrip.Size = new System.Drawing.Size(776, 25);
            newFindToolStrip.TabIndex = 5;
            newFindToolStrip.Text = "toolStrip1";
            // 
            // cbRegex
            // 
            cbRegex.AutoCheck = false;
            cbRegex.AutoSize = true;
            cbRegex.Location = new System.Drawing.Point(490, 49);
            cbRegex.Name = "cbRegex";
            cbRegex.Size = new System.Drawing.Size(96, 19);
            cbRegex.TabIndex = 4;
            cbRegex.Text = "Regex Search";
            cbRegex.UseVisualStyleBackColor = true;
            cbRegex.Click += cbRegex_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 80);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(79, 15);
            label2.TabIndex = 3;
            label2.Text = "Replace With:";
            // 
            // tbReplace
            // 
            tbReplace.Location = new System.Drawing.Point(88, 76);
            tbReplace.Name = "tbReplace";
            tbReplace.Size = new System.Drawing.Size(387, 23);
            tbReplace.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(49, 50);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(33, 15);
            label1.TabIndex = 1;
            label1.Text = "Find:";
            // 
            // tbFind
            // 
            tbFind.Location = new System.Drawing.Point(88, 47);
            tbFind.Name = "tbFind";
            tbFind.Size = new System.Drawing.Size(387, 23);
            tbFind.TabIndex = 0;
            tbFind.TextChanged += tbFilter_TextChanged;
            // 
            // panel2
            // 
            panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel2.BackColor = System.Drawing.SystemColors.Control;
            panel2.Controls.Add(folv);
            panel2.Location = new System.Drawing.Point(0, 129);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(800, 321);
            panel2.TabIndex = 1;
            // 
            // folv
            // 
            folv.AllColumns.Add(olvID);
            folv.AllColumns.Add(olvName);
            folv.AllColumns.Add(olvHierarchy);
            folv.AllColumns.Add(olvObject);
            folv.AllColumns.Add(olvProperty);
            folv.AllColumns.Add(olvValue);
            folv.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            folv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvID, olvName, olvHierarchy, olvObject, olvProperty, olvValue });
            folv.Location = new System.Drawing.Point(0, 0);
            folv.Name = "folv";
            folv.ShowGroups = false;
            folv.Size = new System.Drawing.Size(800, 321);
            folv.TabIndex = 0;
            folv.View = System.Windows.Forms.View.Details;
            folv.CellClick += folv_CellClick;
            // 
            // olvID
            // 
            olvID.AspectName = "";
            olvID.Text = "ID";
            // 
            // olvName
            // 
            olvName.AspectName = "";
            olvName.FillsFreeSpace = true;
            olvName.MinimumWidth = 100;
            olvName.Text = "Name";
            olvName.Width = 100;
            // 
            // olvHierarchy
            // 
            olvHierarchy.AspectName = "";
            olvHierarchy.FillsFreeSpace = true;
            olvHierarchy.MinimumWidth = 100;
            olvHierarchy.Text = "Hierarchy";
            olvHierarchy.Width = 100;
            // 
            // olvObject
            // 
            olvObject.AspectName = "ToString";
            olvObject.FillsFreeSpace = true;
            olvObject.IsVisible = false;
            olvObject.MinimumWidth = 100;
            olvObject.Text = "Object";
            olvObject.Width = 100;
            // 
            // olvProperty
            // 
            olvProperty.AspectName = "";
            olvProperty.FillsFreeSpace = true;
            olvProperty.IsVisible = false;
            olvProperty.MinimumWidth = 100;
            olvProperty.Text = "Property";
            olvProperty.Width = 100;
            // 
            // olvValue
            // 
            olvValue.AspectName = "";
            olvValue.FillsFreeSpace = true;
            olvValue.IsVisible = false;
            olvValue.MinimumWidth = 100;
            olvValue.Text = "Value";
            olvValue.Width = 100;
            // 
            // panel3
            // 
            panel3.Controls.Add(panel1);
            panel3.Controls.Add(panel2);
            panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            panel3.Location = new System.Drawing.Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(800, 450);
            panel3.TabIndex = 2;
            // 
            // NewfindUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(panel3);
            Name = "NewfindUI";
            Text = "NewfindUI";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)folv).EndInit();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbReplace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFind;
        private System.Windows.Forms.CheckBox cbRegex;
        private System.Windows.Forms.Panel panel2;
        private BrightIdeasSoftware.ObjectListView folv;
        private OLVColumn olvName;
        private OLVColumn olvHierarchy;
        private OLVColumn olvID;
        private OLVColumn olvObject;
        private OLVColumn olvProperty;
        private OLVColumn olvValue;
        private System.Windows.Forms.ToolStrip newFindToolStrip;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.CheckBox cbCaseSensitive;
        private System.Windows.Forms.RadioButton rbLocations;
        private System.Windows.Forms.RadioButton rbSqlMode;
        private System.Windows.Forms.RadioButton rbStandard;
        private System.Windows.Forms.Panel panel3;
    }
}