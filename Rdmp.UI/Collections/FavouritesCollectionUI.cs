// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// Collection that shows all of a users favourited objects.  Only root objects will be displayed (this means that if you favourite a Catalogue and 3
/// CatalogueItems within that Catalogue only the root Catalogue will be a top level node in the collection UI)
/// </summary>
public partial class FavouritesCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{
    private List<IMapsDirectlyToDatabaseTable> favourites = new();
    private bool _firstTime = true;

    public FavouritesCollectionUI()
    {
        InitializeComponent();
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        CommonTreeFunctionality.SetUp(RDMPCollection.Favourites, tlvFavourites, Activator, olvName, olvName,
            new RDMPCollectionCommonFunctionalitySettings());
        CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };
        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter =
            a => new IAtomicCommand[]
            {
                new ExecuteCommandAddFavourite(a),
                new ExecuteCommandClearFavourites(a)
            };
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

    /// <summary>
    /// Returns all root objects in RDMP that match the <paramref name="condition"/>.  Handles unpicking tree collisions e.g. where <paramref name="condition"/> matches 2 objects with one being the child of the other
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static List<IMapsDirectlyToDatabaseTable> FindRootObjects(IActivateItems activator,
        Func<IMapsDirectlyToDatabaseTable, bool> condition)
    {
        var potentialRootFavourites =
            activator.CoreChildProvider.GetAllSearchables().Where(k => condition(k.Key)).ToArray();

        var hierarchyCollisions = new List<IMapsDirectlyToDatabaseTable>();

        //find hierarchy collisions (shared hierarchy in which one Favourite object includes a tree of objects some of which are Favourited).  For this only display the parent
        foreach (var currentFavourite in potentialRootFavourites)
        {
            //current favourite is an absolute root object Type (no parents)
            if (currentFavourite.Value == null)
                continue;

            //if any of the current favourites parents
            foreach (var parent in currentFavourite.Value.Parents)
                //are favourites
                if (potentialRootFavourites.Any(kvp => kvp.Key.Equals(parent)))
                    //then this is not a favourite it's a collision (already favourited under another node)
                    hierarchyCollisions.Add(currentFavourite.Key);
        }

        var actualRootFavourites = new List<IMapsDirectlyToDatabaseTable>();

        foreach (var currentFavourite in potentialRootFavourites)
            if (!hierarchyCollisions.Contains(currentFavourite.Key))
                actualRootFavourites.Add(currentFavourite.Key);

        return actualRootFavourites;
    }


    /// <summary>
    /// Return true if the object should be displayed in this pane
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected virtual bool IncludeObject(IMapsDirectlyToDatabaseTable key) =>
        Activator.FavouritesProvider.IsFavourite(key);

    public static bool IsRootObject(IActivateItems activator, object root) =>
        //never favourite
        false;
}