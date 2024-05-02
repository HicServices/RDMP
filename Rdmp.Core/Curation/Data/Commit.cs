// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Captures a user tracked change to the state of objects in RDMP.  A single <see cref="Commit" /> contains
///     one or more <see cref="Memento" /> which track the before and after states of the objects changed
/// </summary>
public class Commit : DatabaseEntity
{
    #region Database Properties

    private string _username;
    private DateTime _date;
    private string _transaction;
    private string _description;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public DateTime Date
    {
        get => _date;
        set => SetField(ref _date, value);
    }

    public string Transaction
    {
        get => _transaction;
        set => SetField(ref _transaction, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    #endregion

    #region Relationships

    [NoMappingToDatabase] public Memento[] Mementos => Repository.GetAllObjectsWithParent<Memento>(this);

    #endregion

    public Commit()
    {
    }

    public Commit(ICatalogueRepository repo, DbDataReader r) : base(repo, r)
    {
        Transaction = r["Transaction"].ToString();
        Username = r["Username"].ToString();
        Date = Convert.ToDateTime(r["Date"]);
        Description = r["Description"].ToString();
    }

    public Commit(ICatalogueRepository repository, Guid transaction, string description)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Transaction", transaction.ToString("N") },
            { "Username", Environment.UserName },
            { "Date", DateTime.Now },
            { "Description", description }
        });
    }

    public override string ToString()
    {
        return Transaction;
    }
}