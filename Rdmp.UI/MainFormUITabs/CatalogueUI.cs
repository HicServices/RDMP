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

        UseCommitSystem = true;
    }

    private string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1]))))
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
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

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        if (_scintillaDescription == null)
        {
            var f = new ScintillaTextEditorFactory();
            _scintillaDescription = f.Create(null, SyntaxLanguage.None, null, true, false, activator.CurrentDirectory);
            _scintillaDescription.Font = SystemFonts.DefaultFont;
            _scintillaDescription.WrapMode = WrapMode.Word;
        }

        base.SetDatabaseObject(activator, databaseObject);

        _catalogue = databaseObject;
        var associatedDatasets = _catalogue.CatalogueItems
            .Select(static catalogueItem => catalogueItem.ColumnInfo.Dataset_ID)
            .Where(static datasetId => datasetId != null)
            .Select(datasetId =>
                _catalogue.CatalogueRepository.GetAllObjectsWhere<Dataset>("ID", datasetId).First())
            .Select(static ds => ds.Name).ToList();
        editableCatalogueName.TextValue = _catalogue.Name;
        editableCatalogueName.Title = "Name";
        editableCatalogueName.Icon = CatalogueIcons.Catalogue.ImageToBitmap();
        editableFolder.TextValue = _catalogue.Folder;
        editableFolder.Title = "Folder";
        editableFolder.Icon = CatalogueIcons.CatalogueFolder.ImageToBitmap();
        RefreshUIFromDatabase();

    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, Catalogue databaseObject)
    {
        base.SetBindings(rules, databaseObject);
        Bind(cbDeprecated, "Checked", "IsDeprecated", c => c.IsDeprecated);
        Bind(cbInternal, "Checked", "IsInternalDataset", c => c.IsInternalDataset);
        Bind(editableCatalogueName, "TextValue", "Name", c => c.Name);
        Bind(editableFolder, "TextValue", "Folder", c => c.Folder);
        tabControl1_SelectedIndexChanged(tabControl1, null);

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

    }


    private void c_tbLastRevisionDate_TextChanged(object sender, EventArgs e)
    {
    }

    private void tbDatasetStartDate_TextChanged(object sender, EventArgs e)
    {
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

    private readonly List<int> setTabBindings = new();

    private void UpdateStartDate(object sender, EventArgs e)
    {
        dtpStart.ValueChanged -= UpdateStartDate;
        _catalogue.StartDate = dtpStart.Value;
        dtpStart.CustomFormat = "dd/MM/yyyy";
        Bind(dtpStart, "Value", "StartDate", c => c.StartDate);
    }

    private void UpdateEndDate(object sender, EventArgs e)
    {
        dtpEndDate.ValueChanged -= UpdateEndDate;
        _catalogue.EndDate = dtpEndDate.Value;
        dtpEndDate.CustomFormat = "dd/MM/yyyy";
        Bind(dtpEndDate, "Value", "EndDate", c => c.EndDate);
    }

    private void UpdateReleaseDate(object sender, EventArgs e)
    {
        dtpReleaseDate.ValueChanged -= UpdateReleaseDate;
        _catalogue.DatasetReleaseDate = dtpReleaseDate.Value;
        dtpReleaseDate.CustomFormat = "dd/MM/yyyy";
        Bind(dtpReleaseDate, "Value", "DatasetReleaseDate", c => c.DatasetReleaseDate);
    }


    private void comboBox1_Format(object sender, ListControlConvertEventArgs e)
    {
        e.Value = AddSpacesToSentence(e.ListItem.ToString(), true);
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
                Bind(tbAcronym, "Text", "Acronym", c => c.Acronym);
                Bind(tbAbstract, "Text", "ShortDescription", c => c.ShortDescription);
                Bind(tbDescription, "Text", "Description", c => c.Description);
                aiAcronym.TooltipText = CatalogueUIHelperText.Acronym;
                aiAcronym.SetItemActivator(Activator);
                aiShortDescription.TooltipText = CatalogueUIHelperText.ShortDescription;
                aiShortDescription.SetItemActivator(Activator);
                aiDescription.TooltipText = CatalogueUIHelperText.Description;
                aiDescription.SetItemActivator(Activator);
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
                cb_resourceType.Format += comboBox1_Format;
                cbPurpose.DataSource = Enum.GetValues(typeof(Catalogue.DatasetPurpose));
                cbPurpose.Format += comboBox1_Format;
                ddDatasetType.Options = Enum.GetNames(typeof(Catalogue.DatasetType));
                ddDatasetSubtype.Options = Enum.GetNames(typeof(Catalogue.DatasetSubType));
                ddDataSource.Options = Enum.GetNames(typeof(Catalogue.DataSourceTypes));
                ddDataSourceSetting.Options = Enum.GetNames(typeof(Catalogue.DataSourceSettingTypes));

                Bind(ffcKeywords, "Value", "Search_keywords", c => c.Search_keywords);
                Bind(cb_resourceType, "SelectedItem", "Type", c => c.Type);
                Bind(cbPurpose, "SelectedItem", "Purpose", c => c.Purpose);
                Bind(ddDatasetType, "Value", "DataType", c => c.DataType);
                Bind(ddDatasetSubtype, "Value", "DataSubType", c => c.DataSubType);
                Bind(ddDataSource, "Value", "DataSource", c => c.DataSource);
                Bind(ddDataSourceSetting, "Value", "DataSourceSetting", c => c.DataSourceSetting);
                aiKeywords.TooltipText = CatalogueUIHelperText.Keywords;
                aiKeywords.SetItemActivator(Activator);

                aiResourceType.TooltipText = CatalogueUIHelperText.ResourceType;
                aiResourceType.SetItemActivator(Activator);

                aiPurposeOfDataset.TooltipText = CatalogueUIHelperText.PurposeOfDataset;
                aiPurposeOfDataset.SetItemActivator(Activator);

                aiDatasetType.TooltipText = CatalogueUIHelperText.DatasetType;
                aiDatasetType.SetItemActivator(Activator);

                aiDatasetSubtype.TooltipText = CatalogueUIHelperText.DatasetSubtype;
                aiDatasetSubtype.SetItemActivator(Activator);

                aiDataSource.TooltipText = CatalogueUIHelperText.DataSource;
                aiDataSource.SetItemActivator(Activator);

                aiDataSourceSetting.TooltipText = CatalogueUIHelperText.DataSourceSetting;
                aiDataSourceSetting.SetItemActivator(Activator);

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
                cb_granularity.Format += comboBox1_Format;
                Bind(cb_granularity, "SelectedItem", "Granularity", c => c.Granularity);
                dtpStart.Format = DateTimePickerFormat.Custom;
                dtpStart.CustomFormat = _catalogue.StartDate != null ? "dd/MM/yyyy" : " ";
                dtpEndDate.Format = DateTimePickerFormat.Custom;
                dtpEndDate.CustomFormat = _catalogue.EndDate != null ? "dd/MM/yyyy" : " ";
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
                aiGeographicalCoverage.TooltipText = CatalogueUIHelperText.GeographicalCoverage;
                aiGeographicalCoverage.SetItemActivator(Activator);

                aiGranularity.TooltipText = CatalogueUIHelperText.Granularity;
                aiGranularity.SetItemActivator(Activator);

                aiStartDate.TooltipText = CatalogueUIHelperText.StartDate;
                aiStartDate.SetItemActivator(Activator);

                aiEndDate.TooltipText = CatalogueUIHelperText.EndDate;
                aiEndDate.SetItemActivator(Activator);


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
                aiAccessContact.TooltipText = CatalogueUIHelperText.AccessContact;
                aiAccessContact.SetItemActivator(Activator);

                aiDataController.TooltipText = CatalogueUIHelperText.DataController;
                aiDataController.SetItemActivator(Activator);

                aiDataProcessor.TooltipText = CatalogueUIHelperText.DataProcessor;
                aiDataProcessor.SetItemActivator(Activator);

                aiJuristiction.TooltipText = CatalogueUIHelperText.Juristiction;
                aiJuristiction.SetItemActivator(Activator);


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
                Bind(ffcPeople, "Value", "AssociatedPeople", c => c.AssociatedPeople);
                Bind(fftControlledVocab, "Value", "ControlledVocabulary", c => c.ControlledVocabulary);
                Bind(tbDOI, "Text", "Doi", c => c.Doi);
                aiPeople.TooltipText = CatalogueUIHelperText.People;
                aiPeople.SetItemActivator(Activator);

                aiControlledGroup.TooltipText = CatalogueUIHelperText.ControlledVocabulary;
                aiControlledGroup.SetItemActivator(Activator);

                aiDOI.TooltipText = CatalogueUIHelperText.DOI;
                aiDOI.SetItemActivator(Activator);


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
                cb_updateFrequency.Format += comboBox1_Format;
                cbUpdateLag.DataSource = Enum.GetValues(typeof(Catalogue.UpdateLagTimes));
                cbUpdateLag.Format += comboBox1_Format;
                Bind(cbUpdateLag, "SelectedItem", "UpdateLag", c => c.UpdateLag);
                Bind(cb_updateFrequency, "SelectedItem", "Update_freq", c => c.Update_freq);
                dtpReleaseDate.Format = DateTimePickerFormat.Custom;
                dtpReleaseDate.CustomFormat = _catalogue.DatasetReleaseDate != null ? "dd/MM/yyyy" : " ";
                if (_catalogue.DatasetReleaseDate != null)
                {
                    Bind(dtpReleaseDate, "Value", "DatasetReleaseDate", c => c.DatasetReleaseDate);
                }
                else
                {
                    dtpReleaseDate.ValueChanged += UpdateReleaseDate;
                }
                aiUpdateLag.TooltipText = CatalogueUIHelperText.UpdateLag;
                aiUpdateLag.SetItemActivator(Activator);

                aiUpdateFrequency.TooltipText = CatalogueUIHelperText.UpdateFrequency;
                aiUpdateFrequency.SetItemActivator(Activator);

                aiInitialReleaseDate.TooltipText = CatalogueUIHelperText.InitialReleaseDate;
                aiInitialReleaseDate.SetItemActivator(Activator);

                foreach (Control item in tabPage6.Controls)
                {
                    item.Visible = true;
                }
                break;
            case 6:
                Bind(ffAssociatedMedia, "Value", "AssociatedMedia", c => c.AssociatedMedia);
                aiAssociatedMedia.TooltipText = CatalogueUIHelperText.AssociatedMedia;
                aiAssociatedMedia.SetItemActivator(Activator);

                break;
            default:
                break;
        }
        setTabBindings.Add(selectedIndex);
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

    private void label18_Click(object sender, EventArgs e)
    {

    }

    private void tbDOI_TextChanged(object sender, EventArgs e)
    {

    }

    private void groupBox1_Enter(object sender, EventArgs e)
    {

    }

    private void tabPage1_Click(object sender, EventArgs e)
    {

    }

    private void freeFormTextChipDisplay1_Load(object sender, EventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueUI_Design, UserControl>))]
public abstract class CatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}