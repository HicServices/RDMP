using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults
{
    /// <summary>
    /// Gathers keywords for use in building connection strings for a given <see cref="DatabaseType"/> in a priority overriding manner.
    /// </summary>
    public class ConnectionStringKeywordAccumulator
    {
        public DatabaseType DatabaseType { get; set; }

        private readonly Dictionary<string, Tuple<string, ConnectionStringKeywordPriority>> _keywords = new Dictionary<string, Tuple<string, ConnectionStringKeywordPriority>>(StringComparer.CurrentCultureIgnoreCase);
        private DbConnectionStringBuilder _builder;

        public ConnectionStringKeywordAccumulator(DatabaseType databaseType)
        {
            DatabaseType = databaseType;

            switch (databaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    _builder = new SqlConnectionStringBuilder();
                    break;
                case DatabaseType.MYSQLServer:
                    _builder = new MySqlConnectionStringBuilder();
                    break;
                case DatabaseType.Oracle:
                    _builder = new OracleConnectionStringBuilder();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }

        public void AddOrUpdateKeyword(string keyword, string value, ConnectionStringKeywordPriority priority)
        {
            var collision = GetCollisionWithKeyword(keyword,value);

            if (collision != null)
            {
                //if there is already a semantically equivalent keyword.... 

                //if it is of lower or equal priority
                if (_keywords[collision].Item2 <= priority)
                    _keywords[collision] = Tuple.Create(value, priority); //update it 
                
                //either way don't record it as a new keyword
                return;
            }

            //if we have not got that keyword yet
            if(!_keywords.ContainsKey(keyword))
                _keywords.Add(keyword,Tuple.Create(value,priority));
            else
            {
                //or the keyword that was previously specified had a lower priority
                if (_keywords[keyword].Item2 <= priority)
                    _keywords[keyword] = Tuple.Create(value, priority); //update it with the new value
            }
        }

        /// <summary>
        /// Returns the best alias for <paramref name="keyword"/> or null if there are no known aliases.  This is because some builders allow multiple keys for changing the same underlying
        /// property.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetCollisionWithKeyword(string keyword, string value)
        {
            //lets evaluate this alleged keyword!
            _builder.Clear();

            //Make sure it is supported by the connection string builder
            _builder.Add(keyword, value);

            //now iterate all the keys we had before and add those too, if the key count doesn't change for any of them we know it's a duplicate semantically
            if (_builder.Keys != null)
                foreach (var current in _keywords)
                {
                    int keysBefore = _builder.Keys.Count;

                    _builder.Add(current.Key, current.Value.Item1);

                    //key count in builder didn't change dispite there being new values added
                    if (_builder.Keys.Count == keysBefore)
                        return current.Key;
                }

            //no collisions
            return null;
        }

        /// <summary>
        /// Adds the currently configured keywords to the connection string builder.
        /// </summary>
        public void EnforceOptions(DbConnectionStringBuilder connectionStringBuilder)
        {
            foreach (var keyword in _keywords)
            {
                //This is a system default so can be overridden by the object
                if (keyword.Value.Item2 <= ConnectionStringKeywordPriority.SystemDefaultHigh)
                {
                    var beforeValue = connectionStringBuilder[keyword.Key];

                    int keysBefore = connectionStringBuilder.Keys.Count;

                    connectionStringBuilder[keyword.Key] = keyword.Value.Item1;

                    if(keysBefore == connectionStringBuilder.Keys.Count && beforeValue != null)
                        connectionStringBuilder[keyword.Key] = beforeValue;
                }
                else
                    connectionStringBuilder[keyword.Key] = keyword.Value.Item1;
            }
        }
    }

    public enum ConnectionStringKeywordPriority
    {
        /// <summary>
        /// Lowest priority e.g. settings defined in app config / global const parameters etc that you are happy to be overriden elsewhere
        /// </summary>
        SystemDefaultLow,
        /// <summary>
        /// Lowest priority e.g. settings defined in app config / global const parameters etc that you are happy to be overriden elsewhere
        /// </summary>
        SystemDefaultMedium,
        /// <summary>
        /// Lowest priority e.g. settings defined in app config / global const parameters etc that you are happy to be overriden elsewhere
        /// </summary>
        SystemDefaultHigh,

        /// <summary>
        /// User specified overrides for System Default settings.
        /// </summary>
        UserOverride,

        /// <summary>
        /// High level priority, the C# object being used is specifying a required keyword for it to operate correctly.  This overrides
        /// user settings and system defaults (but not <see cref="ApiRule"/>)
        /// </summary>
        ObjectOverride,

        /// <summary>
        /// Highest priority for keywords.  This is settings that cannot be unset/overriden by anyone else and are required
        /// for the API to work e.g.  AllowUserVariables in MySql
        /// </summary>
        ApiRule
    }
}
