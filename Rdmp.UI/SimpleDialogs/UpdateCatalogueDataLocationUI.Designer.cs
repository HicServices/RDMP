namespace Rdmp.UI.SimpleDialogs
{
    partial class UpdateCatalogueDataLocationUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateCatalogueDataLocationUI));
            helpIcon1 = new SimpleControls.HelpIcon();
            tlvDatasets = new BrightIdeasSoftware.TreeListView();
            olvName = new BrightIdeasSoftware.OLVColumn();
            olvState = new BrightIdeasSoftware.OLVColumn();
            panel1 = new System.Windows.Forms.Panel();
            lblFilter = new System.Windows.Forms.Label();
            tbFilter = new System.Windows.Forms.TextBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            lbl1 = new System.Windows.Forms.Label();
            tbCurrentLocation = new System.Windows.Forms.TextBox();
            serverDatabaseTableSelector1 = new SimpleControls.ServerDatabaseTableSelector();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)tlvDatasets).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // helpIcon1
            // 
            helpIcon1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            helpIcon1.BackColor = System.Drawing.Color.Transparent;
            helpIcon1.BackgroundImage = (System.Drawing.Image)resources.GetObject("helpIcon1.BackgroundImage");
            helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            helpIcon1.Location = new System.Drawing.Point(1445, 9);
            helpIcon1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            helpIcon1.MaximumSize = new System.Drawing.Size(26, 25);
            helpIcon1.MinimumSize = new System.Drawing.Size(26, 25);
            helpIcon1.Name = "helpIcon1";
            helpIcon1.Size = new System.Drawing.Size(26, 25);
            helpIcon1.SuppressClick = false;
            helpIcon1.TabIndex = 32;
            // 
            // tlvDatasets
            // 
            tlvDatasets.AllColumns.Add(olvName);
            tlvDatasets.AllColumns.Add(olvState);
            tlvDatasets.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tlvDatasets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tlvDatasets.CellEditUseWholeCell = false;
            tlvDatasets.CheckBoxes = true;
            tlvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName, olvState });
            tlvDatasets.Location = new System.Drawing.Point(5, 3);
            tlvDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvDatasets.Name = "tlvDatasets";
            tlvDatasets.RowHeight = 19;
            tlvDatasets.ShowGroups = false;
            tlvDatasets.ShowImagesOnSubItems = true;
            tlvDatasets.Size = new System.Drawing.Size(333, 955);
            tlvDatasets.TabIndex = 28;
            tlvDatasets.UseCompatibleStateImageBehavior = false;
            tlvDatasets.View = System.Windows.Forms.View.Details;
            tlvDatasets.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.AspectName = "ToString";
            olvName.Groupable = false;
            olvName.MinimumWidth = 100;
            olvName.Text = "Name";
            olvName.Width = 118;
            // 
            // olvState
            // 
            olvState.Groupable = false;
            olvState.Text = "Location";
            olvState.Width = 160;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblFilter);
            panel1.Controls.Add(tbFilter);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 563);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(342, 34);
            panel1.TabIndex = 32;
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Location = new System.Drawing.Point(3, 8);
            lblFilter.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new System.Drawing.Size(36, 15);
            lblFilter.TabIndex = 30;
            lblFilter.Text = "Filter:";
            // 
            // tbFilter
            // 
            tbFilter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbFilter.Location = new System.Drawing.Point(39, 5);
            tbFilter.Margin = new System.Windows.Forms.Padding(0, 3, 4, 3);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new System.Drawing.Size(292, 23);
            tbFilter.TabIndex = 29;
            tbFilter.TextChanged += tbFilter_TextChanged;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel1);
            splitContainer1.Panel1.Controls.Add(tlvDatasets);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(serverDatabaseTableSelector1);
            splitContainer1.Panel2.Controls.Add(lbl1);
            splitContainer1.Panel2.Controls.Add(tbCurrentLocation);
            splitContainer1.Panel2.Controls.Add(helpIcon1);
            splitContainer1.Size = new System.Drawing.Size(1114, 601);
            splitContainer1.SplitterDistance = 346;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 30;
            // 
            // lbl1
            // 
            lbl1.AutoSize = true;
            lbl1.Location = new System.Drawing.Point(30, 33);
            lbl1.Name = "lbl1";
            lbl1.Size = new System.Drawing.Size(126, 15);
            lbl1.TabIndex = 34;
            lbl1.Text = "Current Data Location:";
            lbl1.Click += label1_Click;
            // 
            // tbCurrentLocation
            // 
            tbCurrentLocation.Enabled = false;
            tbCurrentLocation.Location = new System.Drawing.Point(159, 30);
            tbCurrentLocation.Name = "tbCurrentLocation";
            tbCurrentLocation.Size = new System.Drawing.Size(460, 23);
            tbCurrentLocation.TabIndex = 33;
            // 
            // serverDatabaseTableSelector1
            // 
            serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            serverDatabaseTableSelector1.AutoSize = true;
            serverDatabaseTableSelector1.Database = "";
            serverDatabaseTableSelector1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            serverDatabaseTableSelector1.Location = new System.Drawing.Point(21, 98);
            serverDatabaseTableSelector1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            serverDatabaseTableSelector1.Password = "";
            serverDatabaseTableSelector1.Server = "";
            serverDatabaseTableSelector1.Size = new System.Drawing.Size(727, 218);
            serverDatabaseTableSelector1.TabIndex = 35;
            serverDatabaseTableSelector1.Timeout = "";
            serverDatabaseTableSelector1.Username = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(30, 80);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 15);
            label1.TabIndex = 36;
            label1.Text = "New Data Location:";
            label1.Click += label1_Click_1;
            // 
            // UpdateCatalogueDataLocationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1114, 601);
            Controls.Add(splitContainer1);
            Name = "UpdateCatalogueDataLocationUI";
            Text = "UpdateCatalogueDataLocationUI";
            ((System.ComponentModel.ISupportInitialize)tlvDatasets).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvDatasets;
        private SimpleControls.HelpIcon helpIcon1;
        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvState;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.TextBox tbCurrentLocation;
        private System.Windows.Forms.Label label1;
        private SimpleControls.ServerDatabaseTableSelector serverDatabaseTableSelector1;
    }
}