// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
/// String based properties that are configured once per Data Export Database.  This includes how to implement Hashing and any text to appear in the Release
/// Document that is provided to researchers (and anything else we might want to configure globally for extraction in future).
/// 
/// <para>Values are stored in the ConfigurationProperties table in the Data Export Database.</para>
/// </summary>
internal class DataExportPropertyManager : IDataExportPropertyManager
{
    private readonly bool _allowCaching;
    private readonly DataExportRepository _repository;
    private bool _cacheOutOfDate = true;
    private readonly ConcurrentDictionary<string, string> _cacheDictionary = new();

    /// <summary>
    /// Creates a new instance ready to read values out of the <paramref name="repository"/> database
    /// </summary>
    /// <param name="allowCaching"></param>
    /// <param name="repository"></param>
    public DataExportPropertyManager(bool allowCaching, DataExportRepository repository)
    {
        _allowCaching = allowCaching;
        _repository = repository;
    }

    /// <summary>
    /// Returns the currently persisted value for the given key (See <see cref="DataExportProperty"/>)
    /// </summary>
    /// <param name="property"></param>
    ///  <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public string GetValue(string property)
    {
        //if we do not allow caching then we effectively pull all values every time.  We also pull every value if cache has not been built yet (is out of date)
        if (!_allowCaching || _cacheOutOfDate)
            RefreshCache();

        return _cacheDictionary.GetValueOrDefault(property);
    }

    /// <inheritdoc cref="GetValue(string)"/>
    public string GetValue(DataExportProperty property) => GetValue(property.ToString());


    /// <summary>
    /// Stores a new <paramref name="value"/> for the given <paramref name="property"/> (and saves to the database)
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public void SetValue(DataExportProperty property, string value)
    {
        SetValue(property.ToString(), value);
    }

    private void SetValue(string property, string value)
    {
        if (_cacheOutOfDate)
            RefreshCache();

        if (string.IsNullOrWhiteSpace(value))
            IssueDeleteCommand(property);

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
    public int DeleteValue(string property) => IssueDeleteCommand(property);

    #region read/write to database

    private int IssueDeleteCommand(string property) =>
        _repository.Delete("DELETE FROM [ConfigurationProperties] WHERE Property=@property",
            new Dictionary<string, object>
            {
                { "property", property }
            });

    private void IssueInsertCommand(string property, string value)
    {
        _repository.Insert("INSERT INTO [ConfigurationProperties](Property,Value) VALUES (@property,@value)",
            new Dictionary<string, object>
            {
                { "value", value },
                { "property", property }
            });
    }

    private void IssueUpdateCommand(string property, string value)
    {
        _repository.Update("UPDATE [ConfigurationProperties] set Value=@value where Property=@property",
            new Dictionary<string, object>
            {
                { "value", value },
                { "property", property }
            });
    }

    private void RefreshCache()
    {
        var repo = (TableRepository)_repository;
        using (var con = repo.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand("SELECT * from [ConfigurationProperties]",
                con.Connection, con.Transaction);
            using var reader = cmd.ExecuteReader();
            _cacheDictionary.Clear();

            //get cache of all answers
            while (reader.Read())
            {
                var val = reader["Value"] as string;
                _cacheDictionary.AddOrUpdate(reader["Property"].ToString(),
                    val, (k, o) => val);
            }
        }

        _cacheOutOfDate = false;
    }

    #endregion
}