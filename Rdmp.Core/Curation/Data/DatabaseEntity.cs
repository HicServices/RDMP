// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using FAnsi;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Base class for all objects which are stored in database repositories (e.g. Catalogue database , Data Export database).  This is the abstract implementation of
/// IMapsDirectlyToDatabaseTable.  You must always have two constructors, one that takes a DbDataReader and is responsible for constructing an instance from a
/// record in the database and one that takes the minimum parameters required to satisfy database constraints and is used to create new objects.
/// 
/// <para>A DatabaseEntity instance cannot exist without there being a matching record in the database repository.  This is the RDMP design pattern for object permenance,
/// sharing and allowing advanced users to update the data model via database queries running directly on the object repository database.</para>
/// 
/// <para>A DatabaseEntity must have the same name as a Table in in the IRepository and must only have public properties that match columns in that table.  This enforces
/// a transparent mapping between code and database.  If you need to add other public properties you must decorate them with [NoMappingToDatabase]</para>
/// </summary>
public abstract class DatabaseEntity : IRevertable, ICanBeSummarised
{
    /// <summary>
    /// The maximum length for any given line in return value of <see cref="GetSummary"/>
    /// </summary>
    public const int MAX_SUMMARY_ITEM_LENGTH = 100;

    protected const string SUMMARY_LINE_DIVIDER = "⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯";

    /// <inheritdoc/>
    public int ID { get; set; }

    private bool _readonly;

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IRepository Repository { get; set; }

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ICatalogueRepository CatalogueRepository => Repository as ICatalogueRepository;


    /// <summary>
    /// Returns <see cref="Repository"/> as <see cref="IDataExportRepository"/> or null if the object does not exist in a data export repository.
    /// </summary>
    [NoMappingToDatabase]
    public IDataExportRepository DataExportRepository => Repository as IDataExportRepository;

    /// <summary>
    /// Constructs a new instance.  You should only use this when your object does not yet exist in the database
    /// and you are trying to create it into the db
    /// </summary>
    protected DatabaseEntity()
    {
    }

    /// <summary>
    /// Creates a new instance and hydrates it from the current values of <paramref name="r"/>
    /// </summary>
    /// <param name="repository">The database which the record/object was read from</param>
    /// <param name="r">Data reader with values for hydrating this object</param>
    protected DatabaseEntity(IRepository repository, DbDataReader r)
    {
        Repository = repository;

        if (!HasColumn(r, "ID"))
            throw new InvalidOperationException(
                $"The DataReader passed to this type ({GetType().Name}) does not contain an 'ID' column. This is a requirement for all IMapsDirectlyToDatabaseTable implementing classes.");

        ID = int.Parse(r["ID"]
            .ToString()); // gets around decimals and other random crud number field types that sql returns
    }

    private static bool HasColumn(IDataRecord reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
            if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                return true;

        return false;
    }

    /// <summary>
    /// Converts the <paramref name="fieldName"/> into a <see cref="Uri"/>.  DBNull.Value and null are returned as null;
    /// </summary>
    /// <param name="r"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    protected static Uri ParseUrl(DbDataReader r, string fieldName)
    {
        var uri = r[fieldName];

        return uri == null || uri == DBNull.Value || string.IsNullOrWhiteSpace(uri.ToString())
            ? null
            : new Uri(uri.ToString());
    }

    /// <inheritdoc cref="IRepository.GetHashCode(IMapsDirectlyToDatabaseTable)"/>
    public override int GetHashCode() => Repository.GetHashCode(this);

    /// <inheritdoc cref="IRepository.AreEqual(IMapsDirectlyToDatabaseTable,object)"/>
    public override bool Equals(object obj) => Repository.AreEqual(this, obj);

    /// <inheritdoc/>
    public virtual void SaveToDatabase()
    {
        Repository.SaveToDatabase(this);
    }

    /// <inheritdoc/>
    public virtual void DeleteInDatabase()
    {
        Repository.DeleteFromDatabase(this);
    }

    /// <inheritdoc/>
    public virtual void RevertToDatabaseState()
    {
        Repository.RevertToDatabaseState(this);

        if (this is IInjectKnown ii)
            ii.ClearAllInjections();
    }

    /// <inheritdoc/>
    public RevertableObjectReport HasLocalChanges() => Repository.HasLocalChanges(this);

    /// <inheritdoc/>
    public virtual bool Exists() => Repository.StillExists(this);

    /// <summary>
    /// Converts the supplied object to a <see cref="DateTime"/> or null if o is null/DBNull.Value
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    protected static DateTime? ObjectToNullableDateTime(object o) =>
        o == null || o == DBNull.Value ? null : (DateTime)o;

    /// <summary>
    /// Converts the supplied object to a <see cref="int"/> or null if o is null/DBNull.Value
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    protected static int? ObjectToNullableInt(object o) =>
        o == null || o == DBNull.Value ? null : int.Parse(o.ToString());

    /// <summary>
    /// Converts the supplied object to a <see cref="bool"/> or null if o is null/DBNull.Value
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    protected static bool? ObjectToNullableBool(object o) =>
        o == null || o == DBNull.Value ? null : Convert.ToBoolean(o);

