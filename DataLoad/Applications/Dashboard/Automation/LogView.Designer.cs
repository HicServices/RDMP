namespace Dashboard.Automation
{
    partial class SingleDataLoadLogView
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
            this.loadEventsTreeView1 = new Dashboard.CatalogueSummary.LoadEvents.LoadEventsTreeView();
            this.SuspendLayout();
            // 
            // loadEventsTreeView1
            // 
            this.loadEventsTreeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadEventsTreeView1.IsTestServerInterrogation = false;
            this.loadEventsTreeView1.Location = new System.Drawing.Point(1, 3);
            this.loadEventsTreeView1.Name = "loadEventsTreeView1";
            this.loadEventsTreeView1.Size = new System.Drawing.Size(1008, 813);
            this.loadEventsTreeView1.TabIndex = 0;
            // 
            // LogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.loadEventsTreeView1);
            this.Name = "LogView";
            this.Size = new System.Drawing.Size(1012, 815);
            this.ResumeLayout(false);

        }

        #endregion

        private CatalogueSummary.LoadEvents.LoadEventsTreeView loadEventsTreeView1;
    }
}