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
///     Describes a point in time state of another <see cref="DatabaseEntity" />.  Note that the state may be invalid if
///     other
///     objects have been since deleted.  e.g. if user updates the
///     <see cref="Catalogue.TimeCoverage_ExtractionInformation_ID" />
///     the memento would point to an old <see cref="ExtractionInformation" /> which may be subsequently deleted
/// </summary>
public class Memento : ReferenceOtherObjectDatabaseEntity
{
    #region Database Properties

    private string _beforeYaml;
    private string _afterYaml;
    private int _commit_ID;
    private MementoType _type;

    public string BeforeYaml
    {
        get => _beforeYaml;
        set => SetField(ref _beforeYaml, value);
    }

    public string AfterYaml
    {
        get => _afterYaml;
        set => SetField(ref _afterYaml, value);
    }

    public int Commit_ID
    {
        get => _commit_ID;
        set => SetField(ref _commit_ID, value);
    }

    public MementoType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    #endregion

    #region Relationships

    [NoMappingToDatabase] public Commit Commit => Repository.GetObjectByID<Commit>(Commit_ID);

    #endregion

    public Memento()
    {
    }

    public Memento(ICatalogueRepository repo, DbDataReader r) : base(repo, r)
    {
        BeforeYaml = r["BeforeYaml"].ToString();
        AfterYaml = r["AfterYaml"].ToString();
        Commit_ID = (int)r["Commit_ID"];
        Type = Enum.Parse<MementoType>(r["Type"].ToString());
    }

    public Memento(ICatalogueRepository repository, Commit commit, MementoType type,
        IMapsDirectlyToDatabaseTable entity, string beforeYaml, string afterYaml)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", entity.ID },
            { "ReferencedObjectType", entity.GetType().Name },
            { "ReferencedObjectRepositoryType", entity.Repository.GetType().Name },
            { "Commit_ID", commit.ID },
            { "BeforeYaml", beforeYaml },
            { "AfterYaml", afterYaml },
            { "Type", type }
        });
    }

    public override string ToString()
    {
        return $"{ReferencedObjectType}:{ReferencedObjectID}";
    }
}