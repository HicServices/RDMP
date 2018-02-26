using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Refactoring;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.ANOEngineering
{
    /// <summary>
    /// Creates a new 'anonymous' version of a Catalogue based on a configuration the user has set up in a ForwardEngineerANOCataloguePlanManager.  This involves creating
    /// a new empty data table in the destination database (adjusted to accomodate anonymous datatypes / dropped columns etc), importing the empty table as a new
    /// TableInfo(s) and creating a new Catalogue entry.  Since Catalogues can have multiple underlying tables (e.g. lookup tables shared join tables etc) the engine
    /// supports migrating only a subset of tables across (the remaining tables must have already been migrated and exist in the destination database).
    /// 
    /// Finally the engine creates a LoadMetadata which when run will migrate (copy) the data from the old table into the new table.
    /// </summary>
    public class ForwardEngineerANOCatalogueEngine
    {
        private readonly ICatalogueRepository _catalogueRepository;
        private readonly ForwardEngineerANOCataloguePlanManager _planManager;
        public Catalogue NewCatalogue { get; private set; }
        public LoadMetadata LoadMetadata { get; private set; }
        public LoadProgress LoadProgressIfAny { get; set; }

        public Dictionary<TableInfo, QueryBuilder> SelectSQLForMigrations = new Dictionary<TableInfo, QueryBuilder>();
        public Dictionary<PreLoadDiscardedColumn,IDilutionOperation> DilutionOperationsForMigrations = new Dictionary<PreLoadDiscardedColumn, IDilutionOperation>(); 

        public ForwardEngineerANOCatalogueEngine(ICatalogueRepository catalogueRepository,ForwardEngineerANOCataloguePlanManager planManager)
        {
            _catalogueRepository = catalogueRepository;
            _planManager = planManager;
        }


        public void Execute()
        {
            using (_catalogueRepository.BeginNewTransactedConnection())
            {
                try
                {
                    //for each table that isn't being skipped
                    foreach (var oldTableInfo in _planManager.TableInfos.Except(_planManager.SkippedTables))
                    {
                        List<DatabaseColumnRequest> columnsToCreate = new List<DatabaseColumnRequest>();

                        Dictionary<string, ColumnInfo> migratedColumns = new Dictionary<string, ColumnInfo>();

                        var querybuilderForMigratingTable = new QueryBuilder(null, null);

                        //for each column we are not skipping (Drop) work out the endpoint datatype (planner knows this)
                        foreach (ColumnInfo columnInfo in oldTableInfo.ColumnInfos)
                        {
                            var columnPlan = _planManager.GetPlanForColumnInfo(columnInfo);

                            if (columnPlan != ForwardEngineerANOCataloguePlanManager.Plan.Drop)
                            {
                                //add the column verbatim to the query builder because we know we have to read it from source
                                querybuilderForMigratingTable.AddColumn(new ColumnInfoToIColumn(columnInfo));

                                string colName = columnInfo.GetRuntimeName();
                                
                                //if it is being ano tabled then give the table name ANO as a prefix
                                if (columnPlan == ForwardEngineerANOCataloguePlanManager.Plan.ANO)
                                    colName = "ANO" + colName;

                                migratedColumns.Add(colName, columnInfo);

                                columnsToCreate.Add(new DatabaseColumnRequest(colName, _planManager.GetEndpointDataType(columnInfo), !columnInfo.IsPrimaryKey){IsPrimaryKey = columnInfo.IsPrimaryKey});
                            }
                        }

                        SelectSQLForMigrations.Add(oldTableInfo, querybuilderForMigratingTable);

                        //Create the actual table
                        var tbl = _planManager.TargetDatabase.CreateTable(oldTableInfo.GetRuntimeName(), columnsToCreate.ToArray());

                        TableInfo newTableInfo;
                        ColumnInfo[] newColumnInfos;

                        //import the created table
                        TableInfoImporter importer = new TableInfoImporter(_catalogueRepository, tbl);
                        importer.DoImport(out newTableInfo, out newColumnInfos);

                        //Audit the parenthood of the TableInfo/ColumnInfos
                        AuditParenthood(oldTableInfo, newTableInfo);

                        foreach (ColumnInfo newColumnInfo in newColumnInfos)
                        {
                            var oldColumnInfo = migratedColumns[newColumnInfo.GetRuntimeName()];

                            var anoTable = _planManager.GetPlannedANOTable(oldColumnInfo);
                            var dilution = _planManager.GetPlannedDilution(oldColumnInfo);

                            if (anoTable != null)
                            {
                                newColumnInfo.ANOTable_ID = anoTable.ID;
                                newColumnInfo.SaveToDatabase();
                            }

                            //if there was a dilution configured we need to setup a virtual DLE load only column of the input type (this ensures RAW has a valid datatype)
                            if (dilution != null)
                            {
                                //Create a discarded (load only) column with name matching the new columninfo
                                var discard = new PreLoadDiscardedColumn(_catalogueRepository, newTableInfo,newColumnInfo.GetRuntimeName());

                                //record that it exists to support dilution and that the data type matches the input (old) ColumnInfo (i.e. not the new data type!)
                                discard.Destination = DiscardedColumnDestination.Dilute;
                                discard.SqlDataType = oldColumnInfo.Data_type;
                                discard.SaveToDatabase();

                                DilutionOperationsForMigrations.Add(discard,dilution);
                            }

                            AuditParenthood(oldColumnInfo, newColumnInfo);
                        }

                        if (DilutionOperationsForMigrations.Any())
                        {
                            newTableInfo.IdentifierDumpServer_ID = _planManager.GetIdentifierDumpServer().ID;
                            newTableInfo.SaveToDatabase();
                        }
                    }
                    
                    NewCatalogue = _catalogueRepository.CloneObjectInTable(_planManager.Catalogue);
                    NewCatalogue.Name = "ANO" + NewCatalogue.Name;
                    NewCatalogue.Folder = new CatalogueFolder(NewCatalogue, "\\anonymous" + NewCatalogue.Folder.Path);
                    NewCatalogue.SaveToDatabase();

                    AuditParenthood(_planManager.Catalogue, NewCatalogue);
                    
                    //For each of the old ExtractionInformations (95% of the time that's just a reference to a ColumnInfo e.g. '[People].[Height]' but 5% of the time it's some horrible aliased transform e.g. 'dbo.RunMyCoolFunction([People].[Height]) as BigHeight'
                    foreach (CatalogueItem oldCatalogueItem in _planManager.Catalogue.CatalogueItems)
                    {
                        var oldColumnInfo = oldCatalogueItem.ColumnInfo;

                        //catalogue item is not connected to any ColumnInfo
                        if(oldColumnInfo == null)
                            continue;

                        //we are not migrating it anyway
                        if(_planManager.GetPlanForColumnInfo(oldColumnInfo) == ForwardEngineerANOCataloguePlanManager.Plan.Drop)
                            continue;
                        
                        ColumnInfo newColumnInfo = GetNewColumnInfoForOld(oldColumnInfo);

                        var newCatalogueItem = _catalogueRepository.CloneObjectInTable(oldCatalogueItem);
                        
                        //wire it to the new Catalogue
                        newCatalogueItem.Catalogue_ID = NewCatalogue.ID;

                        //and rewire it's ColumnInfo to the cloned child one
                        newCatalogueItem.ColumnInfo_ID = newColumnInfo.ID;
                        newCatalogueItem.Name = newColumnInfo.GetRuntimeName();
                        newCatalogueItem.SaveToDatabase();

                        var oldExtractionInformation = oldCatalogueItem.ExtractionInformation;

                        var newExtractionInformation = new ExtractionInformation(_catalogueRepository, newCatalogueItem, newColumnInfo, newColumnInfo.Name);

                        //if it was previously extractable
                        if (oldExtractionInformation != null)
                        {
                            var refactorer = new SelectSQLRefactorer();

                            //restore the old SQL as it existed in the origin table
                            newExtractionInformation.SelectSQL = oldExtractionInformation.SelectSQL;
                            
                            //do a refactor on the old table name for the new table name
                            refactorer.RefactorColumnName(newExtractionInformation,oldColumnInfo,newColumnInfo.Name);

                            //make the new one exactly as extractable
                            newExtractionInformation.Order = oldExtractionInformation.Order;
                            newExtractionInformation.ExtractionCategory = oldExtractionInformation.ExtractionCategory;
                            newExtractionInformation.Alias = oldExtractionInformation.Alias;
                            newExtractionInformation.IsExtractionIdentifier = oldExtractionInformation.IsExtractionIdentifier;
                            newExtractionInformation.HashOnDataRelease = oldExtractionInformation.HashOnDataRelease;
                            newExtractionInformation.IsPrimaryKey = oldExtractionInformation.IsPrimaryKey;
                            newExtractionInformation.SaveToDatabase();
                        }

                        AuditParenthood(oldCatalogueItem, newCatalogueItem);
                        
                        if(oldExtractionInformation != null)
                            AuditParenthood(oldExtractionInformation, newExtractionInformation);
                    }

                    var existingJoinInfos = _catalogueRepository.JoinInfoFinder.GetAllJoinInfos();
                    var existingLookups = _catalogueRepository.GetAllObjects<Lookup>();
                    var existingLookupComposites = _catalogueRepository.GetAllObjects<LookupCompositeJoinInfo>();

                    //migrate join infos
                    foreach (JoinInfo joinInfo in _planManager.GetJoinInfosRequiredCatalogue())
                    {
                        var newFk = GetNewColumnInfoForOld(joinInfo.ForeignKey);
                        var newPk = GetNewColumnInfoForOld(joinInfo.PrimaryKey);

                        //already exists
                        if(!existingJoinInfos.Any(ej=>ej.ForeignKey_ID == newFk.ID && ej.PrimaryKey_ID == newPk.ID))
                            _catalogueRepository.JoinInfoFinder.AddJoinInfo(newFk,newPk,joinInfo.ExtractionJoinType,joinInfo.Collation); //create it
                    }

                    //migrate Lookups
                    foreach (Lookup lookup in _planManager.GetLookupsRequiredCatalogue())
                    {
                        //Find the new columns in the ANO table that match the old lookup columns
                        var newDesc = GetNewColumnInfoForOld(lookup.Description);
                        var newFk = GetNewColumnInfoForOld(lookup.ForeignKey);
                        var newPk = GetNewColumnInfoForOld(lookup.PrimaryKey);

                        //see if we already have a Lookup declared for the NEW columns (unlikely)
                        Lookup newLookup = existingLookups.SingleOrDefault(l => l.Description_ID == newDesc.ID && l.ForeignKey_ID == newFk.ID);
                         
                        //create new Lookup that mirrors the old but references the ANO columns instead
                        if(newLookup == null)
                            newLookup = new Lookup(_catalogueRepository, newDesc, newFk, newPk, lookup.ExtractionJoinType,lookup.Collation);

                        //also mirror any composite (secondary, tertiary join column pairs needed for the Lookup to operate correclty e.g. where TestCode 'HAB1' means 2 different things depending on healthboard) 
                        foreach (LookupCompositeJoinInfo compositeJoin in lookup.GetSupplementalJoins().Cast<LookupCompositeJoinInfo>())
                        {
                            var newCompositeFk = GetNewColumnInfoForOld(compositeJoin.ForeignKey);
                            var newCompositePk = GetNewColumnInfoForOld(compositeJoin.PrimaryKey);

                            if (!existingLookupComposites.Any(c => c.ForeignKey_ID == newCompositeFk.ID && c.PrimaryKey_ID == newCompositePk.ID))
                                new LookupCompositeJoinInfo(_catalogueRepository, newLookup, newCompositeFk,newCompositePk, compositeJoin.Collation);
                        }
                    }

                    //create new data load confguration
                    LoadMetadata = new LoadMetadata(_catalogueRepository, "Anonymising " + NewCatalogue);
                    LoadMetadata.EnsureLoggingWorksFor(NewCatalogue);

                    NewCatalogue.LoadMetadata_ID = LoadMetadata.ID;
                    NewCatalogue.SaveToDatabase();

                    if (_planManager.DateColumn != null)
                    {
                        LoadProgressIfAny = new LoadProgress(_catalogueRepository, LoadMetadata);
                        LoadProgressIfAny.OriginDate = _planManager.StartDate;
                        LoadProgressIfAny.SaveToDatabase();

                        //date column based migration only works for single TableInfo migrations (see Plan Manager checks)
                        var qb = SelectSQLForMigrations.Single().Value;
                        qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(null,
                            new[]
                            {
                                new SpontaneouslyInventedFilter(null,_planManager.DateColumn + " >= @startDate","After batch start date","",null),
                                new SpontaneouslyInventedFilter(null,_planManager.DateColumn + " <= @endDate","Before batch end date","",null),
                            }
                            ,FilterContainerOperation.AND);
                    }
                    try
                    {
                        foreach (QueryBuilder qb in SelectSQLForMigrations.Values)
                            Console.WriteLine(qb.SQL);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to generate migration SQL",e);
                    }

                    _catalogueRepository.EndTransactedConnection(true);
                }
                catch (Exception ex)
                {
                    _catalogueRepository.EndTransactedConnection(false);
                    throw new Exception("Failed to create ANO version, transaction rolled back succesfully",ex);
                }
            }
        }

        /// <summary>
        /// Returns the newly created / already existing NEW ANO column info when passed the old (identifiable original) ColumnInfo
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private ColumnInfo GetNewColumnInfoForOld(ColumnInfo col)
        {
            //it's one we migrated ourselves
            if (_parenthoodDictionary.ContainsKey(col))
                return (ColumnInfo)_parenthoodDictionary[col];
            else
            {
                //it's one that was already existing before we did ANO migration e.g. a SkippedTableInfo (this can happen when there are 2+ tables underlying a Catalogue and you have already ANO one of those Tables previously (e.g. when it is a shared table with other Catalogues)

                //find a reference to the new ColumnInfo Location (note that it is possible the TableInfo was skipped, in which case we should still expect to find ColumnInfos that reference the new location because you must have created it somehow right?)
                var syntaxHelper = _planManager.TargetDatabase.Server.GetQuerySyntaxHelper();

                var toReturn = FindNewColumnNamed(syntaxHelper,col,col.GetRuntimeName());
                
                if (toReturn == null)
                    toReturn = FindNewColumnNamed(syntaxHelper,col,"ANO" + col.GetRuntimeName());

                if(toReturn == null)
                    throw new Exception("Catalogue '" + _planManager.Catalogue + "' contained a CatalogueItem referencing Column '" + col + "' the ColumnInfo was not migrated (which is fine) but we then could not find ColumnInfo in the new ANO dataset (if it was part of SkippedTables why doesn't the Catalogue have a reference to the new location?)");

                return toReturn;
            }
        }


        /// <summary>
        /// Here we are migrating a Catalogue but some of the TableInfos have already been migrated e.g. lookup tables as part of migrating another Catalogue.  We are
        /// now trying to find one of those 'not migrated' ColumnInfos by name without knowing whether the user has since deleted the reference or worse introduced 
        /// duplicate references to the same TableInfo/ColumnInfos.
        /// </summary>
        /// <param name="syntaxHelper"></param>
        /// <param name="col"></param>
        /// <param name="expectedName"></param>
        /// <returns></returns>
        private ColumnInfo FindNewColumnNamed(IQuerySyntaxHelper syntaxHelper, ColumnInfo col, string expectedName)
        {
            string expectedNewName = syntaxHelper.EnsureFullyQualified(
                _planManager.TargetDatabase.GetRuntimeName(),
                null,
                col.TableInfo.GetRuntimeName(),
                expectedName);

            var columns = _catalogueRepository.GetColumnInfosWithNameExactly(expectedNewName);

            if (columns.Length == 1)
                return columns[0];

            var columnsFromCorrectServer = columns.Where(c => c.TableInfo.Server.Equals(_planManager.TargetDatabase.Server.Name)).ToArray();

            if (columnsFromCorrectServer.Length == 1)
                return columnsFromCorrectServer[0];

            var columnsFromCorrectServerThatAreaAlsoLocalImports = columnsFromCorrectServer.Where(_catalogueRepository.ShareManager.IsImportedObject).ToArray();

            if (columnsFromCorrectServerThatAreaAlsoLocalImports.Length == 1)
                return columnsFromCorrectServerThatAreaAlsoLocalImports[0];

            throw new Exception("Found '" + columns.Length + "' ColumnInfos called '" + expectedName +"'");
        }

        Dictionary<IMapsDirectlyToDatabaseTable,IMapsDirectlyToDatabaseTable> _parenthoodDictionary = new Dictionary<IMapsDirectlyToDatabaseTable, IMapsDirectlyToDatabaseTable>();

        private void AuditParenthood(IMapsDirectlyToDatabaseTable parent, IMapsDirectlyToDatabaseTable child)
        {
            //make it shareable
            var export = _catalogueRepository.ShareManager.GetExportFor(parent);

            //share it to yourself where the child is the realisation of the share (this creates relationship in database)
            var import = _catalogueRepository.ShareManager.GetImportAs(export.SharingUID, child);

            //record in memory dictionary
            _parenthoodDictionary.Add(parent,child);
        }
    }
}
