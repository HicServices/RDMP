

namespace DataExportManager.ProjectUI
{
    partial class ExecuteExtractionUI
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
            this.btnStart = new System.Windows.Forms.Button();
            this.cbIsTest = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbSkipValidation = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTopX = new System.Windows.Forms.TextBox();
            this.btnCancelAll = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.chooseExtractablesUI1 = new DataExportManager.ProjectUI.ChooseExtractablesUI();
            this.lblAlreadyReleased = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(0, 78);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 23);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cbIsTest
            // 
            this.cbIsTest.AutoSize = true;
            this.cbIsTest.Location = new System.Drawing.Point(6, 36);
            this.cbIsTest.Name = "cbIsTest";
            this.cbIsTest.Size = new System.Drawing.Size(88, 17);
            this.cbIsTest.TabIndex = 7;
            this.cbIsTest.Text = "Audit as Test";
            this.cbIsTest.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1066, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Extract Datasets:";
            // 
            // cbSkipValidation
            // 
            this.cbSkipValidation.AutoSize = true;
            this.cbSkipValidation.Checked = true;
            this.cbSkipValidation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSkipValidation.Location = new System.Drawing.Point(6, 18);
            this.cbSkipValidation.Name = "cbSkipValidation";
            this.cbSkipValidation.Size = new System.Drawing.Size(96, 17);
            this.cbSkipValidation.TabIndex = 11;
            this.cbSkipValidation.Text = "Skip Validation";
            this.cbSkipValidation.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Limit (TOP X), X=";
            // 
            // tbTopX
            // 
            this.tbTopX.Location = new System.Drawing.Point(100, 53);
            this.tbTopX.Name = "tbTopX";
            this.tbTopX.Size = new System.Drawing.Size(94, 20);
            this.tbTopX.TabIndex = 18;
            this.tbTopX.TextChanged += new System.EventHandler(this.tbTopX_TextChanged);
            // 
            // btnCancelAll
            // 
            this.btnCancelAll.Location = new System.Drawing.Point(100, 78);
            this.btnCancelAll.Name = "btnCancelAll";
            this.btnCancelAll.Size = new System.Drawing.Size(100, 23);
            this.btnCancelAll.TabIndex = 24;
            this.btnCancelAll.Text = "Cancel All";
            this.btnCancelAll.UseVisualStyleBackColor = true;
            this.btnCancelAll.Click += new System.EventHandler(this.btnCancelAll_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnStart);
            this.groupBox2.Controls.Add(this.btnCancelAll);
            this.groupBox2.Controls.Add(this.cbIsTest);
            this.groupBox2.Controls.Add(this.cbSkipValidation);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbTopX);
            this.groupBox2.Location = new System.Drawing.Point(756, 767);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(203, 102);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Execute";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(3, 709);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(747, 160);
            this.panel1.TabIndex = 27;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblAlreadyReleased);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chooseExtractablesUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1481, 882);
            this.splitContainer1.SplitterDistance = 966;
            this.splitContainer1.TabIndex = 29;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(945, 700);
            this.tabControl1.TabIndex = 28;
            // 
            // chooseExtractablesUI1
            // 
            this.chooseExtractablesUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseExtractablesUI1.AutoScroll = true;
            this.chooseExtractablesUI1.Location = new System.Drawing.Point(3, -2);
            this.chooseExtractablesUI1.Name = "chooseExtractablesUI1";
            this.chooseExtractablesUI1.Size = new System.Drawing.Size(501, 877);
            this.chooseExtractablesUI1.TabIndex = 28;
            this.chooseExtractablesUI1.CommandSelected += new DataExportManager.ProjectUI.ExtractCommandSelectedHandler(this.chooseExtractablesUI1_BundleSelected);
            // 
            // lblAlreadyReleased
            // 
            this.lblAlreadyReleased.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblAlreadyReleased.AutoSize = true;
            this.lblAlreadyReleased.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAlreadyReleased.ForeColor = System.Drawing.Color.Red;
            this.lblAlreadyReleased.Location = new System.Drawing.Point(211, 254);
            this.lblAlreadyReleased.Name = "lblAlreadyReleased";
            this.lblAlreadyReleased.Size = new System.Drawing.Size(494, 24);
            this.lblAlreadyReleased.TabIndex = 29;
            this.lblAlreadyReleased.Text = "Extraction Configuration has already been Released";
            this.lblAlreadyReleased.Visible = false;
            // 
            // ExecuteExtractionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label2);
            this.Name = "ExecuteExtractionUI";
            this.Size = new System.Drawing.Size(1481, 882);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.CheckBox cbIsTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbSkipValidation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTopX;
        private System.Windows.Forms.Button btnCancelAll;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private ChooseExtractablesUI chooseExtractablesUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label lblAlreadyReleased;
    }
}