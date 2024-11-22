// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// Defines an anonymisation method for a group of related columns of the same datatype.  For example 'ANOGPCode' could be an instance/record that defines input of type
/// varchar(5) and anonymises into 3 digits and 2 characters with a suffix of _G.  This product would then be used by all ColumnInfos that contain GP codes (current GP
/// previous GP, Prescriber code etc).  Anonymisation occurs at  ColumnInfo level after being loaded from a RAW data load bubble as is pushed to the STAGING bubble.
/// 
/// <para>Each ANOTable describes a corresponding table on an ANO server (see the Server_ID property - we refer to this as an ANOStore) including details of the
/// transformation and a UNIQUE name/suffix.  This let's you quickly identify what data has be annonymised by what ANOTable.</para>
///  
/// <para>It is very important to curate your ANOTables properly or you could end up with irrecoverable data, for example sticking to a single ANO server, taking regular backups
/// NEVER deleting ANOTables that reference existing data  (in the ANOStore database).</para>
/// 
/// </summary>
public class ANOTable : DatabaseEntity, ICheckable, IHasDependencies
{
    /// <summary>
    /// Prefix to put on anonymous columns
    /// </summary>
    public const string ANOPrefix = "ANO";

    private string _identifiableDataType;
    private string _anonymousDataType;

    #region Database Properties

    private string _tableName;
    private int _numberOfIntegersToUseInAnonymousRepresentation;
    private int _numberOfCharactersToUseInAnonymousRepresentation;
    private string _suffix;
    private int _serverID;

    /// <summary>
    /// The name of the table in the ANO database that stores swapped identifiers
    /// </summary>
    public string TableName
    {
        get => _tableName;
        set => SetField(ref _tableName, value);
    }

    /// <summary>
    /// The number of decimal characters to use when creating ANO mapping identifiers.  This will directly impact the number of possible values that can be generated and therefore
    /// the number of unique input values before anonymising fails (due to collisions).
    /// </summary>
    public int NumberOfIntegersToUseInAnonymousRepresentation
    {
        get => _numberOfIntegersToUseInAnonymousRepresentation;
        set => SetField(ref _numberOfIntegersToUseInAnonymousRepresentation, value);
    }

    /// <summary>
    /// The number of alphabetic characters to use when creating ANO mapping identifiers.  This will directly impact the number of possible values that can be generated and therefore
    /// the number of unique input values before anonymising fails (due to collisions).
    /// </summary>
    public int NumberOfCharactersToUseInAnonymousRepresentation
    {
        get => _numberOfCharactersToUseInAnonymousRepresentation;
        set => SetField(ref _numberOfCharactersToUseInAnonymousRepresentation, value);
    }

    /// <summary>
    /// The ID of the ExternalDatabaseServer which stores the anonymous identifier substitutions (e.g. chi=>ANOchi).  This should have been created by the
    /// <see cref="ANOStorePatcher"/>
    /// </summary>
    [Relationship(typeof(ExternalDatabaseServer), RelationshipType.SharedObject)]
    public int Server_ID
    {
        get => _serverID;
        set => SetField(ref _serverID, value);
    }

