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
            this.olvResult = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvEventDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvMessage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.botPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.olvChecks)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(325, 20);
            this.tbFilter.TabIndex = 3;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // btnAbortChecking
            // 
            this.btnAbortChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbortChecking.Enabled = false;
            this.btnAbortChecking.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAbortChecking.Name = "btnAbortChecking";
            this.btnAbortChecking.Size = new System.Drawing.Size(98, 23);
            this.btnAbortChecking.TabIndex = 4;
            this.btnAbortChecking.Text = "Abort Checking";
            this.btnAbortChecking.UseVisualStyleBackColor = true;
            this.btnAbortChecking.Click += new System.EventHandler(this.btnAbortChecking_Click);
            //
            // botPanel
            //
            this.botPanel.Name = "botPanel";
            this.botPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.botPanel.Size = new System.Drawing.Size(20,25);
            this.botPanel.Controls.Add(tbFilter);
            this.botPanel.Controls.Add(label1);
            this.botPanel.Controls.Add(btnAbortChecking);
            // 
            // olvChecks
            // 
            this.olvChecks.AllColumns.Add(this.olvResult);
            this.olvChecks.AllColumns.Add(this.olvEventDate);
            this.olvChecks.AllColumns.Add(this.olvMessage);
            this.olvChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvChecks.CellEditUseWholeCell = false;
            this.olvChecks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvResult,
            this.olvEventDate,
            this.olvMessage});
            this.olvChecks.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvChecks.FullRowSelect = true;
            this.olvChecks.Location = new System.Drawing.Point(3, 3);
            this.olvChecks.Name = "olvChecks";
            this.olvChecks.Size = new System.Drawing.Size(645, 544);
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
            this.olvMessage.Text = "Message";
            this.olvMessage.MinimumWidth = 100;
            // 
            // ChecksUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.olvChecks);
            this.Controls.Add(this.botPanel);
            this.Name = "ChecksUI";
            this.Size = new System.Drawing.Size(651, 579);
            ((System.ComponentModel.ISupportInitialize)(this.olvChecks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
