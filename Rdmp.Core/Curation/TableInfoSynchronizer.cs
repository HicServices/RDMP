// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation;

/// <summary>
///     Synchronizes a <see cref="TableInfo" /> against the live table on your database server.  This involves confirming
///     it still exists, identifying new ColumnInfos and ones that have
///     disapeared as well as checking column types and primary keys etc still match the current RDMP records.
/// </summary>
public class TableInfoSynchronizer
{
    private readonly ITableInfo _tableToSync;
    private readonly DiscoveredServer _toSyncTo;
    private readonly ICatalogueRepository _repository;

    public HashSet<Catalogue> ChangedCatalogues = new();

    /// <summary>
    ///     Synchronizes the TableInfo against the underlying database to ensure the Catalogues understanding of what columns
    ///     exist, what are primary keys,
    ///     collation types etc match the reality.  Pass in an alternative
    /// </summary>
    /// <param name="tableToSync"></param>
    public TableInfoSynchronizer(ITableInfo tableToSync)
    {
        _tableToSync = tableToSync;
        _repository = _tableToSync.CatalogueRepository;

        _toSyncTo = DataAccessPortal.ExpectServer(tableToSync, DataAccessContext.InternalDataProcessing);
    }

    /// <summary>
    /// </summary>
    /// <exception cref="SynchronizationFailedException">
    ///     Could not figure out how to resolve a synchronization problem between
    ///     the TableInfo and the underlying table structure
    /// </exception>
    /// <param name="notifier">
    ///     Called every time a fixable problem is detected, method must return true or false.  True = apply
    ///     fix, False = don't - but carry on checking
    /// </param>
    public bool Synchronize(ICheckNotifier notifier)
    {
        var IsSynched = true;

        //server exists and is accessible?
        try
        {
            _toSyncTo.TestConnection();
        }
        catch (Exception e)
        {
            throw new SynchronizationFailedException($"Could not connect to {_toSyncTo}", e);
        }

        //database exists?
        var expectedDatabase = _toSyncTo.ExpectDatabase(_tableToSync.GetDatabaseRuntimeName());
        if (!expectedDatabase.Exists())
            throw new SynchronizationFailedException(
                $"Server did not contain a database called {_tableToSync.GetDatabaseRuntimeName()}");

        //identify new columns
        DiscoveredColumn[] liveColumns;
        DiscoveredTable expectedTable;

        if (_tableToSync.IsTableValuedFunction)
        {
            expectedTable =
                expectedDatabase.ExpectTableValuedFunction(_tableToSync.GetRuntimeName(), _tableToSync.Schema);
            if (!expectedTable.Exists())
                throw new SynchronizationFailedException(
                    $"Database {expectedDatabase} did not contain a TABLE VALUED FUNCTION called {_tableToSync.GetRuntimeName()}");
        }
        else
        {
            //table exists?
            expectedTable = expectedDatabase.ExpectTable(_tableToSync.GetRuntimeName(), _tableToSync.Schema,
                _tableToSync.IsView ? TableType.View : TableType.Table);
            if (!expectedTable.Exists())
                throw new SynchronizationFailedException(
                    $"Database {expectedDatabase} did not contain a {(_tableToSync.IsView ? "view" : "table")} called {_tableToSync.GetRuntimeName()} (make sure you have marked whether it is a table/view and that it exists in your database)");
        }

        try
        {
            liveColumns = expectedTable.DiscoverColumns();
        }
        catch (SqlException e)
        {
            throw new Exception(
                $"Failed to enumerate columns in {_toSyncTo} (we were attempting to synchronize the TableInfo {_tableToSync} (ID={_tableToSync.ID}).  Check the inner exception for specifics",
                e);
        }

        var catalogueColumns = _tableToSync.ColumnInfos.ToArray();


        var credentialsIfExists = _tableToSync.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
        string pwd = null;
        string usr = null;
        if (credentialsIfExists != null)
        {
            usr = credentialsIfExists.Username;
            pwd = credentialsIfExists.GetDecryptedPassword();
        }

        ITableInfoImporter importer;

        //for importing new stuff
        if (_tableToSync.IsTableValuedFunction)
            importer = new TableValuedFunctionImporter(_repository, (DiscoveredTableValuedFunction)expectedTable);
        else
            importer = new TableInfoImporter(_repository, _toSyncTo.Name,
                _toSyncTo.GetCurrentDatabase().GetRuntimeName(), _tableToSync.GetRuntimeName(),
                _tableToSync.DatabaseType, usr, pwd, importFromSchema: _tableToSync.Schema,
                importTableType: _tableToSync.IsView ? TableType.View : TableType.Table);

        var newColumnsInLive =
            liveColumns.Where(
                live => !catalogueColumns.Any(columnInfo =>
                    columnInfo.GetRuntimeName()
                        .Equals(live.GetRuntimeName()))).ToArray();

        //there are new columns in the live database that are not in the Catalogue
        if (newColumnsInLive.Any())
        {
            //see if user wants to add missing columns
            var addMissingColumns = notifier.OnCheckPerformed(new CheckEventArgs(
                $"The following columns are missing from the TableInfo:{string.Join(",", newColumnsInLive.Select(c => c.GetRuntimeName()))}",
                CheckResult.Fail, null, "The ColumnInfos will be created and added to the TableInfo"));

            var added = new List<ColumnInfo>();

            if (addMissingColumns)
            {
                foreach (var missingColumn in newColumnsInLive)
                    added.Add(importer.CreateNewColumnInfo(_tableToSync, missingColumn));

                ForwardEngineerExtractionInformationIfAppropriate(added, notifier);
            }
            else
            {
                IsSynched = false;
            }
        }

        //See if we need to delete any ColumnInfos
        var columnsInCatalogueButSinceDisapeared =
            catalogueColumns
                .Where(columnInfo => !liveColumns.Any( //there are not any
                        c => columnInfo.GetRuntimeName()
                            .Equals(c.GetRuntimeName())) //columns with the same name between discovery/columninfo
                ).ToArray();

        if (columnsInCatalogueButSinceDisapeared.Any())
            foreach (var columnInfo in columnsInCatalogueButSinceDisapeared)
            {
                var deleteExtraColumnInfos = notifier.OnCheckPerformed(new CheckEventArgs(
                    $"The ColumnInfo {columnInfo.GetRuntimeName()} no longer appears in the live table.",
                    CheckResult.Fail, null,
                    $"Delete ColumnInfo {columnInfo.GetRuntimeName()}"));
                if (deleteExtraColumnInfos)
                    columnInfo.DeleteInDatabase();
                else
                    IsSynched = false;
            }

        _tableToSync.ClearAllInjections();

        if (IsSynched)
            IsSynched = SynchronizeTypes(notifier, liveColumns);

        if (IsSynched && !_tableToSync.IsTableValuedFunction) //table valued functions don't have primary keys!
            IsSynched = SynchronizeField(liveColumns, _tableToSync.ColumnInfos, notifier, "IsPrimaryKey");

        if (IsSynched && !_tableToSync.IsTableValuedFunction) //table valued functions don't have autonum
            IsSynched = SynchronizeField(liveColumns, _tableToSync.ColumnInfos, notifier, "IsAutoIncrement");

        if (IsSynched)
            IsSynched = SynchronizeField(liveColumns, _tableToSync.ColumnInfos, notifier, "Collation");

        if (IsSynched && _tableToSync.IsTableValuedFunction)
            IsSynched = SynchronizeParameters((TableValuedFunctionImporter)importer, notifier);

        _tableToSync.ClearAllInjections();

        //get list of primary keys from underlying table
        return IsSynched;
    }


