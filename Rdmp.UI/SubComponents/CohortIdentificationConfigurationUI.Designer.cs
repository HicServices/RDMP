using BrightIdeasSoftware;
using Rdmp.UI.LocationsMenu.Ticketing;
using Rdmp.UI.LocationsMenu.Versioning;

namespace Rdmp.UI.SubComponents
{
    partial class CohortIdentificationConfigurationUI
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
                timer.Dispose();
                if (_commonFunctionality != null && _commonFunctionality.IsSetup)
                    _commonFunctionality.TearDown();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortIdentificationConfigurationUI));
            tlvCic = new TreeListView();
            olvNameCol = new OLVColumn();
            olvExecute = new OLVColumn();
            olvOrder = new OLVColumn();
            olvCached = new OLVColumn();
            olvCount = new OLVColumn();
            olvCumulativeTotal = new OLVColumn();
            olvWorking = new OLVColumn();
            olvTime = new OLVColumn();
            olvCatalogue = new OLVColumn();
            timer1 = new System.Windows.Forms.Timer(components);
            ticket = new TicketingControlUI();
            version = new VersioningControlUI();
            btnAbortLoad = new System.Windows.Forms.Button();
            btnExecute = new System.Windows.Forms.Button();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            gbCicInfo = new System.Windows.Forms.GroupBox();
            tbDescription = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            panel1 = new System.Windows.Forms.Panel();
            btnClearCache = new System.Windows.Forms.Button();
            lblExecuteAllPhase = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)tlvCic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            gbCicInfo.SuspendLayout();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tlvCic
            // 
            tlvCic.AllColumns.Add(olvNameCol);
            tlvCic.AllColumns.Add(olvExecute);
            tlvCic.AllColumns.Add(olvOrder);
            tlvCic.AllColumns.Add(olvCached);
            tlvCic.AllColumns.Add(olvCount);
            tlvCic.AllColumns.Add(olvCumulativeTotal);
            tlvCic.AllColumns.Add(olvWorking);
            tlvCic.AllColumns.Add(olvTime);
            tlvCic.AllColumns.Add(olvCatalogue);
            tlvCic.CellEditUseWholeCell = false;
            tlvCic.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvNameCol, olvExecute, olvCached, olvCount, olvCumulativeTotal, olvWorking, olvTime, olvCatalogue });
            tlvCic.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvCic.Location = new System.Drawing.Point(0, 0);
            tlvCic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tlvCic.Name = "tlvCic";
            tlvCic.ShowGroups = false;
            tlvCic.Size = new System.Drawing.Size(1450, 691);
            tlvCic.TabIndex = 60;
            tlvCic.UseCompatibleStateImageBehavior = false;
            tlvCic.View = System.Windows.Forms.View.Details;
            tlvCic.VirtualMode = true;
            // 
            // olvNameCol
            // 
            olvNameCol.AspectName = "ToString";
            olvNameCol.MinimumWidth = 100;
            olvNameCol.Text = "Name";
            olvNameCol.Width = 100;
            // 
            // olvExecute
            // 
            olvExecute.IsEditable = false;
            olvExecute.Sortable = false;
            olvExecute.Text = "Execute";
            // 
            // olvOrder
            // 
            olvOrder.DisplayIndex = 2;
            olvOrder.IsVisible = false;
            olvOrder.Sortable = false;
            olvOrder.Text = "Order";
            // 
            // olvCached
            // 
            olvCached.IsEditable = false;
            olvCached.Sortable = false;
            olvCached.Text = "Cached";
            // 
            // olvCount
            // 
            olvCount.IsEditable = false;
            olvCount.Sortable = false;
            olvCount.Text = "Count";
            // 
            // olvCumulativeTotal
            // 
            olvCumulativeTotal.IsEditable = false;
            olvCumulativeTotal.Sortable = false;
            olvCumulativeTotal.Text = "Cumulative Total";
            // 
            // olvWorking
            // 
            olvWorking.IsEditable = false;
            olvWorking.Sortable = false;
            olvWorking.Text = "Working";
            // 
            // olvTime
            // 
            olvTime.IsEditable = false;
            olvTime.Sortable = false;
            olvTime.Text = "Time";
            // 
            // olvCatalogue
            // 
            olvCatalogue.IsEditable = false;
            olvCatalogue.Sortable = false;
            olvCatalogue.Text = "Catalogue";
            olvCatalogue.Width = 150;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // ticket
            // 
            ticket.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ticket.Location = new System.Drawing.Point(1097, 0);
            ticket.Margin = new System.Windows.Forms.Padding(0);
            ticket.Name = "ticket";
            ticket.Size = new System.Drawing.Size(348, 81);
            ticket.TabIndex = 55;
            ticket.TicketText = "";
            ticket.TicketTextChanged += ticket_TicketTextChanged;
            // 
            // version
            // 
            version.Location = new System.Drawing.Point(4, 74);
            version.Margin = new System.Windows.Forms.Padding(0);
            version.Name = "version";
            version.Size = new System.Drawing.Size(170, 33);
            version.TabIndex = 55;
            // 
            // btnAbortLoad
            // 
            btnAbortLoad.Image = (System.Drawing.Image)resources.GetObject("btnAbortLoad.Image");
            btnAbortLoad.Location = new System.Drawing.Point(178, 0);
            btnAbortLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnAbortLoad.Name = "btnAbortLoad";
            btnAbortLoad.Size = new System.Drawing.Size(34, 36);
            btnAbortLoad.TabIndex = 65;
            btnAbortLoad.UseVisualStyleBackColor = true;
            btnAbortLoad.Click += btnAbortLoad_Click;
            // 
            // btnExecute
            // 
            btnExecute.Image = (System.Drawing.Image)resources.GetObject("btnExecute.Image");
            btnExecute.Location = new System.Drawing.Point(0, 0);
            btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new System.Drawing.Size(178, 36);
            btnExecute.TabIndex = 66;
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(version);
            splitContainer2.Panel1.Controls.Add(gbCicInfo);
            splitContainer2.Panel1.Controls.Add(groupBox1);
            splitContainer2.Panel1.Controls.Add(ticket);
            splitContainer2.Panel1MinSize = 82;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tlvCic);
            splitContainer2.Size = new System.Drawing.Size(1450, 801);
            splitContainer2.SplitterDistance = 105;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 67;
            // 
            // gbCicInfo
            // 
            gbCicInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbCicInfo.Controls.Add(tbDescription);
            gbCicInfo.Location = new System.Drawing.Point(261, 3);
            gbCicInfo.Name = "gbCicInfo";
            gbCicInfo.Size = new System.Drawing.Size(833, 99);
            gbCicInfo.TabIndex = 71;
            gbCicInfo.TabStop = false;
            gbCicInfo.Text = "Name:";
            // 
            // tbDescription
            // 
            tbDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tbDescription.Location = new System.Drawing.Point(6, 16);
            tbDescription.Margin = new System.Windows.Forms.Padding(3, 3, 1, 0);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.ReadOnly = true;
            tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbDescription.Size = new System.Drawing.Size(824, 80);
            tbDescription.TabIndex = 54;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(lblExecuteAllPhase);
            groupBox1.Location = new System.Drawing.Point(4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(255, 76);
            groupBox1.TabIndex = 67;
            groupBox1.TabStop = false;
            groupBox1.Text = "Controls";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnClearCache);
            panel1.Controls.Add(btnExecute);
            panel1.Controls.Add(btnAbortLoad);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(4, 19);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(247, 36);
            panel1.TabIndex = 70;
            // 
            // btnClearCache
            // 
            btnClearCache.Image = (System.Drawing.Image)resources.GetObject("btnClearCache.Image");
            btnClearCache.Location = new System.Drawing.Point(212, 0);
            btnClearCache.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClearCache.Name = "btnClearCache";
            btnClearCache.Size = new System.Drawing.Size(34, 36);
            btnClearCache.TabIndex = 67;
            btnClearCache.UseVisualStyleBackColor = true;
            btnClearCache.Click += btnClearCache_Click;
            // 
            // lblExecuteAllPhase
            // 
            lblExecuteAllPhase.Enabled = false;
            lblExecuteAllPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lblExecuteAllPhase.Location = new System.Drawing.Point(4, 58);
            lblExecuteAllPhase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblExecuteAllPhase.Name = "lblExecuteAllPhase";
            lblExecuteAllPhase.Size = new System.Drawing.Size(178, 13);
            lblExecuteAllPhase.TabIndex = 70;
            lblExecuteAllPhase.Text = "Execution status...";
            lblExecuteAllPhase.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CohortIdentificationConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer2);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CohortIdentificationConfigurationUI";
            Size = new System.Drawing.Size(1450, 801);
            ((System.ComponentModel.ISupportInitialize)tlvCic).EndInit();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            gbCicInfo.ResumeLayout(false);
            gbCicInfo.PerformLayout();
            groupBox1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TicketingControlUI ticket;
        private VersioningControlUI version;
        private TreeListView tlvCic;
        private OLVColumn olvNameCol;
        private OLVColumn olvExecute;
        private OLVColumn olvOrder;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnAbortLoad;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private OLVColumn olvCached;
        private OLVColumn olvCount;
        private OLVColumn olvCumulativeTotal;
        private OLVColumn olvWorking;
        private OLVColumn olvTime;
        private System.Windows.Forms.Label lblExecuteAllPhase;
        private OLVColumn olvCatalogue;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox gbCicInfo;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Button btnClearCache;
    }
}
