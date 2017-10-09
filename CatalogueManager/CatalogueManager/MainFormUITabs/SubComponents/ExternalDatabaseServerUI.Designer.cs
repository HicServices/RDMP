namespace CatalogueManager.MainFormUITabs.SubComponents
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
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnClearKnownType = new System.Windows.Forms.Button();
            this.ddSetKnownType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblPasswordError = new System.Windows.Forms.Label();
            this.lblUsernameError = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.btnCheckState = new System.Windows.Forms.Button();
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
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).BeginInit();
            this.SuspendLayout();
            // 
            // pbServer
            // 
            this.pbServer.Location = new System.Drawing.Point(525, 164);
            this.pbServer.Name = "pbServer";
            this.pbServer.Size = new System.Drawing.Size(22, 22);
            this.pbServer.TabIndex = 47;
            this.pbServer.TabStop = false;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(17, 226);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 46;
            this.ragSmiley1.Visible = false;
            // 
            // btnClearKnownType
            // 
            this.btnClearKnownType.Location = new System.Drawing.Point(553, 164);
            this.btnClearKnownType.Name = "btnClearKnownType";
            this.btnClearKnownType.Size = new System.Drawing.Size(65, 23);
            this.btnClearKnownType.TabIndex = 45;
            this.btnClearKnownType.Text = "Clear";
            this.btnClearKnownType.UseVisualStyleBackColor = true;
            this.btnClearKnownType.Click += new System.EventHandler(this.btnClearKnownType_Click);
            // 
            // ddSetKnownType
            // 
            this.ddSetKnownType.FormattingEnabled = true;
            this.ddSetKnownType.Location = new System.Drawing.Point(96, 163);
            this.ddSetKnownType.Name = "ddSetKnownType";
            this.ddSetKnownType.Size = new System.Drawing.Size(423, 21);
            this.ddSetKnownType.TabIndex = 44;
            this.ddSetKnownType.SelectedIndexChanged += new System.EventHandler(this.ddSetKnownType_SelectedIndexChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(7, 187);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(617, 13);
            this.label15.TabIndex = 43;
            this.label15.Text = "(Only choose if you are sure the database is of an RDMP known type, it\'s ok to no" +
    "t have one e.g. for RAW or untyped databases)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(1, 166);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(96, 13);
            this.label14.TabIndex = 42;
            this.label14.Text = "Creation Assembly:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(626, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(130, 13);
            this.label8.TabIndex = 41;
            this.label8.Text = "*Indicates Required Fields";
            // 
            // lblPasswordError
            // 
            this.lblPasswordError.AutoSize = true;
            this.lblPasswordError.BackColor = System.Drawing.Color.Red;
            this.lblPasswordError.Location = new System.Drawing.Point(495, 140);
            this.lblPasswordError.Name = "lblPasswordError";
            this.lblPasswordError.Size = new System.Drawing.Size(0, 13);
            this.lblPasswordError.TabIndex = 40;
            // 
            // lblUsernameError
            // 
            this.lblUsernameError.AutoSize = true;
            this.lblUsernameError.BackColor = System.Drawing.Color.Red;
            this.lblUsernameError.Location = new System.Drawing.Point(495, 114);
            this.lblUsernameError.Name = "lblUsernameError";
            this.lblUsernameError.Size = new System.Drawing.Size(0, 13);
            this.lblUsernameError.TabIndex = 39;
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(48, 236);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(35, 13);
            this.lblState.TabIndex = 38;
            this.lblState.Text = "State:";
            // 
            // btnCheckState
            // 
            this.btnCheckState.Location = new System.Drawing.Point(176, 205);
            this.btnCheckState.Name = "btnCheckState";
            this.btnCheckState.Size = new System.Drawing.Size(82, 23);
            this.btnCheckState.TabIndex = 31;
            this.btnCheckState.Text = "Check State";
            this.btnCheckState.UseVisualStyleBackColor = true;
            this.btnCheckState.Click += new System.EventHandler(this.btnCheckState_Click);
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(96, 137);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(660, 20);
            this.tbPassword.TabIndex = 29;
            this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Password:";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(96, 111);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(660, 20);
            this.tbUsername.TabIndex = 28;
            this.tbUsername.TextChanged += new System.EventHandler(this.tbUsername_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Username:";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Location = new System.Drawing.Point(96, 85);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.Size = new System.Drawing.Size(660, 20);
            this.tbDatabaseName.TabIndex = 27;
            this.tbDatabaseName.TextChanged += new System.EventHandler(this.tbDatabaseName_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Database:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(96, 7);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(660, 20);
            this.tbID.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(69, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "ID:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(96, 33);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(660, 20);
            this.tbName.TabIndex = 25;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(52, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Name*:";
            // 
            // tbServerName
            // 
            this.tbServerName.Location = new System.Drawing.Point(96, 59);
            this.tbServerName.Name = "tbServerName";
            this.tbServerName.Size = new System.Drawing.Size(660, 20);
            this.tbServerName.TabIndex = 26;
            this.tbServerName.TextChanged += new System.EventHandler(this.tbServerName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 37;
            this.label3.Text = "Server*:";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(96, 205);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 48;
            this.objectSaverButton1.Text = "objectSaverButton1";
            // 
            // ExternalDatabaseServerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.pbServer);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.btnClearKnownType);
            this.Controls.Add(this.ddSetKnownType);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblPasswordError);
            this.Controls.Add(this.lblUsernameError);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.btnCheckState);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbDatabaseName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbServerName);
            this.Controls.Add(this.label3);
            this.Name = "ExternalDatabaseServerUI";
            this.Size = new System.Drawing.Size(1021, 766);
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbServer;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnClearKnownType;
        private System.Windows.Forms.ComboBox ddSetKnownType;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblPasswordError;
        private System.Windows.Forms.Label lblUsernameError;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Button btnCheckState;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDatabaseName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbServerName;
        private System.Windows.Forms.Label label3;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}
