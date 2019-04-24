using Dashboard.CatalogueSummary.DataQualityReporting.SubComponents;

namespace Dashboard.CatalogueSummary.DataQualityReporting
{
    partial class ColumnStatesChart
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
            this.gbKey = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.consequenceKey1 = new Dashboard.CatalogueSummary.DataQualityReporting.SubComponents.ConsequenceKey();
            this.gbKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbKey
            // 
            this.gbKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbKey.Controls.Add(this.consequenceKey1);
            this.gbKey.Location = new System.Drawing.Point(604, 0);
            this.gbKey.Name = "gbKey";
            this.gbKey.Size = new System.Drawing.Size(107, 496);
            this.gbKey.TabIndex = 2;
            this.gbKey.TabStop = false;
            this.gbKey.Text = "Key";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(595, 493);
            this.panel1.TabIndex = 3;
            // 
            // consequenceKey1
            // 
            this.consequenceKey1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.consequenceKey1.Location = new System.Drawing.Point(6, 19);
            this.consequenceKey1.Name = "consequenceKey1";
            this.consequenceKey1.Size = new System.Drawing.Size(107, 103);
            this.consequenceKey1.TabIndex = 1;
            // 
            // ColumnStatesChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbKey);
            this.Name = "ColumnStatesChart";
            this.Size = new System.Drawing.Size(714, 499);
            this.gbKey.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ConsequenceKey consequenceKey1;
        private System.Windows.Forms.GroupBox gbKey;
        private System.Windows.Forms.Panel panel1;

    }
}
