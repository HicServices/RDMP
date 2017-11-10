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
            this.automationMonitoringRenderAreaUI1 = new Dashboard.Automation.AutomationServerMonitorUIRenderArea();
            this.SuspendLayout();
            // 
            // automationMonitoringRenderAreaUI1
            // 
            this.automationMonitoringRenderAreaUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.automationMonitoringRenderAreaUI1.Location = new System.Drawing.Point(0, 0);
            this.automationMonitoringRenderAreaUI1.Name = "automationMonitoringRenderAreaUI1";
            this.automationMonitoringRenderAreaUI1.Size = new System.Drawing.Size(694, 367);
            this.automationMonitoringRenderAreaUI1.TabIndex = 1;
            // 
            // AutomationServerMonitorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.automationMonitoringRenderAreaUI1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AutomationServerMonitorUI";
            this.Size = new System.Drawing.Size(694, 367);
            this.ResumeLayout(false);

        }

        #endregion

        private AutomationServerMonitorUIRenderArea automationMonitoringRenderAreaUI1;
    }
}