    private void ForwardEngineerExtractionInformationIfAppropriate(List<ColumnInfo> added, ICheckNotifier notifier)
    {
        //Is there one Catalogue behind this dataset?
        var relatedCatalogues = _tableToSync.GetAllRelatedCatalogues();

        //if there is only one catalogue powered by this TableInfo
        if (relatedCatalogues.Length == 1)
            //And there are ExtractionInformations already for ColumnInfos in this _tableToSync
            if (relatedCatalogues[0].GetAllExtractionInformation(ExtractionCategory.Any).Any(e =>
                    e.ColumnInfo != null && e.ColumnInfo.TableInfo_ID == _tableToSync.ID))
                //And user wants to create new ExtractionInformations for the newly created sync'd ColumnInfos
                if (notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"Would you also like to make these columns Extractable in Catalogue {relatedCatalogues[0].Name}?",
                            CheckResult.Warning, null,
                            "Also make columns Extractable?")))
                {
                    //Create CatalogueItems for the new columns
                    var c = new ForwardEngineerCatalogue(_tableToSync, added.ToArray());

                    //In the Catalogue
                    c.ExecuteForwardEngineering(relatedCatalogues[0], out var cata, out var cis, out var eis);

                    //make them extractable only as internal since it is likely they could contain sensitive data if user is just used to hammering Ok on all dialogues
                    foreach (var e in eis)
                    {
                        e.ExtractionCategory = ExtractionCategory.Internal;
                        e.SaveToDatabase();
                    }

                    ChangedCatalogues.Add(relatedCatalogues[0]);
                }
    }

    private bool SynchronizeTypes(ICheckNotifier notifier, DiscoveredColumn[] liveColumns)
    {
        var IsSynched = true;

        foreach (var columnInfo in _tableToSync.ColumnInfos)
        {
            var liveState = liveColumns.Single(c => c.GetRuntimeName().Equals(columnInfo.GetRuntimeName()));

            //deal with mismatch in type
            if (!liveState.DataType.SQLType.Equals(columnInfo.Data_type))
                if (notifier.OnCheckPerformed(new CheckEventArgs(
                        $"ColumnInfo {{{columnInfo.Name}}} is type {liveState.DataType.SQLType} in the live database but in the Catalogue appears as {columnInfo.Data_type}",
                        CheckResult.Fail, null,
                        "Update type in Catalogue?")))
                {
                    columnInfo.Data_type = liveState.DataType.SQLType;
                    columnInfo.SaveToDatabase();
                }
                else
                {
                    IsSynched = false;
                }

            //if column has collation and live collation is not matching the one in the catalogue
            if (!string.IsNullOrWhiteSpace(liveState.Format) && !liveState.Format.Equals(columnInfo.Format))
                if (
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Mismatch between format in live of {liveState.Format} and Catalogue entry {columnInfo.Format}",
                        CheckResult.Fail, null, "Fix collation on ColumnInfo record to match live")))
                {
                    columnInfo.Format = liveState.Format;
                    columnInfo.SaveToDatabase();
                }
                else
                {
                    IsSynched = false;
                }
        }

        return IsSynched;
    }

    private static bool SynchronizeField(DiscoveredColumn[] liveColumns, ColumnInfo[] columnsInCatalogue,
        ICheckNotifier notifier, string property)
    {
        var IsSynched = true;

        var discoveredPropertyGetter = typeof(DiscoveredColumn).GetProperty(property);
        var cataloguePropertyGetter = typeof(ColumnInfo).GetProperty(property);

        foreach (var cataColumn in columnsInCatalogue)
        {
            var catalogueValue = cataloguePropertyGetter.GetValue(cataColumn);

            //find the corresponding DiscoveredColumn
            var matchingLiveColumn = liveColumns.Single(ci => ci.GetRuntimeName().Equals(cataColumn.GetRuntimeName()));
            var liveValue = discoveredPropertyGetter.GetValue(matchingLiveColumn);

            if (!Equals(catalogueValue, liveValue))
            {
                var fix = notifier.OnCheckPerformed(new CheckEventArgs(
                    $"{property} in ColumnInfo {cataColumn} is '{catalogueValue} but in live table it is '{liveValue}'",
                    CheckResult.Fail, null, "Update to live value?"));

                if (fix)
                {
                    cataloguePropertyGetter.SetValue(cataColumn, liveValue);
                    cataColumn.SaveToDatabase();
                }
                else
                {
                    IsSynched = false;
                }
            }
        }

        return IsSynched;
    }

    private bool SynchronizeParameters(TableValuedFunctionImporter importer, ICheckNotifier notifier)
    {
        var discoveredParameters = _toSyncTo.GetCurrentDatabase()
            .ExpectTableValuedFunction(_tableToSync.GetRuntimeName(), _tableToSync.Schema).DiscoverParameters();
        var currentParameters = _tableToSync.GetAllParameters();

        //For each parameter in underlying database
        foreach (var parameter in discoveredParameters)
        {
            var existingCatalogueReference =
                currentParameters.SingleOrDefault(p => p.ParameterName.Equals(parameter.ParameterName));
            if (existingCatalogueReference == null) // that is not known about by the TableInfo
            {
                var create = notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"TableInfo {_tableToSync} is a Table Valued Function but it does not have a record of the parameter {parameter.ParameterName} which appears in the underlying database",
                        CheckResult.Fail,
                        null, "Create the Parameter"));

                if (!create)
                    return false; //no longer synched

                importer.CreateParameter(_tableToSync, parameter);
            }
            else
            {
                //it is known about by the Catalogue but has it mysteriously changed datatype since it was imported / last synced?

                var dbDefinition = importer.GetParamaterDeclarationSQL(parameter);
                //if there is a disagreement on type etc
                if (existingCatalogueReference.ParameterSQL != dbDefinition)
                {
                    var modify =
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                $"Parameter {existingCatalogueReference.ParameterName} is declared as '{dbDefinition}' but in the Catalogue it appears as '{existingCatalogueReference.ParameterSQL}'",
                                CheckResult.Fail, null,
                                $"Change the definition in the Catalogue to '{dbDefinition}'"));

                    if (!modify)
                        return false;

                    existingCatalogueReference.ParameterSQL = dbDefinition;
                    existingCatalogueReference.SaveToDatabase();
                }
            }
        }

        //Find redundant parameters - parameters that the catalogue knows about but no longer appear in the table valued function signature in the database
        foreach (var currentParameter in currentParameters)
            if (!discoveredParameters.Any(p => p.ParameterName.Equals(currentParameter.ParameterName)))
            {
                var delete =
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"TableInfo {_tableToSync} is a Table Valued Function, in the Catalogue it has a parameter called {currentParameter.ParameterName} but this parameter no longer appears in the underlying database",
                            CheckResult.Fail,
                            null, $"Delete Parameter {currentParameter.ParameterName}"));

                if (!delete)
                    return false;

                ((IDeleteable)currentParameter).DeleteInDatabase();
            }

        return true;
    }
}