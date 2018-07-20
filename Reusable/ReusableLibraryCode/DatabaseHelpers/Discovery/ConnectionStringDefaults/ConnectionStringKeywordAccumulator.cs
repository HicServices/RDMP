using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults
{
    /// <summary>
    /// Gathers keywords for use in building DbConnectionStrings for a given <see cref="DatabaseType"/>
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
            //Make sure it is supported by the connection string builder
            _builder.Add(keyword, value);
            _builder.Clear();

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
        /// Adds the currently configured keywords to the connection string builder.
        /// </summary>
        public void EnforceOptions(DbConnectionStringBuilder connectionStringBuilder)
        {
            foreach (var keyword in _keywords)
                connectionStringBuilder[keyword.Key] = keyword.Value.Item1;
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
        /// User specified overrides for SystemDefault settings.
        /// </summary>
        UserOverride,

        /// <summary>
        /// High level priority, the C# object being used is specifying a required keyword for it to operate correctly.  This overrides
        /// user settings and system defaults (but not <see cref="APIRule"/>)
        /// </summary>
        ObjectOverride,

        /// <summary>
        /// Highest priority for keywords.  This is settings that cannot be unset/overriden by anyone else and are required
        /// for the API to work e.g.  AllowUserVariables in MySql
        /// </summary>
        APIRule
    }
}
