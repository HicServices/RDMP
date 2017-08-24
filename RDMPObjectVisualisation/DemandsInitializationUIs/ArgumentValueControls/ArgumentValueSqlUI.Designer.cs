namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    partial class ArgumentValueSqlUI
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
            this.btnSetSQL = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.SuspendLayout();
            // 
            // btnSetSQL
            // 
            this.btnSetSQL.Location = new System.Drawing.Point(3, 3);
            this.btnSetSQL.Name = "btnSetSQL";
            this.btnSetSQL.Size = new System.Drawing.Size(75, 23);
            this.btnSetSQL.TabIndex = 21;
            this.btnSetSQL.Text = "SetSQL";
            this.btnSetSQL.UseVisualStyleBackColor = true;
            this.btnSetSQL.Click += new System.EventHandler(this.btnSetSQL_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Location = new System.Drawing.Point(84, 0);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 22;
            // 
            // ArgumentValueSqlUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnSetSQL);
            this.Name = "ArgumentValueSqlUI";
            this.Size = new System.Drawing.Size(547, 28);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSetSQL;
        private ReusableUIComponents.RAGSmiley ragSmiley1;


    }
}
