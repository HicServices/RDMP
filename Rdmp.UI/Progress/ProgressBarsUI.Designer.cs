
using RAGSmiley = Rdmp.UI.ChecksUI.RAGSmiley;

namespace Rdmp.UI.Progress
{
    partial class ProgressBarsUI
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
            this.lblTask = new System.Windows.Forms.Label();
            this.ragSmiley1 = new RAGSmiley();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTask
            // 
            this.lblTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTask.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lblTask.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTask.Location = new System.Drawing.Point(4, 5);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(636, 20);
            this.lblTask.TabIndex = 1;
            this.lblTask.Text = "Task";
            this.lblTask.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(646, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Enabled = false;
            this.btnClose.Location = new System.Drawing.Point(296, 155);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ProgressBarsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.ragSmiley1);
            this.Name = "ProgressBarsUI";
            this.Size = new System.Drawing.Size(671, 181);
            this.ResumeLayout(false);

        }

        #endregion

        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Label lblTask;
        private System.Windows.Forms.Button btnClose;
    }
}
