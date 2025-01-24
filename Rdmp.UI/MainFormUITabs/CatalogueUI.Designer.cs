﻿using System;
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
            groupBox23 = new GroupBox();
            tbAcronym = new TextBox();
            groupBox16 = new GroupBox();
            tbDescription = new TextBox();
            groupBox15 = new GroupBox();
            tbAbstract = new TextBox();
            tabPage2 = new TabPage();
            tableLayoutPanel2 = new TableLayoutPanel();
            groupBox18 = new GroupBox();
            ddDatasetType = new SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay();
            groupBox22 = new GroupBox();
            ffcKeywords = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            groupBox19 = new GroupBox();
            ddDatasetSubtype = new SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay();
            groupBox21 = new GroupBox();
            tbDataSourceSetting = new TextBox();
            groupBox20 = new GroupBox();
            tbDataSource = new TextBox();
            groupBox17 = new GroupBox();
            cb_resourceType = new ComboBox();
            tabPage3 = new TabPage();
            groupBox14 = new GroupBox();
            dtpEndDate = new DateTimePicker();
            btnEndDateClear = new Button();
            groupBox13 = new GroupBox();
            dtpStart = new DateTimePicker();
            btnStartDateClear = new Button();
            groupBox12 = new GroupBox();
            cb_granularity = new ComboBox();
            groupBox11 = new GroupBox();
            tbGeoCoverage = new TextBox();
            tabPage4 = new TabPage();
            groupBox10 = new GroupBox();
            tbJuristiction = new TextBox();
            groupBox9 = new GroupBox();
            tbDataProcessor = new TextBox();
            groupBox8 = new GroupBox();
            tbDataController = new TextBox();
            groupBox7 = new GroupBox();
            tbAccessContact = new TextBox();
            tabPage5 = new TabPage();
            groupBox3 = new GroupBox();
            tbDOI = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox2 = new GroupBox();
            fftControlledVocab = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            groupBox1 = new GroupBox();
            ffcPeople = new SimpleControls.MultiSelectChips.FreeFormTextChipDisplay();
            tabPage6 = new TabPage();
            groupBox6 = new GroupBox();
            cbUpdateLag = new ComboBox();
            groupBox5 = new GroupBox();
            dtpReleaseDate = new DateTimePicker();
            btnReleaseDateClear = new Button();
            groupBox4 = new GroupBox();
            cb_updateFrequency = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox23.SuspendLayout();
            groupBox16.SuspendLayout();
            groupBox15.SuspendLayout();
            tabPage2.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBox18.SuspendLayout();
            groupBox22.SuspendLayout();
            groupBox19.SuspendLayout();
            groupBox21.SuspendLayout();
            groupBox20.SuspendLayout();
            groupBox17.SuspendLayout();
            tabPage3.SuspendLayout();
            groupBox14.SuspendLayout();
            groupBox13.SuspendLayout();
            groupBox12.SuspendLayout();
            groupBox11.SuspendLayout();
            tabPage4.SuspendLayout();
            groupBox10.SuspendLayout();
            groupBox9.SuspendLayout();
            groupBox8.SuspendLayout();
            groupBox7.SuspendLayout();
            tabPage5.SuspendLayout();
            groupBox3.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage6.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox4.SuspendLayout();
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
            tabPage1.Controls.Add(groupBox23);
            tabPage1.Controls.Add(groupBox16);
            tabPage1.Controls.Add(groupBox15);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new System.Drawing.Size(873, 895);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Descriptions";
            tabPage1.Click += tabPage1_Click;
            // 
            // groupBox23
            // 
            groupBox23.Controls.Add(tbAcronym);
            groupBox23.Location = new System.Drawing.Point(7, 6);
            groupBox23.Name = "groupBox23";
            groupBox23.Size = new System.Drawing.Size(231, 56);
            groupBox23.TabIndex = 6;
            groupBox23.TabStop = false;
            groupBox23.Text = "Acronym";
            // 
            // tbAcronym
            // 
            tbAcronym.Location = new System.Drawing.Point(6, 22);
            tbAcronym.Name = "tbAcronym";
            tbAcronym.Size = new System.Drawing.Size(201, 23);
            tbAcronym.TabIndex = 0;
            // 
            // groupBox16
            // 
            groupBox16.Controls.Add(tbDescription);
            groupBox16.Location = new System.Drawing.Point(7, 225);
            groupBox16.Name = "groupBox16";
            groupBox16.Size = new System.Drawing.Size(866, 165);
            groupBox16.TabIndex = 5;
            groupBox16.TabStop = false;
            groupBox16.Text = "Description";
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(6, 22);
            tbDescription.MaxLength = 250;
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(851, 116);
            tbDescription.TabIndex = 2;
            // 
            // groupBox15
            // 
            groupBox15.Controls.Add(tbAbstract);
            groupBox15.Location = new System.Drawing.Point(7, 64);
            groupBox15.Name = "groupBox15";
            groupBox15.Size = new System.Drawing.Size(863, 155);
            groupBox15.TabIndex = 4;
            groupBox15.TabStop = false;
            groupBox15.Text = "Short Description";
            // 
            // tbAbstract
            // 
            tbAbstract.Location = new System.Drawing.Point(6, 22);
            tbAbstract.MaxLength = 250;
            tbAbstract.Multiline = true;
            tbAbstract.Name = "tbAbstract";
            tbAbstract.Size = new System.Drawing.Size(851, 116);
            tbAbstract.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage2.Controls.Add(tableLayoutPanel2);
            tabPage2.Controls.Add(groupBox17);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new System.Drawing.Size(873, 895);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Data Details";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(groupBox18, 0, 0);
            tableLayoutPanel2.Controls.Add(groupBox22, 0, 4);
            tableLayoutPanel2.Controls.Add(groupBox19, 0, 1);
            tableLayoutPanel2.Controls.Add(groupBox21, 0, 3);
            tableLayoutPanel2.Controls.Add(groupBox20, 0, 2);
            tableLayoutPanel2.Location = new System.Drawing.Point(7, 70);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new System.Drawing.Size(496, 513);
            tableLayoutPanel2.TabIndex = 20;
            // 
            // groupBox18
            // 
            groupBox18.AutoSize = true;
            groupBox18.Controls.Add(ddDatasetType);
            groupBox18.Location = new System.Drawing.Point(3, 3);
            groupBox18.Name = "groupBox18";
            groupBox18.Size = new System.Drawing.Size(298, 126);
            groupBox18.TabIndex = 15;
            groupBox18.TabStop = false;
            groupBox18.Text = "Dataset Type";
            // 
            // ddDatasetType
            // 
            ddDatasetType.AutoScroll = true;
            ddDatasetType.AutoSize = true;
            ddDatasetType.Location = new System.Drawing.Point(6, 21);
            ddDatasetType.MaximumSize = new System.Drawing.Size(400, 300);
            ddDatasetType.MinimumSize = new System.Drawing.Size(100, 20);
            ddDatasetType.Name = "ddDatasetType";
            ddDatasetType.Size = new System.Drawing.Size(286, 83);
            ddDatasetType.TabIndex = 0;
            // 
            // groupBox22
            // 
            groupBox22.AutoSize = true;
            groupBox22.Controls.Add(ffcKeywords);
            groupBox22.Location = new System.Drawing.Point(3, 413);
            groupBox22.Name = "groupBox22";
            groupBox22.Size = new System.Drawing.Size(312, 97);
            groupBox22.TabIndex = 19;
            groupBox22.TabStop = false;
            groupBox22.Text = "Keywords";
            // 
            // ffcKeywords
            // 
            ffcKeywords.AutoScroll = true;
            ffcKeywords.AutoSize = true;
            ffcKeywords.Location = new System.Drawing.Point(6, 22);
            ffcKeywords.MaximumSize = new System.Drawing.Size(300, 0);
            ffcKeywords.MinimumSize = new System.Drawing.Size(100, 20);
            ffcKeywords.Name = "ffcKeywords";
            ffcKeywords.Size = new System.Drawing.Size(300, 53);
            ffcKeywords.TabIndex = 12;
            // 
            // groupBox19
            // 
            groupBox19.AutoSize = true;
            groupBox19.Controls.Add(ddDatasetSubtype);
            groupBox19.Location = new System.Drawing.Point(3, 135);
            groupBox19.Name = "groupBox19";
            groupBox19.Size = new System.Drawing.Size(298, 126);
            groupBox19.TabIndex = 16;
            groupBox19.TabStop = false;
            groupBox19.Text = "Dataset Subtype";
            // 
            // ddDatasetSubtype
            // 
            ddDatasetSubtype.AutoScroll = true;
            ddDatasetSubtype.AutoSize = true;
            ddDatasetSubtype.Location = new System.Drawing.Point(6, 21);
            ddDatasetSubtype.MaximumSize = new System.Drawing.Size(400, 300);
            ddDatasetSubtype.MinimumSize = new System.Drawing.Size(100, 20);
            ddDatasetSubtype.Name = "ddDatasetSubtype";
            ddDatasetSubtype.Size = new System.Drawing.Size(286, 83);
            ddDatasetSubtype.TabIndex = 0;
            // 
            // groupBox21
            // 
            groupBox21.AutoSize = true;
            groupBox21.Controls.Add(tbDataSourceSetting);
            groupBox21.Location = new System.Drawing.Point(3, 340);
            groupBox21.Name = "groupBox21";
            groupBox21.Size = new System.Drawing.Size(378, 67);
            groupBox21.TabIndex = 18;
            groupBox21.TabStop = false;
            groupBox21.Text = "Data Source Setting";
            // 
            // tbDataSourceSetting
            // 
            tbDataSourceSetting.Location = new System.Drawing.Point(6, 22);
            tbDataSourceSetting.Name = "tbDataSourceSetting";
            tbDataSourceSetting.Size = new System.Drawing.Size(366, 23);
            tbDataSourceSetting.TabIndex = 10;
            // 
            // groupBox20
            // 
            groupBox20.AutoSize = true;
            groupBox20.Controls.Add(tbDataSource);
            groupBox20.Location = new System.Drawing.Point(3, 267);
            groupBox20.Name = "groupBox20";
            groupBox20.Size = new System.Drawing.Size(378, 67);
            groupBox20.TabIndex = 17;
            groupBox20.TabStop = false;
            groupBox20.Text = "Data Source";
            // 
            // tbDataSource
            // 
            tbDataSource.Location = new System.Drawing.Point(6, 22);
            tbDataSource.Name = "tbDataSource";
            tbDataSource.Size = new System.Drawing.Size(366, 23);
            tbDataSource.TabIndex = 8;
            // 
            // groupBox17
            // 
            groupBox17.Controls.Add(cb_resourceType);
            groupBox17.Location = new System.Drawing.Point(7, 6);
            groupBox17.Name = "groupBox17";
            groupBox17.Size = new System.Drawing.Size(194, 58);
            groupBox17.TabIndex = 14;
            groupBox17.TabStop = false;
            groupBox17.Text = "Resource Type";
            // 
            // cb_resourceType
            // 
            cb_resourceType.FormattingEnabled = true;
            cb_resourceType.Location = new System.Drawing.Point(6, 22);
            cb_resourceType.Name = "cb_resourceType";
            cb_resourceType.Size = new System.Drawing.Size(182, 23);
            cb_resourceType.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage3.Controls.Add(groupBox14);
            tabPage3.Controls.Add(groupBox13);
            tabPage3.Controls.Add(groupBox12);
            tabPage3.Controls.Add(groupBox11);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(873, 895);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Geospacial & Temporal";
            // 
            // groupBox14
            // 
            groupBox14.Controls.Add(dtpEndDate);
            groupBox14.Controls.Add(btnEndDateClear);
            groupBox14.Location = new System.Drawing.Point(14, 194);
            groupBox14.Name = "groupBox14";
            groupBox14.Size = new System.Drawing.Size(296, 62);
            groupBox14.TabIndex = 29;
            groupBox14.TabStop = false;
            groupBox14.Text = "End Date";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new System.Drawing.Point(6, 22);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new System.Drawing.Size(200, 23);
            dtpEndDate.TabIndex = 23;
            // 
            // btnEndDateClear
            // 
            btnEndDateClear.Location = new System.Drawing.Point(212, 22);
            btnEndDateClear.Name = "btnEndDateClear";
            btnEndDateClear.Size = new System.Drawing.Size(75, 23);
            btnEndDateClear.TabIndex = 25;
            btnEndDateClear.Text = "Clear";
            btnEndDateClear.UseVisualStyleBackColor = true;
            btnEndDateClear.Click += btnEndDateClear_Click;
            // 
            // groupBox13
            // 
            groupBox13.Controls.Add(dtpStart);
            groupBox13.Controls.Add(btnStartDateClear);
            groupBox13.Location = new System.Drawing.Point(14, 139);
            groupBox13.Name = "groupBox13";
            groupBox13.Size = new System.Drawing.Size(296, 49);
            groupBox13.TabIndex = 28;
            groupBox13.TabStop = false;
            groupBox13.Text = "Start Date";
            // 
            // dtpStart
            // 
            dtpStart.Location = new System.Drawing.Point(6, 16);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new System.Drawing.Size(200, 23);
            dtpStart.TabIndex = 20;
            // 
            // btnStartDateClear
            // 
            btnStartDateClear.Location = new System.Drawing.Point(212, 16);
            btnStartDateClear.Name = "btnStartDateClear";
            btnStartDateClear.Size = new System.Drawing.Size(75, 23);
            btnStartDateClear.TabIndex = 24;
            btnStartDateClear.Text = "Clear";
            btnStartDateClear.UseVisualStyleBackColor = true;
            btnStartDateClear.Click += btnStartDateClear_Click;
            // 
            // groupBox12
            // 
            groupBox12.Controls.Add(cb_granularity);
            groupBox12.Location = new System.Drawing.Point(14, 71);
            groupBox12.Name = "groupBox12";
            groupBox12.Size = new System.Drawing.Size(146, 62);
            groupBox12.TabIndex = 27;
            groupBox12.TabStop = false;
            groupBox12.Text = "Granularity";
            // 
            // cb_granularity
            // 
            cb_granularity.FormattingEnabled = true;
            cb_granularity.Location = new System.Drawing.Point(6, 22);
            cb_granularity.Name = "cb_granularity";
            cb_granularity.Size = new System.Drawing.Size(121, 23);
            cb_granularity.TabIndex = 14;
            // 
            // groupBox11
            // 
            groupBox11.Controls.Add(tbGeoCoverage);
            groupBox11.Location = new System.Drawing.Point(14, 10);
            groupBox11.Name = "groupBox11";
            groupBox11.Size = new System.Drawing.Size(393, 55);
            groupBox11.TabIndex = 26;
            groupBox11.TabStop = false;
            groupBox11.Text = "Geographical Coverage";
            // 
            // tbGeoCoverage
            // 
            tbGeoCoverage.Location = new System.Drawing.Point(8, 22);
            tbGeoCoverage.Name = "tbGeoCoverage";
            tbGeoCoverage.Size = new System.Drawing.Size(366, 23);
            tbGeoCoverage.TabIndex = 12;
            // 
            // tabPage4
            // 
            tabPage4.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage4.Controls.Add(groupBox10);
            tabPage4.Controls.Add(groupBox9);
            tabPage4.Controls.Add(groupBox8);
            tabPage4.Controls.Add(groupBox7);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new System.Drawing.Size(873, 895);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Access";
            // 
            // groupBox10
            // 
            groupBox10.Controls.Add(tbJuristiction);
            groupBox10.Location = new System.Drawing.Point(15, 210);
            groupBox10.Name = "groupBox10";
            groupBox10.Size = new System.Drawing.Size(398, 72);
            groupBox10.TabIndex = 25;
            groupBox10.TabStop = false;
            groupBox10.Text = "Juristiction";
            // 
            // tbJuristiction
            // 
            tbJuristiction.Location = new System.Drawing.Point(6, 22);
            tbJuristiction.Name = "tbJuristiction";
            tbJuristiction.Size = new System.Drawing.Size(366, 23);
            tbJuristiction.TabIndex = 20;
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(tbDataProcessor);
            groupBox9.Location = new System.Drawing.Point(15, 138);
            groupBox9.Name = "groupBox9";
            groupBox9.Size = new System.Drawing.Size(398, 66);
            groupBox9.TabIndex = 24;
            groupBox9.TabStop = false;
            groupBox9.Text = "Data Processor";
            // 
            // tbDataProcessor
            // 
            tbDataProcessor.Location = new System.Drawing.Point(6, 22);
            tbDataProcessor.Name = "tbDataProcessor";
            tbDataProcessor.Size = new System.Drawing.Size(366, 23);
            tbDataProcessor.TabIndex = 18;
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(tbDataController);
            groupBox8.Location = new System.Drawing.Point(15, 69);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new System.Drawing.Size(398, 63);
            groupBox8.TabIndex = 23;
            groupBox8.TabStop = false;
            groupBox8.Text = "Data Controller";
            // 
            // tbDataController
            // 
            tbDataController.Location = new System.Drawing.Point(6, 22);
            tbDataController.Name = "tbDataController";
            tbDataController.Size = new System.Drawing.Size(366, 23);
            tbDataController.TabIndex = 16;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(tbAccessContact);
            groupBox7.Location = new System.Drawing.Point(15, 7);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new System.Drawing.Size(398, 56);
            groupBox7.TabIndex = 22;
            groupBox7.TabStop = false;
            groupBox7.Text = "Access Contact";
            // 
            // tbAccessContact
            // 
            tbAccessContact.Location = new System.Drawing.Point(5, 19);
            tbAccessContact.Name = "tbAccessContact";
            tbAccessContact.Size = new System.Drawing.Size(366, 23);
            tbAccessContact.TabIndex = 14;
            // 
            // tabPage5
            // 
            tabPage5.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage5.Controls.Add(groupBox3);
            tabPage5.Controls.Add(tableLayoutPanel1);
            tabPage5.Location = new System.Drawing.Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new System.Drawing.Size(873, 895);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Attribution";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(tbDOI);
            groupBox3.Location = new System.Drawing.Point(14, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(318, 56);
            groupBox3.TabIndex = 27;
            groupBox3.TabStop = false;
            groupBox3.Text = "DOI";
            // 
            // tbDOI
            // 
            tbDOI.Location = new System.Drawing.Point(6, 22);
            tbDOI.Name = "tbDOI";
            tbDOI.Size = new System.Drawing.Size(303, 23);
            tbDOI.TabIndex = 18;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(groupBox2, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 1);
            tableLayoutPanel1.Location = new System.Drawing.Point(14, 71);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(318, 206);
            tableLayoutPanel1.TabIndex = 26;
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.Controls.Add(fftControlledVocab);
            groupBox2.Location = new System.Drawing.Point(3, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(312, 70);
            groupBox2.TabIndex = 25;
            groupBox2.TabStop = false;
            groupBox2.Text = "ControlledGroup";
            // 
            // fftControlledVocab
            // 
            fftControlledVocab.AutoScroll = true;
            fftControlledVocab.AutoSize = true;
            fftControlledVocab.Location = new System.Drawing.Point(6, 22);
            fftControlledVocab.MaximumSize = new System.Drawing.Size(300, 0);
            fftControlledVocab.MinimumSize = new System.Drawing.Size(100, 20);
            fftControlledVocab.Name = "fftControlledVocab";
            fftControlledVocab.Size = new System.Drawing.Size(300, 26);
            fftControlledVocab.TabIndex = 23;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(ffcPeople);
            groupBox1.Location = new System.Drawing.Point(3, 79);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(312, 97);
            groupBox1.TabIndex = 24;
            groupBox1.TabStop = false;
            groupBox1.Text = "People";
            // 
            // ffcPeople
            // 
            ffcPeople.AutoScroll = true;
            ffcPeople.AutoSize = true;
            ffcPeople.Location = new System.Drawing.Point(6, 22);
            ffcPeople.MaximumSize = new System.Drawing.Size(300, 0);
            ffcPeople.MinimumSize = new System.Drawing.Size(100, 20);
            ffcPeople.Name = "ffcPeople";
            ffcPeople.Size = new System.Drawing.Size(300, 53);
            ffcPeople.TabIndex = 22;
            // 
            // tabPage6
            // 
            tabPage6.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage6.Controls.Add(groupBox6);
            tabPage6.Controls.Add(groupBox5);
            tabPage6.Controls.Add(groupBox4);
            tabPage6.Location = new System.Drawing.Point(4, 24);
            tabPage6.Name = "tabPage6";
            tabPage6.Size = new System.Drawing.Size(873, 895);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "Data Updates";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(cbUpdateLag);
            groupBox6.Location = new System.Drawing.Point(15, 131);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new System.Drawing.Size(155, 59);
            groupBox6.TabIndex = 28;
            groupBox6.TabStop = false;
            groupBox6.Text = "Update Lag";
            // 
            // cbUpdateLag
            // 
            cbUpdateLag.FormattingEnabled = true;
            cbUpdateLag.Location = new System.Drawing.Point(6, 22);
            cbUpdateLag.Name = "cbUpdateLag";
            cbUpdateLag.Size = new System.Drawing.Size(140, 23);
            cbUpdateLag.TabIndex = 23;
            cbUpdateLag.Format += FormatCB;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(dtpReleaseDate);
            groupBox5.Controls.Add(btnReleaseDateClear);
            groupBox5.Location = new System.Drawing.Point(15, 72);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new System.Drawing.Size(293, 53);
            groupBox5.TabIndex = 27;
            groupBox5.TabStop = false;
            groupBox5.Text = "Initial Release Date";
            // 
            // dtpReleaseDate
            // 
            dtpReleaseDate.Location = new System.Drawing.Point(6, 22);
            dtpReleaseDate.Name = "dtpReleaseDate";
            dtpReleaseDate.Size = new System.Drawing.Size(200, 23);
            dtpReleaseDate.TabIndex = 24;
            // 
            // btnReleaseDateClear
            // 
            btnReleaseDateClear.Location = new System.Drawing.Point(212, 22);
            btnReleaseDateClear.Name = "btnReleaseDateClear";
            btnReleaseDateClear.Size = new System.Drawing.Size(75, 23);
            btnReleaseDateClear.TabIndex = 25;
            btnReleaseDateClear.Text = "Clear";
            btnReleaseDateClear.UseVisualStyleBackColor = true;
            btnReleaseDateClear.Click += btnReleaseDateClear_Click;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(cb_updateFrequency);
            groupBox4.Location = new System.Drawing.Point(15, 19);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(137, 47);
            groupBox4.TabIndex = 26;
            groupBox4.TabStop = false;
            groupBox4.Text = "Update Frequency";
            // 
            // cb_updateFrequency
            // 
            cb_updateFrequency.FormattingEnabled = true;
            cb_updateFrequency.Location = new System.Drawing.Point(6, 15);
            cb_updateFrequency.Name = "cb_updateFrequency";
            cb_updateFrequency.Size = new System.Drawing.Size(121, 23);
            cb_updateFrequency.TabIndex = 22;
            cb_updateFrequency.Format += FormatCB;
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
            groupBox23.ResumeLayout(false);
            groupBox23.PerformLayout();
            groupBox16.ResumeLayout(false);
            groupBox16.PerformLayout();
            groupBox15.ResumeLayout(false);
            groupBox15.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            groupBox18.ResumeLayout(false);
            groupBox18.PerformLayout();
            groupBox22.ResumeLayout(false);
            groupBox22.PerformLayout();
            groupBox19.ResumeLayout(false);
            groupBox19.PerformLayout();
            groupBox21.ResumeLayout(false);
            groupBox21.PerformLayout();
            groupBox20.ResumeLayout(false);
            groupBox20.PerformLayout();
            groupBox17.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            groupBox14.ResumeLayout(false);
            groupBox13.ResumeLayout(false);
            groupBox12.ResumeLayout(false);
            groupBox11.ResumeLayout(false);
            groupBox11.PerformLayout();
            tabPage4.ResumeLayout(false);
            groupBox10.ResumeLayout(false);
            groupBox10.PerformLayout();
            groupBox9.ResumeLayout(false);
            groupBox9.PerformLayout();
            groupBox8.ResumeLayout(false);
            groupBox8.PerformLayout();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage6.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
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
        public TextBox tbDescription;
        private TextBox tbDataSource;
        private ComboBox cb_resourceType;
        private TextBox tbDataSourceSetting;
        private ComboBox cb_granularity;
        private TextBox tbGeoCoverage;
        private TextBox tbJuristiction;
        private TextBox tbDataProcessor;
        private TextBox tbDataController;
        private TextBox tbAccessContact;
        private TextBox tbDOI;
        private ComboBox cb_updateFrequency;
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
        private SimpleControls.MultiSelectChips.FreeFormTextChipDisplay fftControlledVocab;
        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private GroupBox groupBox3;
        private GroupBox groupBox6;
        private GroupBox groupBox5;
        private GroupBox groupBox4;
        private GroupBox groupBox8;
        private GroupBox groupBox7;
        private GroupBox groupBox10;
        private GroupBox groupBox9;
        private GroupBox groupBox14;
        private GroupBox groupBox13;
        private GroupBox groupBox12;
        private GroupBox groupBox11;
        private GroupBox groupBox16;
        private GroupBox groupBox15;
        private GroupBox groupBox22;
        private GroupBox groupBox21;
        private GroupBox groupBox20;
        private GroupBox groupBox19;
        private GroupBox groupBox18;
        private GroupBox groupBox17;
        private SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay ddDatasetSubtype;
        private SimpleControls.MultiSelectChips.DropdownOptionsChipDisplay ddDatasetType;
        private TableLayoutPanel tableLayoutPanel2;
        private GroupBox groupBox23;
        public TextBox tbAcronym;
    }
}
