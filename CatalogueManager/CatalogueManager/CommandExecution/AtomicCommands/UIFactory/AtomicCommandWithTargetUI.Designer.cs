namespace CatalogueManager.CommandExecution.AtomicCommands.UIFactory
{
    partial class AtomicCommandWithTargetUI<T>
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
            this.pbCommandIcon = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblGo = new System.Windows.Forms.Label();
            this.suggestComboBox1 = new ReusableUIComponents.SuggestComboBox();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            this.lPick = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbCommandIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCommandIcon
            // 
            this.pbCommandIcon.Location = new System.Drawing.Point(0, 0);
            this.pbCommandIcon.Name = "pbCommandIcon";
            this.pbCommandIcon.Size = new System.Drawing.Size(19, 19);
            this.pbCommandIcon.TabIndex = 0;
            this.pbCommandIcon.TabStop = false;
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Location = new System.Drawing.Point(25, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(215, 19);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "lblName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblGo
            // 
            this.lblGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGo.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.lblGo.Location = new System.Drawing.Point(457, 0);
            this.lblGo.Name = "lblGo";
            this.lblGo.Size = new System.Drawing.Size(33, 19);
            this.lblGo.TabIndex = 3;
            this.lblGo.Text = "GO";
            this.lblGo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGo.Click += new System.EventHandler(this.lblGo_Click);
            this.lblGo.MouseEnter += new System.EventHandler(this.lblGo_MouseEnter);
            this.lblGo.MouseLeave += new System.EventHandler(this.lblGo_MouseLeave);
            // 
            // suggestComboBox1
            // 
            this.suggestComboBox1.FilterRule = null;
            this.suggestComboBox1.FormattingEnabled = true;
            this.suggestComboBox1.Location = new System.Drawing.Point(246, 0);
            this.suggestComboBox1.Name = "suggestComboBox1";
            this.suggestComboBox1.PropertySelector = null;
            this.suggestComboBox1.Size = new System.Drawing.Size(205, 21);
            this.suggestComboBox1.SuggestBoxHeight = 96;
            this.suggestComboBox1.SuggestListOrderRule = null;
            this.suggestComboBox1.TabIndex = 4;
            this.suggestComboBox1.SelectedIndexChanged += new System.EventHandler(this.suggestComboBox1_SelectedIndexChanged);
            // 
            // helpIcon1
            // 
            this.helpIcon1.Location = new System.Drawing.Point(536, -1);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 6;
            // 
            // lPick
            // 
            this.lPick.BackColor = System.Drawing.Color.White;
            this.lPick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lPick.Location = new System.Drawing.Point(452, 0);
            this.lPick.Margin = new System.Windows.Forms.Padding(0);
            this.lPick.Name = "lPick";
            this.lPick.Size = new System.Drawing.Size(18, 21);
            this.lPick.TabIndex = 8;
            this.lPick.Text = "...";
            this.lPick.Click += new System.EventHandler(this.lPick_Click);
            // 
            // AtomicCommandWithTargetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lPick);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.suggestComboBox1);
            this.Controls.Add(this.lblGo);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.pbCommandIcon);
            this.Name = "AtomicCommandWithTargetUI";
            this.Size = new System.Drawing.Size(567, 22);
            ((System.ComponentModel.ISupportInitialize)(this.pbCommandIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCommandIcon;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblGo;
        private ReusableUIComponents.SuggestComboBox suggestComboBox1;
        private ReusableUIComponents.HelpIcon helpIcon1;
        private System.Windows.Forms.Label lPick;
    }
}
