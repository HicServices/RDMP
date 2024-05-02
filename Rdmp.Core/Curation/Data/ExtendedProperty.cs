// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Stores a single named value for a specific <see cref="DatabaseEntity" />.  This can be used to store custom field
///     values e.g. for plugins
/// </summary>
public class ExtendedProperty : Argument, IReferenceOtherObjectWithPersist, IInjectKnown<IMapsDirectlyToDatabaseTable>,
    INamed
{
    /// <summary>
    ///     Key for <see cref="ExtendedProperty" /> that indicates an object is replaced by another
    /// </summary>
    public const string ReplacedBy = "ReplacedBy";

    /// <summary>
    ///     Key for <see cref="ExtendedProperty" /> that indicates an object is a reusable curated template
    ///     that can be reused in subsequently created configurations
    /// </summary>
    public const string IsTemplate = "IsTemplate";


    /// <summary>
    ///     Key for use in <see cref="ExtendedProperty" /> for defining <see cref="IJoin.GetCustomJoinSql" />
    /// </summary>
    public const string CustomJoinSql = "CustomJoinSql";

    public const string CustomJoinSqlDescription =
        "Enter the column comparison(s) SQL for the JOIN line.  Your string should include only the boolean comparison logic that follows the ON keyword.  E.g. col1=col2.  You can optionally use substitution tokens {0} and {1} for table name/alias (e.g. for lookup)";

    /// <summary>
    ///     Key used in <see cref="ExtendedProperty" /> to indicate that a <see cref="LoadMetadata" /> should not
    ///     attempt to DROP/CREATE its RAW database each time it is run
    /// </summary>
    public const string PersistentRaw = "PersistentRaw";

    public const string PersistentRawDescription =
        "Should the load leave old RAW databases in the RAW server and only cleanup/reload tables at runtime? Value must be 'true' or 'false'";

    /// <summary>
    ///     Collection of all known property names.  Plugins are free to add to these if desired but must do so pre startup
    /// </summary>
    public static List<string> KnownProperties = new();

    static ExtendedProperty()
    {
        var fields = typeof(ExtendedProperty).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.FieldType == typeof(string));

        foreach (var field in fields)
        {
            // skip any description fields
            if (field.Name.EndsWith("Description"))
                continue;

            KnownProperties.Add((string)field.GetValue(null));
        }
    }

    #region Database Properties

    private string _referencedObjectType;
    private int _referencedObjectID;
    private string _referencedObjectRepositoryType;

    /// <inheritdoc />
    public string ReferencedObjectType
    {
        get => _referencedObjectType;
        set => SetField(ref _referencedObjectType, value);
    }

    /// <inheritdoc />
    public int ReferencedObjectID
    {
        get => _referencedObjectID;
        set => SetField(ref _referencedObjectID, value);
    }

    /// <inheritdoc />
    public string ReferencedObjectRepositoryType
    {
        get => _referencedObjectRepositoryType;
        set => SetField(ref _referencedObjectRepositoryType, value);
    }

    #endregion

    public ExtendedProperty()
    {
        ClearAllInjections();
    }

    public ExtendedProperty(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable setOn, string name,
        object value)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(setOn);
        ArgumentNullException.ThrowIfNull(name);

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", setOn.ID },
            { "ReferencedObjectType", setOn.GetType().Name },
            { "ReferencedObjectRepositoryType", setOn.Repository.GetType().Name },
            { "Name", name },
            { "Type", value.GetType().ToString() }
        });

        SetValue(value);
        SaveToDatabase();
    }

    /// <inheritdoc />
    public ExtendedProperty(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        ReferencedObjectType = r["ReferencedObjectType"].ToString();
        ReferencedObjectID = Convert.ToInt32(r["ReferencedObjectID"]);
        ReferencedObjectRepositoryType = r["ReferencedObjectRepositoryType"].ToString();
        Name = r["Name"].ToString();
        Value = r["Value"].ToString();
        Type = r["Type"].ToString();
        Description = r["Description"].ToString();
    }

    /// <summary>
    ///     True if the object referenced by this class is of Type <paramref name="type" />
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsReferenceTo(Type type)
    {
        return AreProbablySameType(ReferencedObjectType, type);
    }

    /// <summary>
    ///     True if the <paramref name="o" /> is the object that is explicitly referenced by this class instance
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public bool IsReferenceTo(IMapsDirectlyToDatabaseTable o)
    {
        return o.ID == ReferencedObjectID
               &&
               AreProbablySameType(ReferencedObjectType, o.GetType())
               &&
               AreProbablySameType(ReferencedObjectRepositoryType, o.Repository.GetType());
    }

    private static bool AreProbablySameType(string storedTypeName, Type candidate)
    {
        return storedTypeName.Equals(candidate.Name, StringComparison.CurrentCultureIgnoreCase) ||
               storedTypeName.Equals(candidate.FullName, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    ///     Returns the instance of the object referenced by this class or null if it no longer exists (e.g. has been deleted)
    /// </summary>
    /// <param name="repositoryLocator"></param>
    /// <returns></returns>
    public virtual IMapsDirectlyToDatabaseTable
        GetReferencedObject(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        return repositoryLocator.GetArbitraryDatabaseObject(ReferencedObjectRepositoryType, ReferencedObjectType,
            ReferencedObjectID);
    }

    /// <summary>
    ///     Returns true if the object referenced by this class still exists in the database
    /// </summary>
    /// <param name="repositoryLocator"></param>
    /// <returns></returns>
    public bool ReferencedObjectExists(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        return repositoryLocator.ArbitraryDatabaseObjectExists(ReferencedObjectRepositoryType, ReferencedObjectType,
            ReferencedObjectID);
    }

    public void InjectKnown(IMapsDirectlyToDatabaseTable instance)
    {
        _knownReferenceTo = new Lazy<IMapsDirectlyToDatabaseTable>(instance);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    private Lazy<IMapsDirectlyToDatabaseTable> _knownReferenceTo;

    public void ClearAllInjections()
    {
        _knownReferenceTo = null;
    }

    /// <summary>
    ///     Returns all <see cref="ExtendedProperty" /> defined <paramref name="forObject" /> in the
    ///     <paramref name="repository" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="forObject"></param>
    /// <returns></returns>
    internal static IEnumerable<ExtendedProperty> GetProperties(ICatalogueRepository repository,
        IMapsDirectlyToDatabaseTable forObject)
    {
        return repository.GetAllObjectsWhere<ExtendedProperty>(
                "ReferencedObjectID", forObject.ID,
                ExpressionType.And,
                "ReferencedObjectType", forObject.GetType().Name)
            .Where(p => p.IsReferenceTo(forObject));
    }

    /// <summary>
    ///     Returns the <see cref="ExtendedProperty" /> <paramref name="named" /> or null if not defined
    ///     <paramref name="forObject" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="named"></param>
    /// <param name="forObject"></param>
    internal static ExtendedProperty GetProperty(ICatalogueRepository repository, string named,
        IMapsDirectlyToDatabaseTable forObject)
    {
        return GetProperties(repository, forObject).SingleOrDefault(e => e.Name.Equals(named));
    }
}