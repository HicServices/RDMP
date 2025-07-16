namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    partial class UserDefinedChartRunner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserDefinedChartRunner));
            groupBox1 = new System.Windows.Forms.GroupBox();
            btnEdit = new System.Windows.Forms.Button();
            multiPurposeChart1 = new MultiPurposeChart();
            button1 = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(btnEdit);
            groupBox1.Controls.Add(multiPurposeChart1);
            groupBox1.Controls.Add(button1);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(418, 370);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // btnEdit
            // 
            btnEdit.Image = (System.Drawing.Image)resources.GetObject("btnEdit.Image");
            btnEdit.Location = new System.Drawing.Point(46, 22);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(34, 23);
            btnEdit.TabIndex = 2;
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // multiPurposeChart1
            // 
            multiPurposeChart1.Location = new System.Drawing.Point(6, 51);
            multiPurposeChart1.Name = "multiPurposeChart1";
            multiPurposeChart1.Size = new System.Drawing.Size(406, 297);
            multiPurposeChart1.TabIndex = 1;
            multiPurposeChart1.Load += multiPurposeChart1_Load;
            // 
            // button1
            // 
            button1.Image = (System.Drawing.Image)resources.GetObject("button1.Image");
            button1.Location = new System.Drawing.Point(6, 22);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(34, 23);
            button1.TabIndex = 0;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // UserDefinedChartRunner
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(groupBox1);
            Name = "UserDefinedChartRunner";
            Size = new System.Drawing.Size(418, 370);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private MultiPurposeChart multiPurposeChart1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnEdit;
    }
}
