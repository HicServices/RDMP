namespace Rdmp.UI.SimpleDialogs.Datasets
{
    partial class CreateNewPureDatasetUI
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
            label1 = new System.Windows.Forms.Label();
            cbDatasetProvider = new System.Windows.Forms.ComboBox();
            tbName = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            tbDescription = new System.Windows.Forms.TextBox();
            cbPublisher = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            cbVisibility = new System.Windows.Forms.ComboBox();
            tbDateMadeAvailable = new System.Windows.Forms.TextBox();
            lbPeople = new System.Windows.Forms.ListBox();
            btnCreate = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 21);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(96, 15);
            label1.TabIndex = 0;
            label1.Text = "Dataset Provider:";
            // 
            // cbDatasetProvider
            // 
            cbDatasetProvider.FormattingEnabled = true;
            cbDatasetProvider.Location = new System.Drawing.Point(114, 18);
            cbDatasetProvider.Name = "cbDatasetProvider";
            cbDatasetProvider.Size = new System.Drawing.Size(252, 23);
            cbDatasetProvider.TabIndex = 1;
            cbDatasetProvider.SelectedIndexChanged += cbDatasetProvider_SelectedIndexChanged;
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(116, 62);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(250, 23);
            tbName.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(69, 65);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(42, 15);
            label2.TabIndex = 3;
            label2.Text = "Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(41, 107);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(70, 15);
            label3.TabIndex = 5;
            label3.Text = "Description:";
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(116, 104);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(250, 23);
            tbDescription.TabIndex = 4;
            // 
            // cbPublisher
            // 
            cbPublisher.Enabled = false;
            cbPublisher.FormattingEnabled = true;
            cbPublisher.Location = new System.Drawing.Point(114, 147);
            cbPublisher.Name = "cbPublisher";
            cbPublisher.Size = new System.Drawing.Size(252, 23);
            cbPublisher.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(53, 150);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(59, 15);
            label4.TabIndex = 6;
            label4.Text = "Publisher:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(49, 197);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(46, 15);
            label5.TabIndex = 8;
            label5.Text = "People:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(52, 229);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(54, 15);
            label6.TabIndex = 9;
            label6.Text = "Visibility:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(2, 261);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(116, 15);
            label7.TabIndex = 10;
            label7.Text = "Date made available:";
            // 
            // cbVisibility
            // 
            cbVisibility.FormattingEnabled = true;
            cbVisibility.Location = new System.Drawing.Point(114, 226);
            cbVisibility.Name = "cbVisibility";
            cbVisibility.Size = new System.Drawing.Size(252, 23);
            cbVisibility.TabIndex = 11;
            // 
            // tbDateMadeAvailable
            // 
            tbDateMadeAvailable.Location = new System.Drawing.Point(116, 261);
            tbDateMadeAvailable.Name = "tbDateMadeAvailable";
            tbDateMadeAvailable.Size = new System.Drawing.Size(250, 23);
            tbDateMadeAvailable.TabIndex = 12;
            // 
            // lbPeople
            // 
            lbPeople.Enabled = false;
            lbPeople.FormattingEnabled = true;
            lbPeople.ItemHeight = 15;
            lbPeople.Location = new System.Drawing.Point(116, 190);
            lbPeople.Name = "lbPeople";
            lbPeople.Size = new System.Drawing.Size(250, 34);
            lbPeople.TabIndex = 13;
            // 
            // btnCreate
            // 
            btnCreate.Location = new System.Drawing.Point(708, 406);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(75, 23);
            btnCreate.TabIndex = 14;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(627, 406);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // CreateNewPureDatasetUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(lbPeople);
            Controls.Add(tbDateMadeAvailable);
            Controls.Add(cbVisibility);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(cbPublisher);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(tbDescription);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(cbDatasetProvider);
            Controls.Add(label1);
            Name = "CreateNewPureDatasetUI";
            Text = "Create New Pure Dataset";
            Load += CreateNewPureDatasetUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbDatasetProvider;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.ComboBox cbPublisher;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbVisibility;
        private System.Windows.Forms.TextBox tbDateMadeAvailable;
        private System.Windows.Forms.ListBox lbPeople;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}