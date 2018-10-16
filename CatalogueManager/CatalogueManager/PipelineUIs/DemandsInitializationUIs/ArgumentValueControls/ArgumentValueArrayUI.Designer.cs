namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
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
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.tbArray = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnPickDatabaseEntities
            // 
            this.btnPickDatabaseEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickDatabaseEntities.Location = new System.Drawing.Point(448, 3);
            this.btnPickDatabaseEntities.Name = "btnPickDatabaseEntities";
            this.btnPickDatabaseEntities.Size = new System.Drawing.Size(65, 23);
            this.btnPickDatabaseEntities.TabIndex = 0;
            this.btnPickDatabaseEntities.Text = "Pick...";
            this.btnPickDatabaseEntities.UseVisualStyleBackColor = true;
            this.btnPickDatabaseEntities.Click += new System.EventHandler(this.btnPickDatabaseEntities_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(519, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 1;
            // 
            // tbArray
            // 
            this.tbArray.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbArray.Location = new System.Drawing.Point(3, 5);
            this.tbArray.Name = "tbArray";
            this.tbArray.Size = new System.Drawing.Size(439, 20);
            this.tbArray.TabIndex = 2;
            // 
            // ArgumentValueArrayUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbArray);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnPickDatabaseEntities);
            this.Name = "ArgumentValueArrayUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPickDatabaseEntities;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.TextBox tbArray;


    }
}
