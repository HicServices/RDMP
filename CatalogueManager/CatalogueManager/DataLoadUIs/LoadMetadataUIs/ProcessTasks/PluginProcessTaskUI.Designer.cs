namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{
    partial class PluginProcessTaskUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginProcessTaskUI));
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.pArguments = new System.Windows.Forms.Panel();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.loadStageIconUI1 = new CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadStageIconUI();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnCheckAgain = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(75, 3);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(328, 20);
            this.tbName.TabIndex = 2;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // pArguments
            // 
            this.pArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pArguments.Location = new System.Drawing.Point(0, 29);
            this.pArguments.Name = "pArguments";
            this.pArguments.Size = new System.Drawing.Size(916, 601);
            this.pArguments.TabIndex = 3;
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(420, 0);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(56, 26);
            this.objectSaverButton1.TabIndex = 4;
            // 
            // loadStageIconUI1
            // 
            this.loadStageIconUI1.Location = new System.Drawing.Point(551, 4);
            this.loadStageIconUI1.Name = "loadStageIconUI1";
            this.loadStageIconUI1.Size = new System.Drawing.Size(232, 19);
            this.loadStageIconUI1.TabIndex = 5;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(490, 1);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 6;
            // 
            // btnCheckAgain
            // 
            this.btnCheckAgain.Image = ((System.Drawing.Image)(resources.GetObject("btnCheckAgain.Image")));
            this.btnCheckAgain.Location = new System.Drawing.Point(521, 2);
            this.btnCheckAgain.Name = "btnCheckAgain";
            this.btnCheckAgain.Size = new System.Drawing.Size(24, 24);
            this.btnCheckAgain.TabIndex = 7;
            this.btnCheckAgain.UseVisualStyleBackColor = true;
            this.btnCheckAgain.Click += new System.EventHandler(this.btnCheckAgain_Click);
            // 
            // PluginProcessTaskUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCheckAgain);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.loadStageIconUI1);
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.pArguments);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label1);
            this.Name = "PluginProcessTaskUI";
            this.Size = new System.Drawing.Size(919, 633);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Panel pArguments;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private LoadStageIconUI loadStageIconUI1;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnCheckAgain;
    }
}
