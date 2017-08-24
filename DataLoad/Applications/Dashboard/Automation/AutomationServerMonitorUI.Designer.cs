namespace Dashboard.Automation
{
    partial class AutomationServerMonitorUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutomationServerMonitorUI));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnPickServerSlot = new System.Windows.Forms.ToolStripButton();
            this.automationMonitoringRenderAreaUI1 = new Dashboard.Automation.AutomationServerMonitorUIRenderArea();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPickServerSlot});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(694, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnPickServerSlot
            // 
            this.btnPickServerSlot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPickServerSlot.Image = ((System.Drawing.Image)(resources.GetObject("btnPickServerSlot.Image")));
            this.btnPickServerSlot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPickServerSlot.Name = "btnPickServerSlot";
            this.btnPickServerSlot.Size = new System.Drawing.Size(23, 22);
            this.btnPickServerSlot.Text = "Pick Which Slot Is Monitored";
            this.btnPickServerSlot.Click += new System.EventHandler(this.btnPickServerSlot_Click);
            // 
            // automationMonitoringRenderAreaUI1
            // 
            this.automationMonitoringRenderAreaUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.automationMonitoringRenderAreaUI1.Location = new System.Drawing.Point(0, 28);
            this.automationMonitoringRenderAreaUI1.Name = "automationMonitoringRenderAreaUI1";
            this.automationMonitoringRenderAreaUI1.Size = new System.Drawing.Size(694, 339);
            this.automationMonitoringRenderAreaUI1.TabIndex = 1;
            // 
            // AutomationServerMonitorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.automationMonitoringRenderAreaUI1);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AutomationServerMonitorUI";
            this.Size = new System.Drawing.Size(694, 367);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private AutomationServerMonitorUIRenderArea automationMonitoringRenderAreaUI1;
        private System.Windows.Forms.ToolStripButton btnPickServerSlot;
    }
}
