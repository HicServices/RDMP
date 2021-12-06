using BrightIdeasSoftware;

namespace Rdmp.UI.ChecksUI
{
    partial class ChecksUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.btnAbortChecking = new System.Windows.Forms.Button();
            this.olvChecks = new BrightIdeasSoftware.ObjectListView();
            this.olvResult = new BrightIdeasSoftware.OLVColumn();
            this.olvEventDate = new BrightIdeasSoftware.OLVColumn();
            this.olvMessage = new BrightIdeasSoftware.OLVColumn();
            this.botPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.olvChecks)).BeginInit();
            this.botPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(37, 4);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(611, 23);
            this.tbFilter.TabIndex = 3;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // btnAbortChecking
            // 
            this.btnAbortChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbortChecking.Enabled = false;
            this.btnAbortChecking.Location = new System.Drawing.Point(650, 3);
            this.btnAbortChecking.Margin = new System.Windows.Forms.Padding(2, 3, 0, 3);
            this.btnAbortChecking.Name = "btnAbortChecking";
            this.btnAbortChecking.Size = new System.Drawing.Size(110, 25);
            this.btnAbortChecking.TabIndex = 4;
            this.btnAbortChecking.Text = "Abort Checking";
            this.btnAbortChecking.UseVisualStyleBackColor = true;
            this.btnAbortChecking.Click += new System.EventHandler(this.btnAbortChecking_Click);
            // 
            // olvChecks
            // 
            this.olvChecks.AllColumns.Add(this.olvResult);
            this.olvChecks.AllColumns.Add(this.olvEventDate);
            this.olvChecks.AllColumns.Add(this.olvMessage);
            this.olvChecks.CellEditUseWholeCell = false;
            this.olvChecks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvResult,
            this.olvEventDate,
            this.olvMessage});
            this.olvChecks.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvChecks.FullRowSelect = true;
            this.olvChecks.HideSelection = false;
            this.olvChecks.Location = new System.Drawing.Point(0, 0);
            this.olvChecks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.olvChecks.Name = "olvChecks";
            this.olvChecks.Size = new System.Drawing.Size(760, 639);
            this.olvChecks.TabIndex = 7;
            this.olvChecks.Text = "label3";
            this.olvChecks.UseCompatibleStateImageBehavior = false;
            this.olvChecks.View = System.Windows.Forms.View.Details;
            // 
            // olvResult
            // 
            this.olvResult.AspectName = "Result";
            this.olvResult.Text = "Result";
            this.olvResult.Width = 95;
            // 
            // olvEventDate
            // 
            this.olvEventDate.AspectName = "EventDate";
            this.olvEventDate.Text = "Event Date";
            this.olvEventDate.Width = 130;
            // 
            // olvMessage
            // 
            this.olvMessage.AspectName = "Message";
            this.olvMessage.Groupable = false;
            this.olvMessage.MinimumWidth = 100;
            this.olvMessage.Text = "Message";
            this.olvMessage.Width = 100;
            // 
            // botPanel
            // 
            this.botPanel.Controls.Add(this.tbFilter);
            this.botPanel.Controls.Add(this.label1);
            this.botPanel.Controls.Add(this.btnAbortChecking);
            this.botPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.botPanel.Location = new System.Drawing.Point(0, 639);
            this.botPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.botPanel.Name = "botPanel";
            this.botPanel.Size = new System.Drawing.Size(760, 29);
            this.botPanel.TabIndex = 8;
            // 
            // ChecksUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.olvChecks);
            this.Controls.Add(this.botPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ChecksUI";
            this.Size = new System.Drawing.Size(760, 668);
            ((System.ComponentModel.ISupportInitialize)(this.olvChecks)).EndInit();
            this.botPanel.ResumeLayout(false);
            this.botPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel botPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Button btnAbortChecking;
        private ObjectListView olvChecks;
        private OLVColumn olvMessage;
        private OLVColumn olvEventDate;
        private OLVColumn olvResult;
    }
}
