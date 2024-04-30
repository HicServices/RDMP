namespace Rdmp.UI.SimpleDialogs
{
    partial class ViewRedactedCHIsInCatalogueDialog
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
            btnSearch = new System.Windows.Forms.Button();
            dtResults = new System.Windows.Forms.DataGridView();
            lblLoading = new System.Windows.Forms.Label();
            btmRevertAll = new System.Windows.Forms.Button();
            btnConfirmAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dtResults).BeginInit();
            SuspendLayout();
            // 
            // btnSearch
            // 
            btnSearch.Location = new System.Drawing.Point(12, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(75, 23);
            btnSearch.TabIndex = 0;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += SearchButtonClick;
            // 
            // dtResults
            // 
            dtResults.AllowUserToAddRows = false;
            dtResults.AllowUserToDeleteRows = false;
            dtResults.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            dtResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtResults.Location = new System.Drawing.Point(12, 54);
            dtResults.Name = "dtResults";
            dtResults.ReadOnly = true;
            dtResults.RowTemplate.Height = 25;
            dtResults.Size = new System.Drawing.Size(776, 384);
            dtResults.TabIndex = 1;
            // 
            // lblLoading
            // 
            lblLoading.AutoSize = true;
            lblLoading.Location = new System.Drawing.Point(405, 182);
            lblLoading.Name = "lblLoading";
            lblLoading.Size = new System.Drawing.Size(59, 15);
            lblLoading.TabIndex = 2;
            lblLoading.Text = "Loading...";
            // 
            // btmRevertAll
            // 
            btmRevertAll.Location = new System.Drawing.Point(93, 12);
            btmRevertAll.Name = "btmRevertAll";
            btmRevertAll.Size = new System.Drawing.Size(75, 23);
            btmRevertAll.TabIndex = 3;
            btmRevertAll.Text = "Revert All";
            btmRevertAll.UseVisualStyleBackColor = true;
            btmRevertAll.Click += RevertAll;
            // 
            // btnConfirmAll
            // 
            btnConfirmAll.Location = new System.Drawing.Point(174, 12);
            btnConfirmAll.Name = "btnConfirmAll";
            btnConfirmAll.Size = new System.Drawing.Size(91, 23);
            btnConfirmAll.TabIndex = 4;
            btnConfirmAll.Text = "Confirm All";
            btnConfirmAll.UseVisualStyleBackColor = true;
            btnConfirmAll.Click += ConfirmAll;
            // 
            // ViewRedactedCHIsInCatalogueDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(btnConfirmAll);
            Controls.Add(btmRevertAll);
            Controls.Add(lblLoading);
            Controls.Add(dtResults);
            Controls.Add(btnSearch);
            Name = "ViewRedactedCHIsInCatalogueDialog";
            Text = "Redacted CHIs in Catalogue";
            ((System.ComponentModel.ISupportInitialize)dtResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridView dtResults;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.Button btmRevertAll;
        private System.Windows.Forms.Button btnConfirmAll;
    }
}