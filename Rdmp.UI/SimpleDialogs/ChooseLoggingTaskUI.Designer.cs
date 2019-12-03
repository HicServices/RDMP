using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.SimpleDialogs
{
    partial class ChooseLoggingTaskUI
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnClearLive = new System.Windows.Forms.Button();
            this.btnCreateNewLoggingServer = new System.Windows.Forms.Button();
            this.ragSmiley1 = new RAGSmiley();
            this.ddLoggingServer = new System.Windows.Forms.ComboBox();
            this.btnCreateNewLoggingTask = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxDataLoadTasks = new System.Windows.Forms.ComboBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnClearLive);
            this.groupBox2.Controls.Add(this.btnCreateNewLoggingServer);
            this.groupBox2.Controls.Add(this.ragSmiley1);
            this.groupBox2.Controls.Add(this.ddLoggingServer);
            this.groupBox2.Controls.Add(this.btnCreateNewLoggingTask);
            this.groupBox2.Controls.Add(this.btnRefresh);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cbxDataLoadTasks);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1015, 80);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Logging Architecture";
            // 
            // btnClearLive
            // 
            this.btnClearLive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLive.Location = new System.Drawing.Point(786, 49);
            this.btnClearLive.Name = "btnClearLive";
            this.btnClearLive.Size = new System.Drawing.Size(52, 23);
            this.btnClearLive.TabIndex = 8;
            this.btnClearLive.Text = "Clear";
            this.btnClearLive.UseVisualStyleBackColor = true;
            this.btnClearLive.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCreateNewLoggingServer
            // 
            this.btnCreateNewLoggingServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewLoggingServer.Location = new System.Drawing.Point(841, 48);
            this.btnCreateNewLoggingServer.Name = "btnCreateNewLoggingServer";
            this.btnCreateNewLoggingServer.Size = new System.Drawing.Size(168, 23);
            this.btnCreateNewLoggingServer.TabIndex = 7;
            this.btnCreateNewLoggingServer.Text = "Create New Logging Server...";
            this.btnCreateNewLoggingServer.UseVisualStyleBackColor = true;
            this.btnCreateNewLoggingServer.Click += new System.EventHandler(this.btnCreateNewLoggingServer_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(962, 11);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(37, 38);
            this.ragSmiley1.TabIndex = 6;
            // 
            // ddLoggingServer
            // 
            this.ddLoggingServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddLoggingServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLoggingServer.FormattingEnabled = true;
            this.ddLoggingServer.Location = new System.Drawing.Point(118, 50);
            this.ddLoggingServer.Name = "ddLoggingServer";
            this.ddLoggingServer.Size = new System.Drawing.Size(664, 21);
            this.ddLoggingServer.TabIndex = 5;
            this.ddLoggingServer.SelectedIndexChanged += new System.EventHandler(this.ddLoggingServer_SelectedIndexChanged);
            // 
            // btnCreateNewLoggingTask
            // 
            this.btnCreateNewLoggingTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewLoggingTask.Location = new System.Drawing.Point(869, 21);
            this.btnCreateNewLoggingTask.Name = "btnCreateNewLoggingTask";
            this.btnCreateNewLoggingTask.Size = new System.Drawing.Size(75, 23);
            this.btnCreateNewLoggingTask.TabIndex = 4;
            this.btnCreateNewLoggingTask.Text = "Create";
            this.btnCreateNewLoggingTask.UseVisualStyleBackColor = true;
            this.btnCreateNewLoggingTask.Click += new System.EventHandler(this.btnCreateNewLoggingTask_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(788, 21);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Logging Server:";
            this.label2.Click += new System.EventHandler(this.label1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "DataLoadTask:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // cbxDataLoadTasks
            // 
            this.cbxDataLoadTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDataLoadTasks.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbxDataLoadTasks.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxDataLoadTasks.FormattingEnabled = true;
            this.cbxDataLoadTasks.Location = new System.Drawing.Point(94, 23);
            this.cbxDataLoadTasks.Name = "cbxDataLoadTasks";
            this.cbxDataLoadTasks.Size = new System.Drawing.Size(688, 21);
            this.cbxDataLoadTasks.TabIndex = 0;
            this.cbxDataLoadTasks.SelectedIndexChanged += new System.EventHandler(this.cbxDataLoadTasks_SelectedIndexChanged);
            this.cbxDataLoadTasks.TextChanged += new System.EventHandler(this.cbxDataLoadTasks_TextChanged);
            this.cbxDataLoadTasks.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbxDataLoadTasks_KeyUp);
            this.cbxDataLoadTasks.Leave += new System.EventHandler(this.cbxDataLoadTasks_Leave);
            // 
            // ChooseLoggingTaskUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Name = "ChooseLoggingTaskUI";
            this.Size = new System.Drawing.Size(1039, 99);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxDataLoadTasks;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox ddLoggingServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCreateNewLoggingTask;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnCreateNewLoggingServer;
        private System.Windows.Forms.Button btnClearLive;
    }
}