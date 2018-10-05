namespace Dashboard.CatalogueSummary.LoadEvents
{
    partial class AllLoadEventsUI
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
            this.label3 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.liveLoads = new Dashboard.CatalogueSummary.LoadEvents.LoadEventsTreeView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Filter Loads:";
            // 
            // tbFilter
            // 
            this.tbFilter.Location = new System.Drawing.Point(73, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(313, 20);
            this.tbFilter.TabIndex = 5;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // liveLoads
            // 
            this.liveLoads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveLoads.IsTestServerInterrogation = false;
            this.liveLoads.Location = new System.Drawing.Point(0, 27);
            this.liveLoads.Name = "liveLoads";
            this.liveLoads.Size = new System.Drawing.Size(805, 628);
            this.liveLoads.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.tbFilter);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(805, 27);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // AllLoadEventsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.liveLoads);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "AllLoadEventsUI";
            this.Size = new System.Drawing.Size(805, 655);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbFilter;
        private LoadEventsTreeView liveLoads;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;


    }
}
