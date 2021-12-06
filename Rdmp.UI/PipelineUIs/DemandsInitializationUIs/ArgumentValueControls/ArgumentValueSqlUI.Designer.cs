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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSetSQL = new System.Windows.Forms.Button();
            this.tbSql = new System.Windows.Forms.TextBox();
            this.lblSqlClause = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnSetSQL, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbSql, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSqlClause, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 0, 5, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(315, 27);
            this.tableLayoutPanel1.TabIndex = 25;
            // 
            // btnSetSQL
            // 
            this.btnSetSQL.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSetSQL.Location = new System.Drawing.Point(0, 1);
            this.btnSetSQL.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.btnSetSQL.Name = "btnSetSQL";
            this.btnSetSQL.Size = new System.Drawing.Size(86, 25);
            this.btnSetSQL.TabIndex = 23;
            this.btnSetSQL.Text = "Set SQL...";
            this.btnSetSQL.UseVisualStyleBackColor = true;
            this.btnSetSQL.Click += new System.EventHandler(this.btnSetSQL_Click);
            // 
            // tbSql
            // 
            this.tbSql.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSql.Enabled = false;
            this.tbSql.Location = new System.Drawing.Point(160, 2);
            this.tbSql.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.tbSql.Name = "tbSql";
            this.tbSql.Size = new System.Drawing.Size(155, 23);
            this.tbSql.TabIndex = 27;
            // 
            // lblSqlClause
            // 
            this.lblSqlClause.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSqlClause.AutoSize = true;
            this.lblSqlClause.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSqlClause.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblSqlClause.Location = new System.Drawing.Point(87, 6);
            this.lblSqlClause.Margin = new System.Windows.Forms.Padding(0);
            this.lblSqlClause.Name = "lblSqlClause";
            this.lblSqlClause.Size = new System.Drawing.Size(72, 15);
            this.lblSqlClause.TabIndex = 24;
            this.lblSqlClause.Text = "lblSqlClause";
            // 
            // ArgumentValueSqlUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ArgumentValueSqlUI";
            this.Size = new System.Drawing.Size(323, 32);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnSetSQL;
        private System.Windows.Forms.Label lblSqlClause;
        private System.Windows.Forms.TextBox tbSql;
    }
}
