namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
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
            this.tbSql = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnSetSQL
            // 
            this.btnSetSQL.Location = new System.Drawing.Point(3, 3);
            this.btnSetSQL.Margin = new System.Windows.Forms.Padding(4, 3, 2, 3);
            this.btnSetSQL.Name = "btnSetSQL";
            this.btnSetSQL.Size = new System.Drawing.Size(88, 25);
            this.btnSetSQL.TabIndex = 21;
            this.btnSetSQL.Text = "Set SQL...";
            this.btnSetSQL.UseVisualStyleBackColor = true;
            this.btnSetSQL.Click += new System.EventHandler(this.btnSetSQL_Click);
            // 
            // tbSql
            // 
            this.tbSql.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSql.Enabled = false;
            this.tbSql.Location = new System.Drawing.Point(93, 4);
            this.tbSql.Margin = new System.Windows.Forms.Padding(0, 3, 4, 3);
            this.tbSql.Name = "tbSql";
            this.tbSql.Size = new System.Drawing.Size(541, 23);
            this.tbSql.TabIndex = 22;
            // 
            // ArgumentValueSqlUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbSql);
            this.Controls.Add(this.btnSetSQL);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ArgumentValueSqlUI";
            this.Size = new System.Drawing.Size(638, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetSQL;
        private System.Windows.Forms.TextBox tbSql;
    }
}
