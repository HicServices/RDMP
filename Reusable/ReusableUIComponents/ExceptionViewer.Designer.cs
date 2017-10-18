using BrightIdeasSoftware;

namespace ReusableUIComponents
{
    partial class ExceptionViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionViewer));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnViewException = new System.Windows.Forms.Button();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.keywordHelpTextListbox = new ReusableUIComponents.KeywordHelpTextListbox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(245, 290);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnViewException
            // 
            this.btnViewException.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewException.Location = new System.Drawing.Point(568, 290);
            this.btnViewException.Name = "btnViewException";
            this.btnViewException.Size = new System.Drawing.Size(124, 23);
            this.btnViewException.TabIndex = 1;
            this.btnViewException.Text = "View Exception";
            this.btnViewException.UseVisualStyleBackColor = true;
            this.btnViewException.Click += new System.EventHandler(this.btnViewException_Click);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(438, 290);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(124, 23);
            this.btnCopyToClipboard.TabIndex = 3;
            this.btnCopyToClipboard.Text = "Copy To Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(687, 137);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(3, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.richTextBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.keywordHelpTextListbox);
            this.splitContainer1.Size = new System.Drawing.Size(689, 279);
            this.splitContainer1.SplitterDistance = 139;
            this.splitContainer1.TabIndex = 6;
            // 
            // keywordHelpTextListbox
            // 
            this.keywordHelpTextListbox.AutoSize = true;
            this.keywordHelpTextListbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keywordHelpTextListbox.Location = new System.Drawing.Point(0, 0);
            this.keywordHelpTextListbox.Name = "keywordHelpTextListbox";
            this.keywordHelpTextListbox.Size = new System.Drawing.Size(687, 134);
            this.keywordHelpTextListbox.TabIndex = 0;
            // 
            // ExceptionViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 317);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.btnViewException);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExceptionViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ExceptionViewer";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ExceptionViewer_KeyUp);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnViewException;
        private System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private KeywordHelpTextListbox keywordHelpTextListbox;
    }
}