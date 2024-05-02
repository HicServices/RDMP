// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Implementation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation;

/// <summary>
///     Generates TableInfo entries in the ICatalogueRepository based the table/view specified on the live database server.
///     Can also be used to import new ColumnInfos into existing
///     TableInfos (See TableInfoSynchronizer).
/// </summary>
public class TableInfoImporter : ITableInfoImporter
{
    private readonly ICatalogueRepository _repository;
    private readonly string _importFromServer;
    private readonly string _importDatabaseName;
    private readonly string _importTableName;

    private readonly string _username;
    private readonly string _password;
    private readonly DataAccessContext _usageContext;
    private readonly string _importFromSchema;

    private readonly DatabaseType _type;

    private DiscoveredServer _server;
    private readonly TableType _importTableType;

    #region Construction

    /// <summary>
    ///     Prepares to import the named table as a <see cref="TableInfo" />
    /// </summary>
    /// <param name="repository">Repository to create the <see cref="TableInfo" />/<see cref="ColumnInfo" /> in</param>
    /// <param name="importFromServer"></param>
    /// <param name="importDatabaseName"></param>
    /// <param name="importTableName"></param>
    /// <param name="type"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="usageContext"></param>
    /// <param name="importFromSchema"></param>
    /// <param name="importTableType"></param>
    public TableInfoImporter(ICatalogueRepository repository, string importFromServer, string importDatabaseName,
        string importTableName, DatabaseType type, string username = null, string password = null,
        DataAccessContext usageContext = DataAccessContext.Any, string importFromSchema = null,
        TableType importTableType = TableType.Table)
    {
        var syntax = ImplementationManager.GetImplementation(type).GetQuerySyntaxHelper();

        _repository = repository;
        _importFromServer = importFromServer;
        _importDatabaseName = importDatabaseName;
        _importTableName = importTableName;
        _type = type;

        _username = string.IsNullOrWhiteSpace(username) ? null : username;
        _password = string.IsNullOrWhiteSpace(password) ? null : password;
        _usageContext = usageContext;
        _importFromSchema = importFromSchema ?? syntax.GetDefaultSchemaIfAny();
        _importTableType = importTableType;

        InitializeBuilder();
    }

    /// <summary>
    ///     Prepares to import a reference to the <paramref name="table" /> as <see cref="TableInfo" /> and
    ///     <see cref="ColumnInfo" /> in the RDMP <paramref name="catalogueRepository" />
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <param name="table"></param>
    /// <param name="usageContext"></param>
    public TableInfoImporter(ICatalogueRepository catalogueRepository, DiscoveredTable table,
        DataAccessContext usageContext = DataAccessContext.Any)
        : this(catalogueRepository,
            table.Database.Server.Name,
            table.Database.GetRuntimeName(),
            table.GetRuntimeName(),
            table.Database.Server.DatabaseType,
            table.Database.Server.ExplicitUsernameIfAny,
            table.Database.Server.ExplicitPasswordIfAny,
            usageContext,
            table.Schema,
            table.TableType)
    {
        _usageContext = DataAccessContext.Any;
        InitializeBuilder();
    }

    private void InitializeBuilder()
    {
        _server = new DiscoveredServer(_importFromServer, _importDatabaseName, _type, _username, _password);
    }

    #endregion

    /// <inheritdoc />
    public void DoImport(out ITableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated)
    {
        try
        {
            _server.TestConnection();
        }
        catch (Exception e)
        {
            throw new Exception($"Could not reach server {_server.Name}", e);
        }

        var querySyntaxHelper = _server.GetQuerySyntaxHelper();

        var tableName = querySyntaxHelper.EnsureWrapped(_importDatabaseName);

        tableName += _type switch
        {
            DatabaseType.MicrosoftSQLServer =>
                $".{querySyntaxHelper.EnsureWrapped(_importFromSchema ?? querySyntaxHelper.GetDefaultSchemaIfAny())}.",
            DatabaseType.PostgreSql =>
                $".{querySyntaxHelper.EnsureWrapped(_importFromSchema ?? querySyntaxHelper.GetDefaultSchemaIfAny())}.",
            DatabaseType.MySql => ".",
            DatabaseType.Oracle => ".",
            _ => throw new NotSupportedException($"Unknown Type:{_type}")
        };

        tableName += querySyntaxHelper.EnsureWrapped(_importTableName);
        var databaseName = querySyntaxHelper.EnsureWrapped(_importDatabaseName);

        var discoveredColumns = _server.ExpectDatabase(_importDatabaseName)
            .ExpectTable(_importTableName, _importFromSchema, _importTableType)
            .DiscoverColumns();

        var parent = new TableInfo(_repository, tableName)
        {
            DatabaseType = _type,
            Database = databaseName,
            Server = _importFromServer,
            Schema = _importFromSchema,
            IsView = _importTableType == TableType.View
        };

        parent.SaveToDatabase();

        tableInfoCreated = parent;
        columnInfosCreated = discoveredColumns.Select(discoveredColumn => CreateNewColumnInfo(parent, discoveredColumn))
            .ToArray();

        //if there is a username then we need to associate it with the TableInfo we just created
        if (!string.IsNullOrWhiteSpace(_username))
            new DataAccessCredentialsFactory(_repository).Create(tableInfoCreated, _username, _password, _usageContext);
    }

    /// <inheritdoc />
    public ColumnInfo CreateNewColumnInfo(ITableInfo parent, DiscoveredColumn discoveredColumn)
    {
        var col = new ColumnInfo((ICatalogueRepository)parent.Repository, discoveredColumn.GetFullyQualifiedName(),
            discoveredColumn.DataType.SQLType, parent)
        {
            //if it has an explicitly specified format (Collation)
            Format = discoveredColumn.Format,
            //if it is a primary key
            IsPrimaryKey = discoveredColumn.IsPrimaryKey,
            IsAutoIncrement = discoveredColumn.IsAutoIncrement,
            Collation = discoveredColumn.Collation
        };

        col.SaveToDatabase();


        return col;
    }

    /// <inheritdoc cref="DoImport(out ITableInfo,out ColumnInfo[])" />
    public void DoImport()
    {
        DoImport(out _, out _);
    }

    private static readonly string[] ProhibitedNames =
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