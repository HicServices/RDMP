namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
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
            this.cbValue.Location = new System.Drawing.Point(4, 9);
            this.cbValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbValue.Name = "cbValue";
            this.cbValue.Size = new System.Drawing.Size(15, 14);
            this.cbValue.TabIndex = 24;
            this.cbValue.UseVisualStyleBackColor = true;
            this.cbValue.CheckedChanged += new System.EventHandler(this.cbValue_CheckedChanged);
            // 
            // ArgumentValueBoolUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbValue);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ArgumentValueBoolUI";
            this.Size = new System.Drawing.Size(638, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbValue;

    }
}
