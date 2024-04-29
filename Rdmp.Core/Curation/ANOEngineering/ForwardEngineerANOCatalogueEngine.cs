// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.Sharing.Refactoring;

namespace Rdmp.Core.Curation.ANOEngineering;

/// <summary>
/// Creates a new 'anonymous' version of a Catalogue based on a configuration the user has set up in a ForwardEngineerANOCataloguePlanManager.  This involves creating
/// a new empty data table in the destination database (adjusted to accomodate anonymous datatypes / dropped columns etc), importing the empty table as a new
/// TableInfo(s) and creating a new Catalogue entry.  Since Catalogues can have multiple underlying tables (e.g. lookup tables shared join tables etc) the engine
/// supports migrating only a subset of tables across (the remaining tables must have already been migrated and exist in the destination database).
/// 
/// <para>Finally the engine creates a LoadMetadata which when run will migrate (copy) the data from the old table into the new table.</para>
/// </summary>
public class ForwardEngineerANOCatalogueEngine
{
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly ForwardEngineerANOCataloguePlanManager _planManager;
    public ICatalogue NewCatalogue { get; private set; }
    public LoadMetadata LoadMetadata { get; private set; }
    public LoadProgress LoadProgressIfAny { get; set; }

    public Dictionary<ITableInfo, QueryBuilder> SelectSQLForMigrations = new();
    public Dictionary<PreLoadDiscardedColumn, IDilutionOperation> DilutionOperationsForMigrations = new();

    private ShareManager _shareManager;

    private ColumnInfo[] _allColumnsInfos;

    public ForwardEngineerANOCatalogueEngine(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ForwardEngineerANOCataloguePlanManager planManager)
    {
        _catalogueRepository = repositoryLocator.CatalogueRepository;
        _shareManager = new ShareManager(repositoryLocator);
        _planManager = planManager;
        _allColumnsInfos = _catalogueRepository.GetAllObjects<ColumnInfo>();
    }

