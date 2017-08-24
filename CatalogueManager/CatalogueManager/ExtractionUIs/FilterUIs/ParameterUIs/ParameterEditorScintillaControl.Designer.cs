namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs
{
    partial class ParameterEditorScintillaControl
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
            this.gbCompiledView = new System.Windows.Forms.GroupBox();
            this.lblError = new System.Windows.Forms.Label();
            this.gbCompiledView.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbCompiledView
            // 
            this.gbCompiledView.Controls.Add(this.lblError);
            this.gbCompiledView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCompiledView.Location = new System.Drawing.Point(0, 0);
            this.gbCompiledView.Name = "gbCompiledView";
            this.gbCompiledView.Size = new System.Drawing.Size(837, 696);
            this.gbCompiledView.TabIndex = 13;
            this.gbCompiledView.TabStop = false;
            this.gbCompiledView.Text = "Compiled View (Editable)";
            // 
            // lblError
            // 
            this.lblError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblError.AutoSize = true;
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(6, 680);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(39, 13);
            this.lblError.TabIndex = 0;
            this.lblError.Text = "lblError";
            this.lblError.Visible = false;
            // 
            // ParameterEditorScintillaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbCompiledView);
            this.Name = "ParameterEditorScintillaControl";
            this.Size = new System.Drawing.Size(837, 696);
            this.gbCompiledView.ResumeLayout(false);
            this.gbCompiledView.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbCompiledView;
        private System.Windows.Forms.Label lblError;
    }
}
