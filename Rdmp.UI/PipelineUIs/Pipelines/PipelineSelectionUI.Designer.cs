namespace Rdmp.UI.PipelineUIs.Pipelines
{
    partial class PipelineSelectionUI
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
            this.ddPipelines = new System.Windows.Forms.ComboBox();
            this.btnCreateNewPipeline = new System.Windows.Forms.Button();
            this.btnEditPipeline = new System.Windows.Forms.Button();
            this.gbPrompt = new System.Windows.Forms.GroupBox();
            this.btnClonePipeline = new System.Windows.Forms.Button();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDeletePipeline = new System.Windows.Forms.Button();
            this.gbPrompt.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddPipelines
            // 
            this.ddPipelines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddPipelines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPipelines.FormattingEnabled = true;
            this.ddPipelines.Location = new System.Drawing.Point(16, 24);
            this.ddPipelines.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddPipelines.Name = "ddPipelines";
            this.ddPipelines.Size = new System.Drawing.Size(642, 23);
            this.ddPipelines.TabIndex = 25;
            this.ddPipelines.SelectedIndexChanged += new System.EventHandler(this.ddPipelines_SelectedIndexChanged);
            // 
            // btnCreateNewPipeline
            // 
            this.btnCreateNewPipeline.Location = new System.Drawing.Point(16, 89);
            this.btnCreateNewPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreateNewPipeline.Name = "btnCreateNewPipeline";
            this.btnCreateNewPipeline.Size = new System.Drawing.Size(59, 25);
            this.btnCreateNewPipeline.TabIndex = 27;
            this.btnCreateNewPipeline.Text = "New...";
            this.btnCreateNewPipeline.UseVisualStyleBackColor = true;
            this.btnCreateNewPipeline.Click += new System.EventHandler(this.btnCreateNewPipeline_Click);
            // 
            // btnEditPipeline
            // 
            this.btnEditPipeline.Enabled = false;
            this.btnEditPipeline.Location = new System.Drawing.Point(16, 57);
            this.btnEditPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnEditPipeline.Name = "btnEditPipeline";
            this.btnEditPipeline.Size = new System.Drawing.Size(48, 25);
            this.btnEditPipeline.TabIndex = 27;
            this.btnEditPipeline.Text = "Edit...";
            this.btnEditPipeline.UseVisualStyleBackColor = true;
            this.btnEditPipeline.Click += new System.EventHandler(this.btnEditPipeline_Click);
            // 
            // gbPrompt
            // 
            this.gbPrompt.Controls.Add(this.btnClonePipeline);
            this.gbPrompt.Controls.Add(this.tbDescription);
            this.gbPrompt.Controls.Add(this.label1);
            this.gbPrompt.Controls.Add(this.btnDeletePipeline);
            this.gbPrompt.Controls.Add(this.btnCreateNewPipeline);
            this.gbPrompt.Controls.Add(this.ddPipelines);
            this.gbPrompt.Controls.Add(this.btnEditPipeline);
            this.gbPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPrompt.Location = new System.Drawing.Point(0, 0);
            this.gbPrompt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPrompt.Name = "gbPrompt";
            this.gbPrompt.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPrompt.Size = new System.Drawing.Size(729, 179);
            this.gbPrompt.TabIndex = 35;
            this.gbPrompt.TabStop = false;
            this.gbPrompt.Text = "Select Pipeline";
            this.gbPrompt.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btnClonePipeline
            // 
            this.btnClonePipeline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClonePipeline.Location = new System.Drawing.Point(666, 22);
            this.btnClonePipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClonePipeline.Name = "btnClonePipeline";
            this.btnClonePipeline.Size = new System.Drawing.Size(56, 25);
            this.btnClonePipeline.TabIndex = 31;
            this.btnClonePipeline.Text = "Clone";
            this.btnClonePipeline.UseVisualStyleBackColor = true;
            this.btnClonePipeline.Click += new System.EventHandler(this.btnClonePipeline_Click);
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(83, 57);
            this.tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ReadOnly = true;
            this.tbDescription.Size = new System.Drawing.Size(639, 92);
            this.tbDescription.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(645, 153);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 29;
            this.label1.Text = "(Description)";
            // 
            // btnDeletePipeline
            // 
            this.btnDeletePipeline.Enabled = false;
            this.btnDeletePipeline.Location = new System.Drawing.Point(16, 121);
            this.btnDeletePipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDeletePipeline.Name = "btnDeletePipeline";
            this.btnDeletePipeline.Size = new System.Drawing.Size(59, 25);
            this.btnDeletePipeline.TabIndex = 27;
            this.btnDeletePipeline.Text = "Delete";
            this.btnDeletePipeline.UseVisualStyleBackColor = true;
            this.btnDeletePipeline.Click += new System.EventHandler(this.btnDeletePipeline_Click);
            // 
            // PipelineSelectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.gbPrompt);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(292, 32);
            this.Name = "PipelineSelectionUI";
            this.Size = new System.Drawing.Size(729, 179);
            this.gbPrompt.ResumeLayout(false);
            this.gbPrompt.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox ddPipelines;
        private System.Windows.Forms.Button btnCreateNewPipeline;
        private System.Windows.Forms.Button btnEditPipeline;
        private System.Windows.Forms.GroupBox gbPrompt;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDeletePipeline;
        private System.Windows.Forms.Button btnClonePipeline;
    }
}
