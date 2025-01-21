// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.CohortDescribing;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.CohortUI;

/// <summary>
/// Shows a collection of cohorts that are ready for data extraction (typically all the cohorts associated with a project or a global list of all cohorts).  These are identifier lists
/// and release identifier substitutions stored in the external cohort database(s).  The control provides a readonly summary of the number of unique patient identifiers in each cohort.
/// If a project/global list includes more than one Cohort Source (e.g. you link NHS numbers to ReleaseIdentifiers but also link CHI numbers to ReleaseIdentifiers or if you have the same
/// private identifier but different release identifier formats) then each seperate cohort source table will be listed along with the associated cohorts found by RDMP.
/// </summary>
public partial class ExtractableCohortCollectionUI : RDMPUserControl, ILifetimeSubscriber
{
    private ExtractableCohortAuditLogBuilder _auditLogBuilder = new();

    public ExtractableCohortCollectionUI()
    {
        InitializeComponent();

        lbCohortDatabaseTable.FormatRow += lbCohortDatabaseTable_FormatRow;
        lbCohortDatabaseTable.AlwaysGroupByColumn = olvSource;

        //always show selection in the same highlight colour
        lbCohortDatabaseTable.SelectedForeColor = Color.White;
        lbCohortDatabaseTable.SelectedBackColor = Color.FromArgb(55, 153, 255);
        lbCohortDatabaseTable.UnfocusedSelectedForeColor = Color.White;
        lbCohortDatabaseTable.UnfocusedSelectedBackColor = Color.FromArgb(55, 153, 255);

        olvViewLog.AspectGetter = ViewLogAspectGetter;
        olvID.AspectGetter = IDAspectGetter;
        olvCreatedFrom.AspectGetter = CreatedFromAspectGetter;
        lbCohortDatabaseTable.ButtonClick += lbCohortDatabaseTable_ButtonClick;
        lbCohortDatabaseTable.RowHeight = 19;

        lbCohortDatabaseTable.BeforeSorting += BeforeSorting;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

    }

    private object CreatedFromAspectGetter(object rowObject)
    {
        if (rowObject is ExtractableCohortDescription ecd && Activator is not null)
        {
            var obj = ExtractableCohortAuditLogBuilder.GetObjectIfAny(ecd.Cohort, Activator.RepositoryLocator);
            return obj is ExtractionInformation ei ? $"{ei.CatalogueItem.Catalogue}.{ei}" : obj;
        }


        return null;
    }

    private void BeforeSorting(object sender, BeforeSortingEventArgs e)
    {
        lbCohortDatabaseTable.ListViewItemSorter = new ColumnComparer(
            e.ColumnToSort, e.SortOrder, e.SecondaryColumnToSort, e.SecondarySortOrder);
        e.Handled = true;
    }

    private void lbCohortDatabaseTable_ButtonClick(object sender, CellClickEventArgs e)
    {
        if (e.Column == olvViewLog && e.Model is ExtractableCohortDescription ecd)
            WideMessageBox.Show("Cohort audit log", ecd.Cohort.AuditLog, WideMessageBoxTheme.Help);
    }

    private object ViewLogAspectGetter(object rowObject) =>
        rowObject is ExtractableCohortDescription ecd && !string.IsNullOrWhiteSpace(ecd.Cohort.AuditLog)
            ? "View Log"
            : (object)null;

    private object IDAspectGetter(object rowObject) =>
        rowObject is ExtractableCohortDescription ecd ? ecd.Cohort.ID : (object)null;

    private bool haveSubscribed;

    public void SetupForAllCohorts(IActivateItems activator)
    {
        try
        {
            if (!haveSubscribed)
            {
                //activator.RefreshBus.EstablishLifetimeSubscription(this);
                activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(ExtractableCohort).ToString());
                activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(ExternalCohortTable).ToString());
                haveSubscribed = true;
            }

