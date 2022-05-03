using BrightIdeasSoftware;

namespace Rdmp.UI.Progress
{
    partial class ProgressUI
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblSuccess = new System.Windows.Forms.Label();
            this.lblCrashed = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.olvProgressEvents = new BrightIdeasSoftware.ObjectListView();
            this.olvSender = new BrightIdeasSoftware.OLVColumn();
            this.olvEventDate = new BrightIdeasSoftware.OLVColumn();
            this.olvMessage = new BrightIdeasSoftware.OLVColumn();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbTextFilter = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tbTopX = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.ddGroupBy = new System.Windows.Forms.ToolStripComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Result = new System.Windows.Forms.ColumnHeader();
            this.Time = new System.Windows.Forms.ColumnHeader();
            this.Sender = new System.Windows.Forms.ColumnHeader();
            this.Message = new System.Windows.Forms.ColumnHeader();
            this.ExceptionStack = new System.Windows.Forms.ColumnHeader();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvProgressEvents)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 18);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(1027, 168);
            this.dataGridView1.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblSuccess);
            this.splitContainer1.Panel1.Controls.Add(this.lblCrashed);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.olvProgressEvents);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip1);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(1027, 660);
            this.splitContainer1.SplitterDistance = 214;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 3;
            // 
            // lblSuccess
            // 
            this.lblSuccess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSuccess.BackColor = System.Drawing.Color.ForestGreen;
            this.lblSuccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSuccess.ForeColor = System.Drawing.Color.LemonChiffon;
            this.lblSuccess.Location = new System.Drawing.Point(0, 191);
            this.lblSuccess.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSuccess.Name = "lblSuccess";
            this.lblSuccess.Size = new System.Drawing.Size(1027, 20);
            this.lblSuccess.TabIndex = 8;
            this.lblSuccess.Text = "Success";
            this.lblSuccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSuccess.Visible = false;
            // 
            // lblCrashed
            // 
            this.lblCrashed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCrashed.BackColor = System.Drawing.Color.IndianRed;
            this.lblCrashed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCrashed.ForeColor = System.Drawing.Color.Orange;
            this.lblCrashed.Location = new System.Drawing.Point(0, 191);
            this.lblCrashed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCrashed.Name = "lblCrashed";
            this.lblCrashed.Size = new System.Drawing.Size(1027, 20);
            this.lblCrashed.TabIndex = 7;
            this.lblCrashed.Text = "Crashed";
            this.lblCrashed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCrashed.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(4, 193);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1020, 17);
            this.progressBar1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Progress of transfer jobs:";
            // 
            // olvProgressEvents
            // 
            this.olvProgressEvents.AllColumns.Add(this.olvSender);
            this.olvProgressEvents.AllColumns.Add(this.olvEventDate);
            this.olvProgressEvents.AllColumns.Add(this.olvMessage);
            this.olvProgressEvents.CellEditUseWholeCell = false;
            this.olvProgressEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvSender,
            this.olvEventDate,
            this.olvMessage});
            this.olvProgressEvents.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvProgressEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvProgressEvents.FullRowSelect = true;
            this.olvProgressEvents.Location = new System.Drawing.Point(0, 15);
            this.olvProgressEvents.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvProgressEvents.Name = "olvProgressEvents";
            this.olvProgressEvents.RowHeight = 19;
            this.olvProgressEvents.ShowGroups = false;
            this.olvProgressEvents.Size = new System.Drawing.Size(1027, 401);
            this.olvProgressEvents.TabIndex = 5;
            this.olvProgressEvents.UseCompatibleStateImageBehavior = false;
            this.olvProgressEvents.View = System.Windows.Forms.View.Details;
            // 
            // olvSender
            // 
            this.olvSender.AspectName = "Sender";
            this.olvSender.Text = "Sender";
            this.olvSender.Width = 170;
            // 
            // olvEventDate
            // 
            this.olvEventDate.AspectName = "EventDate";
            this.olvEventDate.Groupable = false;
            this.olvEventDate.Text = "Event Date";
            this.olvEventDate.Width = 140;
            // 
            // olvMessage
            // 
            this.olvMessage.AspectName = "Message";
            this.olvMessage.Groupable = false;
            this.olvMessage.MinimumWidth = 100;
            this.olvMessage.Text = "Message";
            this.olvMessage.Width = 300;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tbTextFilter,
            this.toolStripLabel2,
            this.tbTopX,
            this.toolStripLabel3,
            this.ddGroupBy});
            this.toolStrip1.Location = new System.Drawing.Point(0, 416);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1027, 25);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel1.Text = "Filter:";
            // 
            // tbTextFilter
            // 
            this.tbTextFilter.Name = "tbTextFilter";
            this.tbTextFilter.Size = new System.Drawing.Size(116, 25);
            this.tbTextFilter.TextChanged += new System.EventHandler(this.tbTextFilter_TextChanged);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(26, 22);
            this.toolStripLabel2.Text = "Top";
            // 
            // tbTopX
            // 
            this.tbTopX.Name = "tbTopX";
            this.tbTopX.Size = new System.Drawing.Size(116, 25);
            this.tbTopX.Text = "1000";
            this.tbTopX.TextChanged += new System.EventHandler(this.tbTopX_TextChanged);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(56, 22);
            this.toolStripLabel3.Text = "Group By";
            // 
            // ddGroupBy
            // 
            this.ddGroupBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddGroupBy.Name = "ddGroupBy";
            this.ddGroupBy.Size = new System.Drawing.Size(176, 25);
            this.ddGroupBy.SelectedIndexChanged += new System.EventHandler(this.ddGroupBy_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Notifications:";
            // 
            // Result
            // 
            this.Result.Text = "Result";
            this.Result.Width = 52;
            // 
            // Time
            // 
            this.Time.Text = "Time";
            this.Time.Width = 70;
            // 
            // Sender
            // 
            this.Sender.Text = "Sender";
            this.Sender.Width = 170;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 204;
            // 
            // ExceptionStack
            // 
            this.ExceptionStack.Text = "ExceptionStack";
            this.ExceptionStack.Width = 500;
            // 
            // ProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ProgressUI";
            this.Size = new System.Drawing.Size(1027, 660);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvProgressEvents)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private ObjectListView olvProgressEvents;
        private System.Windows.Forms.ColumnHeader Result;
        private System.Windows.Forms.ColumnHeader Time;
        private System.Windows.Forms.ColumnHeader Sender;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ColumnHeader ExceptionStack;
        private OLVColumn olvSender;
        private OLVColumn olvEventDate;
        private OLVColumn olvMessage;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tbTextFilter;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox tbTopX;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox ddGroupBy;
        private System.Windows.Forms.Label lblCrashed;
        private System.Windows.Forms.Label lblSuccess;
    }
}