    /// <summary>
    /// The letter that appears on the end of all anonymous identifiers generated e.g. AAB11_GP would have the suffix "GP"
    /// 
    /// <para>Once you have started using the <see cref="ANOTable"/> to anonymise identifiers you should not change the Suffix</para>
    /// </summary>
    public string Suffix
    {
        get => _suffix;
        set => SetField(ref _suffix, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="Server_ID"/>
    [NoMappingToDatabase]
    public ExternalDatabaseServer Server => Repository.GetObjectByID<ExternalDatabaseServer>(Server_ID);

    #endregion

    public ANOTable()
    {
        // Defaults
        NumberOfIntegersToUseInAnonymousRepresentation = 1;
        NumberOfCharactersToUseInAnonymousRepresentation = 1;
    }

    /// <summary>
    /// Declares that a new ANOTable (anonymous mapping table) should exist in the referenced database.  You can call this constructor without first creating the table.  If you do
    /// you should set <see cref="NumberOfIntegersToUseInAnonymousRepresentation"/> and <see cref="NumberOfCharactersToUseInAnonymousRepresentation"/> then <see cref="PushToANOServerAsNewTable"/>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="externalDatabaseServer"></param>
    /// <param name="tableName"></param>
    /// <param name="suffix"></param>
    public ANOTable(ICatalogueRepository repository, ExternalDatabaseServer externalDatabaseServer, string tableName,
        string suffix)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new NullReferenceException("ANOTable must have a name");

        // Defaults
        NumberOfIntegersToUseInAnonymousRepresentation = 1;
        NumberOfCharactersToUseInAnonymousRepresentation = 1;

        if (repository.GetAllObjects<ANOTable>().Any(a => string.Equals(a.Suffix, suffix)))
            throw new Exception($"There is already another {nameof(ANOTable)} with the suffix '{suffix}'");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "TableName", tableName },
            { "Suffix", suffix },
            { "Server_ID", externalDatabaseServer.ID }
        });
    }

    internal ANOTable(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Server_ID = Convert.ToInt32(r["Server_ID"]);
        TableName = r["TableName"].ToString();

        NumberOfIntegersToUseInAnonymousRepresentation =
            Convert.ToInt32(r["NumberOfIntegersToUseInAnonymousRepresentation"].ToString());
        NumberOfCharactersToUseInAnonymousRepresentation =
            Convert.ToInt32(r["NumberOfCharactersToUseInAnonymousRepresentation"].ToString());
        Suffix = r["Suffix"].ToString();
    }

    internal ANOTable(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    /// <summary>
    /// Saves the current state to the database if the <see cref="ANOTable"/> is in a valid state according to <see cref="Check"/> otherwise throws an Exception
    /// </summary>
    public override void SaveToDatabase()
    {
        Check(ThrowImmediatelyCheckNotifier.Quiet);
        Repository.SaveToDatabase(this);
    }

    /// <summary>
    /// Attempts to delete the remote mapping table (only works if it is empty) if the <see cref="ANOTable.IsTablePushed"/> then deletes the <see cref="ANOTable"/> reference
    /// object (this) from the RDMP platform database.
    /// </summary>
    public override void DeleteInDatabase()
    {
        DeleteANOTableInANOStore();
        Repository.DeleteFromDatabase(this);
    }

    /// <inheritdoc/>
    public override string ToString() => TableName;

    /// <summary>
    /// Checks that the remote mapping table referenced by this object exists and checks <see cref="ANOTable"/> settings (<see cref="Suffix"/> etc).
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(Suffix))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "You must choose a suffix for your ANO identifiers so that they can be distinguished from regular identifiers",
                    CheckResult.Fail));
        else if (Suffix.StartsWith('_'))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Suffix will automatically include an underscore, there is no need to add it", CheckResult.Fail));

        if (NumberOfIntegersToUseInAnonymousRepresentation < 0)
            notifier.OnCheckPerformed(
                new CheckEventArgs("NumberOfIntegersToUseInAnonymousRepresentation cannot be negative",
                    CheckResult.Fail));

        if (NumberOfCharactersToUseInAnonymousRepresentation < 0)
            notifier.OnCheckPerformed(
                new CheckEventArgs("NumberOfCharactersToUseInAnonymousRepresentation cannot be negative",
                    CheckResult.Fail));

        if (NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation == 0)
            notifier.OnCheckPerformed(
                new CheckEventArgs("Anonymous representations must have at least 1 integer or character",
                    CheckResult.Fail));

        try
        {
            if (!IsTablePushed())
                notifier.OnCheckPerformed(new CheckEventArgs($"Could not find table {TableName} on server {Server}",
                    CheckResult.Warning));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Failed to get list of tables on server {Server}",
                CheckResult.Fail, e));
        }
    }

    /// <summary>
    /// Returns true if the anonymous mapping table (<see cref="TableName"/> exists in the referenced mapping database (<see cref="Server"/>)
    /// </summary>
    /// <returns></returns>
    public bool IsTablePushed() => GetPushedTable() != null;

    /// <summary>
    /// Connects to <see cref="Server"/> and returns a <see cref="DiscoveredTable"/> that contains the anonymous identifier mappings
    /// </summary>
    /// <returns></returns>
    public DiscoveredTable GetPushedTable()
    {
        if (!Server.WasCreatedBy(new ANOStorePatcher()))
            throw new Exception($"ANOTable's Server '{Server}' is not an ANOStore.  ANOTable was '{this}'");

        var tables = DataAccessPortal
            .ExpectDatabase(Server, DataAccessContext.DataLoad)
            .DiscoverTables(false);

        return tables.SingleOrDefault(t => t.GetRuntimeName().Equals(TableName));
    }

    /// <summary>
    /// Attempts to delete the anonymous mapping table referenced by <see cref="TableName"/> on the mapping <see cref="Server"/>.  This is safer than just dropping
    /// from <see cref="GetPushedTable"/> since it will check the table exists, is empty etc.
    /// </summary>
    public void DeleteANOTableInANOStore()
    {
        RevertToDatabaseState();

        var s = Server;
        if (string.IsNullOrWhiteSpace(s.Name) || string.IsNullOrWhiteSpace(s.Database) ||
            string.IsNullOrWhiteSpace(TableName))
            return;

        var tbl = GetPushedTable();

        if (tbl?.Exists() == true)
            if (!tbl.IsEmpty())
                throw new Exception(
                    $"Cannot delete ANOTable because it references {TableName} which is a table on server {Server} which contains rows, deleting this reference would leave that table as an orphan, we can only delete when there are 0 rows in the table");
            else
                tbl.Drop();
    }

    /// <summary>
    /// Connects to the remote ANO Server and creates a swap table of Identifier to ANOIdentifier
    /// </summary>
    /// <param name="identifiableDatatype">The datatype of the identifiable data table</param>
    /// <param name="notifier"></param>
    /// <param name="forceConnection"></param>
    /// <param name="forceTransaction"></param>
    public void PushToANOServerAsNewTable(string identifiableDatatype, ICheckNotifier notifier,
        DbConnection forceConnection = null, DbTransaction forceTransaction = null)
    {
        var server = DataAccessPortal.ExpectServer(Server, DataAccessContext.DataLoad);

        //matches varchar(100) and has capture group 100
        var regexGetLengthOfCharType = new Regex(@".*char.*\((\d*)\)");
        var match = regexGetLengthOfCharType.Match(identifiableDatatype);

        //if user supplies varchar(100) and says he wants 3 ints and 3 chars in his anonymous identifiers he will soon run out of combinations

        if (match.Success)
        {
            var length = Convert.ToInt32(match.Groups[1].Value);

            if (length >
                NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"You asked to create a table with a datatype of length {length}({identifiableDatatype}) but you did not allocate an equal or greater number of anonymous identifier types (NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation={NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation})",
                        CheckResult.Warning));
        }

        var con = forceConnection ?? server.GetConnection(); //use the forced connection or open a new one

        try
        {
            if (forceConnection == null)
                con.Open();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Could not connect to ano server {Server}", CheckResult.Fail,
                e));
            return;
        }

        //if table name is ANOChi there are 2 columns Chi and ANOChi in it
        var anonymousColumnName = TableName;
        var identifiableColumnName = TableName["ANO".Length..];

        var anonymousDatatype =
            $"varchar({NumberOfCharactersToUseInAnonymousRepresentation + NumberOfIntegersToUseInAnonymousRepresentation + "_".Length + Suffix.Length})";


        var sql =
            $"CREATE TABLE {TableName}{Environment.NewLine} ({Environment.NewLine}{identifiableColumnName} {identifiableDatatype} NOT NULL,{Environment.NewLine}{anonymousColumnName} {anonymousDatatype}NOT NULL";

        sql += $@",
CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED 
(
        {identifiableColumnName} ASC
),
CONSTRAINT AK_{TableName} UNIQUE({anonymousColumnName})
)";


        using (var cmd = server.GetCommand(sql, con))
        {
            cmd.Transaction = forceTransaction;

            notifier.OnCheckPerformed(new CheckEventArgs($"Decided appropriate create statement is:{cmd.CommandText}",
                CheckResult.Success));
            try
            {
                cmd.ExecuteNonQuery();

                if (forceConnection == null) //if we opened this ourselves
                    con.Close(); //shut it
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Failed to successfully create the anonymous/identifier mapping Table in the ANO database on server {Server}",
                        CheckResult.Fail, e));
                return;
            }
        }

        try
        {
            if (forceTransaction ==
                null) //if there was no transaction then this has hit the LIVE ANO database and is for real, so save the ANOTable such that it is synchronized with reality
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Saving state because table has been pushed",
                    CheckResult.Success));
                SaveToDatabase();
            }
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Failed to save state after table was successfully? pushed to ANO server", CheckResult.Fail, e));
        }
    }


    /// <summary>
    /// Anonymisation with an <see cref="ANOTable"/> happens during data load.  This means that the column goes from identifiable in RAW to anonymous in STAGING/LIVE.  This means
    /// that the datatype of the column changes depending on the <see cref="LoadStage"/>.
    /// 
    /// <para>Returns the appropriate datatype for the <see cref="LoadStage"/>.  This is done by connecting to the mapping table and retrieving the mapping table types</para>
    /// </summary>
    /// <param name="loadStage"></param>
    /// <returns></returns>
    public string GetRuntimeDataType(LoadStage loadStage)
    {
        //cache answers
        if (_identifiableDataType == null)
        {
            var server = DataAccessPortal.ExpectServer(Server, DataAccessContext.DataLoad);

            var columnsFoundInANO = server.GetCurrentDatabase().ExpectTable(TableName).DiscoverColumns();

            var expectedIdentifiableName = TableName["ANO".Length..];

            var anonymous = columnsFoundInANO.SingleOrDefault(c => c.GetRuntimeName().Equals(TableName));
            var identifiable =
                columnsFoundInANO.SingleOrDefault(c => c.GetRuntimeName().Equals(expectedIdentifiableName));

            if (anonymous == null)
                throw new Exception(
                    $"Could not find a column called {TableName} in table {TableName} on server {Server} (Columns found were {string.Join(",", columnsFoundInANO.Select(static c => c.GetRuntimeName()).ToArray())})");

            if (identifiable == null)
                throw new Exception(
                    $"Could not find a column called {expectedIdentifiableName} in table {TableName} on server {Server} (Columns found were {string.Join(",", columnsFoundInANO.Select(static c => c.GetRuntimeName()).ToArray())})");

            _identifiableDataType = identifiable.DataType.SQLType;
            _anonymousDataType = anonymous.DataType.SQLType;
        }

        //return cached answer
        return loadStage switch
        {
            LoadStage.GetFiles => _identifiableDataType,
            LoadStage.Mounting => _identifiableDataType,
            LoadStage.AdjustRaw => _identifiableDataType,
            LoadStage.AdjustStaging => _anonymousDataType,
            LoadStage.PostLoad => _anonymousDataType,
            _ => throw new ArgumentOutOfRangeException(nameof(loadStage))
        };
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn() => Array.Empty<IHasDependencies>();

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => Repository.GetAllObjectsWithParent<ColumnInfo>(this);
}