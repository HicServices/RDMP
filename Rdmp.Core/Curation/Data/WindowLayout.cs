// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Persisted window layout of RDMPMainForm as Xml.  This can be used to reload RDMP to a given layout of windows and can be shared between users.
/// </summary>
public class WindowLayout : DatabaseEntity, INamed
{
    #region Database Properties

    private string _name;
    private string _layoutData;

    #endregion

    /// <inheritdoc/>
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// The Xml representation of the window layout being (e.g. what tabs are open, objects pinned etc)
    /// </summary>
    public string LayoutData
    {
        get => _layoutData;
        set => SetField(ref _layoutData, value);
    }

    public WindowLayout()
    {
    }

    /// <summary>
    /// Record the new layout in the database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name">Human readable name for the layout</param>
    /// <param name="layoutXml">The layout Xml of RDMPMainForm, use GetCurrentLayoutXml to get this, cannot be null</param>
    public WindowLayout(ICatalogueRepository repository, string name, string layoutXml)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "LayoutData", layoutXml }
        });

        if (ID == 0 ||  !repository.Equals(Repository))
            throw new ArgumentException("Repository failed to properly hydrate this class");
    }

    /// <inheritdoc/>
    public WindowLayout(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"].ToString();
        LayoutData = r["LayoutData"].ToString();
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}