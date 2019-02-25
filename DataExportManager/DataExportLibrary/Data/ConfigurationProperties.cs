// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.Data
{
    /// <summary>
    /// String based properties that are configured once per Data Export Database.  This includes how to implement Hashing and any text to appear in the Release 
    /// Document that is provided to researchers (and anything else we might want to configure globally for extraction in future).
    /// 
    /// <para>Values are stored in the ConfigurationProperties table in the Data Export Database.</para>
    /// </summary>
    public class ConfigurationProperties
    {
        /// <summary>
        /// List of all Keys that can be stored in the <see cref="ConfigurationProperties"/> table of the data export database
        /// </summary>
        public enum ExpectedProperties
        {
            /// <summary>
            /// What to do in order to produce a 'Hash' when an <see cref="ExtractableColumn"/> is marked <see cref="ConcreteColumn.HashOnDataRelease"/>
            /// </summary>
            HashingAlgorithmPattern,

            /// <summary>
            /// What text to write into the release document when releasing datasets
            /// </summary>
            ReleaseDocumentDisclaimer
        }

        private readonly bool _allowCaching;
        private readonly IRepository _repository;
        private bool _cacheOutOfDate = true;
        private readonly Dictionary<string,string> _cacheDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new instance ready to read values out of the <paramref name="repository"/> database
        /// </summary>
        /// <param name="allowCaching"></param>
        /// <param name="repository"></param>
        public ConfigurationProperties(bool allowCaching, IRepository repository)
        {
            _allowCaching = allowCaching;
            _repository = repository;
        }

        /// <summary>
        /// Returns the currently persisted value for the given key (See <see cref="ExpectedProperties"/>)
        /// </summary>
        /// <param name="property"></param>
        ///  <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public string GetValue(string property)
        {

            //if we do not allow caching then we effectively pull all values every time.  We also pull every value if cache has not been built yet (is out of date)
            if (!_allowCaching || _cacheOutOfDate)
                RefreshCache();

            if (_cacheDictionary.ContainsKey(property))
                return _cacheDictionary[property];

            throw new KeyNotFoundException("Could not find property called " + property + " in ConfigurationProperties table in Data Export database");
        }
        
        /// <inheritdoc cref="GetValue(string)"/>
        public string GetValue(ExpectedProperties property)
        {
            return GetValue(property.ToString());
        }

        /// <inheritdoc cref="GetValue(string)"/>
        public string TryGetValue(string property)
        {
            try
            {
                return GetValue(property);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetValue(string)"/>
        public string TryGetValue(ExpectedProperties property)
        {
            string value = TryGetValue(property.ToString());

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }

      
        /// <summary>
        /// Stores a new <paramref name="value"/> for the given <see cref="property"/> (and saves to the database)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void SetValue(ExpectedProperties property, string value)
        {
            SetValue(property.ToString(), value);
        }

        /// <inheritdoc cref="SetValue(DataExportLibrary.Data.ConfigurationProperties.ExpectedProperties,string)"/>
        public void SetValue(string property, string value)
        {
            if(_cacheOutOfDate)
                RefreshCache();

            if (_cacheDictionary.ContainsKey(property))
                IssueUpdateCommand(property, value);
            else
                IssueInsertCommand(property, value);
            
            //a value has been set, reset cache (we could update cache manually in memory but a round trip to database is safer)
            _cacheOutOfDate = true;
        }

        /// <summary>
        /// Deletes the currently stored value of the given <paramref name="property"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public int DeleteValue(string property)
        {
            return IssueDeleteCommand(property);
        }

        #region read/write to database
        private int IssueDeleteCommand(string property)
        {
            return _repository.Delete("DELETE FROM [ConfigurationProperties] WHERE Property=@property",
                new Dictionary<string, object>
                {
                    {"property", property}
                });
        }

        private void IssueInsertCommand(string property, string value)
        {
            _repository.Insert("INSERT INTO [ConfigurationProperties](Property,Value) VALUES (@property,@value)",
                new Dictionary<string, object>
                {
                    {"value", value},
                    {"property", property}
                });
        }

        private void IssueUpdateCommand(string property, string value)
        {
            _repository.Update("UPDATE [ConfigurationProperties] set Value=@value where Property=@property",
                new Dictionary<string, object>
                {
                    {"value", value},
                    {"property", property}
                });
        }

        private void RefreshCache()
        {
            var repo = (TableRepository)_repository;
            using (var con = repo.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand("SELECT * from [ConfigurationProperties]",con.Connection, con.Transaction);
                using (var reader = cmd.ExecuteReader())
                {
                    _cacheDictionary.Clear();

                    //get cache of all answers
                    while (reader.Read())
                        _cacheDictionary.Add(reader["Property"].ToString(), reader["Value"] as string);
                }
            }

            _cacheOutOfDate = false;
        }
        #endregion
    }
}