    public void Execute()
    {
        if (_planManager.TargetDatabase == null)
            throw new Exception("PlanManager has no TargetDatabase set");

        var memoryRepo = new MemoryCatalogueRepository();

        using (_catalogueRepository.BeginNewTransaction())
        {
            try
            {
                //for each skipped table
                foreach (var skippedTable in _planManager.SkippedTables)
                    //we might have to refactor or port JoinInfos to these tables so we should establish what the parenthood of them was
                    foreach (var columnInfo in skippedTable.ColumnInfos)
                        GetNewColumnInfoForOld(columnInfo, true);

                //for each table that isn't being skipped
                foreach (var oldTableInfo in _planManager.TableInfos.Except(_planManager.SkippedTables))
                {
                    var columnsToCreate = new List<DatabaseColumnRequest>();

                    var migratedColumns = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);

                    var querybuilderForMigratingTable = new QueryBuilder(null, null);

                    //for each column we are not skipping (Drop) work out the endpoint datatype (planner knows this)
                    foreach (var columnInfo in oldTableInfo.ColumnInfos)
                    {
                        var columnPlan = _planManager.GetPlanForColumnInfo(columnInfo);

                        if (columnPlan.Plan != Plan.Drop)
                        {
                            //add the column verbatim to the query builder because we know we have to read it from source
                            querybuilderForMigratingTable.AddColumn(new ColumnInfoToIColumn(memoryRepo, columnInfo));

                            var colName = columnInfo.GetRuntimeName();

                            //if it is being ano tabled then give the table name ANO as a prefix
                            if (columnPlan.Plan == Plan.ANO)
                                colName = $"ANO{colName}";

                            migratedColumns.Add(colName, columnInfo);

                            columnsToCreate.Add(
                                new DatabaseColumnRequest(colName, columnPlan.GetEndpointDataType(),
                                    !columnInfo.IsPrimaryKey)
                                { IsPrimaryKey = columnInfo.IsPrimaryKey });
                        }
                    }

                    SelectSQLForMigrations.Add(oldTableInfo, querybuilderForMigratingTable);

                    //Create the actual table
                    var tbl = _planManager.TargetDatabase.CreateTable(oldTableInfo.GetRuntimeName(),
                        columnsToCreate.ToArray());

                    //import the created table
                    var importer = new TableInfoImporter(_catalogueRepository, tbl);
                    importer.DoImport(out var newTableInfo, out var newColumnInfos);

                    //Audit the parenthood of the TableInfo/ColumnInfos
                    AuditParenthood(oldTableInfo, newTableInfo);

                    foreach (var newColumnInfo in newColumnInfos)
                    {
                        var oldColumnInfo = migratedColumns[newColumnInfo.GetRuntimeName()];

                        var columnPlan = _planManager.GetPlanForColumnInfo(oldColumnInfo);

                        if (columnPlan.Plan == Plan.ANO)
                        {
                            newColumnInfo.ANOTable_ID = columnPlan.ANOTable.ID;
                            newColumnInfo.SaveToDatabase();
                        }

                        //if there was a dilution configured we need to setup a virtual DLE load only column of the input type (this ensures RAW has a valid datatype)
                        if (columnPlan.Plan == Plan.Dilute)
                        {
                            //Create a discarded (load only) column with name matching the new columninfo
                            var discard = new PreLoadDiscardedColumn(_catalogueRepository, newTableInfo,
                                newColumnInfo.GetRuntimeName())
                            {
                                //record that it exists to support dilution and that the data type matches the input (old) ColumnInfo (i.e. not the new data type!)
                                Destination = DiscardedColumnDestination.Dilute,
                                SqlDataType = oldColumnInfo.Data_type
                            };

                            discard.SaveToDatabase();

                            DilutionOperationsForMigrations.Add(discard, columnPlan.Dilution);
                        }

                        AuditParenthood(oldColumnInfo, newColumnInfo);
                    }

                    if (DilutionOperationsForMigrations.Any())
                    {
                        newTableInfo.IdentifierDumpServer_ID = _planManager.GetIdentifierDumpServer().ID;
                        newTableInfo.SaveToDatabase();
                    }
                }

                NewCatalogue = _planManager.Catalogue.ShallowClone();
                NewCatalogue.Name = $"ANO{_planManager.Catalogue.Name}";
                NewCatalogue.Folder = $"\\anonymous{NewCatalogue.Folder}";
                NewCatalogue.SaveToDatabase();

                AuditParenthood(_planManager.Catalogue, NewCatalogue);

                //For each of the old ExtractionInformations (95% of the time that's just a reference to a ColumnInfo e.g. '[People].[Height]' but 5% of the time it's some horrible aliased transform e.g. 'dbo.RunMyCoolFunction([People].[Height]) as BigHeight'
                foreach (var oldCatalogueItem in _planManager.Catalogue.CatalogueItems)
                {
                    var oldColumnInfo = oldCatalogueItem.ColumnInfo;

                    //catalogue item is not connected to any ColumnInfo
                    if (oldColumnInfo == null)
                        continue;

                    var columnPlan = _planManager.GetPlanForColumnInfo(oldColumnInfo);

                    //we are not migrating it anyway
                    if (columnPlan.Plan == Plan.Drop)
                        continue;

                    var newColumnInfo = GetNewColumnInfoForOld(oldColumnInfo);

                    var newCatalogueItem = oldCatalogueItem.ShallowClone(NewCatalogue);

                    //and rewire its ColumnInfo to the cloned child one
                    newCatalogueItem.ColumnInfo_ID = newColumnInfo.ID;

                    //If the old CatalogueItem had the same name as its underlying ColumnInfo then we should use the new one otherwise just copy the old name whatever it was
                    newCatalogueItem.Name = oldCatalogueItem.Name.Equals(oldColumnInfo.Name)
                        ? newColumnInfo.GetRuntimeName()
                        : oldCatalogueItem.Name;

                    //add ANO to the front if the underlying column was annoed
                    if (newColumnInfo.GetRuntimeName().StartsWith("ANO") && !newCatalogueItem.Name.StartsWith("ANO"))
                        newCatalogueItem.Name = $"ANO{newCatalogueItem.Name}";

                    newCatalogueItem.SaveToDatabase();

                    var oldExtractionInformation = oldCatalogueItem.ExtractionInformation;

                    //if the plan is to make the ColumnInfo extractable
                    if (columnPlan.ExtractionCategoryIfAny != null)
                    {
                        //Create a new ExtractionInformation for the new Catalogue
                        var newExtractionInformation = new ExtractionInformation(_catalogueRepository, newCatalogueItem,
                            newColumnInfo, newColumnInfo.Name)
                        {
                            ExtractionCategory = columnPlan.ExtractionCategoryIfAny.Value,
                        };

                        newExtractionInformation.SaveToDatabase();

                        //if it was previously extractable
                        if (oldExtractionInformation != null)
                        {
                            var refactorer = new SelectSQLRefactorer();

                            //restore the old SQL as it existed in the origin table
                            newExtractionInformation.SelectSQL = oldExtractionInformation.SelectSQL;

                            //do a refactor on the old column name for the new column name
                            SelectSQLRefactorer.RefactorColumnName(newExtractionInformation, oldColumnInfo,
                                newColumnInfo.Name, true);

                            //also refactor any other column names that might be referenced by the transform SQL e.g. it could be a combo column name where forename + surname is the value of the ExtractionInformation
                            foreach (var kvpOtherCols in _parenthoodDictionary.Where(kvp => kvp.Key is ColumnInfo))
                            {
                                //if it's one we have already done, don't do it again
                                if (Equals(kvpOtherCols.Value, newColumnInfo))
                                    continue;

                                //otherwise do a non strict refactoring (don't worry if you don't finda ny references)
                                SelectSQLRefactorer.RefactorColumnName(newExtractionInformation,
                                    (ColumnInfo)kvpOtherCols.Key, ((ColumnInfo)kvpOtherCols.Value).Name, false);
                            }

                            //make the new one exactly as extractable
                            newExtractionInformation.Order = oldExtractionInformation.Order;
                            newExtractionInformation.Alias = oldExtractionInformation.Alias;
                            newExtractionInformation.IsExtractionIdentifier =
                                oldExtractionInformation.IsExtractionIdentifier;
                            newExtractionInformation.HashOnDataRelease = oldExtractionInformation.HashOnDataRelease;
                            newExtractionInformation.IsPrimaryKey = oldExtractionInformation.IsPrimaryKey;
                            newExtractionInformation.SaveToDatabase();
                        }

                        AuditParenthood(oldCatalogueItem, newCatalogueItem);

                        if (oldExtractionInformation != null)
                            AuditParenthood(oldExtractionInformation, newExtractionInformation);
                    }
                }

                var existingJoinInfos = _catalogueRepository.GetAllObjects<JoinInfo>();
                var existingLookups = _catalogueRepository.GetAllObjects<Lookup>();
                var existingLookupComposites = _catalogueRepository.GetAllObjects<LookupCompositeJoinInfo>();

                //migrate join infos
                foreach (var joinInfo in _planManager.GetJoinInfosRequiredCatalogue())
                {
                    var newFk = GetNewColumnInfoForOld(joinInfo.ForeignKey);
                    var newPk = GetNewColumnInfoForOld(joinInfo.PrimaryKey);

                    //already exists
                    if (!existingJoinInfos.Any(ej => ej.ForeignKey_ID == newFk.ID && ej.PrimaryKey_ID == newPk.ID))
                        new JoinInfo(_catalogueRepository, newFk, newPk, joinInfo.ExtractionJoinType,
                            joinInfo.Collation); //create it
                }

                //migrate Lookups
                foreach (var lookup in _planManager.GetLookupsRequiredCatalogue())
                {
                    //Find the new columns in the ANO table that match the old lookup columns
                    var newDesc = GetNewColumnInfoForOld(lookup.Description);
                    var newFk = GetNewColumnInfoForOld(lookup.ForeignKey);
                    var newPk = GetNewColumnInfoForOld(lookup.PrimaryKey);

                    //see if we already have a Lookup declared for the NEW columns (unlikely)
                    var newLookup =
                        existingLookups.SingleOrDefault(l =>
                            l.Description_ID == newDesc.ID && l.ForeignKey_ID == newFk.ID) ??
                        new Lookup(_catalogueRepository, newDesc, newFk, newPk, lookup.ExtractionJoinType,
                            lookup.Collation);

                    //also mirror any composite (secondary, tertiary join column pairs needed for the Lookup to operate correclty e.g. where TestCode 'HAB1' means 2 different things depending on healthboard)
                    foreach (var compositeJoin in lookup.GetSupplementalJoins().Cast<LookupCompositeJoinInfo>())
                    {
                        var newCompositeFk = GetNewColumnInfoForOld(compositeJoin.ForeignKey);
                        var newCompositePk = GetNewColumnInfoForOld(compositeJoin.PrimaryKey);

                        if (!existingLookupComposites.Any(c =>
                                c.ForeignKey_ID == newCompositeFk.ID && c.PrimaryKey_ID == newCompositePk.ID))
                            new LookupCompositeJoinInfo(_catalogueRepository, newLookup, newCompositeFk, newCompositePk,
                                compositeJoin.Collation);
                    }
                }

                //create new data load confguration
                LoadMetadata = new LoadMetadata(_catalogueRepository, $"Anonymising {NewCatalogue}");
                LoadMetadata.EnsureLoggingWorksFor(NewCatalogue);
                NewCatalogue.SaveToDatabase();
                LoadMetadata.LinkToCatalogue(NewCatalogue);
                if (_planManager.DateColumn != null)
                {
                    LoadProgressIfAny = new LoadProgress(_catalogueRepository, LoadMetadata)
                    {
                        OriginDate = _planManager.StartDate
                    };
                    LoadProgressIfAny.SaveToDatabase();

                    //date column based migration only works for single TableInfo migrations (see Plan Manager checks)
                    var qb = SelectSQLForMigrations.Single(kvp => !kvp.Key.IsLookupTable()).Value;
                    qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(memoryRepo, null,
                        new[]
                        {
                            new SpontaneouslyInventedFilter(memoryRepo, null,
                                $"{_planManager.DateColumn} >= @startDate", "After batch start date", "", null),
                            new SpontaneouslyInventedFilter(memoryRepo, null, $"{_planManager.DateColumn} <= @endDate",
                                "Before batch end date", "", null)
                        }
                        , FilterContainerOperation.AND);
                }

                try
                {
                    foreach (var qb in SelectSQLForMigrations.Values)
                        Console.WriteLine(qb.SQL);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to generate migration SQL", e);
                }

                _catalogueRepository.EndTransaction(true);
            }
            catch (Exception ex)
            {
                _catalogueRepository.EndTransaction(false);
                throw new Exception("Failed to create ANO version, transaction rolled back successfully", ex);
            }
        }
    }

    /// <summary>
    /// Returns the newly created / already existing NEW ANO column info when passed the old (identifiable original) ColumnInfo
    /// </summary>
    /// <param name="col"></param>
    /// <param name="isOptional"></param>
    /// <returns></returns>
    private ColumnInfo GetNewColumnInfoForOld(ColumnInfo col, bool isOptional = false)
    {
        //it's one we migrated ourselves
        if (_parenthoodDictionary.TryGetValue(col, out var value))
            return (ColumnInfo)value;

        //it's one that was already existing before we did ANO migration e.g. a SkippedTableInfo (this can happen when there are 2+ tables underlying a Catalogue and you have already ANO one of those Tables previously (e.g. when it is a shared table with other Catalogues)

        //find a reference to the new ColumnInfo Location (note that it is possible the TableInfo was skipped, in which case we should still expect to find ColumnInfos that reference the new location because you must have created it somehow right?)
        var syntaxHelper = _planManager.TargetDatabase.Server.GetQuerySyntaxHelper();

        var toReturn = FindNewColumnNamed(syntaxHelper, col, col.GetRuntimeName(), isOptional) ??
                       FindNewColumnNamed(syntaxHelper, col, $"ANO{col.GetRuntimeName()}", isOptional);

        if (toReturn == null)
            return isOptional
                ? null
                : throw new Exception(
                    $"Catalogue '{_planManager.Catalogue}' contained a CatalogueItem referencing Column '{col}' the ColumnInfo was not migrated (which is fine) but we then could not find ColumnInfo in the new ANO dataset (if it was part of SkippedTables why doesn't the Catalogue have a reference to the new location?)");

        _parenthoodDictionary.Add(col, toReturn);

        return toReturn;
    }

    /// <summary>
    /// Here we are migrating a Catalogue but some of the TableInfos have already been migrated e.g. lookup tables as part of migrating another Catalogue.  We are
    /// now trying to find one of those 'not migrated' ColumnInfos by name without knowing whether the user has since deleted the reference or worse introduced
    /// duplicate references to the same TableInfo/ColumnInfos.
    /// </summary>
    /// <param name="syntaxHelper"></param>
    /// <param name="col"></param>
    /// <param name="expectedName"></param>
    /// <param name="isOptional"></param>
    /// <returns></returns>
    private ColumnInfo FindNewColumnNamed(IQuerySyntaxHelper syntaxHelper, ColumnInfo col, string expectedName,
        bool isOptional)
    {
        var expectedNewNames = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
        {
            //look for it in the default schema ".." or the API default server ".dbo." or the original table schema.
            syntaxHelper.EnsureFullyQualified(
                _planManager.TargetDatabase.GetRuntimeName(),
                null,
                col.TableInfo.GetRuntimeName(),
                expectedName),
            syntaxHelper.EnsureFullyQualified(
                _planManager.TargetDatabase.GetRuntimeName(),
                syntaxHelper.GetDefaultSchemaIfAny(),
                col.TableInfo.GetRuntimeName(),
                expectedName),
            syntaxHelper.EnsureFullyQualified(
                _planManager.TargetDatabase.GetRuntimeName(),
                col.TableInfo.Schema,
                col.TableInfo.GetRuntimeName(),
                expectedName)
        };


        var columns = _allColumnsInfos.Where(c => expectedNewNames.Contains(c.Name)).ToArray();

        var failedANOToo = false;

        //maybe it was anonymised in the other configuration?
        if (columns.Length == 0 && !expectedName.StartsWith("ANO", StringComparison.CurrentCultureIgnoreCase))
            try
            {
                return FindNewColumnNamed(syntaxHelper, col, $"ANO{expectedName}", isOptional);
            }
            catch (Exception)
            {
                //oh well couldnt find it
                failedANOToo = true;
            }

        if (columns.Length == 1)
            return columns[0];

        var columnsFromCorrectServer =
            columns.Where(c => c.TableInfo.Server.Equals(_planManager.TargetDatabase.Server.Name)).ToArray();

        if (columnsFromCorrectServer.Length == 1)
            return columnsFromCorrectServer[0];

        var columnsFromCorrectServerThatAreaAlsoLocalImports =
            columnsFromCorrectServer.Where(_shareManager.IsImportedObject).ToArray();

        if (columnsFromCorrectServerThatAreaAlsoLocalImports.Length == 1)
            return columnsFromCorrectServerThatAreaAlsoLocalImports[0];

        return isOptional
            ? null
            : throw new Exception(
                $"Found '{columns.Length}' ColumnInfos called '{expectedNewNames.First()}'{(failedANOToo ? $" (Or 'ANO{expectedName}')" : "")}");
    }

    private Dictionary<IMapsDirectlyToDatabaseTable, IMapsDirectlyToDatabaseTable> _parenthoodDictionary = new();


    private void AuditParenthood(IMapsDirectlyToDatabaseTable parent, IMapsDirectlyToDatabaseTable child)
    {
        //make it shareable
        var export = _shareManager.GetNewOrExistingExportFor(parent);

        //share it to yourself where the child is the realisation of the share (this creates relationship in database)
        _shareManager.GetImportAs(export.SharingUID, child);

        //record in memory dictionary
        _parenthoodDictionary.Add(parent, child);
    }
}