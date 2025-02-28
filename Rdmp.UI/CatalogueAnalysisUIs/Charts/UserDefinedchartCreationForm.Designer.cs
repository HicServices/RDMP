namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    partial class UserDefinedChartCreationForm
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            textBox1 = new System.Windows.Forms.TextBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            textBox2 = new System.Windows.Forms.TextBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            comboBox1 = new System.Windows.Forms.ComboBox();
            textBox3 = new System.Windows.Forms.TextBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(textBox1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(200, 45);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Chart Title";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(6, 16);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(188, 23);
            textBox1.TabIndex = 2;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(textBox2);
            groupBox2.Location = new System.Drawing.Point(12, 63);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(200, 45);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Series Name";
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(6, 16);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(188, 23);
            textBox2.TabIndex = 2;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(comboBox1);
            groupBox3.Location = new System.Drawing.Point(12, 118);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(200, 45);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Chart Type";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(6, 16);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(188, 23);
            comboBox1.TabIndex = 5;
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(6, 16);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(558, 377);
            textBox3.TabIndex = 5;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(textBox3);
            groupBox4.Location = new System.Drawing.Point(218, 12);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(570, 400);
            groupBox4.TabIndex = 6;
            groupBox4.TabStop = false;
            groupBox4.Text = "Query";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(137, 169);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 7;
            button1.Text = "Create";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.Red;
            label1.Location = new System.Drawing.Point(18, 202);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(0, 15);
            label1.TabIndex = 8;
            // 
            // UserDefinedChartCreationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "UserDefinedChartCreationForm";
            Text = "Create New UserDefined Chart";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
    }
}