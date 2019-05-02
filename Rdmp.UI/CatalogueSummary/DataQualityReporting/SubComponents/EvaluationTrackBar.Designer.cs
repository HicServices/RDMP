namespace Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents
{
    partial class EvaluationTrackBar
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
            this.tbEvaluation = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbEvaluation)).BeginInit();
            this.SuspendLayout();
            // 
            // tbEvaluation
            // 
            this.tbEvaluation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEvaluation.Location = new System.Drawing.Point(59, 22);
            this.tbEvaluation.Name = "tbEvaluation";
            this.tbEvaluation.Size = new System.Drawing.Size(714, 45);
            this.tbEvaluation.TabIndex = 3;
            this.tbEvaluation.ValueChanged += new System.EventHandler(this.tbEvaluation_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(277, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Timeline of all Evaluations Made By Data Quality Engine";
            // 
            // EvaluationTrackBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbEvaluation);
            this.Name = "EvaluationTrackBar";
            this.Size = new System.Drawing.Size(827, 71);
            ((System.ComponentModel.ISupportInitialize)(this.tbEvaluation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar tbEvaluation;
        private System.Windows.Forms.Label label1;
    }
}
