using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.UI.Collections;

public partial class DatasetsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{

    private Dataset[] _datasets;
    private bool _firstTime = true;

    public DatasetsCollectionUI()
    {
        InitializeComponent();
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        CommonTreeFunctionality.SetUp(RDMPCollection.Datasets, tlvDatasets, Activator, olvName, olvName,
            new RDMPCollectionCommonFunctionalitySettings());
        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter =
            a => new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewDatasetUI(Activator)
                    { OverrideCommandName = "Add New Dataset", Weight = -50.9f }
            };
        Activator.RefreshBus.EstablishLifetimeSubscription(this);
        tlvDatasets.AddObject(activator.CoreChildProvider.DatasetRootFolder);

        RefreshDatasets(Activator.CoreChildProvider.DatasetRootFolder);

        if (_firstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("f8b0481e-378c-4996-9400-cb039c2efc5c"));
            _firstTime = false;
        }
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        RefreshDatasets(Activator.CoreChildProvider.DatasetRootFolder);
    }

    private void RefreshDatasets(object oRefreshFrom)
    {
        var rootFolder = Activator.CoreChildProvider.DatasetRootFolder;
        if (_datasets != null)
        {
            var newCatalogues = CommonTreeFunctionality.CoreChildProvider.AllDatasets.Value.Except(_datasets);
            if (newCatalogues.Any())
            {
                oRefreshFrom = rootFolder; //refresh from the root instead
                tlvDatasets.RefreshObject(oRefreshFrom);
            }
        }
        _datasets = CommonTreeFunctionality.CoreChildProvider.AllDatasets.Value;
        if (_firstTime || Equals(oRefreshFrom, rootFolder))
        {
            tlvDatasets.RefreshObject(rootFolder);
            tlvDatasets.Expand(rootFolder);
            _firstTime = false;
        }

    }


    /// <summary>
    /// Returns all root objects in RDMP that match the <paramref name="condition"/>.  Handles unpicking tree collisions e.g. where <paramref name="condition"/> matches 2 objects with one being the child of the other
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static List<IMapsDirectlyToDatabaseTable> FindRootObjects(IActivateItems activator,
        Func<IMapsDirectlyToDatabaseTable, bool> condition)
    {
        var datasets =
            activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Dataset>();

        var actualRootFavourites = new List<IMapsDirectlyToDatabaseTable>();

        foreach (var currentFavourite in datasets)
            actualRootFavourites.Add(currentFavourite);

        return actualRootFavourites;
    }


    /// <summary>
    /// Return true if the object should be displayed in this pane
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected virtual bool IncludeObject(IMapsDirectlyToDatabaseTable key) =>
        Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Dataset>().Contains(key);

    public static bool IsRootObject(IActivateItems activator, object root) =>
        //never favourite
        false;
}