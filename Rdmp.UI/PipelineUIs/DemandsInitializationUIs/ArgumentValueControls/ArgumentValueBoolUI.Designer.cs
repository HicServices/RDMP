namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueBoolUI
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
            this.cbValue = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbValue
            // 
            this.cbValue.AutoSize = true;
            this.cbValue.Location = new System.Drawing.Point(3, 7);
            this.cbValue.Name = "cbValue";
            this.cbValue.Size = new System.Drawing.Size(15, 14);
            this.cbValue.TabIndex = 24;
            this.cbValue.UseVisualStyleBackColor = true;
            this.cbValue.CheckedChanged += new System.EventHandler(this.cbValue_CheckedChanged);
            // 
            // ArgumentValueBoolUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbValue);
            this.Name = "ArgumentValueBoolUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbValue;

    }
}
