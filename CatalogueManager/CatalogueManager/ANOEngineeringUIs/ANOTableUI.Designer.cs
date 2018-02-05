namespace CatalogueManager.ANOEngineeringUIs
{
    partial class ANOTableUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ANOTableUI));
            this.tbID = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.nCharacters = new System.Windows.Forms.NumericUpDown();
            this.nIntegers = new System.Windows.Forms.NumericUpDown();
            this.btnFinalise = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.btnDropANOTable = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSuffix = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbInputDataType = new System.Windows.Forms.TextBox();
            this.lblPrivate = new System.Windows.Forms.Label();
            this.lblPublic = new System.Windows.Forms.Label();
            this.lblANOTableName = new System.Windows.Forms.Label();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.pbServer = new System.Windows.Forms.PictureBox();
            this.gbPushedTable = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.nCharacters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nIntegers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).BeginInit();
            this.gbPushedTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(55, 2);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(99, 20);
            this.tbID.TabIndex = 14;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "ID:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(223, 108);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "NumberOLetters:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(1, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "NumberOfIntegers:";
            // 
            // nCharacters
            // 
            this.nCharacters.Location = new System.Drawing.Point(312, 106);
            this.nCharacters.Name = "nCharacters";
            this.nCharacters.Size = new System.Drawing.Size(120, 20);
            this.nCharacters.TabIndex = 10;
            this.nCharacters.ValueChanged += new System.EventHandler(this.nCharacters_ValueChanged);
            // 
            // nIntegers
            // 
            this.nIntegers.Location = new System.Drawing.Point(97, 106);
            this.nIntegers.Name = "nIntegers";
            this.nIntegers.Size = new System.Drawing.Size(120, 20);
            this.nIntegers.TabIndex = 10;
            this.nIntegers.ValueChanged += new System.EventHandler(this.nIntegers_ValueChanged);
            // 
            // btnFinalise
            // 
            this.btnFinalise.Location = new System.Drawing.Point(5, 132);
            this.btnFinalise.Name = "btnFinalise";
            this.btnFinalise.Size = new System.Drawing.Size(205, 23);
            this.btnFinalise.TabIndex = 12;
            this.btnFinalise.Text = "Finalise and Push ANOTable";
            this.btnFinalise.UseVisualStyleBackColor = true;
            this.btnFinalise.Click += new System.EventHandler(this.btnFinalise_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(216, 144);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 15;
            // 
            // btnDropANOTable
            // 
            this.btnDropANOTable.Location = new System.Drawing.Point(5, 161);
            this.btnDropANOTable.Name = "btnDropANOTable";
            this.btnDropANOTable.Size = new System.Drawing.Size(205, 23);
            this.btnDropANOTable.TabIndex = 16;
            this.btnDropANOTable.Text = "Drop ANOTable";
            this.btnDropANOTable.UseVisualStyleBackColor = true;
            this.btnDropANOTable.Click += new System.EventHandler(this.btnDropANOTable_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(55, 28);
            this.tbName.Name = "tbName";
            this.tbName.ReadOnly = true;
            this.tbName.Size = new System.Drawing.Size(207, 20);
            this.tbName.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Suffix:";
            // 
            // tbSuffix
            // 
            this.tbSuffix.Location = new System.Drawing.Point(55, 54);
            this.tbSuffix.Name = "tbSuffix";
            this.tbSuffix.ReadOnly = true;
            this.tbSuffix.Size = new System.Drawing.Size(99, 20);
            this.tbSuffix.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Input Datatype:";
            // 
            // tbInputDataType
            // 
            this.tbInputDataType.Location = new System.Drawing.Point(97, 80);
            this.tbInputDataType.Name = "tbInputDataType";
            this.tbInputDataType.Size = new System.Drawing.Size(207, 20);
            this.tbInputDataType.TabIndex = 14;
            // 
            // lblPrivate
            // 
            this.lblPrivate.BackColor = System.Drawing.Color.Blue;
            this.lblPrivate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPrivate.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblPrivate.Location = new System.Drawing.Point(6, 35);
            this.lblPrivate.Name = "lblPrivate";
            this.lblPrivate.Size = new System.Drawing.Size(200, 16);
            this.lblPrivate.TabIndex = 17;
            this.lblPrivate.Text = "lblPrivate";
            this.lblPrivate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPublic
            // 
            this.lblPublic.BackColor = System.Drawing.Color.Blue;
            this.lblPublic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPublic.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblPublic.Location = new System.Drawing.Point(212, 35);
            this.lblPublic.Name = "lblPublic";
            this.lblPublic.Size = new System.Drawing.Size(200, 16);
            this.lblPublic.TabIndex = 17;
            this.lblPublic.Text = "lblPublic";
            this.lblPublic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblANOTableName
            // 
            this.lblANOTableName.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblANOTableName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblANOTableName.ForeColor = System.Drawing.Color.Black;
            this.lblANOTableName.Location = new System.Drawing.Point(6, 16);
            this.lblANOTableName.Name = "lblANOTableName";
            this.lblANOTableName.Size = new System.Drawing.Size(406, 16);
            this.lblANOTableName.TabIndex = 17;
            this.lblANOTableName.Text = "ANOTableName";
            this.lblANOTableName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRowCount
            // 
            this.lblRowCount.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblRowCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblRowCount.ForeColor = System.Drawing.Color.Black;
            this.lblRowCount.Location = new System.Drawing.Point(6, 55);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(406, 16);
            this.lblRowCount.TabIndex = 17;
            this.lblRowCount.Text = "label4";
            this.lblRowCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(30, 193);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(38, 13);
            this.lblServer.TabIndex = 18;
            this.lblServer.Text = "Server";
            // 
            // pbServer
            // 
            this.pbServer.Image = ((System.Drawing.Image)(resources.GetObject("pbServer.Image")));
            this.pbServer.Location = new System.Drawing.Point(5, 190);
            this.pbServer.Name = "pbServer";
            this.pbServer.Size = new System.Drawing.Size(19, 19);
            this.pbServer.TabIndex = 19;
            this.pbServer.TabStop = false;
            // 
            // gbPushedTable
            // 
            this.gbPushedTable.Controls.Add(this.lblANOTableName);
            this.gbPushedTable.Controls.Add(this.lblPrivate);
            this.gbPushedTable.Controls.Add(this.lblRowCount);
            this.gbPushedTable.Controls.Add(this.lblPublic);
            this.gbPushedTable.Location = new System.Drawing.Point(5, 215);
            this.gbPushedTable.Name = "gbPushedTable";
            this.gbPushedTable.Size = new System.Drawing.Size(418, 77);
            this.gbPushedTable.TabIndex = 20;
            this.gbPushedTable.TabStop = false;
            this.gbPushedTable.Text = "PushedTable";
            // 
            // ANOTableUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbPushedTable);
            this.Controls.Add(this.pbServer);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.btnDropANOTable);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.tbSuffix);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbInputDataType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnFinalise);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.nIntegers);
            this.Controls.Add(this.nCharacters);
            this.Name = "ANOTableUI";
            this.Size = new System.Drawing.Size(941, 471);
            ((System.ComponentModel.ISupportInitialize)(this.nCharacters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nIntegers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbServer)).EndInit();
            this.gbPushedTable.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nCharacters;
        private System.Windows.Forms.NumericUpDown nIntegers;
        private System.Windows.Forms.Button btnFinalise;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnDropANOTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSuffix;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbInputDataType;
        private System.Windows.Forms.Label lblPrivate;
        private System.Windows.Forms.Label lblPublic;
        private System.Windows.Forms.Label lblANOTableName;
        private System.Windows.Forms.Label lblRowCount;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.PictureBox pbServer;
        private System.Windows.Forms.GroupBox gbPushedTable;
    }
}
