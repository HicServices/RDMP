// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.Collections.Providers.Filtering;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// Main window for Data Management, this Control lets you view all your datasets, curate descriptive metadata, configure extractable columns, generate reports etc
/// 
/// <para>The tree shows you all the datasets you have configured for use with the RDMP.  Double clicking on a dataset (called a Catalogue) will show you the descriptive data you
/// have recorded. Right clicking a Catalogue will give you access to operations relevant to Catalogues (e.g. viewing dataset extraction logic if any).  Right clicking a
/// CatalogueItem will give you access to operations relevant to CatalogueItems (e.g. adding an Issue).  And so on.</para>
/// 
/// <para>Each Catalogue has 1 or more CatalogueItems (visible through the CatalogueItems tab), these are the columns in the dataset that are maintained by RDMP. If you have very
/// wide data tables with hundreds of columns you might only configure a subset of those columns (the ones most useful  to researchers) for extraction.</para>
/// 
/// <para>You can also drag Catalogues between folders or into other Controls (e.g. dragging a Catalogue into a CohortIdentificationCollectionUI container to add the dataset to the identification
/// criteria).</para>
/// 
/// <para>Pressing the Del key will prompt you to delete the selected item.</para>
/// 
/// <para>By default Deprecated, Internal and ColdStorage Catalogues do not appear, you can turn visibility of these on by selecting the relevant tick boxes.</para>
/// 
/// <para>Finally you can launch 'Checking' for every dataset, this will attempt to verify the extraction SQL you
/// have configured for each dataset and to ensure that it runs and that at least 1 row of data is returned.  Checking all the datasets can take a while so runs asynchronously.</para>
/// </summary>
public partial class CatalogueCollectionUI : RDMPCollectionUI
{
    private Catalogue[] _allCatalogues;
    private const string NewMenu = "New...";
    //constructor

    private bool bLoading = true;

    public CatalogueCollectionUI()
    {
        InitializeComponent();

        olvFilters.AspectGetter += FilterAspectGetter;
        olvOrder.AspectGetter += OrderAspectGetter;

        bLoading = false;

        catalogueCollectionFilterUI1.FiltersChanged += (s, e) => ApplyFilters();
    }

    private object OrderAspectGetter(object rowobject)
    {
        if (rowobject is CatalogueItem ci)
            return ci.ExtractionInformation?.Order;

        return rowobject is ConcreteColumn o ? o.Order : (object)null;
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);

