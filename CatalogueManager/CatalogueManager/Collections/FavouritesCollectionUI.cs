using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Collections
{
    public partial class FavouritesCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;
        private RDMPCollectionCommonFunctionality _commonCollectionFunctionality;

        List<IMapsDirectlyToDatabaseTable> favourites = new List<IMapsDirectlyToDatabaseTable>();

        public FavouritesCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            _commonCollectionFunctionality = new RDMPCollectionCommonFunctionality();
            _commonCollectionFunctionality.SetUp(tlvFavourites,_activator,olvName,olvName);
            _commonCollectionFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };

            _activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            RefreshFavourites();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            RefreshFavourites();
        }

        private void RefreshFavourites()
        {
            var potentialRootFavourites = _activator.CoreChildProvider.GetAllSearchables().Where(kvp => _activator.FavouritesProvider.IsFavourite(kvp.Key)).ToArray();

            List<IMapsDirectlyToDatabaseTable> hierarchyCollisions = new List<IMapsDirectlyToDatabaseTable>();

            //find hierarchy collisions (shared hierarchy in which one Favourite object includes a tree of objects some of which are Favourited).  For this only display the parent
            foreach (var currentFavourite in potentialRootFavourites)
            {
                //current favourite is an absolute root object Type (no parents)
                if(currentFavourite.Value == null)
                    continue;

                //if the current favourite appears in the parental hierarchy of another favourite then skip that one
                var conflicts = potentialRootFavourites.Where(kvp => kvp.Value != null && kvp.Value.Parents.Contains(currentFavourite.Key)).ToArray();

                hierarchyCollisions.AddRange(conflicts.Select(kvp => kvp.Key));
            }

            List<IMapsDirectlyToDatabaseTable> actualRootFavourites = new List<IMapsDirectlyToDatabaseTable>();

            foreach (var currentFavourite in potentialRootFavourites)
            {
                if (!hierarchyCollisions.Contains(currentFavourite.Key))
                    actualRootFavourites.Add(currentFavourite.Key);
            }


            //no change in root favouratism
            if (favourites.SequenceEqual(actualRootFavourites))
                return;

            //remove old objects
            foreach (var unfavourited in favourites.Except(actualRootFavourites))
                tlvFavourites.RemoveObject(unfavourited);

            //add new objects
            foreach (var newFavourite in actualRootFavourites.Except(favourites))
                tlvFavourites.AddObject(newFavourite);

            //update to the new list
            favourites = actualRootFavourites;
        }

        public static bool IsRootObject(IActivateItems activator, object root)
        {
            var m = root as IMapsDirectlyToDatabaseTable;

            return m != null && activator.FavouritesProvider.IsFavourite(m);
        }
    }
}
