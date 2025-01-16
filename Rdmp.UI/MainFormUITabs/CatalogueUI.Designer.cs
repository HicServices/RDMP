using System.Windows.Forms;
using Rdmp.UI.LocationsMenu.Ticketing;

namespace Rdmp.UI.MainFormUITabs
{
    partial class CatalogueUI
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
            components = new System.ComponentModel.Container();
            tbDatasetStartDate = new TextBox();
            label41 = new Label();
            label28 = new Label();
            c_tbSubjectNumbers = new TextBox();
            tbDataStandards = new TextBox();
            label26 = new Label();
            label27 = new Label();
            tbCountryOfOrigin = new TextBox();
            label23 = new Label();
            label22 = new Label();
            c_ddGranularity = new ComboBox();
            label21 = new Label();
            label64 = new Label();
            label20 = new Label();
            c_tbDetailPageURL = new TextBox();
            label19 = new Label();
            label18 = new Label();
            label16 = new Label();
            c_tbUpdateFrequency = new TextBox();
            label4 = new Label();
            label17 = new Label();
            c_tbSourceUrl = new TextBox();
            c_tbTopics = new TextBox();
            label14 = new Label();
            c_tbQueryToolUrl = new TextBox();
            c_tbBackgroundSummary = new TextBox();
            label6 = new Label();
            c_tbBulkDownloadUrl = new TextBox();
            c_tbTimeCoverage = new TextBox();
            label7 = new Label();
            label12 = new Label();
            c_tbBrowseUrl = new TextBox();
            c_tbUpdateSchedule = new TextBox();
            label13 = new Label();
            c_tbAPIAccessURL = new TextBox();
            label10 = new Label();
            c_ddPeriodicity = new ComboBox();
            c_tbAccessOptions = new TextBox();
            c_tbLastRevisionDate = new TextBox();
            label9 = new Label();
            c_ddType = new ComboBox();
            c_tbResourceOwner = new TextBox();
            c_tbAttributionCitation = new TextBox();
            label8 = new Label();
            c_tbNumberOfThese = new TextBox();
            splitContainer1 = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            cbDeprecated = new CheckBox();
            cbInternal = new CheckBox();
            cbColdStorage = new CheckBox();
            tbFolder = new TextBox();
            tbAcronym = new TextBox();
            tbName = new TextBox();
            c_tbID = new TextBox();
            label1 = new Label();
            label24 = new Label();
            label2 = new Label();
            label25 = new Label();
            label3 = new Label();
            panel1 = new Panel();
            lbDatasetslbl = new Label();
            lbDatasets = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            tbSourceOfDataCollection = new TextBox();
            tbEthicsApprover = new TextBox();
            label33 = new Label();
            ddExplicitConsent = new ComboBox();
            label32 = new Label();
            tbAdministrativeContactAddress = new TextBox();
            label31 = new Label();
            tbAdministrativeContactTelephone = new TextBox();
            label11 = new Label();
            tbAdministrativeContactEmail = new TextBox();
            label30 = new Label();
            tbAdministrativeContactName = new TextBox();
            label29 = new Label();
            label15 = new Label();
            label5 = new Label();
            c_tbGeographicalCoverage = new TextBox();
            ticketingControl1 = new TicketingControlUI();
            errorProvider1 = new ErrorProvider(components);
            panel2 = new Panel();
            editableCatalogueName = new SimpleControls.EditableLabelUI();
            editableFolder = new SimpleControls.EditableLabelUI();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // tbDatasetStartDate
            // 
            tbDatasetStartDate.Anchor = AnchorStyles.Left;
            tbDatasetStartDate.Location = new System.Drawing.Point(135, 674);
            tbDatasetStartDate.Margin = new Padding(4, 3, 20, 3);
            tbDatasetStartDate.Name = "tbDatasetStartDate";
            tbDatasetStartDate.Size = new System.Drawing.Size(250, 23);
            tbDatasetStartDate.TabIndex = 14;
            tbDatasetStartDate.TextChanged += tbDatasetStartDate_TextChanged;
            // 
            // label41
            // 
            label41.Anchor = AnchorStyles.Right;
            label41.Location = new System.Drawing.Point(7, 675);
            label41.Margin = new Padding(4, 0, 4, 0);
            label41.Name = "label41";
            label41.Size = new System.Drawing.Size(120, 20);
            label41.TabIndex = 147;
            label41.Text = "Dataset Start Date:";
            label41.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label28
            // 
            label28.Anchor = AnchorStyles.Right;
            label28.ForeColor = System.Drawing.Color.Black;
            label28.Location = new System.Drawing.Point(7, 616);
            label28.Margin = new Padding(4, 0, 4, 0);
            label28.Name = "label28";
            label28.Size = new System.Drawing.Size(120, 23);
            label28.TabIndex = 134;
            label28.Text = "Subject Numbers:";
            label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbSubjectNumbers
            // 
            c_tbSubjectNumbers.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbSubjectNumbers.BackColor = System.Drawing.SystemColors.Window;
            c_tbSubjectNumbers.Location = new System.Drawing.Point(135, 616);
            c_tbSubjectNumbers.Margin = new Padding(4, 3, 20, 3);
            c_tbSubjectNumbers.Name = "c_tbSubjectNumbers";
            c_tbSubjectNumbers.Size = new System.Drawing.Size(369, 23);
            c_tbSubjectNumbers.TabIndex = 12;
            // 
            // tbDataStandards
            // 
            tbDataStandards.AcceptsReturn = true;
            tbDataStandards.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbDataStandards.Location = new System.Drawing.Point(135, 1012);
            tbDataStandards.Margin = new Padding(4, 3, 20, 3);
            tbDataStandards.Multiline = true;
            tbDataStandards.Name = "tbDataStandards";
            tbDataStandards.ScrollBars = ScrollBars.Vertical;
            tbDataStandards.Size = new System.Drawing.Size(369, 100);
            tbDataStandards.TabIndex = 23;
            // 
            // label26
            // 
            label26.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label26.Location = new System.Drawing.Point(7, 1009);
            label26.Margin = new Padding(4, 0, 4, 0);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(120, 20);
            label26.TabIndex = 115;
            label26.Text = "Data Standards:";
            label26.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label27
            // 
            label27.Anchor = AnchorStyles.Right;
            label27.AutoSize = true;
            label27.Location = new System.Drawing.Point(24, 987);
            label27.Margin = new Padding(4, 0, 4, 0);
            label27.Name = "label27";
            label27.Size = new System.Drawing.Size(103, 15);
            label27.TabIndex = 114;
            label27.Text = "Country of Origin:";
            label27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbCountryOfOrigin
            // 
            tbCountryOfOrigin.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbCountryOfOrigin.Location = new System.Drawing.Point(135, 983);
            tbCountryOfOrigin.Margin = new Padding(4, 3, 20, 3);
            tbCountryOfOrigin.Name = "tbCountryOfOrigin";
            tbCountryOfOrigin.Size = new System.Drawing.Size(369, 23);
            tbCountryOfOrigin.TabIndex = 22;
            // 
            // label23
            // 
            label23.Anchor = AnchorStyles.Right;
            label23.AutoSize = true;
            label23.Location = new System.Drawing.Point(57, 958);
            label23.Margin = new Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(70, 15);
            label23.TabIndex = 104;
            label23.Text = "Source URL:";
            label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label22
            // 
            label22.Anchor = AnchorStyles.Right;
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(36, 929);
            label22.Margin = new Padding(4, 0, 4, 0);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(91, 15);
            label22.TabIndex = 103;
            label22.Text = "Query Tool URL:";
            label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_ddGranularity
            // 
            c_ddGranularity.Anchor = AnchorStyles.Left;
            c_ddGranularity.DropDownStyle = ComboBoxStyle.DropDownList;
            c_ddGranularity.FormattingEnabled = true;
            c_ddGranularity.Location = new System.Drawing.Point(135, 167);
            c_ddGranularity.Margin = new Padding(4, 3, 20, 3);
            c_ddGranularity.Name = "c_ddGranularity";
            c_ddGranularity.Size = new System.Drawing.Size(250, 23);
            c_ddGranularity.TabIndex = 3;
            // 
            // label21
            // 
            label21.Anchor = AnchorStyles.Right;
            label21.AutoSize = true;
            label21.Location = new System.Drawing.Point(13, 900);
            label21.Margin = new Padding(4, 0, 4, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(114, 15);
            label21.TabIndex = 102;
            label21.Text = "Bulk Download URL:";
            label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label64
            // 
            label64.Anchor = AnchorStyles.Right;
            label64.AutoSize = true;
            label64.Location = new System.Drawing.Point(59, 171);
            label64.Margin = new Padding(4, 0, 4, 0);
            label64.Name = "label64";
            label64.Size = new System.Drawing.Size(68, 15);
            label64.TabIndex = 108;
            label64.Text = "Granularity:";
            label64.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label20
            // 
            label20.Anchor = AnchorStyles.Right;
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(55, 871);
            label20.Margin = new Padding(4, 0, 4, 0);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(72, 15);
            label20.TabIndex = 101;
            label20.Text = "Browse URL:";
            label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbDetailPageURL
            // 
            c_tbDetailPageURL.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbDetailPageURL.Location = new System.Drawing.Point(135, 3);
            c_tbDetailPageURL.Margin = new Padding(4, 3, 20, 3);
            c_tbDetailPageURL.Name = "c_tbDetailPageURL";
            c_tbDetailPageURL.Size = new System.Drawing.Size(369, 23);
            c_tbDetailPageURL.TabIndex = 0;
            c_tbDetailPageURL.TextChanged += c_tbDetailPageURL_TextChanged;
            // 
            // label19
            // 
            label19.Anchor = AnchorStyles.Right;
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(36, 842);
            label19.Margin = new Padding(4, 0, 4, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(91, 15);
            label19.TabIndex = 100;
            label19.Text = "API Access URL:";
            label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            label18.Anchor = AnchorStyles.Right;
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(36, 813);
            label18.Margin = new Padding(4, 0, 4, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(91, 15);
            label18.TabIndex = 99;
            label18.Text = "Access Options:";
            label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            label16.Anchor = AnchorStyles.Right;
            label16.Location = new System.Drawing.Point(7, 646);
            label16.Margin = new Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(120, 20);
            label16.TabIndex = 98;
            label16.Text = "Resource Owner:";
            label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbUpdateFrequency
            // 
            c_tbUpdateFrequency.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbUpdateFrequency.Location = new System.Drawing.Point(135, 500);
            c_tbUpdateFrequency.Margin = new Padding(4, 3, 20, 3);
            c_tbUpdateFrequency.Name = "c_tbUpdateFrequency";
            c_tbUpdateFrequency.Size = new System.Drawing.Size(369, 23);
            c_tbUpdateFrequency.TabIndex = 7;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(34, 7);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(93, 15);
            label4.TabIndex = 65;
            label4.Text = "Detail Page URL:";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            label17.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label17.Location = new System.Drawing.Point(7, 700);
            label17.Margin = new Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(120, 20);
            label17.TabIndex = 97;
            label17.Text = "Attribution Citation:";
            label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // c_tbSourceUrl
            // 
            c_tbSourceUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbSourceUrl.Location = new System.Drawing.Point(135, 954);
            c_tbSourceUrl.Margin = new Padding(4, 3, 20, 3);
            c_tbSourceUrl.Name = "c_tbSourceUrl";
            c_tbSourceUrl.Size = new System.Drawing.Size(369, 23);
            c_tbSourceUrl.TabIndex = 21;
            c_tbSourceUrl.TextChanged += c_tbSourceUrl_TextChanged;
            // 
            // c_tbTopics
            // 
            c_tbTopics.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbTopics.Location = new System.Drawing.Point(135, 331);
            c_tbTopics.Margin = new Padding(4, 3, 20, 3);
            c_tbTopics.Multiline = true;
            c_tbTopics.Name = "c_tbTopics";
            c_tbTopics.ScrollBars = ScrollBars.Vertical;
            c_tbTopics.Size = new System.Drawing.Size(369, 100);
            c_tbTopics.TabIndex = 6;
            // 
            // label14
            // 
            label14.Anchor = AnchorStyles.Right;
            label14.Location = new System.Drawing.Point(7, 587);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(120, 23);
            label14.TabIndex = 96;
            label14.Text = "Last Revision Date:";
            label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbQueryToolUrl
            // 
            c_tbQueryToolUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbQueryToolUrl.Location = new System.Drawing.Point(135, 925);
            c_tbQueryToolUrl.Margin = new Padding(4, 3, 20, 3);
            c_tbQueryToolUrl.Name = "c_tbQueryToolUrl";
            c_tbQueryToolUrl.Size = new System.Drawing.Size(369, 23);
            c_tbQueryToolUrl.TabIndex = 20;
            c_tbQueryToolUrl.TextChanged += c_tbQueryToolUrl_TextChanged;
            // 
            // c_tbBackgroundSummary
            // 
            c_tbBackgroundSummary.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbBackgroundSummary.Location = new System.Drawing.Point(135, 196);
            c_tbBackgroundSummary.Margin = new Padding(4, 3, 20, 3);
            c_tbBackgroundSummary.Multiline = true;
            c_tbBackgroundSummary.Name = "c_tbBackgroundSummary";
            c_tbBackgroundSummary.ScrollBars = ScrollBars.Vertical;
            c_tbBackgroundSummary.Size = new System.Drawing.Size(369, 100);
            c_tbBackgroundSummary.TabIndex = 4;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(42, 36);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(85, 15);
            label6.TabIndex = 69;
            label6.Text = "Resource Type:";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbBulkDownloadUrl
            // 
            c_tbBulkDownloadUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbBulkDownloadUrl.Location = new System.Drawing.Point(135, 896);
            c_tbBulkDownloadUrl.Margin = new Padding(4, 3, 20, 3);
            c_tbBulkDownloadUrl.Name = "c_tbBulkDownloadUrl";
            c_tbBulkDownloadUrl.Size = new System.Drawing.Size(369, 23);
            c_tbBulkDownloadUrl.TabIndex = 19;
            c_tbBulkDownloadUrl.TextChanged += c_tbBulkDownloadUrl_TextChanged;
            // 
            // c_tbTimeCoverage
            // 
            c_tbTimeCoverage.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbTimeCoverage.Location = new System.Drawing.Point(135, 558);
            c_tbTimeCoverage.Margin = new Padding(4, 3, 20, 3);
            c_tbTimeCoverage.Name = "c_tbTimeCoverage";
            c_tbTimeCoverage.Size = new System.Drawing.Size(369, 23);
            c_tbTimeCoverage.TabIndex = 10;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(61, 306);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(66, 15);
            label7.TabIndex = 72;
            label7.Text = "Periodicity:";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Right;
            label12.Location = new System.Drawing.Point(7, 529);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(120, 23);
            label12.TabIndex = 93;
            label12.Text = "Update Schedule:";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbBrowseUrl
            // 
            c_tbBrowseUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbBrowseUrl.Location = new System.Drawing.Point(135, 867);
            c_tbBrowseUrl.Margin = new Padding(4, 3, 20, 3);
            c_tbBrowseUrl.Name = "c_tbBrowseUrl";
            c_tbBrowseUrl.Size = new System.Drawing.Size(369, 23);
            c_tbBrowseUrl.TabIndex = 18;
            c_tbBrowseUrl.TextChanged += c_tbBrowseUrl_TextChanged;
            // 
            // c_tbUpdateSchedule
            // 
            c_tbUpdateSchedule.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbUpdateSchedule.Location = new System.Drawing.Point(135, 529);
            c_tbUpdateSchedule.Margin = new Padding(4, 3, 20, 3);
            c_tbUpdateSchedule.Name = "c_tbUpdateSchedule";
            c_tbUpdateSchedule.Size = new System.Drawing.Size(369, 23);
            c_tbUpdateSchedule.TabIndex = 9;
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Right;
            label13.Location = new System.Drawing.Point(7, 558);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(120, 23);
            label13.TabIndex = 89;
            label13.Text = "Time Coverage:";
            label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_tbAPIAccessURL
            // 
            c_tbAPIAccessURL.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbAPIAccessURL.Location = new System.Drawing.Point(135, 838);
            c_tbAPIAccessURL.Margin = new Padding(4, 3, 20, 3);
            c_tbAPIAccessURL.Name = "c_tbAPIAccessURL";
            c_tbAPIAccessURL.Size = new System.Drawing.Size(369, 23);
            c_tbAPIAccessURL.TabIndex = 17;
            c_tbAPIAccessURL.TextChanged += c_tbAPIAccessURL_TextChanged;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Right;
            label10.Location = new System.Drawing.Point(7, 500);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(120, 23);
            label10.TabIndex = 77;
            label10.Text = "Update Frequency:";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_ddPeriodicity
            // 
            c_ddPeriodicity.Anchor = AnchorStyles.Left;
            c_ddPeriodicity.DropDownStyle = ComboBoxStyle.DropDownList;
            c_ddPeriodicity.FormattingEnabled = true;
            c_ddPeriodicity.Location = new System.Drawing.Point(135, 302);
            c_ddPeriodicity.Margin = new Padding(4, 3, 20, 3);
            c_ddPeriodicity.Name = "c_ddPeriodicity";
            c_ddPeriodicity.Size = new System.Drawing.Size(250, 23);
            c_ddPeriodicity.TabIndex = 5;
            // 
            // c_tbAccessOptions
            // 
            c_tbAccessOptions.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbAccessOptions.Location = new System.Drawing.Point(135, 809);
            c_tbAccessOptions.Margin = new Padding(4, 3, 20, 3);
            c_tbAccessOptions.Name = "c_tbAccessOptions";
            c_tbAccessOptions.Size = new System.Drawing.Size(369, 23);
            c_tbAccessOptions.TabIndex = 16;
            // 
            // c_tbLastRevisionDate
            // 
            c_tbLastRevisionDate.Anchor = AnchorStyles.Left;
            c_tbLastRevisionDate.Location = new System.Drawing.Point(135, 587);
            c_tbLastRevisionDate.Margin = new Padding(4, 3, 20, 3);
            c_tbLastRevisionDate.Name = "c_tbLastRevisionDate";
            c_tbLastRevisionDate.Size = new System.Drawing.Size(250, 23);
            c_tbLastRevisionDate.TabIndex = 11;
            c_tbLastRevisionDate.TextChanged += c_tbLastRevisionDate_TextChanged;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label9.AutoSize = true;
            label9.ForeColor = System.Drawing.Color.Black;
            label9.Location = new System.Drawing.Point(53, 193);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(74, 15);
            label9.TabIndex = 79;
            label9.Text = "Background:";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // c_ddType
            // 
            c_ddType.Anchor = AnchorStyles.Left;
            c_ddType.DropDownStyle = ComboBoxStyle.DropDownList;
            c_ddType.FormattingEnabled = true;
            c_ddType.Location = new System.Drawing.Point(135, 32);
            c_ddType.Margin = new Padding(4, 3, 20, 3);
            c_ddType.Name = "c_ddType";
            c_ddType.Size = new System.Drawing.Size(250, 23);
            c_ddType.TabIndex = 1;
            // 
            // c_tbResourceOwner
            // 
            c_tbResourceOwner.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbResourceOwner.Location = new System.Drawing.Point(135, 645);
            c_tbResourceOwner.Margin = new Padding(4, 3, 20, 3);
            c_tbResourceOwner.Name = "c_tbResourceOwner";
            c_tbResourceOwner.Size = new System.Drawing.Size(369, 23);
            c_tbResourceOwner.TabIndex = 13;
            // 
            // c_tbAttributionCitation
            // 
            c_tbAttributionCitation.AcceptsReturn = true;
            c_tbAttributionCitation.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbAttributionCitation.Location = new System.Drawing.Point(135, 703);
            c_tbAttributionCitation.Margin = new Padding(4, 3, 20, 3);
            c_tbAttributionCitation.Multiline = true;
            c_tbAttributionCitation.Name = "c_tbAttributionCitation";
            c_tbAttributionCitation.ScrollBars = ScrollBars.Vertical;
            c_tbAttributionCitation.Size = new System.Drawing.Size(369, 100);
            c_tbAttributionCitation.TabIndex = 15;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(28, 328);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(99, 15);
            label8.TabIndex = 83;
            label8.Text = "Search Keywords:";
            label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // c_tbNumberOfThese
            // 
            c_tbNumberOfThese.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            c_tbNumberOfThese.Location = new System.Drawing.Point(601, 537);
            c_tbNumberOfThese.Name = "c_tbNumberOfThese";
            c_tbNumberOfThese.Size = new System.Drawing.Size(28, 23);
            c_tbNumberOfThese.TabIndex = 68;
            c_tbNumberOfThese.Visible = false;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.Fixed3D;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel2);
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel2);
            splitContainer1.Panel2MinSize = 200;
            splitContainer1.Size = new System.Drawing.Size(551, 1085);
            splitContainer1.SplitterDistance = 288;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 114;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 6);
            tableLayoutPanel1.Controls.Add(tbFolder, 1, 3);
            tableLayoutPanel1.Controls.Add(tbAcronym, 1, 2);
            tableLayoutPanel1.Controls.Add(tbName, 1, 1);
            tableLayoutPanel1.Controls.Add(c_tbID, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label24, 0, 3);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(label25, 0, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 7);
            tableLayoutPanel1.Controls.Add(panel1, 1, 7);
            tableLayoutPanel1.Controls.Add(lbDatasetslbl, 0, 8);
            tableLayoutPanel1.Controls.Add(lbDatasets, 1, 8);
            tableLayoutPanel1.Location = new System.Drawing.Point(31, 179);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(0, 0, 5, 0);
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 13F));
            tableLayoutPanel1.Size = new System.Drawing.Size(524, 278);
            tableLayoutPanel1.TabIndex = 160;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(cbDeprecated);
            flowLayoutPanel1.Controls.Add(cbInternal);
            flowLayoutPanel1.Controls.Add(cbColdStorage);
            flowLayoutPanel1.Location = new System.Drawing.Point(132, 119);
            flowLayoutPanel1.Margin = new Padding(3, 3, 20, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(367, 23);
            flowLayoutPanel1.TabIndex = 159;
            // 
            // cbDeprecated
            // 
            cbDeprecated.AutoSize = true;
            cbDeprecated.Location = new System.Drawing.Point(4, 3);
            cbDeprecated.Margin = new Padding(4, 3, 4, 3);
            cbDeprecated.Name = "cbDeprecated";
            cbDeprecated.Size = new System.Drawing.Size(86, 19);
            cbDeprecated.TabIndex = 5;
            cbDeprecated.Text = "Deprecated";
            cbDeprecated.UseVisualStyleBackColor = true;
            // 
            // cbInternal
            // 
            cbInternal.AutoSize = true;
            cbInternal.Location = new System.Drawing.Point(98, 3);
            cbInternal.Margin = new Padding(4, 3, 4, 3);
            cbInternal.Name = "cbInternal";
            cbInternal.Size = new System.Drawing.Size(66, 19);
            cbInternal.TabIndex = 6;
            cbInternal.Text = "Internal";
            cbInternal.UseVisualStyleBackColor = true;
            // 
            // cbColdStorage
            // 
            cbColdStorage.AutoSize = true;
            cbColdStorage.Location = new System.Drawing.Point(172, 3);
            cbColdStorage.Margin = new Padding(4, 3, 4, 3);
            cbColdStorage.Name = "cbColdStorage";
            cbColdStorage.Size = new System.Drawing.Size(91, 19);
            cbColdStorage.TabIndex = 7;
            cbColdStorage.Text = "ColdStorage";
            cbColdStorage.UseVisualStyleBackColor = true;
            // 
            // tbFolder
            // 
            tbFolder.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbFolder.Location = new System.Drawing.Point(133, 90);
            tbFolder.Margin = new Padding(4, 3, 20, 3);
            tbFolder.Name = "tbFolder";
            tbFolder.Size = new System.Drawing.Size(366, 23);
            tbFolder.TabIndex = 3;
            tbFolder.TextChanged += tbFolder_TextChanged;
            // 
            // tbAcronym
            // 
            tbAcronym.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbAcronym.Location = new System.Drawing.Point(133, 61);
            tbAcronym.Margin = new Padding(4, 3, 20, 3);
            tbAcronym.Name = "tbAcronym";
            tbAcronym.Size = new System.Drawing.Size(366, 23);
            tbAcronym.TabIndex = 2;
            // 
            // tbName
            // 
            tbName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(133, 32);
            tbName.Margin = new Padding(4, 3, 20, 3);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(366, 23);
            tbName.TabIndex = 1;
            tbName.TextChanged += tbName_TextChanged;
            // 
            // c_tbID
            // 
            c_tbID.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbID.Location = new System.Drawing.Point(133, 3);
            c_tbID.Margin = new Padding(4, 3, 20, 3);
            c_tbID.Name = "c_tbID";
            c_tbID.ReadOnly = true;
            c_tbID.Size = new System.Drawing.Size(366, 23);
            c_tbID.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoEllipsis = true;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(47, 7);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(78, 15);
            label1.TabIndex = 56;
            label1.Text = "Catalogue ID:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label24
            // 
            label24.Anchor = AnchorStyles.Right;
            label24.AutoSize = true;
            label24.Location = new System.Drawing.Point(82, 94);
            label24.Margin = new Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(43, 15);
            label24.TabIndex = 152;
            label24.Text = "Folder:";
            label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(15, 65);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(110, 15);
            label2.TabIndex = 58;
            label2.Text = "Resource Acronym:";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label25
            // 
            label25.Anchor = AnchorStyles.Right;
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(5, 36);
            label25.Margin = new Padding(4, 0, 4, 0);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(120, 15);
            label25.TabIndex = 107;
            label25.Text = "Resource Name/Title:";
            label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(4, 145);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(121, 15);
            label3.TabIndex = 61;
            label3.Text = "Resource Description:";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Location = new System.Drawing.Point(133, 148);
            panel1.Margin = new Padding(4, 3, 20, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(366, 106);
            panel1.TabIndex = 153;
            // 
            // lbDatasetslbl
            // 
            lbDatasetslbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbDatasetslbl.AutoSize = true;
            lbDatasetslbl.Location = new System.Drawing.Point(72, 257);
            lbDatasetslbl.Name = "lbDatasetslbl";
            lbDatasetslbl.Size = new System.Drawing.Size(54, 15);
            lbDatasetslbl.TabIndex = 160;
            lbDatasetslbl.Text = "Datasets:";
            // 
            // lbDatasets
            // 
            lbDatasets.AutoSize = true;
            lbDatasets.Location = new System.Drawing.Point(132, 257);
            lbDatasets.Name = "lbDatasets";
            lbDatasets.Size = new System.Drawing.Size(0, 15);
            lbDatasets.TabIndex = 161;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(tbSourceOfDataCollection, 1, 30);
            tableLayoutPanel2.Controls.Add(tbEthicsApprover, 1, 29);
            tableLayoutPanel2.Controls.Add(label33, 0, 30);
            tableLayoutPanel2.Controls.Add(ddExplicitConsent, 1, 28);
            tableLayoutPanel2.Controls.Add(label32, 0, 29);
            tableLayoutPanel2.Controls.Add(tbAdministrativeContactAddress, 1, 27);
            tableLayoutPanel2.Controls.Add(label31, 0, 28);
            tableLayoutPanel2.Controls.Add(tbAdministrativeContactTelephone, 1, 26);
            tableLayoutPanel2.Controls.Add(label11, 0, 27);
            tableLayoutPanel2.Controls.Add(tbAdministrativeContactEmail, 1, 25);
            tableLayoutPanel2.Controls.Add(label30, 0, 26);
            tableLayoutPanel2.Controls.Add(tbAdministrativeContactName, 1, 24);
            tableLayoutPanel2.Controls.Add(label29, 0, 25);
            tableLayoutPanel2.Controls.Add(tbDataStandards, 1, 23);
            tableLayoutPanel2.Controls.Add(label15, 0, 24);
            tableLayoutPanel2.Controls.Add(tbCountryOfOrigin, 1, 22);
            tableLayoutPanel2.Controls.Add(label26, 0, 23);
            tableLayoutPanel2.Controls.Add(c_tbSourceUrl, 1, 21);
            tableLayoutPanel2.Controls.Add(label27, 0, 22);
            tableLayoutPanel2.Controls.Add(c_tbQueryToolUrl, 1, 20);
            tableLayoutPanel2.Controls.Add(label23, 0, 21);
            tableLayoutPanel2.Controls.Add(c_tbBulkDownloadUrl, 1, 19);
            tableLayoutPanel2.Controls.Add(label22, 0, 20);
            tableLayoutPanel2.Controls.Add(c_tbBrowseUrl, 1, 18);
            tableLayoutPanel2.Controls.Add(label21, 0, 19);
            tableLayoutPanel2.Controls.Add(c_tbAPIAccessURL, 1, 17);
            tableLayoutPanel2.Controls.Add(label20, 0, 18);
            tableLayoutPanel2.Controls.Add(c_tbAccessOptions, 1, 16);
            tableLayoutPanel2.Controls.Add(label19, 0, 17);
            tableLayoutPanel2.Controls.Add(tbDatasetStartDate, 1, 14);
            tableLayoutPanel2.Controls.Add(label18, 0, 16);
            tableLayoutPanel2.Controls.Add(label17, 0, 15);
            tableLayoutPanel2.Controls.Add(c_tbResourceOwner, 1, 13);
            tableLayoutPanel2.Controls.Add(label41, 0, 14);
            tableLayoutPanel2.Controls.Add(c_tbSubjectNumbers, 1, 12);
            tableLayoutPanel2.Controls.Add(label16, 0, 13);
            tableLayoutPanel2.Controls.Add(c_tbLastRevisionDate, 1, 11);
            tableLayoutPanel2.Controls.Add(label28, 0, 12);
            tableLayoutPanel2.Controls.Add(c_tbTimeCoverage, 1, 10);
            tableLayoutPanel2.Controls.Add(label14, 0, 11);
            tableLayoutPanel2.Controls.Add(c_tbBackgroundSummary, 1, 4);
            tableLayoutPanel2.Controls.Add(label13, 0, 10);
            tableLayoutPanel2.Controls.Add(c_tbUpdateSchedule, 1, 9);
            tableLayoutPanel2.Controls.Add(c_tbUpdateFrequency, 1, 8);
            tableLayoutPanel2.Controls.Add(label12, 0, 9);
            tableLayoutPanel2.Controls.Add(c_tbTopics, 1, 6);
            tableLayoutPanel2.Controls.Add(c_ddPeriodicity, 1, 5);
            tableLayoutPanel2.Controls.Add(label10, 0, 8);
            tableLayoutPanel2.Controls.Add(c_ddGranularity, 1, 3);
            tableLayoutPanel2.Controls.Add(label8, 0, 6);
            tableLayoutPanel2.Controls.Add(c_ddType, 1, 1);
            tableLayoutPanel2.Controls.Add(label7, 0, 5);
            tableLayoutPanel2.Controls.Add(c_tbDetailPageURL, 1, 0);
            tableLayoutPanel2.Controls.Add(label64, 0, 3);
            tableLayoutPanel2.Controls.Add(label4, 0, 0);
            tableLayoutPanel2.Controls.Add(label6, 0, 1);
            tableLayoutPanel2.Controls.Add(label5, 0, 2);
            tableLayoutPanel2.Controls.Add(c_tbGeographicalCoverage, 1, 2);
            tableLayoutPanel2.Controls.Add(label9, 0, 4);
            tableLayoutPanel2.Controls.Add(ticketingControl1, 1, 7);
            tableLayoutPanel2.Controls.Add(c_tbAttributionCitation, 1, 15);
            tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 31;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new System.Drawing.Size(371, 1475);
            tableLayoutPanel2.TabIndex = 194;
            // 
            // tbSourceOfDataCollection
            // 
            tbSourceOfDataCollection.AcceptsReturn = true;
            tbSourceOfDataCollection.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbSourceOfDataCollection.Location = new System.Drawing.Point(135, 1370);
            tbSourceOfDataCollection.Margin = new Padding(4, 3, 20, 3);
            tbSourceOfDataCollection.Multiline = true;
            tbSourceOfDataCollection.Name = "tbSourceOfDataCollection";
            tbSourceOfDataCollection.ScrollBars = ScrollBars.Vertical;
            tbSourceOfDataCollection.Size = new System.Drawing.Size(369, 100);
            tbSourceOfDataCollection.TabIndex = 30;
            // 
            // tbEthicsApprover
            // 
            tbEthicsApprover.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbEthicsApprover.Location = new System.Drawing.Point(135, 1340);
            tbEthicsApprover.Margin = new Padding(4, 3, 20, 3);
            tbEthicsApprover.Name = "tbEthicsApprover";
            tbEthicsApprover.Size = new System.Drawing.Size(369, 23);
            tbEthicsApprover.TabIndex = 29;
            // 
            // label33
            // 
            label33.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label33.AutoSize = true;
            label33.Location = new System.Drawing.Point(43, 1366);
            label33.Margin = new Padding(4, 0, 4, 0);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(84, 30);
            label33.TabIndex = 163;
            label33.Text = "Source of Data\r\nCollection:";
            label33.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ddExplicitConsent
            // 
            ddExplicitConsent.Anchor = AnchorStyles.Left;
            ddExplicitConsent.DropDownStyle = ComboBoxStyle.DropDownList;
            ddExplicitConsent.FormattingEnabled = true;
            ddExplicitConsent.Items.AddRange(new object[] { "", "Yes", "No" });
            ddExplicitConsent.Location = new System.Drawing.Point(135, 1311);
            ddExplicitConsent.Margin = new Padding(4, 3, 20, 3);
            ddExplicitConsent.Name = "ddExplicitConsent";
            ddExplicitConsent.Size = new System.Drawing.Size(250, 23);
            ddExplicitConsent.TabIndex = 28;
            ddExplicitConsent.SelectedIndexChanged += ddExplicitConsent_SelectedIndexChanged;
            // 
            // label32
            // 
            label32.Anchor = AnchorStyles.Right;
            label32.AutoSize = true;
            label32.Location = new System.Drawing.Point(34, 1344);
            label32.Margin = new Padding(4, 0, 4, 0);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(93, 15);
            label32.TabIndex = 162;
            label32.Text = "Ethics Approver:";
            label32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbAdministrativeContactAddress
            // 
            tbAdministrativeContactAddress.AcceptsReturn = true;
            tbAdministrativeContactAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbAdministrativeContactAddress.Location = new System.Drawing.Point(135, 1205);
            tbAdministrativeContactAddress.Margin = new Padding(4, 3, 20, 3);
            tbAdministrativeContactAddress.Multiline = true;
            tbAdministrativeContactAddress.Name = "tbAdministrativeContactAddress";
            tbAdministrativeContactAddress.ScrollBars = ScrollBars.Vertical;
            tbAdministrativeContactAddress.Size = new System.Drawing.Size(369, 100);
            tbAdministrativeContactAddress.TabIndex = 27;
            // 
            // label31
            // 
            label31.Anchor = AnchorStyles.Right;
            label31.AutoSize = true;
            label31.Location = new System.Drawing.Point(32, 1315);
            label31.Margin = new Padding(4, 0, 4, 0);
            label31.Name = "label31";
            label31.Size = new System.Drawing.Size(95, 15);
            label31.TabIndex = 161;
            label31.Text = "Explicit Consent:";
            label31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbAdministrativeContactTelephone
            // 
            tbAdministrativeContactTelephone.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbAdministrativeContactTelephone.Location = new System.Drawing.Point(135, 1176);
            tbAdministrativeContactTelephone.Margin = new Padding(4, 3, 20, 3);
            tbAdministrativeContactTelephone.Name = "tbAdministrativeContactTelephone";
            tbAdministrativeContactTelephone.Size = new System.Drawing.Size(369, 23);
            tbAdministrativeContactTelephone.TabIndex = 26;
            // 
            // label11
            // 
            label11.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label11.Location = new System.Drawing.Point(7, 1202);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(120, 20);
            label11.TabIndex = 160;
            label11.Text = "Admin Address:";
            label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tbAdministrativeContactEmail
            // 
            tbAdministrativeContactEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbAdministrativeContactEmail.Location = new System.Drawing.Point(135, 1147);
            tbAdministrativeContactEmail.Margin = new Padding(4, 3, 20, 3);
            tbAdministrativeContactEmail.Name = "tbAdministrativeContactEmail";
            tbAdministrativeContactEmail.Size = new System.Drawing.Size(369, 23);
            tbAdministrativeContactEmail.TabIndex = 25;
            // 
            // label30
            // 
            label30.Anchor = AnchorStyles.Right;
            label30.AutoSize = true;
            label30.Location = new System.Drawing.Point(19, 1180);
            label30.Margin = new Padding(4, 0, 4, 0);
            label30.Name = "label30";
            label30.Size = new System.Drawing.Size(108, 15);
            label30.TabIndex = 159;
            label30.Text = "Admin Contact Tel:";
            label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbAdministrativeContactName
            // 
            tbAdministrativeContactName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tbAdministrativeContactName.Location = new System.Drawing.Point(135, 1118);
            tbAdministrativeContactName.Margin = new Padding(4, 3, 20, 3);
            tbAdministrativeContactName.Name = "tbAdministrativeContactName";
            tbAdministrativeContactName.Size = new System.Drawing.Size(369, 23);
            tbAdministrativeContactName.TabIndex = 24;
            // 
            // label29
            // 
            label29.Anchor = AnchorStyles.Right;
            label29.AutoSize = true;
            label29.Location = new System.Drawing.Point(4, 1151);
            label29.Margin = new Padding(4, 0, 4, 0);
            label29.Name = "label29";
            label29.Size = new System.Drawing.Size(123, 15);
            label29.TabIndex = 158;
            label29.Text = "Admin Contact Email:";
            label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            label15.Anchor = AnchorStyles.Right;
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(36, 1122);
            label15.Margin = new Padding(4, 0, 4, 0);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(91, 15);
            label15.TabIndex = 157;
            label15.Text = "Admin Contact:";
            label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.Black;
            label5.Location = new System.Drawing.Point(43, 58);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(84, 15);
            label5.TabIndex = 67;
            label5.Text = "Geo Coverage:";
            label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // c_tbGeographicalCoverage
            // 
            c_tbGeographicalCoverage.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            c_tbGeographicalCoverage.Location = new System.Drawing.Point(135, 61);
            c_tbGeographicalCoverage.Margin = new Padding(4, 3, 20, 3);
            c_tbGeographicalCoverage.Multiline = true;
            c_tbGeographicalCoverage.Name = "c_tbGeographicalCoverage";
            c_tbGeographicalCoverage.ScrollBars = ScrollBars.Vertical;
            c_tbGeographicalCoverage.Size = new System.Drawing.Size(369, 100);
            c_tbGeographicalCoverage.TabIndex = 2;
            // 
            // ticketingControl1
            // 
            ticketingControl1.Anchor = AnchorStyles.Left;
            ticketingControl1.Location = new System.Drawing.Point(135, 437);
            ticketingControl1.Margin = new Padding(4, 3, 20, 3);
            ticketingControl1.Name = "ticketingControl1";
            ticketingControl1.Size = new System.Drawing.Size(350, 57);
            ticketingControl1.TabIndex = 8;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // panel2
            // 
            panel2.Controls.Add(editableFolder);
            panel2.Controls.Add(editableCatalogueName);
            panel2.Location = new System.Drawing.Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(541, 170);
            panel2.TabIndex = 161;
            // 
            // editableCatalogueName
            // 
            editableCatalogueName.Location = new System.Drawing.Point(0, 0);
            editableCatalogueName.Name = "editableCatalogueName";
            editableCatalogueName.Size = new System.Drawing.Size(232, 50);
            editableCatalogueName.TabIndex = 0;
            // 
            // editableFolder
            // 
            editableFolder.Location = new System.Drawing.Point(3, 56);
            editableFolder.Name = "editableFolder";
            editableFolder.Size = new System.Drawing.Size(232, 50);
            editableFolder.TabIndex = 1;
            // 
            // CatalogueUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CatalogueUI";
            Size = new System.Drawing.Size(551, 1085);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ComboBox c_ddGranularity;
        private System.Windows.Forms.Label label64;
        private System.Windows.Forms.TextBox c_tbID;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox c_tbSourceUrl;
        private System.Windows.Forms.TextBox c_tbQueryToolUrl;
        private System.Windows.Forms.TextBox c_tbBulkDownloadUrl;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox c_tbBrowseUrl;
        private System.Windows.Forms.TextBox c_tbAPIAccessURL;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox c_tbAccessOptions;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox c_tbResourceOwner;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox c_tbAttributionCitation;
        private System.Windows.Forms.ComboBox c_ddType;
        private System.Windows.Forms.TextBox c_tbLastRevisionDate;
        private System.Windows.Forms.ComboBox c_ddPeriodicity;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox c_tbUpdateSchedule;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox c_tbTimeCoverage;
        private System.Windows.Forms.TextBox c_tbBackgroundSummary;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox c_tbTopics;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox c_tbUpdateFrequency;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox c_tbNumberOfThese;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox c_tbDetailPageURL;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        internal System.Windows.Forms.TextBox tbAcronym;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox tbCountryOfOrigin;
        private System.Windows.Forms.TextBox tbDataStandards;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox c_tbSubjectNumbers;
        private System.Windows.Forms.TextBox tbDatasetStartDate;
        private System.Windows.Forms.Label label41;
        private TicketingControlUI ticketingControl1;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox tbFolder;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox tbSourceOfDataCollection;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox tbEthicsApprover;
        private System.Windows.Forms.ComboBox ddExplicitConsent;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbAdministrativeContactAddress;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox tbAdministrativeContactTelephone;
        private System.Windows.Forms.TextBox tbAdministrativeContactEmail;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tbAdministrativeContactName;
        private System.Windows.Forms.CheckBox cbColdStorage;
        private System.Windows.Forms.CheckBox cbInternal;
        private System.Windows.Forms.CheckBox cbDeprecated;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox c_tbGeographicalCoverage;
        private Label label5;
        private TableLayoutPanel tableLayoutPanel2;
        private Label lbDatasetslbl;
        private Label lbDatasets;
        private Panel panel2;
        private SimpleControls.EditableLabelUI editableCatalogueName;
        private SimpleControls.EditableLabelUI editableFolder;
    }
}
