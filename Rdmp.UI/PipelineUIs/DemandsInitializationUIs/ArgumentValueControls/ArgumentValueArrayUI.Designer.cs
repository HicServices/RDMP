namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueArrayUI
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
            this.btnPickDatabaseEntities = new System.Windows.Forms.Button();
            this.tbArray = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnPickDatabaseEntities
            // 
            this.btnPickDatabaseEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickDatabaseEntities.Location = new System.Drawing.Point(582, 2);
            this.btnPickDatabaseEntities.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPickDatabaseEntities.Name = "btnPickDatabaseEntities";
            this.btnPickDatabaseEntities.Size = new System.Drawing.Size(52, 25);
            this.btnPickDatabaseEntities.TabIndex = 0;
            this.btnPickDatabaseEntities.Text = "Pick...";
            this.btnPickDatabaseEntities.UseVisualStyleBackColor = true;
            this.btnPickDatabaseEntities.Click += new System.EventHandler(this.btnPickDatabaseEntities_Click);
            // 
            // tbArray
            // 
            this.tbArray.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbArray.Location = new System.Drawing.Point(4, 3);
            this.tbArray.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbArray.Name = "tbArray";
            this.tbArray.Size = new System.Drawing.Size(571, 23);
            this.tbArray.TabIndex = 2;
            // 
            // ArgumentValueArrayUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbArray);
            this.Controls.Add(this.btnPickDatabaseEntities);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ArgumentValueArrayUI";
            this.Size = new System.Drawing.Size(638, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPickDatabaseEntities;
        private System.Windows.Forms.TextBox tbArray;


    }
}
