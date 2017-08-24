namespace Dashboard.CatalogueSummary.LoadEvents
{
    partial class ResolveFatalErrors
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
            this.pbExplanation = new System.Windows.Forms.Panel();
            this.tbFatalErrorIDs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSaveAndClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pbExplanation
            // 
            this.pbExplanation.Location = new System.Drawing.Point(12, 54);
            this.pbExplanation.Name = "pbExplanation";
            this.pbExplanation.Size = new System.Drawing.Size(865, 458);
            this.pbExplanation.TabIndex = 0;
            // 
            // tbFatalErrorIDs
            // 
            this.tbFatalErrorIDs.Location = new System.Drawing.Point(93, 12);
            this.tbFatalErrorIDs.Name = "tbFatalErrorIDs";
            this.tbFatalErrorIDs.ReadOnly = true;
            this.tbFatalErrorIDs.Size = new System.Drawing.Size(519, 20);
            this.tbFatalErrorIDs.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Fatal Error ID(s):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Explanation:";
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnSaveAndClose.Location = new System.Drawing.Point(334, 518);
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.Size = new System.Drawing.Size(120, 23);
            this.btnSaveAndClose.TabIndex = 5;
            this.btnSaveAndClose.Text = "Save and Close";
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // ResolveFatalErrors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 557);
            this.Controls.Add(this.btnSaveAndClose);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbFatalErrorIDs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbExplanation);
            this.Name = "ResolveFatalErrors";
            this.Text = "ResolveFatalErrors";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pbExplanation;
        private System.Windows.Forms.TextBox tbFatalErrorIDs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSaveAndClose;
    }
}