namespace Rdmp.UI.CatalogueAnalysisUIs
{
    partial class CatalogueAnalysisExecutionControlUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CatalogueAnalysisExecutionControlUI));
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            timePeriodicityChart1 = new CatalogueSummary.DataQualityReporting.TimePeriodicityChart();
            tabPage2 = new System.Windows.Forms.TabPage();
            button1 = new System.Windows.Forms.Button();
            groupBox3 = new System.Windows.Forms.GroupBox();
            cbPivot = new System.Windows.Forms.ComboBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            cbTime = new System.Windows.Forms.ComboBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            btnSavePrimaryConstraints = new System.Windows.Forms.Button();
            primaryConstrainsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            primaryConstrainsTableLayout.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new System.Drawing.Point(3, 25);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(1238, 847);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(timePeriodicityChart1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(1230, 819);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Analysis";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // timePeriodicityChart1
            // 
            timePeriodicityChart1.Location = new System.Drawing.Point(7, 6);
            timePeriodicityChart1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            timePeriodicityChart1.Name = "timePeriodicityChart1";
            timePeriodicityChart1.Size = new System.Drawing.Size(985, 450);
            timePeriodicityChart1.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(button1);
            tabPage2.Controls.Add(groupBox3);
            tabPage2.Controls.Add(groupBox2);
            tabPage2.Controls.Add(groupBox1);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(1230, 819);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Configuration";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Image = (System.Drawing.Image)resources.GetObject("button1.Image");
            button1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            button1.Location = new System.Drawing.Point(787, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(121, 42);
            button1.TabIndex = 4;
            button1.Text = "Run Validation";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cbPivot);
            groupBox3.Location = new System.Drawing.Point(453, 61);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(334, 49);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Pivot Column";
            // 
            // cbPivot
            // 
            cbPivot.FormattingEnabled = true;
            cbPivot.Location = new System.Drawing.Point(6, 19);
            cbPivot.Name = "cbPivot";
            cbPivot.Size = new System.Drawing.Size(322, 23);
            cbPivot.TabIndex = 5;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cbTime);
            groupBox2.Location = new System.Drawing.Point(453, 6);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(334, 49);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Time Column";
            // 
            // cbTime
            // 
            cbTime.FormattingEnabled = true;
            cbTime.Location = new System.Drawing.Point(6, 19);
            cbTime.Name = "cbTime";
            cbTime.Size = new System.Drawing.Size(322, 23);
            cbTime.TabIndex = 4;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(btnSavePrimaryConstraints);
            groupBox1.Controls.Add(primaryConstrainsTableLayout);
            groupBox1.Location = new System.Drawing.Point(6, 6);
            groupBox1.MinimumSize = new System.Drawing.Size(300, 100);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(441, 100);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Primary Constraints";
            // 
            // btnSavePrimaryConstraints
            // 
            btnSavePrimaryConstraints.Dock = System.Windows.Forms.DockStyle.Bottom;
            btnSavePrimaryConstraints.Location = new System.Drawing.Point(3, 74);
            btnSavePrimaryConstraints.Name = "btnSavePrimaryConstraints";
            btnSavePrimaryConstraints.Size = new System.Drawing.Size(435, 23);
            btnSavePrimaryConstraints.TabIndex = 2;
            btnSavePrimaryConstraints.Text = "Save";
            btnSavePrimaryConstraints.UseVisualStyleBackColor = true;
            btnSavePrimaryConstraints.Click += btnSavePrimaryConstraints_Click;
            // 
            // primaryConstrainsTableLayout
            // 
            primaryConstrainsTableLayout.AutoSize = true;
            primaryConstrainsTableLayout.ColumnCount = 3;
            primaryConstrainsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            primaryConstrainsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            primaryConstrainsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            primaryConstrainsTableLayout.Controls.Add(label3, 2, 0);
            primaryConstrainsTableLayout.Controls.Add(label2, 1, 0);
            primaryConstrainsTableLayout.Controls.Add(label1, 0, 0);
            primaryConstrainsTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            primaryConstrainsTableLayout.Location = new System.Drawing.Point(3, 19);
            primaryConstrainsTableLayout.Name = "primaryConstrainsTableLayout";
            primaryConstrainsTableLayout.RowCount = 2;
            primaryConstrainsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            primaryConstrainsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            primaryConstrainsTableLayout.Size = new System.Drawing.Size(435, 50);
            primaryConstrainsTableLayout.TabIndex = 0;
            primaryConstrainsTableLayout.Paint += primaryConstrainsTableLayout_Paint;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(171, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(94, 15);
            label3.TabIndex = 4;
            label3.Text = "Validation Result";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(59, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(106, 15);
            label2.TabIndex = 3;
            label2.Text = "Primary Constraint";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 15);
            label1.TabIndex = 2;
            label1.Text = "Column";
            label1.Click += label1_Click;
            // 
            // CatalogueAnalysisExecutionControlUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl1);
            Name = "CatalogueAnalysisExecutionControlUI";
            Size = new System.Drawing.Size(1249, 853);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            primaryConstrainsTableLayout.ResumeLayout(false);
            primaryConstrainsTableLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel primaryConstrainsTableLayout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSavePrimaryConstraints;
        private System.Windows.Forms.TabPage tabPage1;
        private CatalogueSummary.DataQualityReporting.TimePeriodicityChart timePeriodicityChart1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cbPivot;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cbTime;
        private System.Windows.Forms.Button button1;
    }
}
