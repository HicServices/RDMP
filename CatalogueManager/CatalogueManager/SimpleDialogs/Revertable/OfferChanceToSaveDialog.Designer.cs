namespace CatalogueManager.SimpleDialogs.Revertable
{
    partial class OfferChanceToSaveDialog
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
            this.lblFirstPrompt = new System.Windows.Forms.Label();
            this.btnYesSave = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnViewStackTrace = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFirstPrompt
            // 
            this.lblFirstPrompt.AutoSize = true;
            this.lblFirstPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblFirstPrompt.Name = "lblFirstPrompt";
            this.lblFirstPrompt.Size = new System.Drawing.Size(92, 13);
            this.lblFirstPrompt.TabIndex = 0;
            this.lblFirstPrompt.Text = "Save Changes to ";
            // 
            // btnYesSave
            // 
            this.btnYesSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnYesSave.Location = new System.Drawing.Point(665, 588);
            this.btnYesSave.Name = "btnYesSave";
            this.btnYesSave.Size = new System.Drawing.Size(194, 23);
            this.btnYesSave.TabIndex = 0;
            this.btnYesSave.Text = "This Version Is Correct (Application)";
            this.btnYesSave.UseVisualStyleBackColor = true;
            this.btnYesSave.Click += new System.EventHandler(this.btnYesSave_Click);
            // 
            // btnNo
            // 
            this.btnNo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnNo.Location = new System.Drawing.Point(192, 588);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(195, 23);
            this.btnNo.TabIndex = 1;
            this.btnNo.Text = "This Version Is Correct (Database)";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnViewStackTrace
            // 
            this.btnViewStackTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewStackTrace.Location = new System.Drawing.Point(1032, 588);
            this.btnViewStackTrace.Name = "btnViewStackTrace";
            this.btnViewStackTrace.Size = new System.Drawing.Size(25, 23);
            this.btnViewStackTrace.TabIndex = 1;
            this.btnViewStackTrace.Text = "?";
            this.btnViewStackTrace.UseVisualStyleBackColor = true;
            this.btnViewStackTrace.Click += new System.EventHandler(this.btnViewStackTrace_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 69);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1044, 513);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Differences:";
            // 
            // OfferChanceToSaveDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1069, 623);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnViewStackTrace);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnYesSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFirstPrompt);
            this.KeyPreview = true;
            this.Name = "OfferChanceToSaveDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OfferChanceToSaveDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFirstPrompt;
        private System.Windows.Forms.Button btnYesSave;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnViewStackTrace;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
    }
}