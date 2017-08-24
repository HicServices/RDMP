namespace DatasetLoaderUI
{
    partial class CacheRebuilder
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.btnRemoveFiles = new System.Windows.Forms.Button();
            this.btnStartRebuild = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ddRebuilderClass = new System.Windows.Forms.ComboBox();
            this.progressUI = new ReusableUIComponents.Progress.ProgressUI();
            this.archiveFilePickerDialog = new System.Windows.Forms.OpenFileDialog();
            this.lbArchiveFileList = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbDestinationPath = new System.Windows.Forms.TextBox();
            this.btnStopRebuild = new System.Windows.Forms.Button();
            this.btnShowDestinationPath = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Archive files to process:";
            // 
            // btnAddFiles
            // 
            this.btnAddFiles.Location = new System.Drawing.Point(14, 453);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(134, 23);
            this.btnAddFiles.TabIndex = 2;
            this.btnAddFiles.Text = "Add Files...";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            // 
            // btnRemoveFiles
            // 
            this.btnRemoveFiles.Location = new System.Drawing.Point(154, 453);
            this.btnRemoveFiles.Name = "btnRemoveFiles";
            this.btnRemoveFiles.Size = new System.Drawing.Size(134, 23);
            this.btnRemoveFiles.TabIndex = 3;
            this.btnRemoveFiles.Text = "Remove Files...";
            this.btnRemoveFiles.UseVisualStyleBackColor = true;
            // 
            // btnStartRebuild
            // 
            this.btnStartRebuild.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnStartRebuild.Location = new System.Drawing.Point(235, 519);
            this.btnStartRebuild.Name = "btnStartRebuild";
            this.btnStartRebuild.Size = new System.Drawing.Size(134, 23);
            this.btnStartRebuild.TabIndex = 4;
            this.btnStartRebuild.Text = "Start Rebuild";
            this.btnStartRebuild.UseVisualStyleBackColor = true;
            this.btnStartRebuild.Click += new System.EventHandler(this.btnStartRebuild_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Rebuilder class:";
            // 
            // ddRebuilderClass
            // 
            this.ddRebuilderClass.FormattingEnabled = true;
            this.ddRebuilderClass.Location = new System.Drawing.Point(106, 17);
            this.ddRebuilderClass.Name = "ddRebuilderClass";
            this.ddRebuilderClass.Size = new System.Drawing.Size(406, 21);
            this.ddRebuilderClass.TabIndex = 6;
            // 
            // progressUI
            // 
            this.progressUI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI.Location = new System.Drawing.Point(347, 100);
            this.progressUI.Name = "progressUI";
            this.progressUI.Size = new System.Drawing.Size(429, 384);
            this.progressUI.TabIndex = 7;
            // 
            // archiveFilePickerDialog
            // 
            this.archiveFilePickerDialog.Multiselect = true;
            this.archiveFilePickerDialog.Title = "Select archive files to process...";
            // 
            // lbArchiveFileList
            // 
            this.lbArchiveFileList.FormattingEnabled = true;
            this.lbArchiveFileList.Location = new System.Drawing.Point(15, 100);
            this.lbArchiveFileList.Name = "lbArchiveFileList";
            this.lbArchiveFileList.Size = new System.Drawing.Size(326, 342);
            this.lbArchiveFileList.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Destination Path:";
            // 
            // tbDestinationPath
            // 
            this.tbDestinationPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.tbDestinationPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.tbDestinationPath.Location = new System.Drawing.Point(106, 44);
            this.tbDestinationPath.Name = "tbDestinationPath";
            this.tbDestinationPath.Size = new System.Drawing.Size(405, 20);
            this.tbDestinationPath.TabIndex = 9;
            // 
            // btnStopRebuild
            // 
            this.btnStopRebuild.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnStopRebuild.Enabled = false;
            this.btnStopRebuild.Location = new System.Drawing.Point(415, 519);
            this.btnStopRebuild.Name = "btnStopRebuild";
            this.btnStopRebuild.Size = new System.Drawing.Size(134, 23);
            this.btnStopRebuild.TabIndex = 10;
            this.btnStopRebuild.Text = "Stop Rebuild";
            this.btnStopRebuild.UseVisualStyleBackColor = true;
            this.btnStopRebuild.Click += new System.EventHandler(this.btnStopRebuild_Click);
            // 
            // btnShowDestinationPath
            // 
            this.btnShowDestinationPath.Location = new System.Drawing.Point(517, 42);
            this.btnShowDestinationPath.Name = "btnShowDestinationPath";
            this.btnShowDestinationPath.Size = new System.Drawing.Size(126, 23);
            this.btnShowDestinationPath.TabIndex = 11;
            this.btnShowDestinationPath.Text = "Show Directory...";
            this.btnShowDestinationPath.UseVisualStyleBackColor = true;
            this.btnShowDestinationPath.Click += new System.EventHandler(this.btnShowDestinationPath_Click);
            // 
            // CacheRebuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 554);
            this.Controls.Add(this.btnShowDestinationPath);
            this.Controls.Add(this.btnStopRebuild);
            this.Controls.Add(this.tbDestinationPath);
            this.Controls.Add(this.lbArchiveFileList);
            this.Controls.Add(this.progressUI);
            this.Controls.Add(this.ddRebuilderClass);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStartRebuild);
            this.Controls.Add(this.btnRemoveFiles);
            this.Controls.Add(this.btnAddFiles);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(804, 532);
            this.Name = "CacheRebuilder";
            this.Text = "Rebuild cache from archive";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnRemoveFiles;
        private System.Windows.Forms.Button btnStartRebuild;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ddRebuilderClass;
        private ReusableUIComponents.Progress.ProgressUI progressUI;
        private System.Windows.Forms.OpenFileDialog archiveFilePickerDialog;
        private System.Windows.Forms.ListBox lbArchiveFileList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDestinationPath;
        private System.Windows.Forms.Button btnStopRebuild;
        private System.Windows.Forms.Button btnShowDestinationPath;
    }
}