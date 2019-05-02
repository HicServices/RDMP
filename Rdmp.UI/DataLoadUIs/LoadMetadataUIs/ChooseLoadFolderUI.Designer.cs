namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs
{
    partial class ChooseLoadDirectoryUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseLoadDirectoryUI));
            this.rbCreateNew = new System.Windows.Forms.RadioButton();
            this.rbUseExisting = new System.Windows.Forms.RadioButton();
            this.tbCreateNew = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbUseExisting = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnBrowseForExisting = new System.Windows.Forms.Button();
            this.btnCreateNewBrowse = new System.Windows.Forms.Button();
            this.lblDataIsReservedWordExisting = new System.Windows.Forms.Label();
            this.lblDataIsReservedWordNew = new System.Windows.Forms.Label();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.SuspendLayout();
            // 
            // rbCreateNew
            // 
            this.rbCreateNew.AutoSize = true;
            this.rbCreateNew.Location = new System.Drawing.Point(12, 12);
            this.rbCreateNew.Name = "rbCreateNew";
            this.rbCreateNew.Size = new System.Drawing.Size(323, 17);
            this.rbCreateNew.TabIndex = 0;
            this.rbCreateNew.TabStop = true;
            this.rbCreateNew.Text = "Create New (new folder with all required folders created for you)";
            this.rbCreateNew.UseVisualStyleBackColor = true;
            this.rbCreateNew.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbUseExisting
            // 
            this.rbUseExisting.AutoSize = true;
            this.rbUseExisting.Location = new System.Drawing.Point(12, 78);
            this.rbUseExisting.Name = "rbUseExisting";
            this.rbUseExisting.Size = new System.Drawing.Size(256, 17);
            this.rbUseExisting.TabIndex = 0;
            this.rbUseExisting.TabStop = true;
            this.rbUseExisting.Text = "Use Existing Directory (must have correct folders)";
            this.rbUseExisting.UseVisualStyleBackColor = true;
            this.rbUseExisting.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // tbCreateNew
            // 
            this.tbCreateNew.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.tbCreateNew.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbCreateNew.Location = new System.Drawing.Point(75, 35);
            this.tbCreateNew.Name = "tbCreateNew";
            this.tbCreateNew.Size = new System.Drawing.Size(630, 20);
            this.tbCreateNew.TabIndex = 1;
            this.tbCreateNew.TextChanged += new System.EventHandler(this.tbCreateNew_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path:";
            // 
            // tbUseExisting
            // 
            this.tbUseExisting.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.tbUseExisting.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbUseExisting.Location = new System.Drawing.Point(75, 101);
            this.tbUseExisting.Name = "tbUseExisting";
            this.tbUseExisting.Size = new System.Drawing.Size(630, 20);
            this.tbUseExisting.TabIndex = 1;
            this.tbUseExisting.TextChanged += new System.EventHandler(this.tbUseExisting_TextChanged);
            this.tbUseExisting.Leave += new System.EventHandler(this.tbUseExisting_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Path:";
            // 
            // btnOk
            // 
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(333, 139);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(414, 139);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(792, 101);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(20, 20);
            this.ragSmiley1.TabIndex = 4;
            this.ragSmiley1.Visible = false;
            // 
            // btnBrowseForExisting
            // 
            this.btnBrowseForExisting.Location = new System.Drawing.Point(711, 99);
            this.btnBrowseForExisting.Name = "btnBrowseForExisting";
            this.btnBrowseForExisting.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseForExisting.TabIndex = 5;
            this.btnBrowseForExisting.Text = "Browse...";
            this.btnBrowseForExisting.UseVisualStyleBackColor = true;
            this.btnBrowseForExisting.Click += new System.EventHandler(this.btnBrowseForExisting_Click);
            // 
            // btnCreateNewBrowse
            // 
            this.btnCreateNewBrowse.Location = new System.Drawing.Point(711, 33);
            this.btnCreateNewBrowse.Name = "btnCreateNewBrowse";
            this.btnCreateNewBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnCreateNewBrowse.TabIndex = 5;
            this.btnCreateNewBrowse.Text = "Browse...";
            this.btnCreateNewBrowse.UseVisualStyleBackColor = true;
            this.btnCreateNewBrowse.Click += new System.EventHandler(this.btnCreateNewBrowse_Click);
            // 
            // lblDataIsReservedWordExisting
            // 
            this.lblDataIsReservedWordExisting.AutoSize = true;
            this.lblDataIsReservedWordExisting.ForeColor = System.Drawing.Color.Red;
            this.lblDataIsReservedWordExisting.Location = new System.Drawing.Point(72, 124);
            this.lblDataIsReservedWordExisting.Name = "lblDataIsReservedWordExisting";
            this.lblDataIsReservedWordExisting.Size = new System.Drawing.Size(206, 13);
            this.lblDataIsReservedWordExisting.TabIndex = 6;
            this.lblDataIsReservedWordExisting.Text = "Project directories cannot end with \"Data\"";
            this.lblDataIsReservedWordExisting.Visible = false;
            // 
            // lblDataIsReservedWordNew
            // 
            this.lblDataIsReservedWordNew.AutoSize = true;
            this.lblDataIsReservedWordNew.ForeColor = System.Drawing.Color.Red;
            this.lblDataIsReservedWordNew.Location = new System.Drawing.Point(72, 58);
            this.lblDataIsReservedWordNew.Name = "lblDataIsReservedWordNew";
            this.lblDataIsReservedWordNew.Size = new System.Drawing.Size(206, 13);
            this.lblDataIsReservedWordNew.TabIndex = 6;
            this.lblDataIsReservedWordNew.Text = "Project directories cannot end with \"Data\"";
            this.lblDataIsReservedWordNew.Visible = false;
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(806, 2);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 7;
            // 
            // ChooseLoadDirectoryUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 174);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.lblDataIsReservedWordNew);
            this.Controls.Add(this.lblDataIsReservedWordExisting);
            this.Controls.Add(this.btnCreateNewBrowse);
            this.Controls.Add(this.btnBrowseForExisting);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbUseExisting);
            this.Controls.Add(this.tbCreateNew);
            this.Controls.Add(this.rbUseExisting);
            this.Controls.Add(this.rbCreateNew);
            this.Name = "ChooseLoadDirectoryUI";
            this.Text = "Load Directory";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbCreateNew;
        private System.Windows.Forms.RadioButton rbUseExisting;
        private System.Windows.Forms.TextBox tbCreateNew;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbUseExisting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnBrowseForExisting;
        private System.Windows.Forms.Button btnCreateNewBrowse;
        private System.Windows.Forms.Label lblDataIsReservedWordExisting;
        private System.Windows.Forms.Label lblDataIsReservedWordNew;
        private ReusableUIComponents.HelpIcon helpIcon1;

    }
}