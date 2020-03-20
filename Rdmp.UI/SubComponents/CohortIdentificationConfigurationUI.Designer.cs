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
            this.lblName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tlvCic = new BrightIdeasSoftware.TreeListView();
            this.olvNameCol = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExecute = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCached = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCumulativeTotal = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvWorking = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvTime = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ticket = new Rdmp.UI.LocationsMenu.Ticketing.TicketingControlUI();
            this.btnAbortLoad = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lblExecuteAllPhase = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblFrozen = new System.Windows.Forms.Label();
            this.olvCatalogue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvCic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(175, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 52;
            this.lblName.Text = "Name";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(150, 34);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(60, 13);
            this.lblDescription.TabIndex = 53;
            this.lblDescription.Text = "Description";
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
            this.tlvCic.Name = "tlvCic";
            this.tlvCic.ShowGroups = false;
            this.tlvCic.Size = new System.Drawing.Size(1243, 578);
            this.tlvCic.TabIndex = 60;
            this.tlvCic.UseCompatibleStateImageBehavior = false;
            this.tlvCic.View = System.Windows.Forms.View.Details;
            this.tlvCic.VirtualMode = true;
            // 
            // olvNameCol
            // 
            this.olvNameCol.AspectName = "ToString";
            this.olvNameCol.FillsFreeSpace = true;
            this.olvNameCol.MinimumWidth = 100;
            this.olvNameCol.Text = "Name";
            this.olvNameCol.Width = 100;
            // 
            // olvExecute
            // 
            this.olvExecute.Text = "Execute";
            olvExecute.Sortable = false;
            // 
            // olvOrder
            // 
            this.olvOrder.DisplayIndex = 2;
            this.olvOrder.IsVisible = false;
            this.olvOrder.Text = "Order";
            olvOrder.Sortable = false;
            // 
            // olvCached
            // 
            this.olvCached.IsEditable = false;
            this.olvCached.Text = "Cached";
            olvCached.Sortable = false;
            // 
            // olvCount
            // 
            this.olvCount.IsEditable = false;
            this.olvCount.Text = "Count";
            olvCount.Sortable = false;
            // 
            // olvCumulativeTotal
            // 
            this.olvCumulativeTotal.IsEditable = false;
            this.olvCumulativeTotal.Text = "Cumulative Total";
            olvCumulativeTotal.Sortable = false;
            // 
            // olvWorking
            // 
            this.olvWorking.IsEditable = false;
            this.olvWorking.Text = "Working";
            olvWorking.Sortable = false;
            // 
            // olvTime
            // 
            this.olvTime.IsEditable = false;
            this.olvTime.Text = "Time";
            olvTime.Sortable = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ticket
            // 
            this.ticket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ticket.Location = new System.Drawing.Point(916, 31);
            this.ticket.Name = "ticket";
            this.ticket.Size = new System.Drawing.Size(314, 62);
            this.ticket.TabIndex = 55;
            this.ticket.TicketText = "";
            this.ticket.TicketTextChanged += new System.EventHandler(this.ticket_TicketTextChanged);
            // 
            // btnAbortLoad
            // 
            this.btnAbortLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbortLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnAbortLoad.Image")));
            this.btnAbortLoad.Location = new System.Drawing.Point(95, 13);
            this.btnAbortLoad.Name = "btnAbortLoad";
            this.btnAbortLoad.Size = new System.Drawing.Size(29, 29);
            this.btnAbortLoad.TabIndex = 65;
            this.btnAbortLoad.UseVisualStyleBackColor = true;
            this.btnAbortLoad.Click += new System.EventHandler(this.btnAbortLoad_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(3, 13);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(86, 29);
            this.btnExecute.TabIndex = 66;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(92, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 63;
            this.label5.Text = "Abort";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 64;
            this.label2.Text = "Execute";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 13);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lblExecuteAllPhase);
            this.splitContainer2.Panel1.Controls.Add(this.tbDescription);
            this.splitContainer2.Panel1.Controls.Add(this.tbName);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel1.Controls.Add(this.ticket);
            this.splitContainer2.Panel1.Controls.Add(this.lblName);
            this.splitContainer2.Panel1.Controls.Add(this.lblDescription);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tlvCic);
            this.splitContainer2.Size = new System.Drawing.Size(1243, 681);
            this.splitContainer2.SplitterDistance = 99;
            this.splitContainer2.TabIndex = 67;
            // 
            // lblExecuteAllPhase
            // 
            this.lblExecuteAllPhase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblExecuteAllPhase.AutoSize = true;
            this.lblExecuteAllPhase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExecuteAllPhase.Location = new System.Drawing.Point(16, 81);
            this.lblExecuteAllPhase.Name = "lblExecuteAllPhase";
            this.lblExecuteAllPhase.Size = new System.Drawing.Size(0, 13);
            this.lblExecuteAllPhase.TabIndex = 70;
            // 
            // tbDescription
            // 
            this.tbDescription.AutoSize = true;
            this.tbDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbDescription.Location = new System.Drawing.Point(216, 34);
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(11, 13);
            this.tbDescription.TabIndex = 69;
            this.tbDescription.Text = "-";
            // 
            // tbName
            // 
            this.tbName.AutoSize = true;
            this.tbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbName.Location = new System.Drawing.Point(216, 8);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(11, 13);
            this.tbName.TabIndex = 68;
            this.tbName.Text = "-";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExecute);
            this.groupBox1.Controls.Add(this.btnAbortLoad);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(14, 17);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(130, 61);
            this.groupBox1.TabIndex = 67;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Controls";
            // 
            // lblFrozen
            // 
            this.lblFrozen.BackColor = System.Drawing.SystemColors.HotTrack;
            this.lblFrozen.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFrozen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFrozen.ForeColor = System.Drawing.Color.Moccasin;
            this.lblFrozen.Location = new System.Drawing.Point(0, 0);
            this.lblFrozen.Name = "lblFrozen";
            this.lblFrozen.Size = new System.Drawing.Size(1243, 13);
            this.lblFrozen.TabIndex = 68;
            this.lblFrozen.Text = "Read Only Mode (Configuration is Frozen)";
            this.lblFrozen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // olvCatalogue
            // 
            this.olvCatalogue.Text = "Catalogue";
            olvCatalogue.Sortable = false;
            olvCatalogue.IsEditable = false;
            olvCatalogue.Width = 150;
            // 
            // CohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.lblFrozen);
            this.Name = "CohortIdentificationConfigurationUI";
            this.Size = new System.Drawing.Size(1243, 694);
            ((System.ComponentModel.ISupportInitialize)(this.tlvCic)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblName;
        private TicketingControlUI ticket;
        private System.Windows.Forms.Label lblDescription;
        private TreeListView tlvCic;
        private OLVColumn olvNameCol;
        private OLVColumn olvExecute;
        private OLVColumn olvOrder;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnAbortLoad;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblFrozen;
        private System.Windows.Forms.Label tbDescription;
        private System.Windows.Forms.Label tbName;
        private OLVColumn olvCached;
        private OLVColumn olvCount;
        private OLVColumn olvCumulativeTotal;
        private OLVColumn olvWorking;
        private OLVColumn olvTime;
        private System.Windows.Forms.Label lblExecuteAllPhase;
        private OLVColumn olvCatalogue;
    }
}
