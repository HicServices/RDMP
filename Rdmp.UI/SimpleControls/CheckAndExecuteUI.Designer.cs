using Rdmp.UI.ChecksUI;
using Rdmp.UI.Progress;

namespace Rdmp.UI.SimpleControls
{
    partial class CheckAndExecuteUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckAndExecuteUI));
            this.gbControls = new System.Windows.Forms.GroupBox();
            this.btnAbortLoad = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.ragChecks = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.gbLoad = new System.Windows.Forms.GroupBox();
            this.checksUI1 = new Rdmp.UI.ChecksUI.ChecksUI();
            this.loadProgressUI1 = new Rdmp.UI.Progress.ProgressUI();
            this.gbControls.SuspendLayout();
            this.gbLoad.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbControls
            // 
            this.gbControls.Controls.Add(this.btnAbortLoad);
            this.gbControls.Controls.Add(this.btnExecute);
            this.gbControls.Controls.Add(this.ragChecks);
            this.gbControls.Controls.Add(this.label5);
            this.gbControls.Controls.Add(this.label2);
            this.gbControls.Controls.Add(this.label1);
            this.gbControls.Controls.Add(this.btnRunChecks);
            this.gbControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbControls.Location = new System.Drawing.Point(0, 0);
            this.gbControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbControls.Name = "gbControls";
            this.gbControls.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbControls.Size = new System.Drawing.Size(938, 70);
            this.gbControls.TabIndex = 58;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "Controls";
            // 
            // btnAbortLoad
            // 
            this.btnAbortLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnAbortLoad.Image")));
            this.btnAbortLoad.Location = new System.Drawing.Point(177, 15);
            this.btnAbortLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAbortLoad.Name = "btnAbortLoad";
            this.btnAbortLoad.Size = new System.Drawing.Size(34, 33);
            this.btnAbortLoad.TabIndex = 58;
            this.btnAbortLoad.UseVisualStyleBackColor = true;
            this.btnAbortLoad.Click += new System.EventHandler(this.btnAbortLoad_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Image = ((System.Drawing.Image)(resources.GetObject("btnExecute.Image")));
            this.btnExecute.Location = new System.Drawing.Point(78, 15);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(100, 33);
            this.btnExecute.TabIndex = 58;
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // ragChecks
            // 
            this.ragChecks.AlwaysShowHandCursor = false;
            this.ragChecks.BackColor = System.Drawing.Color.Transparent;
            this.ragChecks.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragChecks.Location = new System.Drawing.Point(8, 18);
            this.ragChecks.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragChecks.Name = "ragChecks";
            this.ragChecks.Size = new System.Drawing.Size(29, 29);
            this.ragChecks.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(176, 47);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 15);
            this.label5.TabIndex = 2;
            this.label5.Text = "Abort";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(106, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Execute";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 50);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Checks";
            // 
            // btnRunChecks
            // 
            this.btnRunChecks.Image = ((System.Drawing.Image)(resources.GetObject("btnRunChecks.Image")));
            this.btnRunChecks.Location = new System.Drawing.Point(44, 15);
            this.btnRunChecks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRunChecks.Name = "btnRunChecks";
            this.btnRunChecks.Size = new System.Drawing.Size(35, 33);
            this.btnRunChecks.TabIndex = 1;
            this.btnRunChecks.UseVisualStyleBackColor = true;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);
            // 
            // gbLoad
            // 
            this.gbLoad.Controls.Add(this.checksUI1);
            this.gbLoad.Controls.Add(this.loadProgressUI1);
            this.gbLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLoad.Location = new System.Drawing.Point(0, 70);
            this.gbLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbLoad.Name = "gbLoad";
            this.gbLoad.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbLoad.Size = new System.Drawing.Size(938, 704);
            this.gbLoad.TabIndex = 57;
            this.gbLoad.TabStop = false;
            this.gbLoad.Text = "Load Execution";
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(4, 19);
            this.checksUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(930, 682);
            this.checksUI1.TabIndex = 0;
            // 
            // loadProgressUI1
            // 
            this.loadProgressUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadProgressUI1.Location = new System.Drawing.Point(4, 19);
            this.loadProgressUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.loadProgressUI1.Name = "loadProgressUI1";
            this.loadProgressUI1.Size = new System.Drawing.Size(930, 682);
            this.loadProgressUI1.TabIndex = 42;
            // 
            // CheckAndExecuteUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbLoad);
            this.Controls.Add(this.gbControls);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CheckAndExecuteUI";
            this.Size = new System.Drawing.Size(938, 774);
            this.gbControls.ResumeLayout(false);
            this.gbControls.PerformLayout();
            this.gbLoad.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbControls;
        private System.Windows.Forms.Button btnAbortLoad;
        private System.Windows.Forms.Button btnExecute;
        private RAGSmiley ragChecks;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRunChecks;
        private System.Windows.Forms.GroupBox gbLoad;
        private ChecksUI.ChecksUI checksUI1;
        private ProgressUI loadProgressUI1;
    }
}
