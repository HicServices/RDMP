// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <inheritdoc cref="IPermissionWindow" />
public class PermissionWindow : DatabaseEntity, IPermissionWindow
{
    #region Database Properties

    private string _name;
    private string _description;
    private bool _requiresSynchronousAccess;

    /// <inheritdoc />
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <inheritdoc />
    public bool RequiresSynchronousAccess
    {
        get => _requiresSynchronousAccess;
        set => SetField(ref _requiresSynchronousAccess, value);
    }

    /// <summary>
    ///     The serialized string of <see cref="PermissionWindowPeriods" /> which is written/read from the catalogue database
    /// </summary>
    public string PermissionPeriodConfig
    {
        get => SerializePermissionWindowPeriods();
        set
        {
            var old = SerializePermissionWindowPeriods();
            DeserializePermissionWindowPeriods(value);
            OnPropertyChanged(old, value);
        }
    }

    #endregion

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public IEnumerable<ICacheProgress> CacheProgresses => Repository.GetAllObjectsWithParent<CacheProgress>(this);

    #endregion

    /// <inheritdoc />
    [NoMappingToDatabase]
    public List<PermissionWindowPeriod> PermissionWindowPeriods { get; private set; }

    private static readonly XmlSerializer Serializer = new(typeof(List<PermissionWindowPeriod>));

    private string SerializePermissionWindowPeriods()
    {
        using var output = new StringWriter();
        Serializer.Serialize(output, PermissionWindowPeriods);
        return output.ToString();
    }

    private void DeserializePermissionWindowPeriods(string permissionPeriodConfig)
    {
        if (string.IsNullOrWhiteSpace(permissionPeriodConfig))
            PermissionWindowPeriods = new List<PermissionWindowPeriod>();
        else
            PermissionWindowPeriods =
                Serializer.Deserialize(new StringReader(permissionPeriodConfig)) as List<PermissionWindowPeriod>;
    }

    /// <inheritdoc />
    public bool WithinPermissionWindow()
    {
        return WithinPermissionWindow(DateTime.UtcNow);
    }

    /// <inheritdoc />
    public virtual bool WithinPermissionWindow(DateTime dateTimeUTC)
    {
        return !PermissionWindowPeriods.Any() ||
               PermissionWindowPeriods.Any(permissionPeriod => permissionPeriod.Contains(dateTimeUTC));
    }

    public PermissionWindow()
    {
    }

    /// <summary>
    ///     Create a new time window in which you can restrict things (caching, loading etc) from happening outside
    /// </summary>
    /// <param name="repository"></param>
    public PermissionWindow(ICatalogueRepository repository)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "PermissionPeriodConfig", DBNull.Value },
            { "Name", $"New PermissionWindow{Guid.NewGuid()}" }
        });
    }

    internal PermissionWindow(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();
        Description = r["Description"].ToString();
        RequiresSynchronousAccess = Convert.ToBoolean(r["RequiresSynchronousAccess"]);
        PermissionPeriodConfig = r["PermissionPeriodConfig"].ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{(string.IsNullOrWhiteSpace(Name) ? "Unnamed" : Name)}(ID = {ID})";
    }

    /// <inheritdoc />
    public void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods)
    {
        PermissionWindowPeriods = windowPeriods;
        PermissionPeriodConfig = SerializePermissionWindowPeriods();
    }
}