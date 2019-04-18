// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
    /// <summary>
    /// Collection that shows all of a users favourited objects.  Only root objects will be displayed (this means that if you favourite a Catalogue and 3 
    /// CatalogueItems within that Catalogue only the root Catalogue will be a top level node in the collection UI)
    /// </summary>
    public partial class FavouritesCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;

        List<IMapsDirectlyToDatabaseTable> favourites = new List<IMapsDirectlyToDatabaseTable>();

        public FavouritesCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            CommonTreeFunctionality.SetUp(RDMPCollection.Favourites,tlvFavourites,_activator,olvName,olvName,new RDMPCollectionCommonFunctionalitySettings {AllowPinning = false});
            CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };

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

                //if any of the current favourites parents
                foreach (object parent in currentFavourite.Value.Parents)
                    //are favourites
                    if (potentialRootFavourites.Any(kvp => kvp.Key.Equals(parent)))
                        //then this is not a favourite it's a collision (already favourited under another node)
                        hierarchyCollisions.Add(currentFavourite.Key);    
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
            tlvFavourites.RebuildAll(true);
        }

        public static bool IsRootObject(IActivateItems activator, object root)
        {
            //never favourite
            return false;
        }
    }
}
