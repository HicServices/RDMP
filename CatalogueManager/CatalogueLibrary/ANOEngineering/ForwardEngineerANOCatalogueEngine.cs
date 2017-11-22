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

                        //it wasn't migrated
                        if (!_parenthoodDictionary.ContainsKey(col))
                            continue;//skip it

                        var newCatalogueItem = _catalogueRepository.CloneObjectInTable(oldCatalogueItem);

                        //wire it to the new Catalogue
                        newCatalogueItem.Catalogue_ID = NewCatalogue.ID;

                        //and rewire it's ColumnInfo to the cloned child one
                        var newColumnInfo = (ColumnInfo)_parenthoodDictionary[oldCatalogueItem.ColumnInfo];
                        newCatalogueItem.ColumnInfo_ID =newColumnInfo.ID;
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
            var export = ObjectExport.GetExportFor(_catalogueRepository, parent);

            //share it to yourself where the child is the realisation of the share (this creates relationship in database)
            var import = new ObjectImport(_catalogueRepository, export.SharingUID, child);

            //record in memory dictionary
            _parenthoodDictionary.Add(parent,child);
        }
    }
}
