// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Providers;

/// <summary>
///     Determines whether objects are a <see cref="Favourite" /> of the current user and handles creating/deleting them.
/// </summary>
public class FavouritesProvider
{
    private readonly IBasicActivateItems _activator;
    private readonly ICatalogueRepository _catalogueRepository;
    public List<Favourite> CurrentFavourites { get; set; }

    public FavouritesProvider(IBasicActivateItems activator)
    {
        _activator = activator;
        _catalogueRepository = _activator.RepositoryLocator.CatalogueRepository;
        CurrentFavourites = _catalogueRepository.GetAllObjectsWhere<Favourite>("Username", Environment.UserName)
            .ToList();
    }

    public void AddFavourite(object sender, IMapsDirectlyToDatabaseTable o)
    {
        //it's already a favourite
        if (IsFavourite(o))
            return;

        var newFavourite = new Favourite(_catalogueRepository, o);
        CurrentFavourites.Add(newFavourite);
        _activator.Publish(newFavourite);
    }

    public void RemoveFavourite(object sender, IMapsDirectlyToDatabaseTable o)
    {
        var toRemove = CurrentFavourites.SingleOrDefault(f => f.IsReferenceTo(o));

        if (toRemove != null)
        {
            CurrentFavourites.Remove(toRemove);
            toRemove.DeleteInDatabase();
            _activator.Publish(toRemove);
        }

        //it wasn't a favourite anyway
    }

    public bool IsFavourite(IMapsDirectlyToDatabaseTable o)
    {
        return GetFavouriteIfAny(o) != null;
    }

    public Favourite GetFavouriteIfAny(IMapsDirectlyToDatabaseTable o)
    {
        return CurrentFavourites.SingleOrDefault(f => f.IsReferenceTo(o));
    }
}