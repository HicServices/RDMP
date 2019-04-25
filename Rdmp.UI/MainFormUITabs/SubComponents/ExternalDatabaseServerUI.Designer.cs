namespace Rdmp.UI.MainFormUITabs.SubComponents
{
    partial class ExternalDatabaseServerUI
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
            this.pbServer = new System.Windows.Forms.PictureBox();
            this.btnClearKnownType = new System.Windows.Forms.Button();
            this.ddSetKnownType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblPasswordError = new System.Windows.Forms.Label();
            this.lblUsernameError = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbDatabaseName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbServerName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbMappedDataPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pbDatabaseProvider = new System.Windows.Forms.PictureBox();
            this.ddDatabaseType = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbServer
            // 
            this.pbServer.Location = new System.Drawing.Point(561, 190);
            this.pbServer.Name = "pbServer";
            this.pbServer.Size = new System.Drawing.Size(22, 22);
            this.pbServer.TabIndex = 47;
            this.pbServer.TabStop = false;
            // 
            // btnClearKnownType
            // 
            this.btnClearKnownType.Location = new System.Drawing.Point(589, 190);
            this.btnClearKnownType.Name = "btnClearKnownType";
            this.btnClearKnownType.Size = new System.Drawing.Size(65, 23);
            this.btnClearKnownType.TabIndex = 8;
            this.btnClearKnownType.Text = "Clear";
            this.btnClearKnownType.UseVisualStyleBackColor = true;
            this.btnClearKnownType.Click += new System.EventHandler(this.btnClearKnownType_Click);
            // 
            // ddSetKnownType
            // 
            this.ddSetKnownType.FormattingEnabled = true;
            this.ddSetKnownType.Location = new System.Drawing.Point(132, 189);
            this.ddSetKnownType.Name = "ddSetKnownType";
            this.ddSetKnownType.Size = new System.Drawing.Size(423, 21);
            this.ddSetKnownType.TabIndex = 7;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(43, 213);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(617, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "(Only choose if you are sure the database is of an RDMP known type, it\'s ok to no" +
    "t have one e.g. for RAW or untyped databases)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(37, 192);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(96, 13);
            this.label14.TabIndex = 42;
            this.label14.Text = "Creation Assembly:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(662, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(130, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "*Indicates Required Fields";
            // 
            // lblPasswordError
            // 
            this.lblPasswordError.AutoSize = true;
            this.lblPasswordError.BackColor = System.Drawing.Color.Red;
            this.lblPasswordError.Location = new System.Drawing.Point(531, 166);
            this.lblPasswordError.Name = "lblPasswordError";
            this.lblPasswordError.Size = new System.Drawing.Size(0, 13);
            this.lblPasswordError.TabIndex = 40;
            // 
            // lblUsernameError
            // 
            this.lblUsernameError.AutoSize = true;
            this.lblUsernameError.BackColor = System.Drawing.Color.Red;
            this.lblUsernameError.Location = new System.Drawing.Point(531, 140);
            this.lblUsernameError.Name = "lblUsernameError";
            this.lblUsernameError.Size = new System.Drawing.Size(0, 13);
            this.lblUsernameError.TabIndex = 39;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(132, 163);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(660, 20);
            this.tbPassword.TabIndex = 6;
            this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 166);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Password:";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(132, 137);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(660, 20);
            this.tbUsername.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 140);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Username:";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Location = new System.Drawing.Point(132, 111);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.Size = new System.Drawing.Size(660, 20);
            this.tbDatabaseName.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(73, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Database:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(132, 7);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(660, 20);
            this.tbID.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(105, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "ID:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(132, 33);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(660, 20);
            this.tbName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(88, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Name*:";
            // 
            // tbServerName
            // 
            this.tbServerName.Location = new System.Drawing.Point(132, 59);
            this.tbServerName.Name = "tbServerName";
            this.tbServerName.Size = new System.Drawing.Size(507, 20);
            this.tbServerName.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(88, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 37;
            this.label3.Text = "Server*:";
            // 
            // tbMappedDataPath
            // 
            this.tbMappedDataPath.Location = new System.Drawing.Point(132, 85);
            this.tbMappedDataPath.Name = "tbMappedDataPath";
            this.tbMappedDataPath.Size = new System.Drawing.Size(660, 20);
            this.tbMappedDataPath.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(50, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 50;
            this.label6.Text = "Data Path Map:";
            // 
            // pbDatabaseProvider
            // 
            this.pbDatabaseProvider.Location = new System.Drawing.Point(773, 59);
            this.pbDatabaseProvider.Name = "pbDatabaseProvider";
            this.pbDatabaseProvider.Size = new System.Drawing.Size(19, 19);
            this.pbDatabaseProvider.TabIndex = 170;
            this.pbDatabaseProvider.TabStop = false;
            // 
            // ddDatabaseType
            // 
            this.ddDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseType.FormattingEnabled = true;
            this.ddDatabaseType.Location = new System.Drawing.Point(648, 59);
            this.ddDatabaseType.Name = "ddDatabaseType";
            this.ddDatabaseType.Size = new System.Drawing.Size(121, 21);
            this.ddDatabaseType.TabIndex = 169;
            this.ddDatabaseType.SelectedIndexChanged += new System.EventHandler(this.ddDatabaseType_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.pbDatabaseProvider);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.ddDatabaseType);
            this.panel1.Controls.Add(this.tbServerName);
            this.panel1.Controls.Add(this.tbMappedDataPath);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Controls.Add(this.pbServer);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.btnClearKnownType);
            this.panel1.Controls.Add(this.tbDatabaseName);
            this.panel1.Controls.Add(this.ddSetKnownType);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label15);
            this.panel1.Controls.Add(this.tbUsername);
            this.panel1.Controls.Add(this.label14);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.tbPassword);
            this.panel1.Controls.Add(this.lblPasswordError);
            this.panel1.Controls.Add(this.lblUsernameError);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(808, 255);
            this.panel1.TabIndex = 171;
            // 
            // ExternalDatabaseServerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "ExternalDatabaseServerUI";
            this.Size = new System.Drawing.Size(808, 255);
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbServer;
        private System.Windows.Forms.Button btnClearKnownType;
        private System.Windows.Forms.ComboBox ddSetKnownType;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblPasswordError;
        private System.Windows.Forms.Label lblUsernameError;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDatabaseName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbServerName;
        private System.Windows.Forms.Label label3;
        
        private System.Windows.Forms.TextBox tbMappedDataPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pbDatabaseProvider;
        private System.Windows.Forms.ComboBox ddDatabaseType;
        private System.Windows.Forms.Panel panel1;
    }
}
