// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NPOI.HSSF.Record.Chart;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.MainFormUITabs;

/// <summary>
/// Allows you to modify the descriptive data stored in the RDMP database about the selected Catalogue (dataset).  Pressing Ctrl+S will save any changes.  You should make sure that you
/// provide as much background about your datasets as possible since this is the information that will be given to researchers when you extract the dataset (as well as being a great
/// reference for when you find a dataset and you're not quite sure about what it contains or how it got there or who supplied it etc).
/// 
/// <para>The collection of fields for documentation were chosen by committee and based on the 'Dublin Core'.  Realistically though just entering all the information into 'Resource
/// Description' is probably a more realistic goal.  Documentation may be boring but it is absolutely vital for handling providence of research datasets especially if you frequently
/// get given small datasets from researchers (e.g. questionnaire data they have collected) for use in cohort generation etc).</para>
/// 
/// <para>There is also a box for storing a ticket number, this will let you reference a ticket in your ticketing system (e.g. Jira, Fogbugz etc).  This requires selecting/writing a compatible
/// plugin for your ticketing system and configuring it (see TicketingSystemConfigurationUI)</para>
/// </summary>
public partial class CatalogueUI : CatalogueUI_Design, ISaveableUI
{
    internal Scintilla _scintillaDescription;

    private Catalogue _catalogue;

    public CatalogueUI()
    {
        InitializeComponent();

        ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;

        AssociatedCollection = RDMPCollection.Catalogue;

        //c_ddType.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueType));
        //c_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
        //c_ddGranularity.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueGranularity));

        UseCommitSystem = true;
    }

    private void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
    {
        if (_catalogue != null)
            _catalogue.Ticket = ticketingControl1.TicketText;
    }

    private void c_ddOverrideChildren_Click(object sender, EventArgs e)
    {
        if (_catalogue != null)
            if (Activator.YesNo("Are you sure?", "Confirm"))
            {
                var catalogueItems = _catalogue.CatalogueItems.ToArray();
                foreach (var catalogueItem in catalogueItems)
                {
                    catalogueItem.Periodicity = _catalogue.Periodicity;
                    catalogueItem.Topic = _catalogue.Search_keywords;
                    catalogueItem.SaveToDatabase();
                }
            }
    }

    //private void tbName_TextChanged(object sender, EventArgs e)
    //{
    //    if (!Catalogue.IsAcceptableName(tbName.Text, out var reasonInvalid))
    //        errorProvider1.SetError(tbName, reasonInvalid);
    //    else
    //        errorProvider1.Clear();
    //}

    //private void ddExplicitConsent_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if (ddExplicitConsent.Text.Equals("Yes"))
    //        _catalogue.Explicit_consent = true;

    //    if (ddExplicitConsent.Text.Equals("No"))
    //        _catalogue.Explicit_consent = false;

    //    if (string.IsNullOrWhiteSpace(ddExplicitConsent.Text))
    //        _catalogue.Explicit_consent = null;
    //}

    //private void tbFolder_TextChanged(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        _catalogue.Folder = tbFolder.Text;
    //        tbFolder.ForeColor = Color.Black;
    //    }
    //    catch (Exception)
    //    {
    //        tbFolder.ForeColor = Color.Red;
    //    }
    //}

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        if (_scintillaDescription == null)
        {
            var f = new ScintillaTextEditorFactory();
            _scintillaDescription = f.Create(null, SyntaxLanguage.None, null, true, false, activator.CurrentDirectory);
            _scintillaDescription.Font = SystemFonts.DefaultFont;
            _scintillaDescription.WrapMode = WrapMode.Word;
            //panel1.Controls.Add(_scintillaDescription);
        }

        base.SetDatabaseObject(activator, databaseObject);

