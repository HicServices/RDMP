using CatalogueManager.LocationsMenu.Ticketing;

namespace CatalogueManager.Issues
{
    partial class IssueUI
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
            this.pSQL = new System.Windows.Forms.Panel();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ddStatus = new System.Windows.Forms.ComboBox();
            this.tbUserWhoCreated = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbDateOfLastStatusChange = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbDateCreated = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbUserWhoLastChangedStatus = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbAction = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.ddSeverity = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbNotesToResearcher = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.tbReportedBy = new System.Windows.Forms.TextBox();
            this.tbOwner = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tbReportedOnDate = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.btnChangeReportedBy = new System.Windows.Forms.Button();
            this.btnChangeOwner = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.tbPathToExcelSheetWithAdditionalInformation = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.lbError = new System.Windows.Forms.Label();
            this.ticketingControl1 = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.SuspendLayout();
            // 
            // pSQL
            // 
            this.pSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSQL.Location = new System.Drawing.Point(3, 519);
            this.pSQL.Name = "pSQL";
            this.pSQL.Size = new System.Drawing.Size(1182, 280);
            this.pSQL.TabIndex = 39;
            // 
            // tbDescription
            // 
            this.tbDescription.AcceptsReturn = true;
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(83, 60);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbDescription.Size = new System.Drawing.Size(1087, 140);
            this.tbDescription.TabIndex = 38;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Description:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(83, 24);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(794, 20);
            this.tbName.TabIndex = 36;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Name:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(83, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "ID:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 503);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 43;
            this.label1.Text = "SQL to reproduce issue:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 210);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 44;
            this.label5.Text = "Status:";
            // 
            // ddStatus
            // 
            this.ddStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddStatus.FormattingEnabled = true;
            this.ddStatus.Location = new System.Drawing.Point(83, 207);
            this.ddStatus.Name = "ddStatus";
            this.ddStatus.Size = new System.Drawing.Size(165, 21);
            this.ddStatus.TabIndex = 45;
            this.ddStatus.SelectedIndexChanged += new System.EventHandler(this.ddStatus_SelectedIndexChanged);
            // 
            // tbUserWhoCreated
            // 
            this.tbUserWhoCreated.Location = new System.Drawing.Point(255, 4);
            this.tbUserWhoCreated.Name = "tbUserWhoCreated";
            this.tbUserWhoCreated.ReadOnly = true;
            this.tbUserWhoCreated.Size = new System.Drawing.Size(136, 20);
            this.tbUserWhoCreated.TabIndex = 48;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(187, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 47;
            this.label7.Text = "Created By:";
            // 
            // tbDateOfLastStatusChange
            // 
            this.tbDateOfLastStatusChange.Location = new System.Drawing.Point(410, 209);
            this.tbDateOfLastStatusChange.Name = "tbDateOfLastStatusChange";
            this.tbDateOfLastStatusChange.ReadOnly = true;
            this.tbDateOfLastStatusChange.Size = new System.Drawing.Size(142, 20);
            this.tbDateOfLastStatusChange.TabIndex = 50;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 212);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(141, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "Date of Last Status Change:";
            // 
            // tbDateCreated
            // 
            this.tbDateCreated.Location = new System.Drawing.Point(453, 3);
            this.tbDateCreated.Name = "tbDateCreated";
            this.tbDateCreated.ReadOnly = true;
            this.tbDateCreated.Size = new System.Drawing.Size(144, 20);
            this.tbDateCreated.TabIndex = 52;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(397, 4);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "On Date:";
            // 
            // tbUserWhoLastChangedStatus
            // 
            this.tbUserWhoLastChangedStatus.Location = new System.Drawing.Point(632, 207);
            this.tbUserWhoLastChangedStatus.Name = "tbUserWhoLastChangedStatus";
            this.tbUserWhoLastChangedStatus.ReadOnly = true;
            this.tbUserWhoLastChangedStatus.Size = new System.Drawing.Size(157, 20);
            this.tbUserWhoLastChangedStatus.TabIndex = 54;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(558, 210);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 13);
            this.label10.TabIndex = 53;
            this.label10.Text = "Changed By:";
            // 
            // tbAction
            // 
            this.tbAction.AcceptsReturn = true;
            this.tbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbAction.Location = new System.Drawing.Point(84, 261);
            this.tbAction.Multiline = true;
            this.tbAction.Name = "tbAction";
            this.tbAction.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbAction.Size = new System.Drawing.Size(1041, 70);
            this.tbAction.TabIndex = 56;
            this.tbAction.TextChanged += new System.EventHandler(this.tbAction_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(0, 261);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 55;
            this.label11.Text = "Action:";
            // 
            // ddSeverity
            // 
            this.ddSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddSeverity.FormattingEnabled = true;
            this.ddSeverity.Location = new System.Drawing.Point(84, 234);
            this.ddSeverity.Name = "ddSeverity";
            this.ddSeverity.Size = new System.Drawing.Size(165, 21);
            this.ddSeverity.TabIndex = 58;
            this.ddSeverity.SelectedIndexChanged += new System.EventHandler(this.ddSeverity_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 237);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(48, 13);
            this.label12.TabIndex = 57;
            this.label12.Text = "Severity:";
            // 
            // tbNotesToResearcher
            // 
            this.tbNotesToResearcher.AcceptsReturn = true;
            this.tbNotesToResearcher.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNotesToResearcher.Location = new System.Drawing.Point(83, 337);
            this.tbNotesToResearcher.Multiline = true;
            this.tbNotesToResearcher.Name = "tbNotesToResearcher";
            this.tbNotesToResearcher.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbNotesToResearcher.Size = new System.Drawing.Size(1042, 70);
            this.tbNotesToResearcher.TabIndex = 60;
            this.tbNotesToResearcher.TextChanged += new System.EventHandler(this.tbNotesToResearcher_TextChanged);
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(1, 337);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(71, 70);
            this.label13.TabIndex = 59;
            this.label13.Text = "Notes to Researcher:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 416);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(69, 13);
            this.label14.TabIndex = 61;
            this.label14.Text = "Reported By:";
            // 
            // tbReportedBy
            // 
            this.tbReportedBy.Location = new System.Drawing.Point(84, 413);
            this.tbReportedBy.Name = "tbReportedBy";
            this.tbReportedBy.ReadOnly = true;
            this.tbReportedBy.Size = new System.Drawing.Size(100, 20);
            this.tbReportedBy.TabIndex = 62;
            // 
            // tbOwner
            // 
            this.tbOwner.Location = new System.Drawing.Point(85, 439);
            this.tbOwner.Name = "tbOwner";
            this.tbOwner.ReadOnly = true;
            this.tbOwner.Size = new System.Drawing.Size(100, 20);
            this.tbOwner.TabIndex = 64;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 442);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(41, 13);
            this.label15.TabIndex = 63;
            this.label15.Text = "Owner:";
            // 
            // tbReportedOnDate
            // 
            this.tbReportedOnDate.Location = new System.Drawing.Point(339, 413);
            this.tbReportedOnDate.Name = "tbReportedOnDate";
            this.tbReportedOnDate.Size = new System.Drawing.Size(201, 20);
            this.tbReportedOnDate.TabIndex = 66;
            this.tbReportedOnDate.TextChanged += new System.EventHandler(this.tbReportedOnDate_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(283, 416);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(50, 13);
            this.label16.TabIndex = 65;
            this.label16.Text = "On Date:";
            // 
            // btnChangeReportedBy
            // 
            this.btnChangeReportedBy.Location = new System.Drawing.Point(190, 410);
            this.btnChangeReportedBy.Name = "btnChangeReportedBy";
            this.btnChangeReportedBy.Size = new System.Drawing.Size(75, 23);
            this.btnChangeReportedBy.TabIndex = 67;
            this.btnChangeReportedBy.Text = "Change";
            this.btnChangeReportedBy.UseVisualStyleBackColor = true;
            this.btnChangeReportedBy.Click += new System.EventHandler(this.btnChangeReportedBy_Click);
            // 
            // btnChangeOwner
            // 
            this.btnChangeOwner.Location = new System.Drawing.Point(191, 437);
            this.btnChangeOwner.Name = "btnChangeOwner";
            this.btnChangeOwner.Size = new System.Drawing.Size(75, 23);
            this.btnChangeOwner.TabIndex = 68;
            this.btnChangeOwner.Text = "Change";
            this.btnChangeOwner.UseVisualStyleBackColor = true;
            this.btnChangeOwner.Click += new System.EventHandler(this.btnChangeOwner_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(3, 479);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(219, 13);
            this.label17.TabIndex = 69;
            this.label17.Text = "PathToExcelSheetWithAdditionalInformation:";
            // 
            // tbPathToExcelSheetWithAdditionalInformation
            // 
            this.tbPathToExcelSheetWithAdditionalInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPathToExcelSheetWithAdditionalInformation.Location = new System.Drawing.Point(228, 476);
            this.tbPathToExcelSheetWithAdditionalInformation.Name = "tbPathToExcelSheetWithAdditionalInformation";
            this.tbPathToExcelSheetWithAdditionalInformation.Size = new System.Drawing.Size(791, 20);
            this.tbPathToExcelSheetWithAdditionalInformation.TabIndex = 70;
            this.tbPathToExcelSheetWithAdditionalInformation.TextChanged += new System.EventHandler(this.tbPathToExcelSheetWithAdditionalInformation_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(1025, 474);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 71;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.Location = new System.Drawing.Point(1106, 474);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(75, 23);
            this.btnOpenFolder.TabIndex = 72;
            this.btnOpenFolder.Text = "Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFile.Location = new System.Drawing.Point(1106, 445);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(75, 23);
            this.btnOpenFile.TabIndex = 72;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // lbError
            // 
            this.lbError.AutoSize = true;
            this.lbError.ForeColor = System.Drawing.Color.Red;
            this.lbError.Location = new System.Drawing.Point(796, 209);
            this.lbError.Name = "lbError";
            this.lbError.Size = new System.Drawing.Size(98, 13);
            this.lbError.TabIndex = 73;
            this.lbError.Text = "error text goes here";
            // 
            // ticketingControl1
            // 
            this.ticketingControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ticketingControl1.Location = new System.Drawing.Point(883, 0);
            this.ticketingControl1.Name = "ticketingControl1";
            this.ticketingControl1.Size = new System.Drawing.Size(303, 54);
            this.ticketingControl1.TabIndex = 74;
            this.ticketingControl1.TicketText = "";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(0, 77);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 75;
            this.objectSaverButton1.Text = "objectSaverButton1";
            // 
            // IssueUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.ticketingControl1);
            this.Controls.Add(this.lbError);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbPathToExcelSheetWithAdditionalInformation);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.btnChangeOwner);
            this.Controls.Add(this.btnChangeReportedBy);
            this.Controls.Add(this.tbReportedOnDate);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.tbOwner);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.tbReportedBy);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbNotesToResearcher);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.ddSeverity);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbAction);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbUserWhoLastChangedStatus);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbDateCreated);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbDateOfLastStatusChange);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbUserWhoCreated);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.ddStatus);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pSQL);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label2);
            this.Name = "IssueUI";
            this.Size = new System.Drawing.Size(1192, 833);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pSQL;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ddStatus;
        private System.Windows.Forms.TextBox tbUserWhoCreated;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbDateOfLastStatusChange;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbDateCreated;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbUserWhoLastChangedStatus;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbAction;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox ddSeverity;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbNotesToResearcher;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbReportedBy;
        private System.Windows.Forms.TextBox tbOwner;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tbReportedOnDate;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button btnChangeReportedBy;
        private System.Windows.Forms.Button btnChangeOwner;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbPathToExcelSheetWithAdditionalInformation;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Label lbError;
        private TicketingControl ticketingControl1;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
    }
}
