using BrightIdeasSoftware;
using Rdmp.UI.LocationsMenu.Ticketing;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CohortIdentificationConfigurationUI));
            this.tlvCic = new BrightIdeasSoftware.TreeListView();
            this.olvNameCol = new BrightIdeasSoftware.OLVColumn();
            this.olvExecute = new BrightIdeasSoftware.OLVColumn();
            this.olvOrder = new BrightIdeasSoftware.OLVColumn();
            this.olvCached = new BrightIdeasSoftware.OLVColumn();
            this.olvCount = new BrightIdeasSoftware.OLVColumn();
            this.olvCumulativeTotal = new BrightIdeasSoftware.OLVColumn();
            this.olvWorking = new BrightIdeasSoftware.OLVColumn();
            this.olvTime = new BrightIdeasSoftware.OLVColumn();
            this.olvCatalogue = new BrightIdeasSoftware.OLVColumn();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ticket = new Rdmp.UI.LocationsMenu.Ticketing.TicketingControlUI();
            this.btnAbortLoad = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.gbCicInfo = new System.Windows.Forms.GroupBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblExecuteAllPhase = new System.Windows.Forms.Label();
            this.btnClearCache = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tlvCic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbCicInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlvCic
            // 
            this.tlvCic.AllColumns.Add(this.olvNameCol);
            this.tlvCic.AllColumns.Add(this.olvExecute);
            this.tlvCic.AllColumns.Add(this.olvOrder);
            this.tlvCic.AllColumns.Add(this.olvCached);
            this.tlvCic.AllColumns.Add(this.olvCount);
            this.tlvCic.AllColumns.Add(this.olvCumulativeTotal);
            this.tlvCic.AllColumns.Add(this.olvWorking);
            this.tlvCic.AllColumns.Add(this.olvTime);
            this.tlvCic.AllColumns.Add(this.olvCatalogue);
            this.tlvCic.CellEditUseWholeCell = false;
            this.tlvCic.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvNameCol,
            this.olvExecute,
            this.olvCached,
            this.olvCount,
            this.olvCumulativeTotal,
            this.olvWorking,
            this.olvTime,
            this.olvCatalogue});
            this.tlvCic.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvCic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvCic.HideSelection = false;
            this.tlvCic.Location = new System.Drawing.Point(0, 0);
            this.tlvCic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tlvCic.Name = "tlvCic";
            this.tlvCic.ShowGroups = false;
            this.tlvCic.Size = new System.Drawing.Size(1450, 714);
            this.tlvCic.TabIndex = 60;
            this.tlvCic.UseCompatibleStateImageBehavior = false;
            this.tlvCic.View = System.Windows.Forms.View.Details;
            this.tlvCic.VirtualMode = true;
            // 
            // olvNameCol
            // 
            this.olvNameCol.AspectName = "ToString";
            this.olvNameCol.MinimumWidth = 100;
            this.olvNameCol.Text = "Name";
            this.olvNameCol.Width = 100;
            // 
            // olvExecute
            // 
            this.olvExecute.Sortable = false;
            this.olvExecute.Text = "Execute";
            // 
            // olvOrder
            // 
            this.olvOrder.DisplayIndex = 2;
            this.olvOrder.IsVisible = false;
            this.olvOrder.Sortable = false;
            this.olvOrder.Text = "Order";
            // 
            // olvCached
            // 
            this.olvCached.IsEditable = false;
            this.olvCached.Sortable = false;
            this.olvCached.Text = "Cached";
            // 
            // olvCount
            // 
            this.olvCount.IsEditable = false;
            this.olvCount.Sortable = false;
            this.olvCount.Text = "Count";
            // 
            // olvCumulativeTotal
            // 
            this.olvCumulativeTotal.IsEditable = false;
            this.olvCumulativeTotal.Sortable = false;
            this.olvCumulativeTotal.Text = "Cumulative Total";
            // 
            // olvWorking
            // 
            this.olvWorking.IsEditable = false;
            this.olvWorking.Sortable = false;
            this.olvWorking.Text = "Working";
            // 
            // olvTime
            // 
            this.olvTime.IsEditable = false;
            this.olvTime.Sortable = false;
            this.olvTime.Text = "Time";
            // 
            // olvCatalogue
            // 
            this.olvCatalogue.IsEditable = false;
            this.olvCatalogue.Sortable = false;
            this.olvCatalogue.Text = "Catalogue";
            this.olvCatalogue.Width = 150;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ticket
            // 
            this.ticket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ticket.Location = new System.Drawing.Point(1097, 0);
            this.ticket.Margin = new System.Windows.Forms.Padding(0);
            this.ticket.Name = "ticket";
            this.ticket.Size = new System.Drawing.Size(348, 79);
            this.ticket.TabIndex = 55;
            this.ticket.TicketText = "";
            this.ticket.TicketTextChanged += new System.EventHandler(this.ticket_TicketTextChanged);
            // 
            // btnAbortLoad
            // 
            this.btnAbortLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnAbortLoad.Image")));
            this.btnAbortLoad.Location = new System.Drawing.Point(178, 0);
            this.btnAbortLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAbortLoad.Name = "btnAbortLoad";
            this.btnAbortLoad.Size = new System.Drawing.Size(34, 36);
            this.btnAbortLoad.TabIndex = 65;
            this.btnAbortLoad.UseVisualStyleBackColor = true;
            this.btnAbortLoad.Click += new System.EventHandler(this.btnAbortLoad_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(0, 0);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(178, 36);
            this.btnExecute.TabIndex = 66;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.gbCicInfo);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel1.Controls.Add(this.ticket);
            this.splitContainer2.Panel1MinSize = 82;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tlvCic);
            this.splitContainer2.Size = new System.Drawing.Size(1450, 801);
            this.splitContainer2.SplitterDistance = 82;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 67;
            // 
            // gbCicInfo
            // 
            this.gbCicInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbCicInfo.Controls.Add(this.tbDescription);
            this.gbCicInfo.Location = new System.Drawing.Point(261, 3);
            this.gbCicInfo.Name = "gbCicInfo";
            this.gbCicInfo.Size = new System.Drawing.Size(833, 76);
            this.gbCicInfo.TabIndex = 71;
            this.gbCicInfo.TabStop = false;
            this.gbCicInfo.Text = "Name:";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbDescription.Location = new System.Drawing.Point(6, 16);
            this.tbDescription.Margin = new System.Windows.Forms.Padding(3, 3, 1, 0);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ReadOnly = true;
            this.tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbDescription.Size = new System.Drawing.Size(824, 57);
            this.tbDescription.TabIndex = 54;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.lblExecuteAllPhase);
            this.groupBox1.Location = new System.Drawing.Point(4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(255, 76);
            this.groupBox1.TabIndex = 67;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Controls";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClearCache);
            this.panel1.Controls.Add(this.btnExecute);
            this.panel1.Controls.Add(this.btnAbortLoad);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(4, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 36);
            this.panel1.TabIndex = 70;
            // 
            // lblExecuteAllPhase
            // 
            this.lblExecuteAllPhase.Enabled = false;
            this.lblExecuteAllPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblExecuteAllPhase.Location = new System.Drawing.Point(4, 58);
            this.lblExecuteAllPhase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExecuteAllPhase.Name = "lblExecuteAllPhase";
            this.lblExecuteAllPhase.Size = new System.Drawing.Size(178, 13);
            this.lblExecuteAllPhase.TabIndex = 70;
            this.lblExecuteAllPhase.Text = "Execution status...";
            this.lblExecuteAllPhase.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnClearCache
            // 
            this.btnClearCache.Image = ((System.Drawing.Image)(resources.GetObject("btnClearCache.Image")));
            this.btnClearCache.Location = new System.Drawing.Point(212, 0);
            this.btnClearCache.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(34, 36);
            this.btnClearCache.TabIndex = 67;
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // CohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CohortIdentificationConfigurationUI";
            this.Size = new System.Drawing.Size(1450, 801);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCic)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbCicInfo.ResumeLayout(false);
            this.gbCicInfo.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private TicketingControlUI ticket;
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
