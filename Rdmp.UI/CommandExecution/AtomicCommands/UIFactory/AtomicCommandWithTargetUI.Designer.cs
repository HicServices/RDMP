using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.CommandExecution.AtomicCommands.UIFactory
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
            this.helpIcon1 = new HelpIcon();
            this.selectIMapsDirectlyToDatabaseTableComboBox1 = new SelectIMapsDirectlyToDatabaseTableComboBox();
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
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Location = new System.Drawing.Point(25, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(350, 19);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "lblName";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblGo
            // 
            this.lblGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGo.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.lblGo.Location = new System.Drawing.Point(345, 19);
            this.lblGo.Name = "lblGo";
            this.lblGo.Size = new System.Drawing.Size(30, 19);
            this.lblGo.TabIndex = 3;
            this.lblGo.Text = "GO";
            this.lblGo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGo.Click += new System.EventHandler(this.lblGo_Click);
            this.lblGo.MouseEnter += new System.EventHandler(this.lblGo_MouseEnter);
            this.lblGo.MouseLeave += new System.EventHandler(this.lblGo_MouseLeave);
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.Location = new System.Drawing.Point(378, 19);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 6;
            // 
            // selectIMapsDirectlyToDatabaseTableComboBox1
            // 
            this.selectIMapsDirectlyToDatabaseTableComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectIMapsDirectlyToDatabaseTableComboBox1.Location = new System.Drawing.Point(25, 17);
            this.selectIMapsDirectlyToDatabaseTableComboBox1.Name = "selectIMapsDirectlyToDatabaseTableComboBox1";
            this.selectIMapsDirectlyToDatabaseTableComboBox1.SelectedItem = null;
            this.selectIMapsDirectlyToDatabaseTableComboBox1.Size = new System.Drawing.Size(319, 24);
            this.selectIMapsDirectlyToDatabaseTableComboBox1.TabIndex = 10;
            // 
            // AtomicCommandWithTargetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.selectIMapsDirectlyToDatabaseTableComboBox1);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.lblGo);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.pbCommandIcon);
            this.Name = "AtomicCommandWithTargetUI";
            this.Size = new System.Drawing.Size(400, 44);
            ((System.ComponentModel.ISupportInitialize)(this.pbCommandIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCommandIcon;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblGo;
        private HelpIcon helpIcon1;
        private SelectIMapsDirectlyToDatabaseTableComboBox selectIMapsDirectlyToDatabaseTableComboBox1;
    }
}
