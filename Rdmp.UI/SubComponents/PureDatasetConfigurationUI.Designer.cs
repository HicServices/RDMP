namespace Rdmp.UI.SubComponents
{
    partial class PureDatasetConfigurationUI
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
            columnButtonRenderer1 = new BrightIdeasSoftware.ColumnButtonRenderer();
            btnViewOnPure = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            tbDescription = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            tbTemporalStart = new System.Windows.Forms.TextBox();
            tbTemporalEnd = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            btnSave = new System.Windows.Forms.Button();
            panelLinks = new System.Windows.Forms.Panel();
            lblLinks = new System.Windows.Forms.Label();
            panelLinks.SuspendLayout();
            SuspendLayout();
            // 
            // columnButtonRenderer1
            // 
            columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
            // 
            // btnViewOnPure
            // 
            btnViewOnPure.Enabled = false;
            btnViewOnPure.Location = new System.Drawing.Point(29, 26);
            btnViewOnPure.Name = "btnViewOnPure";
            btnViewOnPure.Size = new System.Drawing.Size(121, 23);
            btnViewOnPure.TabIndex = 1;
            btnViewOnPure.Text = "View on Pure";
            btnViewOnPure.UseVisualStyleBackColor = true;
            btnViewOnPure.Click += btnViewOnPure_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(122, 72);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 15);
            label1.TabIndex = 2;
            label1.Text = "Name:";
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(170, 69);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(712, 23);
            tbName.TabIndex = 3;
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(170, 111);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(712, 295);
            tbDescription.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(97, 114);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 15);
            label2.TabIndex = 4;
            label2.Text = "Description:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(68, 428);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(96, 15);
            label3.TabIndex = 6;
            label3.Text = "Temporal Period:";
            // 
            // tbTemporalStart
            // 
            tbTemporalStart.Location = new System.Drawing.Point(170, 425);
            tbTemporalStart.Name = "tbTemporalStart";
            tbTemporalStart.Size = new System.Drawing.Size(181, 23);
            tbTemporalStart.TabIndex = 7;
            // 
            // tbTemporalEnd
            // 
            tbTemporalEnd.Location = new System.Drawing.Point(375, 425);
            tbTemporalEnd.Name = "tbTemporalEnd";
            tbTemporalEnd.Size = new System.Drawing.Size(181, 23);
            tbTemporalEnd.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(357, 428);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(12, 15);
            label4.TabIndex = 9;
            label4.Text = "-";
            label4.Click += label4_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(736, 49);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 23);
            btnSave.TabIndex = 14;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // panelLinks
            // 
            panelLinks.Controls.Add(btnSave);
            panelLinks.Location = new System.Drawing.Point(68, 471);
            panelLinks.Name = "panelLinks";
            panelLinks.Size = new System.Drawing.Size(814, 84);
            panelLinks.TabIndex = 15;
            // 
            // lblLinks
            // 
            lblLinks.AutoSize = true;
            lblLinks.Location = new System.Drawing.Point(68, 453);
            lblLinks.Name = "lblLinks";
            lblLinks.Size = new System.Drawing.Size(37, 15);
            lblLinks.TabIndex = 16;
            lblLinks.Text = "Links:";
            // 
            // PureDatasetConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblLinks);
            Controls.Add(panelLinks);
            Controls.Add(label4);
            Controls.Add(tbTemporalEnd);
            Controls.Add(tbTemporalStart);
            Controls.Add(label3);
            Controls.Add(tbDescription);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(label1);
            Controls.Add(btnViewOnPure);
            Name = "PureDatasetConfigurationUI";
            Size = new System.Drawing.Size(955, 648);
            Load += PureDatasetConfigurationUI_Load;
            panelLinks.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
        private System.Windows.Forms.Button btnViewOnPure;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTemporalStart;
        private System.Windows.Forms.TextBox tbTemporalEnd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel panelLinks;
        private System.Windows.Forms.Label lblLinks;
    }
}