            ReFetchCohortDetailsAsync();
        }
        catch (Exception e)
        {
            ExceptionViewer.Show(
                $"{GetType().Name} could not load Cohorts:{Environment.NewLine}{ExceptionHelper.ExceptionToListOfInnerMessages(e)}",
                e);
        }
    }

    public void SetupFor(ExtractableCohort[] cohorts)
    {
        try
        {
            lbCohortDatabaseTable.ClearObjects();
            lbCohortDatabaseTable.AddObjects(cohorts.Select(cohort => new ExtractableCohortDescription(cohort))
                .ToArray());
        }
        catch (Exception e)
        {
            ExceptionViewer.Show(
                $"{GetType().Name} could not load Cohorts:{Environment.NewLine}{ExceptionHelper.ExceptionToListOfInnerMessages(e)}",
                e);
        }
    }


    private void ReFetchCohortDetailsAsync()
    {
        lbCohortDatabaseTable.ClearObjects();

        //gets the empty placeholder cohort objects, these have string values like "Loading..." and -1 for counts but each one comes with a Fetch object, the node will populate itself once the callback finishes
        var factory = new CohortDescriptionFactory(Activator.RepositoryLocator.DataExportRepository);
        var fetchDescriptionsDictionary = factory.Create();

        lbCohortDatabaseTable.AddObjects(fetchDescriptionsDictionary.SelectMany(kvp => kvp.Value).ToArray());

        //Just because the object updates itself doesn't mean ObjectListView will notice, so we must also subscribe to the fetch completion (1 per cohort source table)
        //when the fetch completes, update the UI nodes (they also themselves subscribe to the fetch completion handler and should be registered further up the invocation list)
        foreach (var (fetch, nodes) in fetchDescriptionsDictionary)
            //Could be we are disposed when this happens
            fetch.Finished += () =>
            {
                if (!lbCohortDatabaseTable.IsDisposed)
                    lbCohortDatabaseTable.RefreshObjects(nodes);
            };
    }

    private void lbCohortDatabaseTable_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    public event SelectedCohortChangedHandler SelectedCohortChanged;

    private void lbCohortDatabaseTable_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && lbCohortDatabaseTable.SelectedObject != null)
        {
            var node = (ExtractableCohortDescription)lbCohortDatabaseTable.SelectedObject;

            var toDelete = node.Cohort;

            if (!Activator.YesNo($"Are you sure you want to delete {toDelete} (ID={toDelete.ID})", "Confirm Delete"))
                return;

            try
            {
                toDelete.DeleteInDatabase();

                lbCohortDatabaseTable.RemoveObject(node);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        lbCohortDatabaseTable.UseFiltering = true;
        lbCohortDatabaseTable.ModelFilter = new TextMatchFilter(lbCohortDatabaseTable, tbFilter.Text,
            StringComparison.CurrentCultureIgnoreCase);
    }

    private void lbCohortDatabaseTable_FormatRow(object sender, FormatRowEventArgs e)
    {
        if (e.Model is not ExtractableCohortDescription model)
            return;

        if (model.Exception != null)
            e.Item.BackColor = Color.Red;
    }

    private void lbCohortDatabaseTable_ItemActivate(object sender, EventArgs e)
    {
        if (lbCohortDatabaseTable.SelectedObject is not ExtractableCohortDescription model)
            return;

        if (model.Exception != null)
            ExceptionViewer.Show(model.Exception);
    }

    public void SetSelectedCohort(ExtractableCohort toSelect)
    {
        if (toSelect == null)
        {
            lbCohortDatabaseTable.SelectedObject = null;
            return;
        }

        var matchingNode = lbCohortDatabaseTable.Objects.Cast<ExtractableCohortDescription>()
            .SingleOrDefault(c => c.Cohort.ID == toSelect.ID);

        lbCohortDatabaseTable.SelectedObject = matchingNode;
    }

    private void lbCohortDatabaseTable_SelectionChanged(object sender, EventArgs e)
    {
        var selected = lbCohortDatabaseTable.SelectedObject is not ExtractableCohortDescription node
            ? null
            : node.Cohort;

        SelectedCohortChanged?.Invoke(this, selected);
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        if (e.Object is ExtractableCohort || e.Object is ExternalCohortTable)
            ReFetchCohortDetailsAsync();
    }
}

public delegate void SelectedCohortChangedHandler(object sender, ExtractableCohort selected);