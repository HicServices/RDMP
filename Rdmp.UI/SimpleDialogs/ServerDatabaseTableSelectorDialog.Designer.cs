using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.SimpleDialogs
{
    partial class ServerDatabaseTableSelectorDialog
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
            this.serverDatabaseTableSelector1 = new ServerDatabaseTableSelector();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTaskDescription = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(3, 3);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(581, 181);
            this.serverDatabaseTableSelector1.TabIndex = 0;
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(574, 85);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 1;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnOk.Location = new System.Drawing.Point(3, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(84, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lblTaskDescription);
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(684, 281);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // lblTaskDescription
            // 
            this.lblTaskDescription.AutoSize = true;
            this.lblTaskDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTaskDescription.Location = new System.Drawing.Point(3, 0);
            this.lblTaskDescription.Name = "lblTaskDescription";
            this.lblTaskDescription.Size = new System.Drawing.Size(658, 13);
            this.lblTaskDescription.TabIndex = 5;
            this.lblTaskDescription.Text = "Task Description";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCreate);
            this.panel1.Controls.Add(this.serverDatabaseTableSelector1);
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(658, 191);
            this.panel1.TabIndex = 4;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.btnOk);
            this.flowLayoutPanel2.Controls.Add(this.btnCancel);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 213);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(658, 37);
            this.flowLayoutPanel2.TabIndex = 3;
            // 
            // ServerDatabaseTableSelectorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 281);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "ServerDatabaseTableSelectorDialog";
            this.Text = "Select Server";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label lblTaskDescription;
    }
}