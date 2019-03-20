namespace CatalogueManager.PluginManagement.CodeGeneration
{
    partial class GenerateClassCodeFromTableUI
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
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnGenerateCode = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(6, 19);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(581, 181);
            this.serverDatabaseTableSelector1.TabIndex = 1;
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGenerateCode);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.serverDatabaseTableSelector1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(916, 704);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Generate Class Code From Database Table";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(6, 235);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(907, 463);
            this.panel1.TabIndex = 2;
            // 
            // btnGenerateCode
            // 
            this.btnGenerateCode.Location = new System.Drawing.Point(6, 206);
            this.btnGenerateCode.Name = "btnGenerateCode";
            this.btnGenerateCode.Size = new System.Drawing.Size(114, 23);
            this.btnGenerateCode.TabIndex = 3;
            this.btnGenerateCode.Text = "Generate Code";
            this.btnGenerateCode.UseVisualStyleBackColor = true;
            this.btnGenerateCode.Click += new System.EventHandler(this.btnGenerateCode_Click);
            // 
            // GenerateClassCodeFromTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "GenerateClassCodeFromTable";
            this.Size = new System.Drawing.Size(916, 704);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnGenerateCode;
    }
}
