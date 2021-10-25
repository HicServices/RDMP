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
            this.olvID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSelected = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnSelectNULL = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.catalogueCollectionFilterUI1 = new Rdmp.UI.Collections.CatalogueCollectionFilterUI();
            ((System.ComponentModel.ISupportInitialize)(this.olvObjects)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvObjects
            // 
            this.olvObjects.AllColumns.Add(this.olvID);
            this.olvObjects.AllColumns.Add(this.olvSelected);
            this.olvObjects.AllColumns.Add(this.olvName);
            this.olvObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvObjects.CellEditUseWholeCell = false;
            this.olvObjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvID,
            this.olvSelected,
            this.olvName});
            this.olvObjects.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvObjects.FullRowSelect = true;
            this.olvObjects.HideSelection = false;
            this.olvObjects.Location = new System.Drawing.Point(0, 3);
            this.olvObjects.Name = "olvObjects";
            this.olvObjects.ShowGroups = false;
            this.olvObjects.Size = new System.Drawing.Size(276, 351);
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
            this.btnSelect.Location = new System.Drawing.Point(0, 444);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(276, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnSelectNULL
            // 
            this.btnSelectNULL.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnSelectNULL.Location = new System.Drawing.Point(0, 467);
            this.btnSelectNULL.Name = "btnSelectNULL";
            this.btnSelectNULL.Size = new System.Drawing.Size(276, 23);
            this.btnSelectNULL.TabIndex = 3;
            this.btnSelectNULL.Text = "Select \'NULL\'";
            this.btnSelectNULL.UseVisualStyleBackColor = true;
            this.btnSelectNULL.Click += new System.EventHandler(this.btnSelectNULL_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(0, 490);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(276, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(37, 358);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(236, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            this.tbFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 361);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
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
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tbFilter);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.olvObjects);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.catalogueCollectionFilterUI1);
            this.splitContainer1.Size = new System.Drawing.Size(276, 444);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.TabIndex = 5;
            // 
            // catalogueCollectionFilterUI1
            // 
            this.catalogueCollectionFilterUI1.Location = new System.Drawing.Point(3, 3);
            this.catalogueCollectionFilterUI1.Name = "catalogueCollectionFilterUI1";
            this.catalogueCollectionFilterUI1.Size = new System.Drawing.Size(267, 52);
            this.catalogueCollectionFilterUI1.TabIndex = 0;
            // 
            // SelectIMapsDirectlyToDatabaseTableDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 513);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnSelectNULL);
            this.Controls.Add(this.btnCancel);
            this.Name = "SelectIMapsDirectlyToDatabaseTableDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Object(s)";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SelectIMapsDirectlyToDatabaseTableDialog_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.olvObjects)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
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
    }
}