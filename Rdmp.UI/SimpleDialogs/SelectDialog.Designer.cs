using BrightIdeasSoftware;

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
            this.components = new System.ComponentModel.Container();
            this.olvObjects = new BrightIdeasSoftware.ObjectListView();
            this.olvID = new BrightIdeasSoftware.OLVColumn();
            this.olvSelected = new BrightIdeasSoftware.OLVColumn();
            this.olvName = new BrightIdeasSoftware.OLVColumn();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnSelectNULL = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.catalogueCollectionFilterUI1 = new Rdmp.UI.Collections.CatalogueCollectionFilterUI();
            this.panel1 = new System.Windows.Forms.Panel();
            this.taskDescriptionLabel1 = new Rdmp.UI.SimpleDialogs.TaskDescriptionLabel();
            ((System.ComponentModel.ISupportInitialize)(this.olvObjects)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvObjects
            // 
            this.olvObjects.AllColumns.Add(this.olvID);
            this.olvObjects.AllColumns.Add(this.olvSelected);
            this.olvObjects.AllColumns.Add(this.olvName);
            this.olvObjects.CellEditUseWholeCell = false;
            this.olvObjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvID,
            this.olvSelected,
            this.olvName});
            this.olvObjects.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvObjects.FullRowSelect = true;
            this.olvObjects.HideSelection = false;
            this.olvObjects.Location = new System.Drawing.Point(0, 40);
            this.olvObjects.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvObjects.Name = "olvObjects";
            this.olvObjects.ShowGroups = false;
            this.olvObjects.Size = new System.Drawing.Size(314, 372);
            this.olvObjects.TabIndex = 1;
            this.olvObjects.UseCompatibleStateImageBehavior = false;
            this.olvObjects.UseFiltering = true;
            this.olvObjects.View = System.Windows.Forms.View.Details;
            this.olvObjects.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.listBox1_CellClick);
            this.olvObjects.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.olvObjects.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyUp);
            // 
            // olvID
            // 
            this.olvID.AspectName = "";
            this.olvID.Text = "ID";
            // 
            // olvSelected
            // 
            this.olvSelected.Text = "Selected";
            // 
            // olvName
            // 
            this.olvName.AspectName = "";
            this.olvName.MinimumWidth = 100;
            this.olvName.Text = "Name";
            this.olvName.Width = 100;
            // 
            // btnSelect
            // 
            this.btnSelect.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSelect.Location = new System.Drawing.Point(0, 511);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(322, 27);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnSelectNULL
            // 
            this.btnSelectNULL.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSelectNULL.Location = new System.Drawing.Point(0, 538);
            this.btnSelectNULL.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectNULL.Name = "btnSelectNULL";
            this.btnSelectNULL.Size = new System.Drawing.Size(322, 27);
            this.btnSelectNULL.TabIndex = 3;
            this.btnSelectNULL.Text = "Select \'NULL\'";
            this.btnSelectNULL.UseVisualStyleBackColor = true;
            this.btnSelectNULL.Click += new System.EventHandler(this.btnSelectNULL_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(0, 565);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(322, 27);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(43, 421);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(275, 23);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            this.tbFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 425);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.catalogueCollectionFilterUI1);
            this.splitContainer1.Size = new System.Drawing.Size(322, 511);
            this.splitContainer1.SplitterDistance = 446;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 5;
            // 
            // catalogueCollectionFilterUI1
            // 
            this.catalogueCollectionFilterUI1.Location = new System.Drawing.Point(4, 3);
            this.catalogueCollectionFilterUI1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.catalogueCollectionFilterUI1.Name = "catalogueCollectionFilterUI1";
            this.catalogueCollectionFilterUI1.Size = new System.Drawing.Size(312, 60);
            this.catalogueCollectionFilterUI1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.olvObjects);
            this.panel1.Controls.Add(this.taskDescriptionLabel1);
            this.panel1.Location = new System.Drawing.Point(4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(314, 412);
            this.panel1.TabIndex = 4;
            // 
            // taskDescriptionLabel1
            // 
            this.taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
            this.taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            this.taskDescriptionLabel1.Size = new System.Drawing.Size(314, 40);
            this.taskDescriptionLabel1.TabIndex = 0;
            // 
            // SelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 592);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnSelectNULL);
            this.Controls.Add(this.btnCancel);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SelectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Object(s)";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SelectIMapsDirectlyToDatabaseTableDialog_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.olvObjects)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnSelectNULL;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private OLVColumn olvName;
        private OLVColumn olvID;
        private ObjectListView olvObjects;
        private OLVColumn olvSelected;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Collections.CatalogueCollectionFilterUI catalogueCollectionFilterUI1;
        private System.Windows.Forms.Panel panel1;
        private TaskDescriptionLabel taskDescriptionLabel1;
    }
}