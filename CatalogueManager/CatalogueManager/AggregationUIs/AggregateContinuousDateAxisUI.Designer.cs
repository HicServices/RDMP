namespace CatalogueManager.AggregationUIs
{
    partial class AggregateContinuousDateAxisUI
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
            this.tbStartDate = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ddIncrement = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbEndDate = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbStartDate
            // 
            this.tbStartDate.Location = new System.Drawing.Point(79, 19);
            this.tbStartDate.Name = "tbStartDate";
            this.tbStartDate.Size = new System.Drawing.Size(122, 20);
            this.tbStartDate.TabIndex = 1;
            this.tbStartDate.Visible = false;
            this.tbStartDate.TextChanged += new System.EventHandler(this.tbDates_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ddIncrement);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbEndDate);
            this.groupBox1.Controls.Add(this.tbStartDate);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(601, 52);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Axis Settings";
            // 
            // ddIncrement
            // 
            this.ddIncrement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddIncrement.FormattingEnabled = true;
            this.ddIncrement.Location = new System.Drawing.Point(440, 20);
            this.ddIncrement.Name = "ddIncrement";
            this.ddIncrement.Size = new System.Drawing.Size(144, 21);
            this.ddIncrement.TabIndex = 5;
            this.ddIncrement.Visible = false;
            this.ddIncrement.SelectedIndexChanged += new System.EventHandler(this.ddIncrement_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(378, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Increment:";
            this.label3.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(207, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "EndDate:";
            this.label2.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "StartDate:";
            this.label1.Visible = false;
            // 
            // tbEndDate
            // 
            this.tbEndDate.Location = new System.Drawing.Point(265, 19);
            this.tbEndDate.Name = "tbEndDate";
            this.tbEndDate.Size = new System.Drawing.Size(107, 20);
            this.tbEndDate.TabIndex = 3;
            this.tbEndDate.Visible = false;
            this.tbEndDate.TextChanged += new System.EventHandler(this.tbDates_TextChanged);
            // 
            // AggregateContinuousDateAxisUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "AggregateContinuousDateAxisUI";
            this.Size = new System.Drawing.Size(601, 52);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox ddIncrement;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbEndDate;
    }
}