        catalogueCollectionFilterUI1.CheckForChanges();
    }

    //The color to highlight each Catalogue based on its extractability status


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private bool isFirstTime = true;

    public void RefreshUIFromDatabase(object oRefreshFrom)
    {
        var rootFolder = Activator.CoreChildProvider.CatalogueRootFolder;

        if (tlvCatalogues.ModelFilter is CatalogueCollectionFilter f)
            f.ChildProvider = Activator.CoreChildProvider;

        if (oRefreshFrom is ExtractionInformation ei)
            tlvCatalogues.RefreshObject(ei.CatalogueItem.Catalogue);

        //if there are new catalogues we don't already have in our tree
        if (_allCatalogues != null)
        {
            var newCatalogues = CommonTreeFunctionality.CoreChildProvider.AllCatalogues.Except(_allCatalogues);
            if (newCatalogues.Any())
            {
                oRefreshFrom = rootFolder; //refresh from the root instead
                tlvCatalogues.RefreshObject(oRefreshFrom);
            }
        }

        _allCatalogues = CommonTreeFunctionality.CoreChildProvider.AllCatalogues;

        if (isFirstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvColumn1, new Guid("1d912137-22ab-4536-b40b-bd984e27dc7a"));
            CommonTreeFunctionality.SetupColumnTracking(olvOrder, new Guid("0d8e6e49-03ae-48f2-9bf8-acc5107f65f8"));
            CommonTreeFunctionality.SetupColumnTracking(olvFilters, new Guid("c4c9b2ac-c9b5-4d23-b06d-d1f55013b4e9"));

            CommonFunctionality.Add(new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator),
                GlobalStrings.FromFile, null, NewMenu);
            CommonFunctionality.Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator),
                GlobalStrings.FromDatabase, null, NewMenu);
            CommonFunctionality.Add(new ExecuteCommandCreateNewEmptyCatalogue(Activator), "Empty Catalogue (Advanced)",
                null, NewMenu);

            CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

            CommonFunctionality.Add(new ExecuteCommandAddNewCatalogueItem(Activator, null), "CatalogueItem", null,
                NewMenu);

            CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

            CommonFunctionality.Add(new ExecuteCommandAddNewAggregateGraph(Activator, null), "Aggregate Graph", null,
                NewMenu);

            CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

            CommonFunctionality.Add(new ExecuteCommandAddNewSupportingDocument(Activator, null), "Supporting Document",
                null, NewMenu);
            CommonFunctionality.Add(new ExecuteCommandAddNewSupportingSqlTable(Activator, null), "Supporting SQL Table",
                null, NewMenu);

            CommonFunctionality.Add(new ToolStripSeparator(), NewMenu);

            CommonFunctionality.Add(new ExecuteCommandCreateNewGovernancePeriod(Activator), "Governance Period", null,
                NewMenu);
            CommonFunctionality.Add(new ExecuteCommandAddNewGovernanceDocument(Activator, null), "Governance Document",
                null, NewMenu);
            var _refresh = new ToolStripMenuItem
            {
                Visible = true,
                Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
                Alignment = ToolStripItemAlignment.Right,
                ToolTipText = "Refresh Object"
            };
            _refresh.Click += delegate (object sender, EventArgs e) {
                var catalogue = Activator.CoreChildProvider.AllCatalogues.First();
                if (catalogue is not null)
                {
                    var cmd = new ExecuteCommandRefreshObject(Activator, catalogue);
                    cmd.Execute();
                }
            };
            CommonFunctionality.Add(_refresh);
        }

        if (isFirstTime || Equals(oRefreshFrom, rootFolder))
        {
            tlvCatalogues.RefreshObject(rootFolder);
            tlvCatalogues.Expand(rootFolder);
            isFirstTime = false;
        }
    }

    public void ApplyFilters()
    {
        if (bLoading)
            return;

        tlvCatalogues.UseFiltering = true;
        tlvCatalogues.ModelFilter = new CatalogueCollectionFilter(Activator.CoreChildProvider);
    }

    public enum HighlightCatalogueType
    {
        None,
        Extractable,
        ExtractionBroken,
        TOP1Worked
    }

    private object FilterAspectGetter(object rowObject)
    {
        try
        {
            if (rowObject is CatalogueItem cataItem)
                return Activator.RefreshBus.PublishInProgress
                    ? (object)null
                    : Activator.CoreChildProvider.GetAllChildrenRecursively(cataItem).OfType<IFilter>().Count();
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        Activator.Emphasise += _activator_Emphasise;

        //important to register the setup before the lifetime subscription so it gets priority on events
        CommonTreeFunctionality.SetUp(
            RDMPCollection.Catalogue,
            tlvCatalogues,
            Activator,
            olvColumn1, //the icon column
                        //we have our own custom filter logic so no need to pass tbFilter
            olvColumn1 //also the renameable column
        );

        CommonTreeFunctionality.MaintainRootObjects = new[]
        {
            typeof(AllGovernanceNode),
            typeof(FolderNode<Catalogue>)
        };

        //Things that are always visible regardless
        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = a => new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator)
                { OverrideCommandName = "Add New Catalogue From File...", Weight = -50.9f },
            new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator)
                { OverrideCommandName = "Add New Catalogue From Database...", Weight = -50.8f },
            new ExecuteCommandCreateNewEmptyCatalogue(Activator)
                { OverrideCommandName = "Add New Empty Catalogue (Advanced)", Weight = -50.7f },

            new ExecuteCommandAddNewCatalogueItem(Activator, null) { Weight = -49.9f },

            new ExecuteCommandAddNewAggregateGraph(Activator, null) { Weight = -48.9f },

            new ExecuteCommandAddNewSupportingDocument(Activator, null) { Weight = -46.9f },
            new ExecuteCommandAddNewSupportingSqlTable(Activator, null) { Weight = -46.8f },

            new ExecuteCommandCreateNewGovernancePeriod(Activator)
                { OverrideCommandName = "Add New Governance Period", Weight = 44.9f },
            new ExecuteCommandAddNewGovernanceDocument(Activator, null) { Weight = 44.9f }
        };

        Activator.RefreshBus.EstablishLifetimeSubscription(this);

        tlvCatalogues.AddObject(activator.CoreChildProvider.AllGovernanceNode);
        tlvCatalogues.AddObject(activator.CoreChildProvider.CatalogueRootFolder);
        ApplyFilters();

        RefreshUIFromDatabase(activator.CoreChildProvider.CatalogueRootFolder);
    }

    private void _activator_Emphasise(object sender, EmphasiseEventArgs args)
    {
        //user wants this object emphasised
        var c = args.Request.ObjectToEmphasise as Catalogue;

        if (c == null)
        {
            var descendancy = Activator.CoreChildProvider.GetDescendancyListIfAnyFor(args.Request.ObjectToEmphasise);

            if (descendancy != null)
                c = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();
        }

        if (c != null)
        {
            catalogueCollectionFilterUI1.EnsureVisible(c);
            ApplyFilters();
        }
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        var o = e.Object;

        switch (o)
        {
            case GovernancePeriod or GovernanceDocument:
                tlvCatalogues.RefreshObject(Activator.CoreChildProvider.AllGovernanceNode);
                break;
            case Catalogue cata:
                {
                    //if there's a change to the folder of the catalogue or it is a new Catalogue (no parent folder) we have to rebuild the entire tree
                    if (tlvCatalogues.GetParent(cata) is not string oldFolder || !oldFolder.Equals(cata.Folder))
                        RefreshUIFromDatabase(Activator.CoreChildProvider.CatalogueRootFolder);
                    else
                        RefreshUIFromDatabase(o);
                    return;
                }
            case CatalogueItem or AggregateConfiguration or ColumnInfo or TableInfo or ExtractionFilter
                or ExtractionFilterParameter or ExtractionFilterParameterSet or ExtractionInformation
                or AggregateFilterContainer or AggregateFilter or AggregateFilterParameter:
                //then refresh us
                RefreshUIFromDatabase(o);
                break;
        }

        ApplyFilters();
    }

    public static bool IsRootObject(object root)
    {
        // The root ICatalogue FolderNode is a root element in this tree
        if (root is FolderNode<Catalogue> f) return f.Name == FolderHelper.Root;

        // as is the GovernanceNode
        return root is AllGovernanceNode;
    }

    public void SelectCatalogue(Catalogue catalogue)
    {
        tlvCatalogues.SelectObject(catalogue, true);
    }
}