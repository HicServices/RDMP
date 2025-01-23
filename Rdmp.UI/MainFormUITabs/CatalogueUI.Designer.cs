using System;
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
            splitContainer1 = new SplitContainer();
            cbColdStorage = new CheckBox();
            cbInternal = new CheckBox();
            cbDeprecated = new CheckBox();
            editableFolder = new SimpleControls.EditableLabelUI();
            editableCatalogueName = new SimpleControls.EditableLabelUI();
            ticketingControl1 = new TicketingControlUI();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label2 = new Label();
            tbDescription = new TextBox();
            label1 = new Label();
            tbAbstract = new TextBox();
            tabPage2 = new TabPage();
            ddchipDatasetType = new SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay();
            ffcKeywords = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            label8 = new Label();
            tbDataSourceSetting = new TextBox();
            label7 = new Label();
            tbDataSource = new TextBox();
            label6 = new Label();
            cb_datasetSubtype = new ComboBox();
            label5 = new Label();
            cb_datasetType = new ComboBox();
            label4 = new Label();
            label3 = new Label();
            cb_resourceType = new ComboBox();
            tabPage3 = new TabPage();
            btnEndDateClear = new Button();
            btnStartDateClear = new Button();
            dtpEndDate = new DateTimePicker();
            dtpStart = new DateTimePicker();
            label12 = new Label();
            label11 = new Label();
            label10 = new Label();
            cb_granularity = new ComboBox();
            label9 = new Label();
            tbGeoCoverage = new TextBox();
            tabPage4 = new TabPage();
            label16 = new Label();
            tbJuristiction = new TextBox();
            label15 = new Label();
            tbDataProcessor = new TextBox();
            label14 = new Label();
            tbDataController = new TextBox();
            label13 = new Label();
            tbAccessContact = new TextBox();
            tabPage5 = new TabPage();
            fftControlledVocab = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            ffcPeople = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            label19 = new Label();
            label18 = new Label();
            tbDOI = new TextBox();
            label17 = new Label();
            tabPage6 = new TabPage();
            btnReleaseDateClear = new Button();
            dtpReleaseDate = new DateTimePicker();
            cbUpdateLag = new ComboBox();
            cb_updateFrequency = new ComboBox();
            label22 = new Label();
            label21 = new Label();
            label20 = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            tabPage6.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(cbColdStorage);
            splitContainer1.Panel1.Controls.Add(cbInternal);
            splitContainer1.Panel1.Controls.Add(cbDeprecated);
            splitContainer1.Panel1.Controls.Add(editableFolder);
            splitContainer1.Panel1.Controls.Add(editableCatalogueName);
            splitContainer1.Panel1.Controls.Add(ticketingControl1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new System.Drawing.Size(881, 1085);
            splitContainer1.SplitterDistance = 158;
            splitContainer1.TabIndex = 0;
            // 
            // cbColdStorage
            // 
            cbColdStorage.AutoSize = true;
            cbColdStorage.Location = new System.Drawing.Point(695, 75);
            cbColdStorage.Name = "cbColdStorage";
            cbColdStorage.Size = new System.Drawing.Size(94, 19);
            cbColdStorage.TabIndex = 5;
            cbColdStorage.Text = "Cold Storage";
            cbColdStorage.UseVisualStyleBackColor = true;
            // 
            // cbInternal
            // 
            cbInternal.AutoSize = true;
            cbInternal.Location = new System.Drawing.Point(623, 74);
            cbInternal.Name = "cbInternal";
            cbInternal.Size = new System.Drawing.Size(66, 19);
            cbInternal.TabIndex = 4;
            cbInternal.Text = "Internal";
            cbInternal.UseVisualStyleBackColor = true;
            cbInternal.CheckedChanged += checkBox2_CheckedChanged;
            // 
            // cbDeprecated
            // 
            cbDeprecated.AutoSize = true;
            cbDeprecated.Location = new System.Drawing.Point(531, 73);
            cbDeprecated.Name = "cbDeprecated";
            cbDeprecated.Size = new System.Drawing.Size(86, 19);
            cbDeprecated.TabIndex = 3;
            cbDeprecated.Text = "Deprecated";
            cbDeprecated.UseVisualStyleBackColor = true;
            // 
            // editableFolder
            // 
            editableFolder.AutoValidate = AutoValidate.EnableAllowFocusChange;
            editableFolder.Location = new System.Drawing.Point(11, 63);
            editableFolder.Name = "editableFolder";
            editableFolder.Size = new System.Drawing.Size(278, 48);
            editableFolder.TabIndex = 2;
            // 
            // editableCatalogueName
            // 
            editableCatalogueName.Location = new System.Drawing.Point(11, 5);
            editableCatalogueName.Name = "editableCatalogueName";
            editableCatalogueName.Size = new System.Drawing.Size(278, 59);
            editableCatalogueName.TabIndex = 1;
            // 
            // ticketingControl1
            // 
            ticketingControl1.Location = new System.Drawing.Point(523, 7);
            ticketingControl1.Margin = new Padding(4, 3, 4, 3);
            ticketingControl1.Name = "ticketingControl1";
            ticketingControl1.Size = new System.Drawing.Size(354, 62);
            ticketingControl1.TabIndex = 0;
            ticketingControl1.Load += ticketingControl1_Load;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(881, 923);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(tbDescription);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(tbAbstract);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new System.Drawing.Size(873, 895);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Descriptions";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 143);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 15);
            label2.TabIndex = 3;
            label2.Text = "Description";
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(6, 158);
            tbDescription.MaxLength = 250;
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(861, 116);
            tbDescription.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(98, 15);
            label1.TabIndex = 1;
            label1.Text = "Short Description";
            // 
            // tbAbstract
            // 
            tbAbstract.Location = new System.Drawing.Point(6, 18);
            tbAbstract.MaxLength = 250;
            tbAbstract.Multiline = true;
            tbAbstract.Name = "tbAbstract";
            tbAbstract.Size = new System.Drawing.Size(861, 116);
            tbAbstract.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage2.Controls.Add(ddchipDatasetType);
            tabPage2.Controls.Add(ffcKeywords);
            tabPage2.Controls.Add(label8);
            tabPage2.Controls.Add(tbDataSourceSetting);
            tabPage2.Controls.Add(label7);
            tabPage2.Controls.Add(tbDataSource);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(cb_datasetSubtype);
            tabPage2.Controls.Add(label5);
            tabPage2.Controls.Add(cb_datasetType);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(cb_resourceType);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new System.Drawing.Size(873, 895);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Data Details";
            // 
            // ddchipDatasetType
            // 
            ddchipDatasetType.AutoScroll = true;
            ddchipDatasetType.AutoSize = true;
            ddchipDatasetType.Location = new System.Drawing.Point(431, 65);
            ddchipDatasetType.MinimumSize = new System.Drawing.Size(100, 20);
            ddchipDatasetType.Name = "ddchipDatasetType";
            ddchipDatasetType.Size = new System.Drawing.Size(286, 83);
            ddchipDatasetType.TabIndex = 13;
            // 
            // ffcKeywords
            // 
            ffcKeywords.AutoScroll = true;
            ffcKeywords.AutoSize = true;
            ffcKeywords.Location = new System.Drawing.Point(7, 275);
            ffcKeywords.MaximumSize = new System.Drawing.Size(300, 0);
            ffcKeywords.MinimumSize = new System.Drawing.Size(100, 20);
            ffcKeywords.Name = "ffcKeywords";
            ffcKeywords.Size = new System.Drawing.Size(300, 53);
            ffcKeywords.TabIndex = 12;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 209);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(110, 15);
            label8.TabIndex = 11;
            label8.Text = "Data Source Setting";
            // 
            // tbDataSourceSetting
            // 
            tbDataSourceSetting.Location = new System.Drawing.Point(6, 227);
            tbDataSourceSetting.Name = "tbDataSourceSetting";
            tbDataSourceSetting.Size = new System.Drawing.Size(366, 23);
            tbDataSourceSetting.TabIndex = 10;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 158);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(70, 15);
            label7.TabIndex = 9;
            label7.Text = "Data Source";
            // 
            // tbDataSource
            // 
            tbDataSource.Location = new System.Drawing.Point(6, 176);
            tbDataSource.Name = "tbDataSource";
            tbDataSource.Size = new System.Drawing.Size(366, 23);
            tbDataSource.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 107);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(92, 15);
            label6.TabIndex = 7;
            label6.Text = "Dataset Subtype";
            // 
            // cb_datasetSubtype
            // 
            cb_datasetSubtype.FormattingEnabled = true;
            cb_datasetSubtype.Location = new System.Drawing.Point(6, 125);
            cb_datasetSubtype.Name = "cb_datasetSubtype";
            cb_datasetSubtype.Size = new System.Drawing.Size(121, 23);
            cb_datasetSubtype.TabIndex = 6;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 58);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(73, 15);
            label5.TabIndex = 5;
            label5.Text = "Dataset Type";
            // 
            // cb_datasetType
            // 
            cb_datasetType.FormattingEnabled = true;
            cb_datasetType.Location = new System.Drawing.Point(6, 76);
            cb_datasetType.Name = "cb_datasetType";
            cb_datasetType.Size = new System.Drawing.Size(121, 23);
            cb_datasetType.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 257);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(58, 15);
            label4.TabIndex = 3;
            label4.Text = "Keywords";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 9);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(82, 15);
            label3.TabIndex = 1;
            label3.Text = "Resource Type";
            // 
            // cb_resourceType
            // 
            cb_resourceType.FormattingEnabled = true;
            cb_resourceType.Location = new System.Drawing.Point(6, 27);
            cb_resourceType.Name = "cb_resourceType";
            cb_resourceType.Size = new System.Drawing.Size(121, 23);
            cb_resourceType.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage3.Controls.Add(btnEndDateClear);
            tabPage3.Controls.Add(btnStartDateClear);
            tabPage3.Controls.Add(dtpEndDate);
            tabPage3.Controls.Add(dtpStart);
            tabPage3.Controls.Add(label12);
            tabPage3.Controls.Add(label11);
            tabPage3.Controls.Add(label10);
            tabPage3.Controls.Add(cb_granularity);
            tabPage3.Controls.Add(label9);
            tabPage3.Controls.Add(tbGeoCoverage);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(873, 895);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Geospacial & Temporal";
            // 
            // btnEndDateClear
            // 
            btnEndDateClear.Location = new System.Drawing.Point(220, 194);
            btnEndDateClear.Name = "btnEndDateClear";
            btnEndDateClear.Size = new System.Drawing.Size(75, 23);
            btnEndDateClear.TabIndex = 25;
            btnEndDateClear.Text = "Clear";
            btnEndDateClear.UseVisualStyleBackColor = true;
            btnEndDateClear.Click += btnEndDateClear_Click;
            // 
            // btnStartDateClear
            // 
            btnStartDateClear.Location = new System.Drawing.Point(220, 140);
            btnStartDateClear.Name = "btnStartDateClear";
            btnStartDateClear.Size = new System.Drawing.Size(75, 23);
            btnStartDateClear.TabIndex = 24;
            btnStartDateClear.Text = "Clear";
            btnStartDateClear.UseVisualStyleBackColor = true;
            btnStartDateClear.Click += btnStartDateClear_Click;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new System.Drawing.Point(14, 194);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new System.Drawing.Size(200, 23);
            dtpEndDate.TabIndex = 23;
            // 
            // dtpStart
            // 
            dtpStart.Location = new System.Drawing.Point(14, 140);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new System.Drawing.Size(200, 23);
            dtpStart.TabIndex = 20;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(14, 176);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(54, 15);
            label12.TabIndex = 19;
            label12.Text = "End Date";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(14, 122);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(58, 15);
            label11.TabIndex = 17;
            label11.Text = "Start Date";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(14, 68);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(65, 15);
            label10.TabIndex = 15;
            label10.Text = "Granularity";
            // 
            // cb_granularity
            // 
            cb_granularity.FormattingEnabled = true;
            cb_granularity.Location = new System.Drawing.Point(14, 86);
            cb_granularity.Name = "cb_granularity";
            cb_granularity.Size = new System.Drawing.Size(121, 23);
            cb_granularity.TabIndex = 14;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(14, 17);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(130, 15);
            label9.TabIndex = 13;
            label9.Text = "Geographical Coverage";
            // 
            // tbGeoCoverage
            // 
            tbGeoCoverage.Location = new System.Drawing.Point(14, 35);
            tbGeoCoverage.Name = "tbGeoCoverage";
            tbGeoCoverage.Size = new System.Drawing.Size(366, 23);
            tbGeoCoverage.TabIndex = 12;
            // 
            // tabPage4
            // 
            tabPage4.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage4.Controls.Add(label16);
            tabPage4.Controls.Add(tbJuristiction);
            tabPage4.Controls.Add(label15);
            tabPage4.Controls.Add(tbDataProcessor);
            tabPage4.Controls.Add(label14);
            tabPage4.Controls.Add(tbDataController);
            tabPage4.Controls.Add(label13);
            tabPage4.Controls.Add(tbAccessContact);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new System.Drawing.Size(873, 895);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Access";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new System.Drawing.Point(15, 169);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(64, 15);
            label16.TabIndex = 21;
            label16.Text = "Juristiction";
            // 
            // tbJuristiction
            // 
            tbJuristiction.Location = new System.Drawing.Point(15, 187);
            tbJuristiction.Name = "tbJuristiction";
            tbJuristiction.Size = new System.Drawing.Size(366, 23);
            tbJuristiction.TabIndex = 20;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(15, 116);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(85, 15);
            label15.TabIndex = 19;
            label15.Text = "Data Processor";
            // 
            // tbDataProcessor
            // 
            tbDataProcessor.Location = new System.Drawing.Point(15, 134);
            tbDataProcessor.Name = "tbDataProcessor";
            tbDataProcessor.Size = new System.Drawing.Size(366, 23);
            tbDataProcessor.TabIndex = 18;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(15, 66);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(87, 15);
            label14.TabIndex = 17;
            label14.Text = "Data Controller";
            // 
            // tbDataController
            // 
            tbDataController.Location = new System.Drawing.Point(15, 84);
            tbDataController.Name = "tbDataController";
            tbDataController.Size = new System.Drawing.Size(366, 23);
            tbDataController.TabIndex = 16;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(15, 10);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(88, 15);
            label13.TabIndex = 15;
            label13.Text = "Access Contact";
            // 
            // tbAccessContact
            // 
            tbAccessContact.Location = new System.Drawing.Point(15, 28);
            tbAccessContact.Name = "tbAccessContact";
            tbAccessContact.Size = new System.Drawing.Size(366, 23);
            tbAccessContact.TabIndex = 14;
            // 
            // tabPage5
            // 
            tabPage5.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage5.Controls.Add(fftControlledVocab);
            tabPage5.Controls.Add(ffcPeople);
            tabPage5.Controls.Add(label19);
            tabPage5.Controls.Add(label18);
            tabPage5.Controls.Add(tbDOI);
            tabPage5.Controls.Add(label17);
            tabPage5.Location = new System.Drawing.Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new System.Drawing.Size(873, 895);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Attribution";
            // 
            // fftControlledVocab
            // 
            fftControlledVocab.AutoScroll = true;
            fftControlledVocab.AutoSize = true;
            fftControlledVocab.Location = new System.Drawing.Point(14, 98);
            fftControlledVocab.MaximumSize = new System.Drawing.Size(300, 0);
            fftControlledVocab.MinimumSize = new System.Drawing.Size(100, 20);
            fftControlledVocab.Name = "fftControlledVocab";
            fftControlledVocab.Size = new System.Drawing.Size(300, 26);
            fftControlledVocab.TabIndex = 23;
            // 
            // ffcPeople
            // 
            ffcPeople.AutoScroll = true;
            ffcPeople.AutoSize = true;
            ffcPeople.Location = new System.Drawing.Point(14, 195);
            ffcPeople.MaximumSize = new System.Drawing.Size(300, 0);
            ffcPeople.MinimumSize = new System.Drawing.Size(100, 20);
            ffcPeople.Name = "ffcPeople";
            ffcPeople.Size = new System.Drawing.Size(300, 53);
            ffcPeople.TabIndex = 22;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(14, 74);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(124, 15);
            label19.TabIndex = 21;
            label19.Text = "Controlled Vocabulary";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(14, 15);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(27, 15);
            label18.TabIndex = 19;
            label18.Text = "DOI";
            // 
            // tbDOI
            // 
            tbDOI.Location = new System.Drawing.Point(14, 33);
            tbDOI.Name = "tbDOI";
            tbDOI.Size = new System.Drawing.Size(366, 23);
            tbDOI.TabIndex = 18;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(14, 177);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(43, 15);
            label17.TabIndex = 17;
            label17.Text = "People";
            // 
            // tabPage6
            // 
            tabPage6.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage6.Controls.Add(btnReleaseDateClear);
            tabPage6.Controls.Add(dtpReleaseDate);
            tabPage6.Controls.Add(cbUpdateLag);
            tabPage6.Controls.Add(cb_updateFrequency);
            tabPage6.Controls.Add(label22);
            tabPage6.Controls.Add(label21);
            tabPage6.Controls.Add(label20);
            tabPage6.Location = new System.Drawing.Point(4, 24);
            tabPage6.Name = "tabPage6";
            tabPage6.Size = new System.Drawing.Size(873, 895);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "Data Updates";
            // 
            // btnReleaseDateClear
            // 
            btnReleaseDateClear.Location = new System.Drawing.Point(221, 88);
            btnReleaseDateClear.Name = "btnReleaseDateClear";
            btnReleaseDateClear.Size = new System.Drawing.Size(75, 23);
            btnReleaseDateClear.TabIndex = 25;
            btnReleaseDateClear.Text = "Clear";
            btnReleaseDateClear.UseVisualStyleBackColor = true;
            btnReleaseDateClear.Click += btnReleaseDateClear_Click;
            // 
            // dtpReleaseDate
            // 
            dtpReleaseDate.Location = new System.Drawing.Point(15, 88);
            dtpReleaseDate.Name = "dtpReleaseDate";
            dtpReleaseDate.Size = new System.Drawing.Size(200, 23);
            dtpReleaseDate.TabIndex = 24;
            // 
            // cbUpdateLag
            // 
            cbUpdateLag.FormattingEnabled = true;
            cbUpdateLag.Location = new System.Drawing.Point(15, 142);
            cbUpdateLag.Name = "cbUpdateLag";
            cbUpdateLag.Size = new System.Drawing.Size(140, 23);
            cbUpdateLag.TabIndex = 23;
            cbUpdateLag.Format += FormatCB;
            // 
            // cb_updateFrequency
            // 
            cb_updateFrequency.FormattingEnabled = true;
            cb_updateFrequency.Location = new System.Drawing.Point(15, 30);
            cb_updateFrequency.Name = "cb_updateFrequency";
            cb_updateFrequency.Size = new System.Drawing.Size(121, 23);
            cb_updateFrequency.TabIndex = 22;
            cb_updateFrequency.Format += FormatCB;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(15, 124);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(67, 15);
            label22.TabIndex = 21;
            label22.Text = "Update Lag";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new System.Drawing.Point(15, 70);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(105, 15);
            label21.TabIndex = 19;
            label21.Text = "Initial Release Date";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(15, 12);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(99, 15);
            label20.TabIndex = 17;
            label20.Text = "Update Fequency";
            // 
            // CatalogueUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CatalogueUI";
            Size = new System.Drawing.Size(881, 1085);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            tabPage6.ResumeLayout(false);
            tabPage6.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainer1;
        private TicketingControlUI ticketingControl1;
        private SimpleControls.EditableLabelUI editableFolder;
        private SimpleControls.EditableLabelUI editableCatalogueName;
        private CheckBox cbColdStorage;
        private CheckBox cbInternal;
        private CheckBox cbDeprecated;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private Label label1;
        private Label label2;
        private TextBox tbDescription;
        private Label label7;
        private TextBox tbDataSource;
        private Label label6;
        private ComboBox cb_datasetSubtype;
        private Label label5;
        private ComboBox cb_datasetType;
        private Label label4;
        private Label label3;
        private ComboBox cb_resourceType;
        private Label label8;
        private TextBox tbDataSourceSetting;
        private Label label12;
        private Label label11;
        private Label label10;
        private ComboBox cb_granularity;
        private Label label9;
        private TextBox tbGeoCoverage;
        private Label label16;
        private TextBox tbJuristiction;
        private Label label15;
        private TextBox tbDataProcessor;
        private Label label14;
        private TextBox tbDataController;
        private Label label13;
        private TextBox tbAccessContact;
        private Label label19;
        private Label label18;
        private TextBox tbDOI;
        private Label label17;
        private ComboBox cb_updateFrequency;
        private Label label22;
        private Label label21;
        private Label label20;
        private TextBox tbAbstract;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEndDate;
        private Button btnEndDateClear;
        private Button btnStartDateClear;
        private ComboBox cbUpdateLag;
        private DateTimePicker dtpReleaseDate;
        private Button btnReleaseDateClear;
        private SimpleControls.MultiSelectChips.FreeFormTextChipDisplay ffcKeywords;
        private SimpleControls.MultiSelectChips.FreeFormTextChipDisplay ffcPeople;
        private SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay ddchipDatasetType;
        private SimpleControls.MultiSelectChips.FreeFormTextChipDisplay fftControlledVocab;
    }
}