    /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Fired when any database tied property is changed in memory.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <param name="propertyName"></param>
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(object oldValue, object newValue,
        [CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue));
    }

    /// <summary>
    /// Changes the value of <paramref name="field"/> to <paramref name="value"/> and triggers <see cref="OnPropertyChanged"/>.  You should have a public Property and
    /// a backing field for all database fields on your object.  The Property should use this method to change the underlying fields value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        //treat null and "" as the same
        if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(field as string) &&
            string.IsNullOrWhiteSpace(value as string))
            return false;

        if (_readonly)
            throw new Exception(
                $"An attempt was made to modify Property '{propertyName}' of Database Object of Type '{GetType().Name}' while it was in read only mode.  Object was called '{this}'");

        var old = field;

        field = value;
        OnPropertyChanged(old, value, propertyName);
        return true;
    }

    /// <inheritdoc/>
    public void SetReadOnly()
    {
        _readonly = true;
    }

    /// <summary>
    /// Copies all properties not marked with [NoMappingToDatabase] or [Relationship] from the this object to the <paramref name="to"/> object.
    /// Also skips 'Name' and 'ID'
    /// </summary>
    /// <param name="to"></param>
    /// <param name="copyName"></param>
    /// <param name="save"></param>
    protected void CopyShallowValuesTo(DatabaseEntity to, bool copyName = false, bool save = true)
    {
        if (GetType() != to.GetType())
            throw new NotSupportedException(
                $"Object to must be the same Type as us, we were '{GetType().Name}' and it was '{to.GetType().Name}'");

        var noMappingFinder = new AttributePropertyFinder<NoMappingToDatabase>(to);
        var relationsFinder = new AttributePropertyFinder<RelationshipAttribute>(to);

        foreach (var p in GetType().GetProperties())
        {
            if (p.Name.Equals("ID"))
                continue;

            if (p.Name.Equals("Name") && !copyName)
                continue;

            if (noMappingFinder.GetAttribute(p) != null || relationsFinder.GetAttribute(p) != null)
                continue;

            p.SetValue(to, p.GetValue(this));
        }

        if (save)
            to.SaveToDatabase();
    }

    /// <inheritdoc/>
    public virtual string GetSummary(bool includeName, bool includeID)
    {
        var sbPart1 = new StringBuilder();
        foreach (var prop in GetType().GetProperties().Where(p => p.Name.Contains("Description")))
            AppendPropertyToSummary(sbPart1, prop, includeName, includeID, false);

        var sbPart2 = new StringBuilder();
        foreach (var prop in GetType().GetProperties().Where(p => !p.Name.Contains("Description")))
            AppendPropertyToSummary(sbPart2, prop, includeName, includeID);

        if (sbPart1.Length > 0 && sbPart2.Length > 0)
            sbPart1.AppendLine(SUMMARY_LINE_DIVIDER);
        sbPart1.Append(sbPart2);

        return sbPart1.ToString();
    }

    protected void AppendPropertyToSummary(StringBuilder sb, PropertyInfo prop, bool includeName, bool includeID,
        bool includePropertyName = true)
    {
        // don't show Name if we are being told not to
        if (!includeName && prop.Name.EndsWith("Name"))
            return;

        if (!includeID && prop.Name.Equals("ID"))
            return;

        if (prop.Name.Contains("Password"))
            return;
        if (prop.Name.Equals(nameof(ExtractableCohort.Count)))
            return;
        if (prop.Name.Equals(nameof(ExtractableCohort.CountDistinct)))
            return;

        // don't show foreign key ID properties
        if (prop.Name.EndsWith("_ID"))
            return;

        object val;
        try
        {
            val = prop.GetValue(this);
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException is NotSupportedException) return;

            throw;
        }

        // skip properties marked with 'do not extract'
        if (prop.GetCustomAttributes(typeof(DoNotExtractProperty), true).Any())
            return;

        if (val is string || val is IFormattable || val is bool)
        {
            // skip properties values that are "unknown"
            if (val is Enum e && Convert.ToInt32(e) == 0 && val is not DatabaseType)
                return;

            var representation =
                $"{(includePropertyName ? $"{FormatPropertyNameForSummary(prop)}: " : "")}{FormatForSummary(val)}";

            if (representation.Length > MAX_SUMMARY_ITEM_LENGTH)
                representation = $"{representation[..(MAX_SUMMARY_ITEM_LENGTH - 3)]}...";

            if (representation.Contains('\n')) representation = Regex.Replace(representation, @"\r?\n", " ");

            sb.AppendLine(representation);
        }
    }

    /// <summary>
    /// Returns the human readable property name adjusted to be elegant to read (e.g. with spaces not pascal case)
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    protected virtual string FormatPropertyNameForSummary(PropertyInfo prop) =>
        UsefulStuff.PascalCaseStringToHumanReadable(prop.Name);

    /// <summary>
    /// Formats a given value for user readability in the results of <see cref="GetSummary(bool,bool)"/>
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    protected static string FormatForSummary(object val) => val is bool b ? b ? "Yes" : "No" : val.ToString()?.Trim();
}