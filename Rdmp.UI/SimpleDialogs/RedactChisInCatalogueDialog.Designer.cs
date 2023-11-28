namespace Rdmp.UI.SimpleDialogs
{
    partial class RedactChisInCatalogueDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnExecute = new System.Windows.Forms.Button();
            dgResults = new System.Windows.Forms.DataGridView();
            cbDoRedaction = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            tbAllowList = new System.Windows.Forms.TextBox();
            lbResults = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)dgResults).BeginInit();
            SuspendLayout();
            // 
            // btnExecute
            // 
            btnExecute.Location = new System.Drawing.Point(12, 9);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new System.Drawing.Size(75, 23);
            btnExecute.TabIndex = 1;
            btnExecute.Text = "Find CHIs";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += FindCHIs;
            // 
            // dgResults
            // 
            dgResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgResults.Location = new System.Drawing.Point(12, 84);
            dgResults.Name = "dgResults";
            dgResults.RowTemplate.Height = 25;
            dgResults.Size = new System.Drawing.Size(759, 376);
            dgResults.TabIndex = 2;
            // 
            // cbDoRedaction
            // 
            cbDoRedaction.AutoSize = true;
            cbDoRedaction.Location = new System.Drawing.Point(106, 13);
            cbDoRedaction.Name = "cbDoRedaction";
            cbDoRedaction.Size = new System.Drawing.Size(97, 19);
            cbDoRedaction.TabIndex = 3;
            cbDoRedaction.Text = "Do Redaction";
            cbDoRedaction.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(200, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 15);
            label1.TabIndex = 4;
            label1.Text = "Allow List Location:";
            // 
            // tbAllowList
            // 
            tbAllowList.Location = new System.Drawing.Point(316, 9);
            tbAllowList.Name = "tbAllowList";
            tbAllowList.Size = new System.Drawing.Size(260, 23);
            tbAllowList.TabIndex = 5;
            // 
            // lbResults
            // 
            lbResults.AutoSize = true;
            lbResults.Location = new System.Drawing.Point(325, 52);
            lbResults.Name = "lbResults";
            lbResults.Size = new System.Drawing.Size(0, 15);
            lbResults.TabIndex = 6;
            // 
            // RedactChisInCatalogueDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(lbResults);
            Controls.Add(tbAllowList);
            Controls.Add(label1);
            Controls.Add(cbDoRedaction);
            Controls.Add(dgResults);
            Controls.Add(btnExecute);
            Name = "RedactChisInCatalogueDialog";
            Text = "RedactChisInCatalogueDialog";
            ((System.ComponentModel.ISupportInitialize)dgResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.DataGridView dgResults;
        private System.Windows.Forms.CheckBox cbDoRedaction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbAllowList;
        private System.Windows.Forms.Label lbResults;
    }
}