        _catalogue = databaseObject;
        var associatedDatasets = _catalogue.CatalogueItems
            .Select(static catalogueItem => catalogueItem.ColumnInfo.Dataset_ID)
            .Where(static datasetId => datasetId != null)
            .Select(datasetId =>
                _catalogue.CatalogueRepository.GetAllObjectsWhere<Dataset>("ID", datasetId).First())
            .Select(static ds => ds.Name).ToList();
        //if (associatedDatasets.Count > 0)
        //{
        //    lbDatasets.Visible = true;
        //    lbDatasetslbl.Visible = true;
        //    var finalString = associatedDatasets.Count == 1 ? associatedDatasets[0] : string.Join(", ", associatedDatasets.ToArray(), 0, associatedDatasets.Count - 1) + " and " + associatedDatasets.LastOrDefault();
        //    lbDatasets.Text = $"This catalogues contains data from the datasets:{finalString}";
        //}
        //else
        //{
        //    lbDatasets.Visible = false;
        //    lbDatasetslbl.Visible = false;
        //}
        this.editableCatalogueName.TextValue = _catalogue.Name;
        this.editableCatalogueName.Title = "Name";
        this.editableCatalogueName.Icon = CatalogueIcons.Catalogue.ImageToBitmap();
        this.editableFolder.TextValue = _catalogue.Folder;
        this.editableFolder.Title = "Folder";
        this.editableFolder.Icon = CatalogueIcons.CatalogueFolder.ImageToBitmap();
        RefreshUIFromDatabase();

    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, Catalogue databaseObject)
    {
        base.SetBindings(rules, databaseObject);
        Bind(cbColdStorage, "Checked", "IsColdStorageDataset", c => c.IsColdStorageDataset);
        Bind(cbDeprecated, "Checked", "IsDeprecated", c => c.IsDeprecated);
        Bind(cbInternal, "Checked", "IsInternalDataset", c => c.IsInternalDataset);
        Bind(editableCatalogueName, "TextValue", "Name", c => c.Name);
        Bind(editableFolder, "TextValue", "Folder", c => c.Folder);
        tabControl1_SelectedIndexChanged(this.tabControl1, null);

        //--------------------------------
        //Bind(tbAcronym, "Text", "Acronym", c => c.Acronym);
        //Bind(tbName, "Text", "Name", c => c.Name);
        //Bind(c_tbID, "Text", "ID", c => c.ID);
        //Bind(_scintillaDescription, "Text", "Description", c => c.Description);

        //Bind(c_ddType, "SelectedItem", "Type", c => c.Type);
        //Bind(c_ddGranularity, "SelectedItem", "Granularity", c => c.Granularity);
        //Bind(c_ddPeriodicity, "SelectedItem", "Periodicity", c => c.Periodicity);

        //Bind(cbColdStorage, "Checked", "IsColdStorageDataset", c => c.IsColdStorageDataset);
        //Bind(cbDeprecated, "Checked", "IsDeprecated", c => c.IsDeprecated);
        //Bind(cbInternal, "Checked", "IsInternalDataset", c => c.IsInternalDataset);

        //Bind(c_tbGeographicalCoverage, "Text", "Geographical_coverage", c => c.Geographical_coverage);
        //Bind(c_tbBackgroundSummary, "Text", "Background_summary", c => c.Background_summary);
        //Bind(c_tbTopics, "Text", "Search_keywords", c => c.Search_keywords);
        //Bind(c_tbUpdateFrequency, "Text", "Update_freq", c => c.Update_freq);
        //Bind(c_tbUpdateSchedule, "Text", "Update_sched", c => c.Update_sched);
        //Bind(c_tbTimeCoverage, "Text", "Time_coverage", c => c.Time_coverage);
        //Bind(tbAdministrativeContactName, "Text", "Contact_details", c => c.Contact_details);
        //Bind(c_tbResourceOwner, "Text", "Resource_owner", c => c.Resource_owner);
        //Bind(c_tbAttributionCitation, "Text", "Attribution_citation", c => c.Attribution_citation);
        //Bind(c_tbAccessOptions, "Text", "Access_options", c => c.Access_options);
        //Bind(c_tbSubjectNumbers, "Text", "SubjectNumbers", c => c.SubjectNumbers);

        //Bind(tbDataStandards, "Text", "Data_standards", c => c.Data_standards);
        //Bind(tbAdministrativeContactName, "Text", "Administrative_contact_name", c => c.Administrative_contact_name);
        //Bind(tbAdministrativeContactEmail, "Text", "Administrative_contact_email", c => c.Administrative_contact_email);
        //Bind(tbAdministrativeContactTelephone, "Text", "Administrative_contact_telephone",
        //    c => c.Administrative_contact_telephone);
        //Bind(tbAdministrativeContactAddress, "Text", "Administrative_contact_address",
        //    c => c.Administrative_contact_address);
        //Bind(tbCountryOfOrigin, "Text", "Country_of_origin", c => c.Country_of_origin);
        //Bind(tbEthicsApprover, "Text", "Ethics_approver", c => c.Ethics_approver);
        //Bind(tbSourceOfDataCollection, "Text", "Source_of_data_collection", c => c.Source_of_data_collection);
        //Bind(c_tbAttributionCitation, "Text", "Attribution_citation", c => c.Attribution_citation);
        //Bind(c_tbAccessOptions, "Text", "Access_options", c => c.Access_options);
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        ticketingControl1.SetItemActivator(activator);
    }

    private void RefreshUIFromDatabase()
    {
        try
        {
            ticketingControl1.ReCheckTicketingSystemInCatalogue();
        }
        catch (Exception e)
        {
            ExceptionViewer.Show(e);
        }

        splitContainer1.Enabled = true;
        ticketingControl1.Enabled = true;

        ticketingControl1.TicketText = _catalogue.Ticket;

        //tbFolder.Text = _catalogue.Folder;

        //if (_catalogue.Explicit_consent == null)
        //    ddExplicitConsent.Text = "";
        //else if (_catalogue.Explicit_consent == true)
        //    ddExplicitConsent.Text = "Yes";
        //else
        //    ddExplicitConsent.Text = "No";

        //c_tbLastRevisionDate.Text = _catalogue.Last_revision_date.ToString();
        //tbDatasetStartDate.Text = _catalogue.DatasetStartDate.ToString();

        //c_tbAPIAccessURL.Text = _catalogue.API_access_URL != null ? _catalogue.API_access_URL.ToString() : "";
        //c_tbBrowseUrl.Text = _catalogue.Browse_URL != null ? _catalogue.Browse_URL.ToString() : "";
        //c_tbBulkDownloadUrl.Text = _catalogue.Bulk_Download_URL != null ? _catalogue.Bulk_Download_URL.ToString() : "";
        //c_tbQueryToolUrl.Text = _catalogue.Query_tool_URL != null ? _catalogue.Query_tool_URL.ToString() : "";
        //c_tbSourceUrl.Text = _catalogue.Source_URL != null ? _catalogue.Source_URL.ToString() : "";
        //c_tbDetailPageURL.Text = _catalogue.Detail_Page_URL != null ? _catalogue.Detail_Page_URL.ToString() : "";
    }


    private void c_tbLastRevisionDate_TextChanged(object sender, EventArgs e)
    {
        //SetDate(c_tbLastRevisionDate, v => _catalogue.Last_revision_date = v);
    }

    private void tbDatasetStartDate_TextChanged(object sender, EventArgs e)
    {
        //SetDate(tbDatasetStartDate, v => _catalogue.DatasetStartDate = v);
    }

    private void c_tbDetailPageURL_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.Detail_Page_URL = v);
    }

    private void c_tbAPIAccessURL_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.API_access_URL = v);
    }

    private void c_tbBrowseUrl_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.Browse_URL = v);
    }

    private void c_tbBulkDownloadUrl_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.Bulk_Download_URL = v);
    }

    private void c_tbQueryToolUrl_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.Query_tool_URL = v);
    }

    private void c_tbSourceUrl_TextChanged(object sender, EventArgs e)
    {
        SetUrl((TextBox)sender, v => _catalogue.Source_URL = v);
    }

    private void ticketingControl1_Load(object sender, EventArgs e)
    {

    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private List<int> setTabBindings = new();

    private void UpdateStartDate(object sender, EventArgs e)
    {
        dtpStart.ValueChanged -= UpdateStartDate;
        _catalogue.StartDate = dtpStart.Value;
        dtpStart.CustomFormat = "yyyy/MM/dd";
        Bind(dtpStart, "Value", "StartDate", c => c.StartDate);
    }

    private void UpdateEndDate(object sender, EventArgs e)
    {
        dtpEndDate.ValueChanged -= UpdateEndDate;
        _catalogue.EndDate = dtpEndDate.Value;
        dtpEndDate.CustomFormat = "yyyy/MM/dd";
        Bind(dtpEndDate, "Value", "EndDate", c => c.EndDate);
    }

    private void UpdateReleaseDate(object sender, EventArgs e)
    {
        dtpReleaseDate.ValueChanged -= UpdateReleaseDate;
        _catalogue.DatasetReleaseDate = dtpReleaseDate.Value;
        dtpReleaseDate.CustomFormat = "yyyy/MM/dd";
        Bind(dtpReleaseDate, "Value", "DatasetReleaseDate", c => c.DatasetReleaseDate);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        var tabControl = (TabControl)sender;
        var selectedIndex = tabControl.SelectedIndex;
        if (setTabBindings.Contains(selectedIndex)) return;
        switch (selectedIndex)
        {
            case 0:
                foreach (Control item in tabPage1.Controls)
                {
                    item.Visible = false;
                }
                Bind(tbAbstract, "Text", "ShortDescription", c => c.ShortDescription);
                Bind(tbDescription, "Text", "Description", c => c.Description);
                foreach (Control item in tabPage1.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 1:
                foreach (Control item in tabPage2.Controls)
                {
                    item.Visible = false;
                }
                cb_resourceType.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueType));
                cb_datasetType.DataSource = Enum.GetValues(typeof(Catalogue.DatasetType));
                cb_datasetSubtype.DataSource = Enum.GetValues(typeof(Catalogue.DatasetSubType));
                Bind(tbKeywords, "Text", "Search_keywords", c => c.Search_keywords);
                Bind(cb_resourceType, "SelectedItem", "Type", c => c.Type);
                Bind(cb_datasetType, "SelectedItem", "DataType", c => c.DataType);
                Bind(cb_datasetSubtype, "SelectedItem", "DataSubtype", c => c.DataSubtype);
                Bind(tbDataSource, "Text", "DataSource", c => c.DataSource);
                Bind(tbDataSourceSetting, "Text", "DataSourceSetting", c => c.DataSourceSetting);


                foreach (Control item in tabPage2.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 2:
                foreach (Control item in tabPage3.Controls)
                {
                    item.Visible = false;
                }
                Bind(tbGeoCoverage, "Text", "Geographical_coverage", c => c.Geographical_coverage);
                cb_granularity.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueGranularity));
                Bind(cb_granularity, "SelectedItem", "Granularity", c => c.Granularity);
                dtpStart.Format = DateTimePickerFormat.Custom;
                dtpStart.CustomFormat = _catalogue.StartDate != null ? "yyyy/MM/dd" : " ";
                dtpEndDate.Format = DateTimePickerFormat.Custom;
                dtpEndDate.CustomFormat = _catalogue.EndDate != null ? "yyyy/MM/dd" : " ";
                if (_catalogue.StartDate != null)
                {
                    Bind(dtpStart, "Value", "StartDate", c => c.StartDate);
                }
                else
                {
                    dtpStart.ValueChanged += UpdateStartDate;
                }
                if (_catalogue.EndDate != null)
                {
                    Bind(dtpEndDate, "Value", "EndDate", c => c.EndDate);
                }
                else
                {
                    dtpEndDate.ValueChanged += UpdateEndDate;
                }
                foreach (Control item in tabPage3.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 3:
                foreach (Control item in tabPage4.Controls)
                {
                    item.Visible = false;
                }
                Bind(tbAccessContact, "Text", "Administrative_contact_email", c => c.Administrative_contact_email);
                Bind(tbDataController, "Text", "DataController", c => c.DataController);
                Bind(tbDataProcessor, "Text", "DataProcessor", c => c.DataProcessor);
                Bind(tbJuristiction, "Text", "Juristiction", c => c.Juristiction);
                foreach (Control item in tabPage4.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 4:
                foreach (Control item in tabPage5.Controls)
                {
                    item.Visible = false;
                }
                Bind(tbPeople, "Text", "AssociatedPeople", c => c.AssociatedPeople);
                Bind(tbControlledVocabulary, "Text", "ControlledVocabulary", c => c.ControlledVocabulary);
                Bind(tbDOI, "Text", "Doi", c => c.Doi);
                foreach (Control item in tabPage5.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 5:
                foreach (Control item in tabPage6.Controls)
                {
                    item.Visible = false;
                }

                cb_updateFrequency.DataSource = Enum.GetValues(typeof(Catalogue.UpdateFrequencies));
                cbUpdateLag.DataSource = Enum.GetValues(typeof(Catalogue.UpdateLagTimes));
                //todo cb not being set correctly

                Bind(cbUpdateLag, "SelectedItem", "UpdateLag", c => c.UpdateLag);
                Bind(cb_updateFrequency, "SelectedItem", "Update_freq", c => c.Update_freq);
                dtpReleaseDate.Format = DateTimePickerFormat.Custom;
                dtpReleaseDate.CustomFormat = _catalogue.StartDate != null ? "yyyy/MM/dd" : " ";
                if (_catalogue.DatasetReleaseDate != null)
                {
                    Bind(dtpReleaseDate, "Value", "DatasetReleaseDate", c => c.DatasetReleaseDate);
                }
                else
                {
                    dtpReleaseDate.ValueChanged += UpdateReleaseDate;
                }
                foreach (Control item in tabPage6.Controls)
                {
                    item.Visible = true;
                }
                break;
            default:
                break;
        }
        setTabBindings.Add(selectedIndex);

        //Bind(tbAbstract, "Text", "ShortDescription", c => c.ShortDescription);
        //Bind(tbDescription, "Text", "Description", c => c.Description);
        //Bind(tbKeywords, "Text", "Search_keywords", c => c.Search_keywords);
        //Bind(tbDataSource, "Text", "DataSource", c => c.DataSource);
        //Bind(tbDataSourceSetting, "Text", "DataSourceSetting", c => c.DataSourceSetting);
        //Bind(tbGeoCoverage, "Text", "Geographical_coverage", c => c.Geographical_coverage);
        ////Bind(dtpStart, "Value", "StartDate", c => c.StartDate);
        ////Bind(dtpEndDate, "Value", "EndDate", c => c.EndDate);
        //Bind(tbAccessContact, "Text", "Administrative_contact_email", c => c.Administrative_contact_email);
        //Bind(tbDataController, "Text", "DataController", c => c.DataController);
        //Bind(tbDataProcessor, "Text", "DataProcessor", c => c.DataProcessor);
        //Bind(tbJuristiction, "Text", "Juristiction", c => c.Juristiction);
        //Bind(tbPeople, "Text", "AssociatedPeople", c => c.AssociatedPeople);
        //Bind(tbDOI, "Text", "Doi", c => c.Doi);
        //Bind(tbInitialReleaseDate, "Text", "DatasetReleaseDate", c => c.DatasetReleaseDate);
        //Bind(tbUpdateLag, "Text", "UpdateLag", c => c.UpdateLag);
    }

    private void btnStartDateClear_Click(object sender, EventArgs e)
    {
        dtpStart.CustomFormat = " ";
        dtpStart.DataBindings.Clear();
        _catalogue.StartDate = null;
        dtpStart.ValueChanged += UpdateStartDate;
    }

    private void btnEndDateClear_Click(object sender, EventArgs e)
    {
        dtpEndDate.CustomFormat = " ";
        dtpEndDate.DataBindings.Clear();
        _catalogue.EndDate = null;
        dtpEndDate.ValueChanged += UpdateEndDate;
    }

    private void btnReleaseDateClear_Click(object sender, EventArgs e)
    {
        dtpReleaseDate.CustomFormat = " ";
        dtpReleaseDate.DataBindings.Clear();
        _catalogue.DatasetReleaseDate = null;
        dtpReleaseDate.ValueChanged += UpdateReleaseDate;
    }

    string AddSpacesToSentence(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }


    private void FormatCB(object sender, ListControlConvertEventArgs e)
    {
        e.Value = AddSpacesToSentence(e.Value.ToString());
    }

}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueUI_Design, UserControl>))]
public abstract class CatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}