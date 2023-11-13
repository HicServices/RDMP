using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;

namespace Rdmp.UI.Collections
{
    public partial class DatasetsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {

        private List<IMapsDirectlyToDatabaseTable> _datasets = new();
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
            //CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };
            //CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter =
            //    a => new IAtomicCommand[]
            //    {
            //    new ExecuteCommandAddFavourite(a),
            //    new ExecuteCommandClearFavourites(a)
            //    };
            Activator.RefreshBus.EstablishLifetimeSubscription(this);

            RefreshFavourites();

            if (_firstTime)
            {
                CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("f8b0481e-378c-4996-9400-cb039c2efc5c"));
                _firstTime = false;
            }
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            RefreshFavourites();
        }

        private void RefreshFavourites()
        {
            var actualRootFavourites = FindRootObjects(Activator, IncludeObject);

            //no change in root favouratism
            if (_datasets.SequenceEqual(actualRootFavourites))
                return;

            //remove old objects
            foreach (var unfavourited in _datasets.Except(actualRootFavourites))
                tlvDatasets.RemoveObject(unfavourited);

            //add new objects
            foreach (var newFavourite in actualRootFavourites.Except(_datasets))
                tlvDatasets.AddObject(newFavourite);

            //update to the new list
            _datasets = actualRootFavourites;
            tlvDatasets.RebuildAll(true);
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

            //var hierarchyCollisions = new List<IMapsDirectlyToDatabaseTable>();

            ////find hierarchy collisions (shared hierarchy in which one Favourite object includes a tree of objects some of which are Favourited).  For this only display the parent
            //foreach (var currentFavourite in potentialRootFavourites)
            //{
            //    //current favourite is an absolute root object Type (no parents)
            //    if (currentFavourite.Value == null)
            //        continue;

            //    //if any of the current favourites parents
            //    foreach (var parent in currentFavourite.Value.Parents)
            //        //are favourites
            //        if (potentialRootFavourites.Any(kvp => kvp.Key.Equals(parent)))
            //            //then this is not a favourite it's a collision (already favourited under another node)
            //            hierarchyCollisions.Add(currentFavourite.Key);
            //}

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
}
