namespace Rdmp.UI.SimpleDialogs
{
    partial class ConfigurePrimaryKeyCollisionResolverUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurePrimaryKeyCollisionResolverUI));
            this.label1 = new System.Windows.Forms.Label();
            this.lbConflictResolutionColumns = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnSetAllDescending = new System.Windows.Forms.Button();
            this.btnSetAllAscending = new System.Windows.Forms.Button();
            this.tbHighlight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lbPrimaryKeys = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCopyPreview = new System.Windows.Forms.Button();
            this.btnCopyDetection = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1084, 56);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // lbConflictResolutionColumns
            // 
            this.lbConflictResolutionColumns.AllowDrop = true;
            this.lbConflictResolutionColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbConflictResolutionColumns.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbConflictResolutionColumns.FormattingEnabled = true;
            this.lbConflictResolutionColumns.Location = new System.Drawing.Point(3, 28);
            this.lbConflictResolutionColumns.Name = "lbConflictResolutionColumns";
            this.lbConflictResolutionColumns.Size = new System.Drawing.Size(360, 511);
            this.lbConflictResolutionColumns.TabIndex = 2;
            this.lbConflictResolutionColumns.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbConflictResolutionColumns_DrawItem);
            this.lbConflictResolutionColumns.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.lbConflictResolutionColumns.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.lbConflictResolutionColumns.DragOver += new System.Windows.Forms.DragEventHandler(this.listBox1_DragOver);
            this.lbConflictResolutionColumns.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            this.lbConflictResolutionColumns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbConflictResolutionColumns_MouseUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(-2, 163);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnSetAllDescending);
            this.splitContainer1.Panel1.Controls.Add(this.btnSetAllAscending);
            this.splitContainer1.Panel1.Controls.Add(this.tbHighlight);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.lbConflictResolutionColumns);
            this.splitContainer1.Size = new System.Drawing.Size(1099, 615);
            this.splitContainer1.SplitterDistance = 366;
            this.splitContainer1.TabIndex = 3;
            // 
            // btnSetAllDescending
            // 
            this.btnSetAllDescending.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetAllDescending.Location = new System.Drawing.Point(133, 546);
            this.btnSetAllDescending.Name = "btnSetAllDescending";
            this.btnSetAllDescending.Size = new System.Drawing.Size(109, 23);
            this.btnSetAllDescending.TabIndex = 10;
            this.btnSetAllDescending.Text = "Set All Descending";
            this.btnSetAllDescending.UseVisualStyleBackColor = true;
            this.btnSetAllDescending.Click += new System.EventHandler(this.btnSetAllDescending_Click);
            // 
            // btnSetAllAscending
            // 
            this.btnSetAllAscending.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetAllAscending.Location = new System.Drawing.Point(18, 546);
            this.btnSetAllAscending.Name = "btnSetAllAscending";
            this.btnSetAllAscending.Size = new System.Drawing.Size(109, 23);
            this.btnSetAllAscending.TabIndex = 10;
            this.btnSetAllAscending.Text = "Set All Ascending";
            this.btnSetAllAscending.UseVisualStyleBackColor = true;
            this.btnSetAllAscending.Click += new System.EventHandler(this.btnSetAllAscending_Click);
            // 
            // tbHighlight
            // 
            this.tbHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbHighlight.Location = new System.Drawing.Point(64, 582);
            this.tbHighlight.Name = "tbHighlight";
            this.tbHighlight.Size = new System.Drawing.Size(299, 20);
            this.tbHighlight.TabIndex = 9;
            this.tbHighlight.TextChanged += new System.EventHandler(this.tbHighlight_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 582);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Highlight:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Non PK Columns";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.Location = new System.Drawing.Point(273, 812);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(235, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lbPrimaryKeys
            // 
            this.lbPrimaryKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPrimaryKeys.FormattingEnabled = true;
            this.lbPrimaryKeys.Location = new System.Drawing.Point(816, 49);
            this.lbPrimaryKeys.Name = "lbPrimaryKeys";
            this.lbPrimaryKeys.Size = new System.Drawing.Size(272, 108);
            this.lbPrimaryKeys.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(744, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Primary keys";
            // 
            // btnCopyPreview
            // 
            this.btnCopyPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyPreview.Location = new System.Drawing.Point(920, 784);
            this.btnCopyPreview.Name = "btnCopyPreview";
            this.btnCopyPreview.Size = new System.Drawing.Size(177, 51);
            this.btnCopyPreview.TabIndex = 7;
            this.btnCopyPreview.Text = "Copy Preview SQL to Clipboard\r\n(e.g. so you can run it in RAW)";
            this.btnCopyPreview.UseVisualStyleBackColor = true;
            this.btnCopyPreview.Click += new System.EventHandler(this.btnSelectToClipboard_Click);
            // 
            // btnCopyDetection
            // 
            this.btnCopyDetection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyDetection.Location = new System.Drawing.Point(737, 784);
            this.btnCopyDetection.Name = "btnCopyDetection";
            this.btnCopyDetection.Size = new System.Drawing.Size(177, 51);
            this.btnCopyDetection.TabIndex = 7;
            this.btnCopyDetection.Text = "Copy Detection SQL to Clipboard \r\n(e.g. so you can run it in RAW)";
            this.btnCopyDetection.UseVisualStyleBackColor = true;
            this.btnCopyDetection.Click += new System.EventHandler(this.btnSelectToClipboard_Click);
            // 
            // ConfigurePrimaryKeyCollisionResolution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 847);
            this.Controls.Add(this.btnCopyDetection);
            this.Controls.Add(this.btnCopyPreview);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbPrimaryKeys);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Name = "ConfigurePrimaryKeyCollisionResolution";
            this.Text = "Configure Primary Key Collision Resolution";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lbConflictResolutionColumns;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListBox lbPrimaryKeys;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbHighlight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCopyPreview;
        private System.Windows.Forms.Button btnCopyDetection;
        private System.Windows.Forms.Button btnSetAllDescending;
        private System.Windows.Forms.Button btnSetAllAscending;

    }
}