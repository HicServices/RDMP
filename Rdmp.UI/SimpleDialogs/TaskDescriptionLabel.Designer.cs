namespace Rdmp.UI.SimpleDialogs
{
    partial class TaskDescriptionLabel
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
            this.tbTaskDescription = new System.Windows.Forms.TextBox();
            this.tbEntryLabel = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTaskDescription = new System.Windows.Forms.Panel();
            this.pnlTaskDescriptionBorder = new System.Windows.Forms.Panel();
            this.pnlEntryLabel = new System.Windows.Forms.Panel();
            this.pnlTopMargin = new System.Windows.Forms.Panel();
            this.pnlTaskDescription.SuspendLayout();
            this.pnlTaskDescriptionBorder.SuspendLayout();
            this.pnlEntryLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbTaskDescription
            // 
            this.tbTaskDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTaskDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            this.tbTaskDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbTaskDescription.Cursor = System.Windows.Forms.Cursors.Default;
            this.tbTaskDescription.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tbTaskDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(108)))), ((int)(((byte)(128)))));
            this.tbTaskDescription.Location = new System.Drawing.Point(11, 13);
            this.tbTaskDescription.Margin = new System.Windows.Forms.Padding(10);
            this.tbTaskDescription.Multiline = true;
            this.tbTaskDescription.Name = "tbTaskDescription";
            this.tbTaskDescription.ReadOnly = true;
            this.tbTaskDescription.Size = new System.Drawing.Size(588, 94);
            this.tbTaskDescription.TabIndex = 2;
            this.tbTaskDescription.TabStop = false;
            this.tbTaskDescription.Text = "tbTaskDescription";
            this.tbTaskDescription.Resize += new System.EventHandler(this.textBox1_Resize);
            // 
            // tbEntryLabel
            // 
            this.tbEntryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEntryLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbEntryLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.tbEntryLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tbEntryLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbEntryLabel.Location = new System.Drawing.Point(6, 0);
            this.tbEntryLabel.Margin = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.tbEntryLabel.Multiline = true;
            this.tbEntryLabel.Name = "tbEntryLabel";
            this.tbEntryLabel.ReadOnly = true;
            this.tbEntryLabel.Size = new System.Drawing.Size(612, 30);
            this.tbEntryLabel.TabIndex = 3;
            this.tbEntryLabel.TabStop = false;
            this.tbEntryLabel.Text = "tbEntryLabel";
            this.tbEntryLabel.Resize += new System.EventHandler(this.tbEntryLabel_Resize);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(283, 28);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // pnlTaskDescription
            // 
            this.pnlTaskDescription.Controls.Add(this.pnlTaskDescriptionBorder);
            this.pnlTaskDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTaskDescription.Location = new System.Drawing.Point(0, 10);
            this.pnlTaskDescription.Name = "pnlTaskDescription";
            this.pnlTaskDescription.Size = new System.Drawing.Size(632, 132);
            this.pnlTaskDescription.TabIndex = 5;
            this.pnlTaskDescription.Visible = false;
            // 
            // pnlTaskDescriptionBorder
            // 
            this.pnlTaskDescriptionBorder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTaskDescriptionBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            this.pnlTaskDescriptionBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTaskDescriptionBorder.Controls.Add(this.tbTaskDescription);
            this.pnlTaskDescriptionBorder.Location = new System.Drawing.Point(10, 0);
            this.pnlTaskDescriptionBorder.Margin = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.pnlTaskDescriptionBorder.Name = "pnlTaskDescriptionBorder";
            this.pnlTaskDescriptionBorder.Size = new System.Drawing.Size(612, 119);
            this.pnlTaskDescriptionBorder.TabIndex = 3;
            // 
            // pnlEntryLabel
            // 
            this.pnlEntryLabel.Controls.Add(this.tbEntryLabel);
            this.pnlEntryLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEntryLabel.Location = new System.Drawing.Point(0, 142);
            this.pnlEntryLabel.Name = "pnlEntryLabel";
            this.pnlEntryLabel.Size = new System.Drawing.Size(632, 147);
            this.pnlEntryLabel.TabIndex = 6;
            // 
            // pnlTopMargin
            // 
            this.pnlTopMargin.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopMargin.Location = new System.Drawing.Point(0, 0);
            this.pnlTopMargin.Name = "pnlTopMargin";
            this.pnlTopMargin.Size = new System.Drawing.Size(632, 10);
            this.pnlTopMargin.TabIndex = 4;
            // 
            // TaskDescriptionLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.pnlEntryLabel);
            this.Controls.Add(this.pnlTaskDescription);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.pnlTopMargin);
            this.Name = "TaskDescriptionLabel";
            this.Size = new System.Drawing.Size(632, 289);
            this.pnlTaskDescription.ResumeLayout(false);
            this.pnlTaskDescriptionBorder.ResumeLayout(false);
            this.pnlTaskDescriptionBorder.PerformLayout();
            this.pnlEntryLabel.ResumeLayout(false);
            this.pnlEntryLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbTaskDescription;
        private System.Windows.Forms.TextBox tbEntryLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel pnlTaskDescription;
        private System.Windows.Forms.Panel pnlEntryLabel;
        private System.Windows.Forms.Panel pnlTaskDescriptionBorder;
        private System.Windows.Forms.Panel pnlTopMargin;
    }
}
