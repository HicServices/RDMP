// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Allows you to store a record of a faviourite database object including all objects in Catalogue and DataExport
///     databases.  The Objects type and ID are stored and then
///     whenever an RDMPCollectionUI is visible and that object is onscreen a star will appear beside it.  Favourites are
///     stored on a 'per user' basis in the Catalogue database so
///     even if you switch computers/change sessions Favourites are preserved.
/// </summary>
public class Favourite : ReferenceOtherObjectDatabaseEntity
{
    #region Database Properties

    private string _username;
    private DateTime _favouritedDate;

    /// <summary>
    ///     The user that favourited the object
    /// </summary>
    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    /// <summary>
    ///     When the <see cref="Username" /> favourited the object
    /// </summary>
    public DateTime FavouritedDate
    {
        get => _favouritedDate;
        set => SetField(ref _favouritedDate, value);
    }

    #endregion

    public Favourite()
    {
    }

    internal Favourite(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Username = r["Username"].ToString();
        FavouritedDate = Convert.ToDateTime(r["FavouritedDate"]);
    }


    /// <summary>
    ///     Records that the current Environment.UserName wants to mark the <paramref name="objectToFavourite" /> as one of his
    ///     favourite objects
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="objectToFavourite"></param>
    public Favourite(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable objectToFavourite)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", objectToFavourite.ID },
            { "ReferencedObjectType", objectToFavourite.GetType().Name },
            { "ReferencedObjectRepositoryType", objectToFavourite.Repository.GetType().Name },
            { "Username", Environment.UserName },
            { "FavouritedDate", DateTime.Now }
        });
    }
}