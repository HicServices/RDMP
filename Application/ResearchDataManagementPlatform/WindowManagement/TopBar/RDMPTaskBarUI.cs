// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.Theme;

namespace ResearchDataManagementPlatform.WindowManagement.TopBar;

/// <summary>
/// Allows you to access the main object collections that make up the RDMP.  These include
/// </summary>
public partial class RDMPTaskBarUI : UserControl
{
    private WindowManager _manager;

    private const string CreateNewLayout = "<<New Layout>>";

    public RDMPTaskBarUI()
    {
        InitializeComponent();

        btnHome.Image = FamFamFamIcons.application_home.ImageToBitmap();
        btnCatalogues.Image = CatalogueIcons.Catalogue.ImageToBitmap();
        btnCatalogues.BackgroundImage =
            BackColorProvider.GetBackgroundImage(btnCatalogues.Size, RDMPCollection.Catalogue);

        btnCohorts.Image = CatalogueIcons.CohortIdentificationConfiguration.ImageToBitmap();
        btnCohorts.BackgroundImage = BackColorProvider.GetBackgroundImage(btnCohorts.Size, RDMPCollection.Cohort);

        btnSavedCohorts.Image = CatalogueIcons.AllCohortsNode.ImageToBitmap();
        btnSavedCohorts.BackgroundImage =
            BackColorProvider.GetBackgroundImage(btnSavedCohorts.Size, RDMPCollection.SavedCohorts);

        btnDataExport.Image = CatalogueIcons.Project.ImageToBitmap();
        btnDataExport.BackgroundImage =
            BackColorProvider.GetBackgroundImage(btnDataExport.Size, RDMPCollection.DataExport);

        btnTables.Image = CatalogueIcons.TableInfo.ImageToBitmap();
        btnTables.BackgroundImage = BackColorProvider.GetBackgroundImage(btnTables.Size, RDMPCollection.Tables);

        btnDataSets.Image = CatalogueIcons.Dataset.ImageToBitmap();
        btnDataSets.BackgroundImage = BackColorProvider.GetBackgroundImage(btnDataSets.Size, RDMPCollection.Datasets);

        btnLoads.Image = CatalogueIcons.LoadMetadata.ImageToBitmap();
        btnLoads.BackgroundImage = BackColorProvider.GetBackgroundImage(btnDataSets.Size, RDMPCollection.DataLoad);

        btnFavourites.Image = CatalogueIcons.Favourite.ImageToBitmap();
        btnDeleteLayout.Image = FamFamFamIcons.delete.ImageToBitmap();

        cbCommits.Image = CatalogueIcons.Commit.ImageToBitmap();
        cbCommits.Checked = UserSettings.EnableCommits;
        cbCommits.CheckedChanged += (s, e) => UserSettings.EnableCommits = cbCommits.Checked;
        cbCommits.CheckOnClick = true;
    }

    public void SetWindowManager(WindowManager manager)
    {
        _manager = manager;

        //Update task bar buttons enabledness when the user navigates somewhere
        _manager.Navigation.Changed += (s, e) => UpdateForwardBackEnabled();

        btnDataExport.Enabled = manager.RepositoryLocator.DataExportRepository != null;

        ReCreateDropDowns();

        SetupToolTipText();

        _manager.ActivateItems.Value.Theme.ApplyTo(toolStrip1);

        // if we don't support commit system then disable the task bar button for it
        if (!_manager.ActivateItems.Value.RepositoryLocator.CatalogueRepository.SupportsCommits)
        {
            cbCommits.Enabled = false;
            cbCommits.Text = "Repository does not support commits";
        }
    }

    /// <summary>
    /// Updates the enabled status (greyed out) of the Forward/Back buttons based on the current <see cref="_manager"/> <see cref="NavigationTrack{T}"/>
    /// </summary>
    private void UpdateForwardBackEnabled()
    {
        btnBack.Enabled = _manager.Navigation.CanBack();
        btnForward.Enabled = _manager.Navigation.CanForward();
    }


    private void SetupToolTipText()
    {
        try
        {
            btnHome.ToolTipText = "Home screen, shows recent objects etc";
            btnCatalogues.ToolTipText = "All datasets configured for access by RDMP";
            btnCohorts.ToolTipText = "Built queries for creating cohorts";
            btnSavedCohorts.ToolTipText = "Finalised identifier lists, ready for linkage and extraction";
            btnDataExport.ToolTipText = "Show Projects and Extractable Dataset Packages allowing data extraction";
            btnTables.ToolTipText = "Advanced features e.g. logging, credentials, dashboards etc";
            btnLoads.ToolTipText = "Load configurations for reading data into your databases";
            btnFavourites.ToolTipText = "Collection of all objects that you have favourited";
            btnDataSets.ToolTipText = "All external datasets that have been configured for use in RDMP";
        }
        catch (Exception e)
        {
            _manager.ActivateItems.Value.GlobalErrorCheckNotifier.OnCheckPerformed(
                new CheckEventArgs("Failed to setup tool tips", CheckResult.Fail, e));
        }
    }

    private void ReCreateDropDowns()
    {
        CreateDropDown<WindowLayout>(cbxLayouts, CreateNewLayout);
    }

