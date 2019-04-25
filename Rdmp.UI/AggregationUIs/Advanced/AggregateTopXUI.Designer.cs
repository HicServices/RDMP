namespace Rdmp.UI.AggregationUIs.Advanced
{
    partial class AggregateTopXUI
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
            this.tbTopX = new System.Windows.Forms.TextBox();
            this.ddOrderByDimension = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ddAscOrDesc = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // tbTopX
            // 
            this.tbTopX.Location = new System.Drawing.Point(3, 3);
            this.tbTopX.Name = "tbTopX";
            this.tbTopX.Size = new System.Drawing.Size(100, 20);
            this.tbTopX.TabIndex = 25;
            this.tbTopX.TextChanged += new System.EventHandler(this.tbTopX_TextChanged);
            // 
            // ddOrderByDimension
            // 
            this.ddOrderByDimension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddOrderByDimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOrderByDimension.FormattingEnabled = true;
            this.ddOrderByDimension.Location = new System.Drawing.Point(163, 3);
            this.ddOrderByDimension.Name = "ddOrderByDimension";
            this.ddOrderByDimension.Size = new System.Drawing.Size(443, 21);
            this.ddOrderByDimension.TabIndex = 26;
            this.ddOrderByDimension.SelectedIndexChanged += new System.EventHandler(this.ddOrderByDimension_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(109, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Order By";
            // 
            // ddAscOrDesc
            // 
            this.ddAscOrDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddAscOrDesc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAscOrDesc.FormattingEnabled = true;
            this.ddAscOrDesc.Location = new System.Drawing.Point(612, 3);
            this.ddAscOrDesc.Name = "ddAscOrDesc";
            this.ddAscOrDesc.Size = new System.Drawing.Size(111, 21);
            this.ddAscOrDesc.TabIndex = 26;
            this.ddAscOrDesc.SelectedIndexChanged += new System.EventHandler(this.ddAscOrDesc_SelectedIndexChanged);
            // 
            // AggregateTopXUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddAscOrDesc);
            this.Controls.Add(this.ddOrderByDimension);
            this.Controls.Add(this.tbTopX);
            this.Name = "AggregateTopXUI";
            this.Size = new System.Drawing.Size(726, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbTopX;
        private System.Windows.Forms.ComboBox ddOrderByDimension;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddAscOrDesc;
    }
}
