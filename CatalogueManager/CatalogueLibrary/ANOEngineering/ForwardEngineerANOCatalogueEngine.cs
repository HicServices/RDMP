using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
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

                        //for each column we are not skipping (Drop) work out the endpoint datatype (planner knows this)
                        foreach (ColumnInfo columnInfo in t.ColumnInfos)
                        {
                            var columnPlan = _planManager.GetPlanForColumnInfo(columnInfo);

                            if (columnPlan != ForwardEngineerANOCataloguePlanManager.Plan.Drop)
                            {
                                string colName = columnInfo.GetRuntimeName();

                                //if it is being ano tabled then give the table name ANO as a prefix
                                if (columnPlan == ForwardEngineerANOCataloguePlanManager.Plan.ANO)
                                    colName = "ANO" + colName;

                                migratedColumns.Add(colName, columnInfo);

                                columnsToCreate.Add(new DatabaseColumnRequest(colName, _planManager.GetEndpointDataType(columnInfo), !columnInfo.IsPrimaryKey));
                            }
                        }

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

                    //todo create data load
                    
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
            var export = _catalogueRepository.GetExportFor(parent);

            //share it to yourself where the child is the realisation of the share (this creates relationship in database)
            var import = new ObjectImport(_catalogueRepository, export.SharingUID, child);

            //record in memory dictionary
            _parenthoodDictionary.Add(parent,child);
        }
    }
}
