namespace Rdmp.UI.SimpleControls
{
    partial class HeatmapUI
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
            this.hoverToolTipTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // hoverToolTipTimer
            // 
            this.hoverToolTipTimer.Enabled = true;
            this.hoverToolTipTimer.Interval = 500;
            this.hoverToolTipTimer.Tick += new System.EventHandler(this.hoverToolTipTimer_Tick);
            // 
            // HeatmapUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "HeatmapUI";
            this.Size = new System.Drawing.Size(1109, 819);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer hoverToolTipTimer;
    }
}
