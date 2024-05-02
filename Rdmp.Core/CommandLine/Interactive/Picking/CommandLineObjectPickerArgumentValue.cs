// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using NLog;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

/// <summary>
///     Holds a string value entered in at the console (or other source).  Typically produced by a
///     <see cref="CommandLineObjectPicker" /> or <see cref="PickObjectBase" /> this class
///     can be an expression of multiple different types of objects (e.g. a <see cref="Database" /> or <see cref="Table" />
///     ).  You can always access the original string via <see cref="RawValue" />.
/// </summary>
public class CommandLineObjectPickerArgumentValue
{
    /// <summary>
    ///     The exact value typed in at the console
    /// </summary>
    public string RawValue { get; }

    /// <summary>
    ///     Which element in the sequence of arguments
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     <see cref="DiscoveredDatabase" /> if <see cref="RawValue" /> matches the syntax (see <see cref="PickDatabase" />)
    ///     otherwise null
    /// </summary>
    public DiscoveredDatabase Database { get; private set; }

    /// <summary>
    ///     <see cref="DiscoveredDatabase" /> if <see cref="RawValue" /> matches the syntax (see <see cref="PickTable" />)
    ///     otherwise null
    /// </summary>
    public DiscoveredTable Table { get; private set; }

    /// <summary>
    ///     A collection of <see cref="DatabaseEntity" /> if the <see cref="RawValue" /> indicates selecting one or more such
    ///     objects
    ///     (e.g. see <see cref="PickObjectByID" />, <see cref="PickObjectByName" />) otherwise null
    /// </summary>
    public ReadOnlyCollection<IMapsDirectlyToDatabaseTable> DatabaseEntities { get; private set; }

    /// <summary>
    ///     <see cref="System.Type" /> if the <see cref="RawValue" /> matches a single type name (see <see cref="PickType" />)
    ///     otherwise null
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    ///     True if the <see cref="RawValue" /> is an explicit indication by the user that they want a null
    ///     value used (rather than just skipping out selection)
    /// </summary>
    public bool ExplicitNull =>
        RawValue != null && RawValue.Equals("null", StringComparison.CurrentCultureIgnoreCase);

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public CommandLineObjectPickerArgumentValue(string rawValue, int idx)
    {
        RawValue = rawValue;
        Index = idx;
    }

    public CommandLineObjectPickerArgumentValue(string rawValue, int idx, IMapsDirectlyToDatabaseTable[] entities) :
        this(rawValue, idx)
    {
        DatabaseEntities = new ReadOnlyCollection<IMapsDirectlyToDatabaseTable>(entities);
    }

    public CommandLineObjectPickerArgumentValue(string rawValue, int idx, DiscoveredDatabase database) : this(rawValue,
        idx)
    {
        Database = database;
    }

    public CommandLineObjectPickerArgumentValue(string rawValue, int idx, DiscoveredTable table) : this(rawValue, idx)
    {
        Table = table;
    }

    public CommandLineObjectPickerArgumentValue(string rawValue, int idx, Type type) : this(rawValue, idx)
    {
        Type = type;
    }

    /// <summary>
    ///     Returns the contents of this class expressed as the given <paramref name="paramType" /> or null if the current
    ///     state
    ///     does not describe an object of that Type
    /// </summary>
    /// <param name="paramType"></param>
    /// <returns></returns>
    public object GetValueForParameterOfType(Type paramType)
    {
        if (ExplicitNull)
            return null;

        if (typeof(DirectoryInfo) == paramType)
            return new DirectoryInfo(RawValue);

        if (typeof(FileInfo) == paramType)
            return new FileInfo(RawValue);

        if (typeof(string) == paramType)
            return RawValue;

        if (typeof(Type) == paramType)
            return Type;

        if (typeof(DiscoveredDatabase) == paramType)
            return Database;

        if (typeof(DiscoveredTable) == paramType)
            return Table;

        var element = paramType.GetElementType();

        // If paramType is nullable e.g. 'int?' then this is the underlying time i.e. 'int' otherwise null
        var nullableType = Nullable.GetUnderlyingType(paramType);

