using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;

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
            rbCreateNew = new System.Windows.Forms.RadioButton();
            rbUseExisting = new System.Windows.Forms.RadioButton();
            tbCreateNew = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbUseExisting = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ragSmiley1 = new RAGSmiley();
            btnBrowseForExisting = new System.Windows.Forms.Button();
            btnCreateNewBrowse = new System.Windows.Forms.Button();
            lblDataIsReservedWordExisting = new System.Windows.Forms.Label();
            lblDataIsReservedWordNew = new System.Windows.Forms.Label();
            helpIcon1 = new HelpIcon();
            rbChooseYourOwn = new System.Windows.Forms.RadioButton();
            btnBrowseForLoading = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            tbForLoadingPath = new System.Windows.Forms.TextBox();
            btnBrowseForArchiving = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            tbForArchivingPath = new System.Windows.Forms.TextBox();
            btnBrowseExecutables = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            tbExecutablesPath = new System.Windows.Forms.TextBox();
            btnBrowseCache = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            tbCachePath = new System.Windows.Forms.TextBox();
            lblForLoadingError = new System.Windows.Forms.Label();
            lblExecutablesError = new System.Windows.Forms.Label();
            lblForArchivingError = new System.Windows.Forms.Label();
            lblCacheError = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // rbCreateNew
            // 
            rbCreateNew.AutoSize = true;
            rbCreateNew.Location = new System.Drawing.Point(14, 14);
            rbCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbCreateNew.Name = "rbCreateNew";
            rbCreateNew.Size = new System.Drawing.Size(363, 19);
            rbCreateNew.TabIndex = 0;
            rbCreateNew.TabStop = true;
            rbCreateNew.Text = "Create New (new folder with all required folders created for you)";
            rbCreateNew.UseVisualStyleBackColor = true;
            rbCreateNew.CheckedChanged += rb_CheckedChanged;
            // 
            // rbUseExisting
            // 
            rbUseExisting.AutoSize = true;
            rbUseExisting.Location = new System.Drawing.Point(14, 90);
            rbUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbUseExisting.Name = "rbUseExisting";
            rbUseExisting.Size = new System.Drawing.Size(284, 19);
            rbUseExisting.TabIndex = 0;
            rbUseExisting.TabStop = true;
            rbUseExisting.Text = "Use Existing Directory (must have correct folders)";
            rbUseExisting.UseVisualStyleBackColor = true;
            rbUseExisting.CheckedChanged += rb_CheckedChanged;
            // 
            // tbCreateNew
            // 
            tbCreateNew.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbCreateNew.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbCreateNew.Location = new System.Drawing.Point(88, 40);
            tbCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbCreateNew.Name = "tbCreateNew";
            tbCreateNew.Size = new System.Drawing.Size(734, 23);
            tbCreateNew.TabIndex = 1;
            tbCreateNew.TextChanged += tbCreateNew_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(43, 44);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(34, 15);
            label1.TabIndex = 2;
            label1.Text = "Path:";
            // 
            // tbUseExisting
            // 
            tbUseExisting.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbUseExisting.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbUseExisting.Location = new System.Drawing.Point(88, 117);
            tbUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbUseExisting.Name = "tbUseExisting";
            tbUseExisting.Size = new System.Drawing.Size(734, 23);
            tbUseExisting.TabIndex = 1;
            tbUseExisting.TextChanged += tbUseExisting_TextChanged;
            tbUseExisting.Leave += tbUseExisting_Leave;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(43, 120);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(34, 15);
            label2.TabIndex = 2;
            label2.Text = "Path:";
            // 
            // btnOk
            // 
            btnOk.Enabled = false;
            btnOk.Location = new System.Drawing.Point(380, 369);
            btnOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(88, 27);
            btnOk.TabIndex = 3;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(476, 369);
            btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 27);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // ragSmiley1
            // 
            ragSmiley1.AlwaysShowHandCursor = false;
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(924, 117);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(23, 23);
            ragSmiley1.TabIndex = 4;
            ragSmiley1.Visible = false;
            // 
            // btnBrowseForExisting
            // 
            btnBrowseForExisting.Location = new System.Drawing.Point(830, 114);
            btnBrowseForExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseForExisting.Name = "btnBrowseForExisting";
            btnBrowseForExisting.Size = new System.Drawing.Size(88, 27);
            btnBrowseForExisting.TabIndex = 5;
            btnBrowseForExisting.Text = "Browse...";
            btnBrowseForExisting.UseVisualStyleBackColor = true;
            btnBrowseForExisting.Click += btnBrowseForExisting_Click;
            // 
            // btnCreateNewBrowse
            // 
            btnCreateNewBrowse.Location = new System.Drawing.Point(830, 38);
            btnCreateNewBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCreateNewBrowse.Name = "btnCreateNewBrowse";
            btnCreateNewBrowse.Size = new System.Drawing.Size(88, 27);
            btnCreateNewBrowse.TabIndex = 5;
            btnCreateNewBrowse.Text = "Browse...";
            btnCreateNewBrowse.UseVisualStyleBackColor = true;
            btnCreateNewBrowse.Click += btnCreateNewBrowse_Click;
            // 
            // lblDataIsReservedWordExisting
            // 
            lblDataIsReservedWordExisting.AutoSize = true;
            lblDataIsReservedWordExisting.ForeColor = System.Drawing.Color.Red;
            lblDataIsReservedWordExisting.Location = new System.Drawing.Point(84, 143);
            lblDataIsReservedWordExisting.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblDataIsReservedWordExisting.Name = "lblDataIsReservedWordExisting";
            lblDataIsReservedWordExisting.Size = new System.Drawing.Size(228, 15);
            lblDataIsReservedWordExisting.TabIndex = 6;
            lblDataIsReservedWordExisting.Text = "Project directories cannot end with \"Data\"";
            lblDataIsReservedWordExisting.Visible = false;
            // 
            // lblDataIsReservedWordNew
            // 
            lblDataIsReservedWordNew.AutoSize = true;
            lblDataIsReservedWordNew.ForeColor = System.Drawing.Color.Red;
            lblDataIsReservedWordNew.Location = new System.Drawing.Point(84, 67);
            lblDataIsReservedWordNew.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblDataIsReservedWordNew.Name = "lblDataIsReservedWordNew";
            lblDataIsReservedWordNew.Size = new System.Drawing.Size(228, 15);
            lblDataIsReservedWordNew.TabIndex = 6;
            lblDataIsReservedWordNew.Text = "Project directories cannot end with \"Data\"";
            lblDataIsReservedWordNew.Visible = false;
            // 
            // helpIcon1
            // 
            helpIcon1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            helpIcon1.BackColor = System.Drawing.Color.Transparent;
            helpIcon1.BackgroundImage = (System.Drawing.Image)resources.GetObject("helpIcon1.BackgroundImage");
            helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            helpIcon1.Location = new System.Drawing.Point(950, 2);
            helpIcon1.Margin = new System.Windows.Forms.Padding(0);
            helpIcon1.MinimumSize = new System.Drawing.Size(26, 25);
            helpIcon1.Name = "helpIcon1";
            helpIcon1.Size = new System.Drawing.Size(26, 25);
            helpIcon1.SuppressClick = false;
            helpIcon1.TabIndex = 7;
            // 
            // rbChooseYourOwn
            // 
            rbChooseYourOwn.AutoSize = true;
            rbChooseYourOwn.Location = new System.Drawing.Point(13, 161);
            rbChooseYourOwn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rbChooseYourOwn.Name = "rbChooseYourOwn";
            rbChooseYourOwn.Size = new System.Drawing.Size(311, 19);
            rbChooseYourOwn.TabIndex = 8;
            rbChooseYourOwn.TabStop = true;
            rbChooseYourOwn.Text = "Use Different Locations For Each Subfolder (advanced)";
            rbChooseYourOwn.UseVisualStyleBackColor = true;
            // 
            // btnBrowseForLoading
            // 
            btnBrowseForLoading.Location = new System.Drawing.Point(830, 184);
            btnBrowseForLoading.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseForLoading.Name = "btnBrowseForLoading";
            btnBrowseForLoading.Size = new System.Drawing.Size(88, 27);
            btnBrowseForLoading.TabIndex = 11;
            btnBrowseForLoading.Text = "Browse...";
            btnBrowseForLoading.UseVisualStyleBackColor = true;
            btnBrowseForLoading.Click += btnBrowseForLoading_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(14, 190);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(97, 15);
            label3.TabIndex = 10;
            label3.Text = "ForLoading Path:";
            label3.Click += label3_Click;
            // 
            // tbForLoadingPath
            // 
            tbForLoadingPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbForLoadingPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbForLoadingPath.Location = new System.Drawing.Point(113, 186);
            tbForLoadingPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbForLoadingPath.Name = "tbForLoadingPath";
            tbForLoadingPath.Size = new System.Drawing.Size(709, 23);
            tbForLoadingPath.TabIndex = 9;
            tbForLoadingPath.TextChanged += tbForLoadingPath_TextChanged;
            // 
            // btnBrowseForArchiving
            // 
            btnBrowseForArchiving.Location = new System.Drawing.Point(830, 231);
            btnBrowseForArchiving.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseForArchiving.Name = "btnBrowseForArchiving";
            btnBrowseForArchiving.Size = new System.Drawing.Size(88, 27);
            btnBrowseForArchiving.TabIndex = 14;
            btnBrowseForArchiving.Text = "Browse...";
            btnBrowseForArchiving.UseVisualStyleBackColor = true;
            btnBrowseForArchiving.Click += btnBrowseForArchiving_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 237);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(105, 15);
            label4.TabIndex = 13;
            label4.Text = "ForArchiving Path:";
            // 
            // tbForArchivingPath
            // 
            tbForArchivingPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbForArchivingPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbForArchivingPath.Location = new System.Drawing.Point(113, 233);
            tbForArchivingPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbForArchivingPath.Name = "tbForArchivingPath";
            tbForArchivingPath.Size = new System.Drawing.Size(709, 23);
            tbForArchivingPath.TabIndex = 12;
            tbForArchivingPath.TextChanged += tbForArchivingPath_TextChanged;
            // 
            // btnBrowseExecutables
            // 
            btnBrowseExecutables.Location = new System.Drawing.Point(830, 276);
            btnBrowseExecutables.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseExecutables.Name = "btnBrowseExecutables";
            btnBrowseExecutables.Size = new System.Drawing.Size(88, 27);
            btnBrowseExecutables.TabIndex = 17;
            btnBrowseExecutables.Text = "Browse...";
            btnBrowseExecutables.UseVisualStyleBackColor = true;
            btnBrowseExecutables.Click += btnBrowseForExecutables_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(14, 282);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(99, 15);
            label5.TabIndex = 16;
            label5.Text = "Executables Path:";
            // 
            // tbExecutablesPath
            // 
            tbExecutablesPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbExecutablesPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbExecutablesPath.Location = new System.Drawing.Point(113, 278);
            tbExecutablesPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbExecutablesPath.Name = "tbExecutablesPath";
            tbExecutablesPath.Size = new System.Drawing.Size(709, 23);
            tbExecutablesPath.TabIndex = 15;
            tbExecutablesPath.TextChanged += tbExecutablesPath_TextChanged;
            // 
            // btnBrowseCache
            // 
            btnBrowseCache.Location = new System.Drawing.Point(830, 322);
            btnBrowseCache.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseCache.Name = "btnBrowseCache";
            btnBrowseCache.Size = new System.Drawing.Size(88, 27);
            btnBrowseCache.TabIndex = 20;
            btnBrowseCache.Text = "Browse...";
            btnBrowseCache.UseVisualStyleBackColor = true;
            btnBrowseCache.Click += btnBrowseForCache_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(41, 328);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(70, 15);
            label6.TabIndex = 19;
            label6.Text = "Cache Path:";
            // 
            // tbCachePath
            // 
            tbCachePath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbCachePath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbCachePath.Location = new System.Drawing.Point(113, 324);
            tbCachePath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbCachePath.Name = "tbCachePath";
            tbCachePath.Size = new System.Drawing.Size(709, 23);
            tbCachePath.TabIndex = 18;
            tbCachePath.TextChanged += tbCachePath_TextChanged;
            // 
            // lblForLoadingError
            // 
            lblForLoadingError.AutoSize = true;
            lblForLoadingError.ForeColor = System.Drawing.Color.Red;
            lblForLoadingError.Location = new System.Drawing.Point(113, 212);
            lblForLoadingError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblForLoadingError.Name = "lblForLoadingError";
            lblForLoadingError.Size = new System.Drawing.Size(187, 15);
            lblForLoadingError.TabIndex = 21;
            lblForLoadingError.Text = "ForLoading Path cannot be empty";
            lblForLoadingError.Visible = false;
            lblForLoadingError.Click += label7_Click;
            // 
            // lblExecutablesError
            // 
            lblExecutablesError.AutoSize = true;
            lblExecutablesError.ForeColor = System.Drawing.Color.Red;
            lblExecutablesError.Location = new System.Drawing.Point(113, 306);
            lblExecutablesError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblExecutablesError.Name = "lblExecutablesError";
            lblExecutablesError.Size = new System.Drawing.Size(189, 15);
            lblExecutablesError.TabIndex = 22;
            lblExecutablesError.Text = "Executables Path cannot be empty";
            lblExecutablesError.Visible = false;
            // 
            // lblForArchivingError
            // 
            lblForArchivingError.AutoSize = true;
            lblForArchivingError.ForeColor = System.Drawing.Color.Red;
            lblForArchivingError.Location = new System.Drawing.Point(113, 260);
            lblForArchivingError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblForArchivingError.Name = "lblForArchivingError";
            lblForArchivingError.Size = new System.Drawing.Size(195, 15);
            lblForArchivingError.TabIndex = 23;
            lblForArchivingError.Text = "ForArchiving Path cannot be empty";
            lblForArchivingError.Visible = false;
            lblForArchivingError.Click += label8_Click;
            // 
            // lblCacheError
            // 
            lblCacheError.AutoSize = true;
            lblCacheError.ForeColor = System.Drawing.Color.Red;
            lblCacheError.Location = new System.Drawing.Point(113, 350);
            lblCacheError.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCacheError.Name = "lblCacheError";
            lblCacheError.Size = new System.Drawing.Size(160, 15);
            lblCacheError.TabIndex = 24;
            lblCacheError.Text = "Cache Path cannot be empty";
            lblCacheError.UseMnemonic = false;
            lblCacheError.Visible = false;
            // 
            // ChooseLoadDirectoryUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(979, 421);
            Controls.Add(lblCacheError);
            Controls.Add(lblForArchivingError);
            Controls.Add(lblExecutablesError);
            Controls.Add(lblForLoadingError);
            Controls.Add(btnBrowseCache);
            Controls.Add(label6);
            Controls.Add(tbCachePath);
            Controls.Add(btnBrowseExecutables);
            Controls.Add(label5);
            Controls.Add(tbExecutablesPath);
            Controls.Add(btnBrowseForArchiving);
            Controls.Add(label4);
            Controls.Add(tbForArchivingPath);
            Controls.Add(btnBrowseForLoading);
            Controls.Add(label3);
            Controls.Add(tbForLoadingPath);
            Controls.Add(rbChooseYourOwn);
            Controls.Add(helpIcon1);
            Controls.Add(lblDataIsReservedWordNew);
            Controls.Add(lblDataIsReservedWordExisting);
            Controls.Add(btnCreateNewBrowse);
            Controls.Add(btnBrowseForExisting);
            Controls.Add(ragSmiley1);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbUseExisting);
            Controls.Add(tbCreateNew);
            Controls.Add(rbUseExisting);
            Controls.Add(rbCreateNew);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ChooseLoadDirectoryUI";
            Text = "Load Directory";
            ResumeLayout(false);
            PerformLayout();
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
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnBrowseForExisting;
        private System.Windows.Forms.Button btnCreateNewBrowse;
        private System.Windows.Forms.Label lblDataIsReservedWordExisting;
        private System.Windows.Forms.Label lblDataIsReservedWordNew;
        private HelpIcon helpIcon1;
        private System.Windows.Forms.RadioButton rbChooseYourOwn;
        private System.Windows.Forms.Button btnBrowseForLoading;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbForLoadingPath;
        private System.Windows.Forms.Button btnBrowseForArchiving;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbForArchivingPath;
        private System.Windows.Forms.Button btnBrowseExecutables;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbExecutablesPath;
        private System.Windows.Forms.Button btnBrowseCache;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbCachePath;
        private System.Windows.Forms.Label lblForLoadingError;
        private System.Windows.Forms.Label lblExecutablesError;
        private System.Windows.Forms.Label lblForArchivingError;
        private System.Windows.Forms.Label lblCacheError;
    }
}