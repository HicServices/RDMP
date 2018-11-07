using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.DataHelper
{
    /// <summary>
    /// Generates TableInfo entries in the ICatalogueRepository based the table/view specified on the live database server.  Can also be used to import new ColumnInfos into existing
    /// TableInfos (See TableInfoSynchronizer).
    /// </summary>
    public class TableInfoImporter:ITableInfoImporter
    {
        private readonly ICatalogueRepository _repository;
        private readonly string _importFromServer;
        private readonly string _importDatabaseName;
        private readonly string _importTableName;
        
        private readonly string _username;
        private readonly string _password;
        private readonly DataAccessContext _usageContext;
        
        private readonly DatabaseType _type;
        
        private IDiscoveredServerHelper _helper;
        private DbConnectionStringBuilder _builder;
        private DiscoveredServer _server;

        #region Construction
        public TableInfoImporter(ICatalogueRepository repository,string importFromServer, string importDatabaseName, string importTableName, DatabaseType type, string username=null,string password=null, DataAccessContext usageContext=DataAccessContext.Any)
        {
            _repository = repository;
            _importFromServer = importFromServer;
            _importDatabaseName = importDatabaseName;
            _importTableName = importTableName;
            _type = type;

            _username = string.IsNullOrWhiteSpace(username) ? null : username;
            _password = string.IsNullOrWhiteSpace(password) ? null : password;
            _usageContext = usageContext;

            _helper = new DatabaseHelperFactory(type).CreateInstance();
            
            InitializeBuilder();
        }

        public TableInfoImporter(ICatalogueRepository catalogueRepository, DiscoveredTable table)
            : this(catalogueRepository,
            table.Database.Server.Name,
            table.Database.GetRuntimeName(),
            table.GetRuntimeName(),
            table.Database.Server.DatabaseType,
            table.Database.Server.ExplicitUsernameIfAny,
            table.Database.Server.ExplicitPasswordIfAny)
        {
            _usageContext = DataAccessContext.Any;
            InitializeBuilder();
        }

        private void InitializeBuilder()
        {
            _builder = _helper.GetConnectionStringBuilder(_importFromServer, _importDatabaseName, _username, _password);
            _server = new DiscoveredServer(_builder);
        }
        #endregion
        
        /// <inheritdoc/>
        public void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated)
        {
            var cataRepository = (CatalogueRepository) _repository;
            string tableName;
            string databaseName;

            tableName = RDMPQuerySyntaxHelper.EnsureValueIsWrapped(_importDatabaseName, _type);

            if (_type == DatabaseType.MicrosoftSQLServer)
                tableName += "..";
            else if (_type == DatabaseType.MYSQLServer || _type == DatabaseType.Oracle)
                tableName += ".";
            else
                throw new NotSupportedException("Unknown Type:" + _type);

            tableName += RDMPQuerySyntaxHelper.EnsureValueIsWrapped(_importTableName, _type);
            databaseName = RDMPQuerySyntaxHelper.EnsureValueIsWrapped(_importDatabaseName, _type);

            DiscoveredColumn[] discoveredColumns = _server.ExpectDatabase(_importDatabaseName).ExpectTable(_importTableName).DiscoverColumns();

            ThrowIfDodgyEntityNames(tableName,discoveredColumns);

            TableInfo parent = new TableInfo(cataRepository, tableName)
            {
                DatabaseType = _type,
                Database = databaseName,
                Server = _importFromServer
            };

            parent.SaveToDatabase();

            List<ColumnInfo> newCols = new List<ColumnInfo>();

            foreach (DiscoveredColumn discoveredColumn in discoveredColumns)
                newCols.Add(CreateNewColumnInfo(parent,discoveredColumn));

            tableInfoCreated = parent;
            columnInfosCreated = newCols.ToArray();

            //if there is a username then we need to associate it with the TableInfo we just created
            if(!string.IsNullOrWhiteSpace(_username) )
            {
                DataAccessCredentialsFactory credentialsFactory = new DataAccessCredentialsFactory(cataRepository);
                credentialsFactory.Create(tableInfoCreated, _username, _password, _usageContext);
            }
        }

        private void ThrowIfDodgyEntityNames(string tableName, DiscoveredColumn[] columns)
        {
            var syntax = _server.GetQuerySyntaxHelper();

            string rejectionReason;
            if (!IsValidEntityName(tableName,syntax,out rejectionReason))
                throw new Exception("Table name '" + _server.GetQuerySyntaxHelper().GetRuntimeName(tableName) +
                                    "' was rejected because of it's name.  Reason given was:" + Environment.NewLine + rejectionReason);

            var rejectedColumnNames = columns.Where(c => !IsValidEntityName(c.GetRuntimeName(),syntax)).Select(c => c.GetRuntimeName()).ToArray();

            if (rejectedColumnNames.Any())
                throw new Exception(
                    ("The following column names were rejected because they contained freaky characters or were reserved keywords like 'USER' and 'TABLE' (table import abandoned):" +
                     Environment.NewLine + string.Join(Environment.NewLine, rejectedColumnNames)));
        }

        public static bool IsValidEntityName(string name, IQuerySyntaxHelper syntax)
        {
            string whoCares;
            return IsValidEntityName(name, syntax,out whoCares);
        }

        public static bool IsValidEntityName(string name, IQuerySyntaxHelper syntax, out string reason)
        {
            //if it is a table valued function it's ok too
            if (name != null && name.Count(c => c == '(') == 1 && name.Count(c => c ==')') == 1 && name.Contains("@"))
            {
                reason = null;
                return true;
            }

            if(name != null)
                if (ProhibitedNames.Any(p => name.ToLower().Equals(p.ToLower())))//if it matches any of the prohibited names - SQL reserved words complain
                {
                    reason = "Column or Table name " + name +" was REJECTED because it is an invalid name (probably it is a reserved word in SQL)";
                    return false;
                }

            if (
                UsefulStuff.RegexThingsThatAreNotNumbersOrLettersOrUnderscores.IsMatch(
                    syntax.GetRuntimeName(name)))
            {
                reason = "Column or Table name " + name + " was REJECTED because it matches the regex " +
                         UsefulStuff.RegexThingsThatAreNotNumbersOrLettersOrUnderscores +
                         " (UsefulStuff.RegexThingsThatAreNotNumbersOrLettersOrUnderscores), probably your column contains spaces or something";
                return false;
            }
            
            reason = null;
            return true;
        }

        /// <inheritdoc/>
        public ColumnInfo CreateNewColumnInfo(TableInfo parent,DiscoveredColumn discoveredColumn)
        {
            var col = new ColumnInfo((ICatalogueRepository) parent.Repository,
                RDMPQuerySyntaxHelper.EnsureValueIsWrapped(parent.Name, _type) +
                "." + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(discoveredColumn.GetRuntimeName(), _type),
                discoveredColumn.DataType.SQLType, parent);

            //if it has an explicitly specified format (Collation)
            col.Format = discoveredColumn.Format;
            
            //if it is a primary key
            col.IsPrimaryKey = discoveredColumn.IsPrimaryKey;
            col.IsAutoIncrement = discoveredColumn.IsAutoIncrement;
            col.Collation = discoveredColumn.Collation;

            col.SaveToDatabase();
        

            return col;
        }
        
        public void DoImport()
        {
            TableInfo ignored;
            ColumnInfo[] alsoIgnored;

            DoImport(out ignored, out alsoIgnored);
        }

        private static readonly string[] ProhibitedNames = new[]
        {
            "ADD",
"EXTERNAL",
"PROCEDURE",
"ALL",
"FETCH",
"PUBLIC",
"ALTER",
"FILE",
"RAISERROR",
"AND",
"FILLFACTOR",
"READ",
"ANY",
"FOR",
"READTEXT",
"AS",
"FOREIGN",
"RECONFIGURE",
"ASC",
"FREETEXT",
"REFERENCES",
"AUTHORIZATION",
"FREETEXTTABLE",
"REPLICATION",
"BACKUP",
"FROM",
"RESTORE",
"BEGIN",
"FULL",
"RESTRICT",
"BETWEEN",
"FUNCTION",
"RETURN",
"BREAK",
"GOTO",
"REVERT",
"BROWSE",
"GRANT",
"REVOKE",
"BULK",
"GROUP",
"RIGHT",
"BY",
"HAVING",
"ROLLBACK",
"CASCADE",
"HOLDLOCK",
"ROWCOUNT",
"CASE",
"IDENTITY",
"ROWGUIDCOL",
"CHECK",
"IDENTITY_INSERT",
"RULE",
"CHECKPOINT",
"IDENTITYCOL",
"SAVE",
"CLOSE",
"IF",
"SCHEMA",
"CLUSTERED",
"IN",
"SECURITYAUDIT",
"COALESCE",
"INDEX",
"SELECT",
"COLLATE",
"INNER",
"SEMANTICKEYPHRASETABLE",
"COLUMN",
"INSERT",
"SEMANTICSIMILARITYDETAILSTABLE",
"COMMIT",
"INTERSECT",
"SEMANTICSIMILARITYTABLE",
"COMPUTE",
"INTO",
"SESSION_USER",
"CONSTRAINT",
"IS",
"SET",
"CONTAINS",
"JOIN",
"SETUSER",
"CONTAINSTABLE",
"KEY",
"SHUTDOWN",
"CONTINUE",
"KILL",
"SOME",
"CONVERT",
"LEFT",
"STATISTICS",
"CREATE",
"LIKE",
"SYSTEM_USER",
"CROSS",
"LINENO",
"TABLE",
"CURRENT",
"LOAD",
"TABLESAMPLE",
"CURRENT_DATE",
"MERGE",
"TEXTSIZE",
"CURRENT_TIME",
"NATIONAL",
"THEN",
"CURRENT_TIMESTAMP",
"NOCHECK",
"TO",
"CURRENT_USER",
"NONCLUSTERED",
"TOP",
"CURSOR",
"NOT",
"TRAN",
"DATABASE",
"NULL",
"TRANSACTION",
"DBCC",
"NULLIF",
"TRIGGER",
"DEALLOCATE",
"OF",
"TRUNCATE",
"DECLARE",
"OFF",
"TRY_CONVERT",
"DEFAULT",
"OFFSETS",
"TSEQUAL",
"DELETE",
"ON",
"UNION",
"DENY",
"OPEN",
"UNIQUE",
"DESC",
"OPENDATASOURCE",
"UNPIVOT",
"DISK",
"OPENQUERY",
"UPDATE",
"DISTINCT",
"OPENROWSET",
"UPDATETEXT",
"DISTRIBUTED",
"OPENXML",
"USE",
"DOUBLE",
"OPTION",
"USER",
"DROP",
"OR",
"VALUES",
"DUMP",
"ORDER",
"VARYING",
"ELSE",
"OUTER",
"VIEW",
"END",
"OVER",
"WAITFOR",
"ERRLVL",
"PERCENT",
"WHEN",
"ESCAPE",
"PIVOT",
"WHERE",
"EXCEPT",
"PLAN",
"WHILE",
"EXEC",
"PRECISION",
"WITH",
"EXECUTE",
"PRIMARY",
"WITHIN GROUP",
"EXISTS",
"PRINT",
"WRITETEXT",
"EXIT",
"PROC"
        };

    }
}