        //it's an array of DatabaseEntities (IMapsDirectlyToDatabaseTable implements IDeleteable)
        if (paramType.IsArray && DatabaseEntities != null &&
            typeof(IDeleteable).IsAssignableFrom(paramType.GetElementType()))
        {
            if (DatabaseEntities.Count == 0)
                _logger.Warn($"Pattern matched no objects '{RawValue}'");

            if (element != typeof(IDeleteable))
            {
                var typedArray = Array.CreateInstance(element, DatabaseEntities.Count);
                for (var i = 0; i < DatabaseEntities.Count; i++)
                    typedArray.SetValue(DatabaseEntities[i], i);

                return typedArray;
            }

            return DatabaseEntities.ToArray();
        }

        if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(paramType))
            return GetOneDatabaseEntity<DatabaseEntity>();

        if (typeof(IMightBeDeprecated).IsAssignableFrom(paramType))
            return GetOneDatabaseEntity<IMightBeDeprecated>();

        if (typeof(IDisableable) == paramType)
            return GetOneDatabaseEntity<IDisableable>();

        if (typeof(INamed) == paramType)
            return GetOneDatabaseEntity<INamed>();

        if (typeof(IDeleteable) == paramType)
            return GetOneDatabaseEntity<IDeleteable>();

        if (typeof(ICheckable) == paramType)
            return GetOneDatabaseEntity<ICheckable>();

        if (typeof(ILoggedActivityRootObject) == paramType)
            return GetOneDatabaseEntity<ILoggedActivityRootObject>();

        if (typeof(ICollectSqlParameters) == paramType)
            return GetOneDatabaseEntity<ICollectSqlParameters>();


        // is it a basic Type (value type or Enum)?
        var basicType = nullableType ?? paramType;

        if (basicType.IsValueType && !typeof(Enum).IsAssignableFrom(basicType))
            return UsefulStuff.ChangeType(RawValue, basicType);

        return basicType.IsEnum ? Enum.Parse(basicType, RawValue, true) : null;
    }

    private object GetOneDatabaseEntity<T>()
    {
        //if there are not exactly 1 database entity
        if (DatabaseEntities == null)
            return null;

        if (DatabaseEntities.Count != 1)
        {
            var latest = NewObjectPool.Latest(DatabaseEntities.Where(d => d is T));

            if (latest == null)
                _logger.Warn(
                    $"Pattern matched {DatabaseEntities.Count} objects '{RawValue}':{Environment.NewLine} {string.Join(Environment.NewLine, DatabaseEntities)}");

            return latest;
        }

        //return the single object as the type you want e.g. ICheckable
        if (DatabaseEntities[0] is T)
            return DatabaseEntities.Single();

        //it's not ICheckable, user made an invalid object selection
        throw new CommandLineObjectPickerParseException($"Specified object was not an '{typeof(T)}''", Index, RawValue);
    }

    /// <summary>
    ///     Returns true if the current <see cref="CommandLineObjectPickerArgumentValue" /> could be used to provide a
    ///     value of the given <paramref name="paramType" /> or the user has picked an <see cref="ExplicitNull" />.
    /// </summary>
    /// <param name="paramType"></param>
    /// <returns></returns>
    public bool HasValueOfType(Type paramType)
    {
        try
        {
            var canBeNull = !paramType.IsValueType || Nullable.GetUnderlyingType(paramType) != null;

            if (ExplicitNull && canBeNull)
                return true;
            var val = GetValueForParameterOfType(paramType);

            return val != null && paramType.IsInstanceOfType(val);
        }
        catch (Exception e)
        {
            //could be parse error or something
            Console.WriteLine($"Bad parameter.  Expected Type '{paramType}' for index {Index}.  Error was:{e.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Modifies this instance to include missing values identified in <paramref name="others" />
    /// </summary>
    /// <param name="others"></param>
    /// <returns></returns>
    public CommandLineObjectPickerArgumentValue Merge(IEnumerable<CommandLineObjectPickerArgumentValue> others)
    {
        foreach (var other in others)
        {
            if (other.Index != Index || other.RawValue != RawValue)
                throw new ArgumentException("Merge only arguments of the same object");

            Type ??= other.Type;
            Database ??= other.Database;
            Table ??= other.Table;

            //if they have some
            if (other.DatabaseEntities != null)
                //do we have any? yet
                if (DatabaseEntities == null || !DatabaseEntities.Any()) //no
                    DatabaseEntities = other.DatabaseEntities; //use theirs
                else if (other.DatabaseEntities.Any())
                    throw new Exception("Did not know which set to pick during merge.  Both had DatabaseEntities");
        }

        return this;
    }
}