    private void CreateDropDown<T>(ToolStripComboBox cbx, string createNewDashboard)
        where T : IMapsDirectlyToDatabaseTable, INamed
    {
        const int xPaddingForComboText = 10;

        if (cbx.ComboBox == null)
            throw new Exception("Expected combo box!");

        cbx.ComboBox.Items.Clear();

        var objects = _manager.RepositoryLocator.CatalogueRepository.GetAllObjects<T>();

        cbx.ComboBox.Items.Add("");

        //minimum size that it will be (same width as the combo box)
        var proposedComboBoxWidth = cbx.Width - xPaddingForComboText;

        foreach (var o in objects)
        {
            //add dropdown item
            cbx.ComboBox.Items.Add(o);

            //will that label be too big to fit in text box? if so expand the max width
            proposedComboBoxWidth = Math.Max(proposedComboBoxWidth, TextRenderer.MeasureText(o.Name, cbx.Font).Width);
        }

        cbx.DropDownWidth = Math.Min(400, proposedComboBoxWidth + xPaddingForComboText);
        cbx.ComboBox.SelectedItem = "";

        cbx.Items.Add(createNewDashboard);
    }

    private void btnHome_Click(object sender, EventArgs e)
    {
        _manager.PopHome();
    }

    private void ToolboxButtonClicked(object sender, EventArgs e)
    {
        var collection = ButtonToEnum(sender);

        if (_manager.IsVisible(collection))
            _manager.Pop(collection);
        else
            _manager.Create(collection);
    }

    private RDMPCollection ButtonToEnum(object button)
    {
        RDMPCollection collectionToToggle;

        if (button == btnCatalogues)
            collectionToToggle = RDMPCollection.Catalogue;
        else if (button == btnCohorts)
            collectionToToggle = RDMPCollection.Cohort;
        else if (button == btnDataExport)
            collectionToToggle = RDMPCollection.DataExport;
        else if (button == btnTables)
            collectionToToggle = RDMPCollection.Tables;
        else if (button == btnLoads)
            collectionToToggle = RDMPCollection.DataLoad;
        else if (button == btnSavedCohorts)
            collectionToToggle = RDMPCollection.SavedCohorts;
        else if (button == btnFavourites)
            collectionToToggle = RDMPCollection.Favourites;
        else if (button == btnDataSets)
            collectionToToggle = RDMPCollection.Datasets;
        else
            throw new ArgumentOutOfRangeException(nameof(button));

        return collectionToToggle;
    }


    private void cbx_DropDownClosed(object sender, EventArgs e)
    {
        var cbx = (ToolStripComboBox)sender;

        if (ReferenceEquals(cbx.SelectedItem, CreateNewLayout))
            AddNewLayout();

        if (cbx.SelectedItem is INamed toOpen)
        {
            var cmd = new ExecuteCommandActivate(_manager.ActivateItems.Value, toOpen);
            cmd.Execute();
        }

        UpdateButtonEnabledness();
    }


    private void cbx_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateButtonEnabledness();
    }

    private void UpdateButtonEnabledness()
    {
        btnSaveWindowLayout.Enabled = cbxLayouts.SelectedItem is WindowLayout;
        btnDeleteLayout.Enabled = cbxLayouts.SelectedItem is WindowLayout;
    }

    private void AddNewLayout()
    {
        var xml = _manager.MainForm.GetCurrentLayoutXml();

        var dialog = new TypeTextOrCancelDialog("Layout Name", "Name", 100, null, false);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var layout = new WindowLayout(_manager.RepositoryLocator.CatalogueRepository, dialog.ResultText, xml);

            var cmd = new ExecuteCommandActivate(_manager.ActivateItems.Value, layout);
            cmd.Execute();

            ReCreateDropDowns();
        }
    }


    public void InjectButton(ToolStripButton button)
    {
        toolStrip1.Items.Add(button);
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        ToolStripComboBox cbx;
        if (sender == btnDeleteLayout)
            cbx = cbxLayouts;
        else
            throw new Exception("Unexpected sender");

        if (cbx.SelectedItem is IDeleteable d)
        {
            _manager.ActivateItems.Value.DeleteWithConfirmation(d);
            ReCreateDropDowns();
        }
    }

    private void btnSaveWindowLayout_Click(object sender, EventArgs e)
    {
        if (cbxLayouts.SelectedItem is WindowLayout layout)
        {
            var xml = _manager.MainForm.GetCurrentLayoutXml();

            layout.LayoutData = xml;
            layout.SaveToDatabase();
        }
    }

    private void btnBack_ButtonClick(object sender, EventArgs e)
    {
        _manager.Navigation.Back(true);
    }

    private void btnForward_Click(object sender, EventArgs e)
    {
        _manager.Navigation.Forward(true);
    }

    private void btnBack_DropDownOpening(object sender, EventArgs e)
    {
        btnBack.DropDownItems.Clear();

        var backIndex = 1;

        foreach (var history in _manager.Navigation.GetHistory(16))
        {
            var i = backIndex++;
            btnBack.DropDownItems.Add(history.ToString(), null, (a, b) => _manager.Navigation.Back(i, true));
        }
    }
}