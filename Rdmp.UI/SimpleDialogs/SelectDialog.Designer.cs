using BrightIdeasSoftware;
using System;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{

    partial class SelectDialog<T>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDialog<>));
            this.olv = new BrightIdeasSoftware.FastDataListView();
            this.olvSelected = new BrightIdeasSoftware.OLVColumn();
            this.olvID = new BrightIdeasSoftware.OLVColumn();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.olvHierarchy = new BrightIdeasSoftware.OLVColumn();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnSelectNULL = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            this.pFilter = new System.Windows.Forms.Panel();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.olv)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.pFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olv
            // 
            this.olv.AllColumns.Add(this.olvSelected);
            this.olv.AllColumns.Add(this.olvID);
            this.olv.AllColumns.Add(this.olvName);
            this.olv.AllColumns.Add(this.olvHierarchy);
            this.olv.CellEditUseWholeCell = false;
            this.olv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvSelected,
            this.olvID,
            this.olvName,
            this.olvHierarchy});
            this.olv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olv.FullRowSelect = true;
            this.olv.Location = new System.Drawing.Point(0, 0);
            this.olv.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olv.Name = "olv";
            this.olv.ShowGroups = false;
            this.olv.Size = new System.Drawing.Size(587, 270);
            this.olv.TabIndex = 5;
            this.olv.UseCompatibleStateImageBehavior = false;
            this.olv.View = System.Windows.Forms.View.Details;
            this.olv.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.listBox1_CellClick);
            this.olv.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.olv.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyUp);
            // 
            // olvSelected
            // 
            this.olvSelected.Text = "Selected";
            this.olvSelected.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // olvID
            // 
            this.olvID.AspectName = "";
            this.olvID.Text = "ID";
            // 
            // olvName
            // 
            this.olvName.AspectName = "";
            this.olvName.MinimumWidth = 100;
            this.olvName.Text = "Name";
            this.olvName.Width = 100;
            // 
            // olvHierarchy
            // 
            this.olvHierarchy.AspectName = "";
            this.olvHierarchy.MinimumWidth = 100;
            this.olvHierarchy.Text = "Hierarchy";
            this.olvHierarchy.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(587, 25);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel1.Text = "Filter:";
            // 
            // btnSelect
            // 
            this.btnSelect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSelect.Location = new System.Drawing.Point(0, 362);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(587, 27);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnSelectNULL
            // 
            this.btnSelectNULL.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSelectNULL.Location = new System.Drawing.Point(0, 389);
            this.btnSelectNULL.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectNULL.Name = "btnSelectNULL";
            this.btnSelectNULL.Size = new System.Drawing.Size(587, 27);
            this.btnSelectNULL.TabIndex = 3;
            this.btnSelectNULL.Text = "Select \'NULL\'";
            this.btnSelectNULL.UseVisualStyleBackColor = true;
            this.btnSelectNULL.Click += new System.EventHandler(this.btnSelectNULL_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(0, 416);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(587, 27);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // taskDescriptionLabel1
            // 
            this.taskDescriptionLabel1.AutoSize = true;
            this.taskDescriptionLabel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.taskDescriptionLabel1.Location = new System.Drawing.Point(0, 25);
            this.taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            this.taskDescriptionLabel1.Size = new System.Drawing.Size(587, 42);
            this.taskDescriptionLabel1.TabIndex = 8;
            // 
            // pFilter
            // 
            this.pFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pFilter.Controls.Add(this.tbFilter);
            this.pFilter.Controls.Add(this.label1);
            this.pFilter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pFilter.Location = new System.Drawing.Point(0, 337);
            this.pFilter.Name = "pFilter";
            this.pFilter.Size = new System.Drawing.Size(587, 25);
            this.pFilter.TabIndex = 9;
            // 
            // tbFilter
            // 
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFilter.Location = new System.Drawing.Point(36, 0);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(551, 23);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            this.tbFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyUp);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbLoading.Location = new System.Drawing.Point(219, 85);
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(125, 125);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLoading.TabIndex = 4;
            this.pbLoading.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pbLoading);
            this.panel1.Controls.Add(this.olv);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 67);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(587, 270);
            this.panel1.TabIndex = 10;
            // 
            // SelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 443);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pFilter);
            this.Controls.Add(this.taskDescriptionLabel1);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnSelectNULL);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.toolStrip1);
            this.Name = "SelectDialog";
            this.Text = "SelectDialog2";
            ((System.ComponentModel.ISupportInitialize)(this.olv)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.pFilter.ResumeLayout(false);
            this.pFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.FastDataListView olv;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnSelectNULL;
        private System.Windows.Forms.Button btnCancel;
        private TaskDescriptionLabel taskDescriptionLabel1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private OLVColumn olvName;
        private OLVColumn olvHierarchy;
        private OLVColumn olvID;
        private OLVColumn olvSelected;
        private Panel pFilter;
        private PictureBox pbLoading;
        private Panel panel1;
    }
}