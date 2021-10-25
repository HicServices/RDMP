using BrightIdeasSoftware;

namespace Rdmp.UI.Performance
{
    partial class PerformanceCounterResultsUI
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
            this.components = new System.ComponentModel.Container();
            this.tlvLocations = new BrightIdeasSoftware.TreeListView();
            this.olvStackFrame = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvQueryCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.tlvLocations)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlvLocations
            // 
            this.tlvLocations.AllColumns.Add(this.olvStackFrame);
            this.tlvLocations.AllColumns.Add(this.olvQueryCount);
            this.tlvLocations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvLocations.CellEditUseWholeCell = false;
            this.tlvLocations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvStackFrame,
            this.olvQueryCount});
            this.tlvLocations.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvLocations.FullRowSelect = true;
            this.tlvLocations.Location = new System.Drawing.Point(3, 16);
            this.tlvLocations.Name = "tlvLocations";
            this.tlvLocations.ShowGroups = false;
            this.tlvLocations.Size = new System.Drawing.Size(1020, 645);
            this.tlvLocations.TabIndex = 0;
            this.tlvLocations.UseCompatibleStateImageBehavior = false;
            this.tlvLocations.View = System.Windows.Forms.View.Details;
            this.tlvLocations.VirtualMode = true;
            this.tlvLocations.ItemActivate += new System.EventHandler(this.tlvLocations_ItemActivate);
            // 
            // olvStackFrame
            // 
            this.olvStackFrame.AspectName = "ToString";
            this.olvStackFrame.Text = "CurrentFrame";
            this.olvStackFrame.MinimumWidth = 100;
            // 
            // olvQueryCount
            // 
            this.olvQueryCount.AspectName = "QueryCount";
            this.olvQueryCount.Text = "QueryCount";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbFilter);
            this.groupBox1.Controls.Add(this.tlvLocations);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1026, 693);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Unique Query Locations";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 670);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(44, 667);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(796, 20);
            this.tbFilter.TabIndex = 1;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // PerformanceCounterResultsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "PerformanceCounterResultsUI";
            this.Size = new System.Drawing.Size(1026, 693);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLocations)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvLocations;
        private System.Windows.Forms.GroupBox groupBox1;
        private OLVColumn olvStackFrame;
        private OLVColumn olvQueryCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
