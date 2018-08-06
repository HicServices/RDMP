namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueCustomUIDrivenClassUI
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
            this.btnLaunchCustomUI = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.SuspendLayout();
            // 
            // btnLaunchCustomUI
            // 
            this.btnLaunchCustomUI.Location = new System.Drawing.Point(3, 2);
            this.btnLaunchCustomUI.Name = "btnLaunchCustomUI";
            this.btnLaunchCustomUI.Size = new System.Drawing.Size(168, 23);
            this.btnLaunchCustomUI.TabIndex = 21;
            this.btnLaunchCustomUI.Text = "Launch Custom UI";
            this.btnLaunchCustomUI.UseVisualStyleBackColor = true;
            this.btnLaunchCustomUI.Click += new System.EventHandler(this.btnLaunchCustomUI_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(177, 0);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 22;
            // 
            // ArgumentValueCustomUIDrivenClassUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnLaunchCustomUI);
            this.Name = "ArgumentValueCustomUIDrivenClassUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLaunchCustomUI;
        private ReusableUIComponents.RAGSmiley ragSmiley1;



    }
}
