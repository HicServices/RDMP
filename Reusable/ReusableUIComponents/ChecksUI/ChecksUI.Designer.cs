namespace ReusableUIComponents.ChecksUI
{
    partial class ChecksUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChecksUI));
            this.listView1 = new System.Windows.Forms.ListView();
            this.Result = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ExceptionStack = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.btnAbortChecking = new System.Windows.Forms.Button();
            this.ddFilterSeverity = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Result,
            this.Message,
            this.ExceptionStack});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(651, 550);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyUp);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // Result
            // 
            this.Result.Text = "Result";
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 300;
            // 
            // ExceptionStack
            // 
            this.ExceptionStack.Text = "ExceptionStack";
            this.ExceptionStack.Width = 500;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Pass");
            this.imageList1.Images.SetKeyName(1, "Fail");
            this.imageList1.Images.SetKeyName(2, "Warning");
            this.imageList1.Images.SetKeyName(3, "FailedWithException");
            this.imageList1.Images.SetKeyName(4, "WarningWithException");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 559);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilter.Location = new System.Drawing.Point(37, 556);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(325, 20);
            this.tbFilter.TabIndex = 3;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // btnAbortChecking
            // 
            this.btnAbortChecking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbortChecking.Enabled = false;
            this.btnAbortChecking.Location = new System.Drawing.Point(550, 553);
            this.btnAbortChecking.Name = "btnAbortChecking";
            this.btnAbortChecking.Size = new System.Drawing.Size(98, 23);
            this.btnAbortChecking.TabIndex = 4;
            this.btnAbortChecking.Text = "Abort Checking";
            this.btnAbortChecking.UseVisualStyleBackColor = true;
            this.btnAbortChecking.Click += new System.EventHandler(this.btnAbortChecking_Click);
            // 
            // ddFilterSeverity
            // 
            this.ddFilterSeverity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddFilterSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddFilterSeverity.FormattingEnabled = true;
            this.ddFilterSeverity.Location = new System.Drawing.Point(432, 555);
            this.ddFilterSeverity.Name = "ddFilterSeverity";
            this.ddFilterSeverity.Size = new System.Drawing.Size(112, 21);
            this.ddFilterSeverity.TabIndex = 5;
            this.ddFilterSeverity.SelectedIndexChanged += new System.EventHandler(this.ddFilterSeverity_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(369, 558);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Threshold:";
            // 
            // ChecksUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ddFilterSeverity);
            this.Controls.Add(this.btnAbortChecking);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Name = "ChecksUI";
            this.Size = new System.Drawing.Size(651, 579);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Result;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.ColumnHeader ExceptionStack;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Button btnAbortChecking;
        private System.Windows.Forms.ComboBox ddFilterSeverity;
        private System.Windows.Forms.Label label2;
    }
}
