using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.ANOEngineering
{
    public class ForwardEngineerANOCatalogueEngine
    {
        private readonly ICatalogueRepository _catalogueRepository;
        private readonly ForwardEngineerANOCataloguePlanManager _planManager;
        public Catalogue NewCatalogue { get; private set; }
        public LoadMetadata LoadMetadata { get; private set; }
        public LoadProgress LoadProgressIfAny { get; set; }

        public Dictionary<TableInfo, QueryBuilder> SelectSQLForMigrations = new Dictionary<TableInfo, QueryBuilder>();

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
                    foreach (var t in _planManager.TableInfos.Except(_planManager.SkippedTables))
                    {
                        List<DatabaseColumnRequest> columnsToCreate = new List<DatabaseColumnRequest>();

                        Dictionary<string, ColumnInfo> migratedColumns = new Dictionary<string, ColumnInfo>();

                        var querybuilderForMigratingTable = new QueryBuilder(null, null);

                        //for each column we are not skipping (Drop) work out the endpoint datatype (planner knows this)
                        foreach (ColumnInfo columnInfo in t.ColumnInfos)
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

                                columnsToCreate.Add(new DatabaseColumnRequest(colName, _planManager.GetEndpointDataType(columnInfo), !columnInfo.IsPrimaryKey));
                            }
                        }

                        SelectSQLForMigrations.Add(t, querybuilderForMigratingTable);

                        //Create the actual table
                        var tbl = _planManager.TargetDatabase.CreateTable(t.GetRuntimeName(), columnsToCreate.ToArray());

                        TableInfo newTableInfo;
                        ColumnInfo[] newColumnInfos;

                        //import the created table
                        TableInfoImporter importer = new TableInfoImporter(_catalogueRepository, tbl);
                        importer.DoImport(out newTableInfo, out newColumnInfos);

                        //Audit the parenthood of the TableInfo/ColumnInfos
                        AuditParenthood(t, newTableInfo);

                        foreach (ColumnInfo newColumnInfo in newColumnInfos)
                        {
                            var oldColumnInfo = migratedColumns[newColumnInfo.GetRuntimeName()];

                            var anoTable = _planManager.GetPlannedANOTable(oldColumnInfo);

                            if (anoTable != null)
                            {
                                newColumnInfo.ANOTable_ID = anoTable.ID;
                                newColumnInfo.SaveToDatabase();
                            }

                            AuditParenthood(oldColumnInfo, newColumnInfo);
                        }
                    }

                    NewCatalogue = _catalogueRepository.CloneObjectInTable(_planManager.Catalogue);
                    NewCatalogue.Name = "ANO" + NewCatalogue.Name;
                    NewCatalogue.Folder = new CatalogueFolder(NewCatalogue, "\\anonymous" + NewCatalogue.Folder.Path);
                    NewCatalogue.SaveToDatabase();

                    AuditParenthood(_planManager.Catalogue, NewCatalogue);
                    
                    foreach (CatalogueItem oldCatalogueItem in _planManager.Catalogue.CatalogueItems)
                    {
                        var col = oldCatalogueItem.ColumnInfo;

                        //catalogue item is not connected to any ColumnInfo
                        if(col == null)
                            continue;

                        ColumnInfo newColumnInfo;

                        if (_parenthoodDictionary.ContainsKey(col))
                            newColumnInfo = (ColumnInfo) _parenthoodDictionary[col];
                        else
                        {
                            //find a reference to the new ColumnInfo Location (note that it is possible the TableInfo was skipped, in which case we should still expect to find ColumnInfos that reference the new location because you must have created it somehow right?)
                            var syntaxHelper = _planManager.TargetDatabase.Server.GetQuerySyntaxHelper();
                            string expectedNewName = syntaxHelper.EnsureFullyQualified(
                                _planManager.TargetDatabase.GetRuntimeName(),
                                null,
                                col.TableInfo.GetRuntimeName(),
                                col.GetRuntimeName());

                            newColumnInfo = _catalogueRepository.GetColumnInfoWithNameExactly(expectedNewName);

                            if (newColumnInfo == null)
                                throw new Exception("Catalogue '"+_planManager.Catalogue+"' contained a CatalogueItem referencing Column '"+col+"' the ColumnInfo was not migrated (which is fine) but we then could not find ColumnInfo with expected name '" + expectedNewName + "' (if it was part of SkippedTables why doesn't the Catalogue have a reference to the new location?)");
                        }
                        
                        var newCatalogueItem = _catalogueRepository.CloneObjectInTable(oldCatalogueItem);
                        
                        //wire it to the new Catalogue
                        newCatalogueItem.Catalogue_ID = NewCatalogue.ID;

                        //and rewire it's ColumnInfo to the cloned child one
                        newCatalogueItem.ColumnInfo_ID = newColumnInfo.ID;
                        newCatalogueItem.SaveToDatabase();

                        var oldExtractionInformation = oldCatalogueItem.ExtractionInformation;

                        var newExtractionInformation = new ExtractionInformation(_catalogueRepository, newCatalogueItem, newColumnInfo, newColumnInfo.Name);
                        if (oldExtractionInformation.IsExtractionIdentifier)
                        {
                            newExtractionInformation.IsExtractionIdentifier = true;
                            newExtractionInformation.SaveToDatabase();
                        }

                        AuditParenthood(oldCatalogueItem, newCatalogueItem);
                        AuditParenthood(oldExtractionInformation, newExtractionInformation);
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
                catch (Exception)
                {
                    _catalogueRepository.EndTransactedConnection(false);
                    throw;
                }
            }
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
