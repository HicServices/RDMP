using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting
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
            this.consequenceKey1 = new Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents.ConsequenceKey();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gbKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbKey
            // 
            this.gbKey.Controls.Add(this.consequenceKey1);
            this.gbKey.Dock = System.Windows.Forms.DockStyle.Right;
            this.gbKey.Location = new System.Drawing.Point(714, 0);
            this.gbKey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbKey.Name = "gbKey";
            this.gbKey.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbKey.Size = new System.Drawing.Size(119, 576);
            this.gbKey.TabIndex = 2;
            this.gbKey.TabStop = false;
            this.gbKey.Text = "Key";
            // 
            // consequenceKey1
            // 
            this.consequenceKey1.Dock = System.Windows.Forms.DockStyle.Top;
            this.consequenceKey1.Location = new System.Drawing.Point(4, 19);
            this.consequenceKey1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.consequenceKey1.Name = "consequenceKey1";
            this.consequenceKey1.Size = new System.Drawing.Size(111, 119);
            this.consequenceKey1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(714, 576);
            this.panel1.TabIndex = 3;
            // 
            // ColumnStatesChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbKey);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ColumnStatesChart";
            this.Size = new System.Drawing.Size(833, 576);
            this.gbKey.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ConsequenceKey consequenceKey1;
        private System.Windows.Forms.GroupBox gbKey;
        private System.Windows.Forms.Panel panel1;

    }
}
