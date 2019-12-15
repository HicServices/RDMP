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
using MapsDirectlyToDatabaseTable;
using NLog;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    /// <summary>
    /// Holds a string value entered in at the console (or other source).  Typically produced by a <see cref="CommandLineObjectPicker"/> or <see cref="PickObjectBase"/> this class
    /// can be an expression of multiple different types of objects (e.g. a <see cref="Database"/> or <see cref="Table"/>).  You can always access the original string via <see cref="RawValue"/>.
    /// </summary>
    public class CommandLineObjectPickerArgumentValue
    {
        public string RawValue { get; }
        public int Index { get; }

        public DiscoveredDatabase Database { get; private set; }

        public DiscoveredTable Table { get; private set; }

        public ReadOnlyCollection<IMapsDirectlyToDatabaseTable> DatabaseEntities { get; private set; }

        public Type Type { get; private set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        public CommandLineObjectPickerArgumentValue(string rawValue,int idx)
        {
            RawValue = rawValue;
            Index = idx;
        }

        public CommandLineObjectPickerArgumentValue(string rawValue,int idx,IMapsDirectlyToDatabaseTable[] entities):this(rawValue, idx)
        {
            DatabaseEntities = new ReadOnlyCollection<IMapsDirectlyToDatabaseTable>(entities);
        }

        public CommandLineObjectPickerArgumentValue(string rawValue, int idx, DiscoveredDatabase database):this(rawValue, idx)
        {
            Database = database;
        }

        public CommandLineObjectPickerArgumentValue(string rawValue, int idx, DiscoveredTable table):this(rawValue, idx)
        {
            Table = table;
        }

        public CommandLineObjectPickerArgumentValue(string rawValue, int idx, Type type):this(rawValue,idx)
        {
            Type = type;
        }

        /// <summary>
        /// Returns the contents of this class expressed as the given <paramref name="paramType"/> or null if the current state
        /// does not describe an object of that Type
        /// </summary>
        /// <param name="paramType"></param>
        /// <returns></returns>
        public object GetValueForParameterOfType(Type paramType)
        {
            if (typeof(DirectoryInfo) == paramType)
                return new DirectoryInfo(RawValue);

            if(typeof(FileInfo) == paramType)
                return new FileInfo(RawValue);

            if (typeof(string) == paramType)
                return RawValue;

            if (typeof(Type) == paramType)
                return Type;

            if (typeof(DiscoveredDatabase) == paramType)
                return Database;

            if (typeof(DiscoveredTable) == paramType)
                return Table;

            //it's an array of DatabaseEntities
            if(paramType.IsArray && typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(paramType.GetElementType()))
            {
                if(DatabaseEntities.Count == 0)
                    _logger.Warn($"Pattern matched no objects '{RawValue}'");

                return DatabaseEntities.ToArray();
            }
            if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(paramType))
                return GetOneDatabaseEntity<DatabaseEntity>();

            if (typeof(IMightBeDeprecated).IsAssignableFrom(paramType))
                return GetOneDatabaseEntity<IMightBeDeprecated>();

            if (typeof (IDisableable) == paramType)
                return GetOneDatabaseEntity<IDisableable>();

            if (typeof (INamed) == paramType)
                return GetOneDatabaseEntity<INamed>();
            
            if (typeof (IDeleteable) == paramType)
                return GetOneDatabaseEntity<IDeleteable>();
            
            if (typeof(ICheckable) == paramType)
                return GetOneDatabaseEntity<ICheckable>();
            
            if (paramType.IsValueType && !typeof(Enum).IsAssignableFrom(paramType))
                return UsefulStuff.ChangeType(RawValue, paramType);
            
            if(paramType.IsEnum)
                return Enum.Parse(paramType,RawValue,true);
            
            return null;
        }

        private object GetOneDatabaseEntity<T>()
        {
            //if there are not exactly 1 database entity
            if (DatabaseEntities == null)
                return null;

            if(DatabaseEntities.Count != 1)
            {
                _logger.Warn($"Pattern matched {DatabaseEntities.Count} objects '{RawValue}'");
                return null;
            }   

            //return the single object as the type you want e.g. ICheckable
            if(DatabaseEntities[0] is T)
                return DatabaseEntities.Single();
            
            //it's not ICheckable, user made an invalid object selection
            throw new CommandLineObjectPickerParseException($"Specified object was not an '{typeof(T)}''",Index,RawValue);
        }

        public bool HasValueOfType(Type paramType)
        {
            try
            {
                return GetValueForParameterOfType(paramType) != null;
            }
            catch (Exception e)
            {
                //could be parse error or something
                Console.WriteLine($"Bad parameter.  Expected Type '{paramType}' for index {Index}.  Error was:{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Modifies this instance to include missing values identified in <paramref name="others"/>
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        public CommandLineObjectPickerArgumentValue Merge(IEnumerable<CommandLineObjectPickerArgumentValue> others)
        {
            foreach (var other in others)
            {
                if(other.Index != Index || other.RawValue != RawValue)
                    throw new ArgumentException("Merge only arguments of the same object");

                Type = Type ?? other.Type;
                Database = Database ?? other.Database;
                Table = Table ?? other.Table;

                //do we have any? yet
                if (DatabaseEntities == null || !DatabaseEntities.Any()) //no
                    DatabaseEntities = other.DatabaseEntities; //use theirs
                else
                    if(other.DatabaseEntities != null && other.DatabaseEntities.Any())
                        throw new Exception("Did not know what set to pick during merge.  Both had DatabaseEntitites");
                
            }

            return this;
        }
    }
}