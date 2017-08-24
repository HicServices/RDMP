using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Database;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable.Versioning;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Cloning
{
    public class CatalogueCloner
    {
        private readonly CatalogueRepository _sourceRepository;
        private readonly CatalogueRepository _destinationRepository;

        /// <summary>
        /// CatalogueCloner is constructed with source and destination repositories which are immutable. Bad things could happen if you can mutate the source/destination during cloning operations.
        /// </summary>
        /// <param name="sourceRepository"></param>
        /// <param name="destinationRepository"></param>
        public CatalogueCloner(CatalogueRepository sourceRepository, CatalogueRepository destinationRepository)
        {
            _sourceRepository = sourceRepository;
            _destinationRepository = destinationRepository;
        }

        #region Catalogue into same database (with new ID)
        public Catalogue CreateDuplicateInSameDatabase(Catalogue toClone)
        {
            var repo = (ICatalogueRepository)toClone.Repository;
            //create a duplicate in the same database with the name _DUPLICATE (all other fields will be null/default at the moment)
            Catalogue duplicate = new Catalogue(repo, toClone.Name + "_DUPLICATE");

            //populate the duplicate with all the values from the thing we are trying to clone (via reflection of every property that is readable/writeable)
            CopyValuesFromCatalogueIntoCatalogue(toClone, duplicate);
            
            //copying values also overwrites name! so overwrite that overwrite
            duplicate.Name = duplicate.Name + "_DUPLICATE";

            duplicate.SaveToDatabase();

            //duplicate all catalougeItems
            foreach (var catalogueItem in toClone.CatalogueItems)
            {
                //create a new item under the new duplicate (with blank values)
                CatalogueItem cataItemDuplicate = new CatalogueItem(repo, duplicate, catalogueItem.Name);

                //copy via reflection
                CopyValuesFromCatalogueItemIntoCatalogueItem(catalogueItem, cataItemDuplicate);

                //save the duplicate
                cataItemDuplicate.SaveToDatabase();
            }

            return duplicate;
        }

        private void CopyValuesFromCatalogueIntoCatalogue(Catalogue from, Catalogue to)
        {
            foreach (var propertyInfo in typeof(Catalogue).GetProperties())
            {
                if(propertyInfo.Name == "ID")
                    continue;

                if (propertyInfo.CanWrite == false || propertyInfo.CanRead == false)
                    continue;

                object value = propertyInfo.GetValue(from, null);
                propertyInfo.SetValue(to,value,null);
            }
        }

        public static void CopyValuesFromCatalogueItemIntoCatalogueItem(CatalogueItem from, CatalogueItem to, bool skipNameProperty = false)
        {
            foreach (var propertyInfo in typeof(CatalogueItem).GetProperties())
            {
                if (propertyInfo.Name == "ID")
                    continue;

                if (propertyInfo.Name.EndsWith("_ID"))
                    continue;

                if(propertyInfo.Name == "Name"  && skipNameProperty)
                    continue;

                if (propertyInfo.CanWrite == false || propertyInfo.CanRead == false)
                    continue;

                object value = propertyInfo.GetValue(from, null);
                propertyInfo.SetValue(to, value, null);
            }
        }
        #endregion

        public enum CloneDepth 
        {
            CatalogueOnly,
            CatalogueItem,
            FullTree

        }

        #region Catalogue into different database (with SAME ID)

        public void CloneIntoNewDatabase(Catalogue toClone, CloneDepth depth, List<CatalogueItem> cloneOnlyTheseCatalogueItems=null,bool alsoCloneLoadMetadata=false)
        {
            if (_destinationRepository == toClone.Repository)
                throw new NotSupportedException("Cannot clone the object into the same repository using this method");

            if (depth == CloneDepth.FullTree && cloneOnlyTheseCatalogueItems != null)
                throw new Exception("Cannot clone full tree when only using a subset of CatalogueItems for cloning because of referential integrity (we could clone ExtractionInformation for one of the CatalogueItems you decided you didn't want)");

            _destinationRepository.BeginNewTransactedConnection();

            try
            {
                CheckTargetDatabaseHasRequiredTables();

                List<ColumnInfo> clonedColumns = new List<ColumnInfo>();
                List<TableInfo> clonedTables = new List<TableInfo>();
                List<ExtractionInformation> clonedExtractionInformations = new List<ExtractionInformation>();

                ///////////////////CATALOGUE////////////////

                //Have not created the ExtractionInformations yet but we might later on so record them for now
                int? timeCoverage_extractionInformationID = toClone.TimeCoverage_ExtractionInformation_ID;
                int? pivotCategory_extractionInformationID = toClone.PivotCategory_ExtractionInformation_ID;

                //but do not set them yet because of referential integrity problems
                toClone.TimeCoverage_ExtractionInformation_ID = null;
                toClone.PivotCategory_ExtractionInformation_ID = null;
                
                //clone external references
                //clone the datasource too
                CloneExternalSourceIfNotNullAndNotAlreadyExists(toClone.LiveLoggingServer_ID);
                CloneExternalSourceIfNotNullAndNotAlreadyExists(toClone.TestLoggingServer_ID);

               //if catalogue has metadata
                LoadMetadata lmd = toClone.LoadMetadata;
                Catalogue theClone;

                if (lmd != null)
                    if (alsoCloneLoadMetadata)
                    {
                        //clone load metadata
                        _sourceRepository.CloneObjectInTableIfDoesntExist(lmd, _destinationRepository);
                        
                        //clone Catalogue - because process tasks could relate to this catalogue (infact probably do)
                        theClone = _sourceRepository.CloneObjectInTable(toClone, _destinationRepository);

                        //clone the processes
                        foreach (ProcessTask processTask in lmd.ProcessTasks)
                        {
                            //if this is a catalogue specific step
                            if(processTask.RelatesSolelyToCatalogue_ID != null)
                            {
                                //it is catalogue specifc and relates to a different Catalogue (than the one we are cloning) - so continue
                                if (processTask.RelatesSolelyToCatalogue_ID != toClone.ID)
                                    continue;
                            }

                            _sourceRepository.CloneObjectInTableIfDoesntExist(processTask, _destinationRepository);

                            //clone arguments
                            foreach (ProcessTaskArgument processTaskArgument in processTask.ProcessTaskArguments)
                            {
                                _sourceRepository.CloneObjectInTableIfDoesntExist(processTaskArgument, _destinationRepository);
                            }
                        }
                    }
                    else
                    {

                        //user doesn't want to clone the metadata but the Catalogue has a load metadata... so make it null so that it doesnt violate FOREIGN KEY constraint
                        toClone.HardDisassociateLoadMetadata();
                        //then clone
                        theClone = _sourceRepository.CloneObjectInTable(toClone, _destinationRepository);
                    }
                else
                {
                    //clone Catalogue - the user does not want to clone the metadata so just clone the catalogue 
                    theClone = _sourceRepository.CloneObjectInTable(toClone, _destinationRepository);
                    
                }

                if(depth == CloneDepth.CatalogueOnly)
                {
                    _destinationRepository.EndTransactedConnection(true);
                    return;
                }

                ////////////////////////////////////////////// TABLE INFOs/////////////////////////////
                //clone entire underlying table 
                if (depth == CloneDepth.FullTree)
                {
                    foreach (var tableToClone in toClone.GetTableInfoList(true))
                    {
                        //have already cloned this table
                        if (!clonedTables.Contains(tableToClone))
                        {
                            //clone the datasource too
                            CloneExternalSourceIfNotNullAndNotAlreadyExists(tableToClone.IdentifierDumpServer_ID);

                            //although we haven't cloned it yet this pass, it could alreayd exist because the same table could be used by another Catalogue
                            _sourceRepository.CloneObjectInTableIfDoesntExist(tableToClone, _destinationRepository);
                            clonedTables.Add(tableToClone);

                            //clone ALL underlying columns (not just those extracted as some might be used as join keys)
                            foreach (var columnInfo in tableToClone.ColumnInfos)
                            {
                                _sourceRepository.CloneObjectInTableIfDoesntExist(columnInfo, _destinationRepository);
                                clonedColumns.Add(columnInfo);
                            }
                        }
                    }
                }

                ///////////////////CATALOGUE ITEMS////////////////
                //clone CatalogueItems
                if (cloneOnlyTheseCatalogueItems !=  null)
                {
                    //clone only these specific catalogue items (make sure the user didnt pass us a bunch of duff stuff that didnt even come from the correct catalogue!)
                    foreach (CatalogueItem catalogueItem in cloneOnlyTheseCatalogueItems)
                        if (catalogueItem.Catalogue_ID != toClone.ID)
                            throw new Exception("Could not clone CatalogueItem " + catalogueItem.Name + " because it belongs to Catalogue with ID " + catalogueItem.Catalogue_ID + " when we are cloning a different one (ID=" + toClone.ID + ")");
                        else
                            _sourceRepository.CloneObjectInTable(catalogueItem, _destinationRepository);

                }   
                else
                    foreach (CatalogueItem catalogueItem in toClone.CatalogueItems)
                    {
                        _sourceRepository.CloneObjectInTable(catalogueItem, _destinationRepository);
                    }

                if (depth == CloneDepth.CatalogueItem)
                {
                    _destinationRepository.EndTransactedConnection(true);
                    return;
                }

                ///////////////////EXTRACTION INFO////////////////
                if (depth == CloneDepth.FullTree)
                {
                    foreach (var extractionInformation in toClone.GetAllExtractionInformation(ExtractionCategory.Any))
                    {
                        _sourceRepository.CloneObjectInTable(extractionInformation, _destinationRepository);
                        clonedExtractionInformations.Add(extractionInformation);

                        foreach (var extractionFilter in extractionInformation.ExtractionFilters)
                        {
                            _sourceRepository.CloneObjectInTable(extractionFilter, _destinationRepository);

                            foreach (var extractionFilterParameter in extractionFilter.ExtractionFilterParameters)
                                _sourceRepository.CloneObjectInTable(extractionFilterParameter, _destinationRepository);
                        }
                    }

                    //Now that we have created the ExtractionInformations we can wire them up
                    if (timeCoverage_extractionInformationID != null || pivotCategory_extractionInformationID != null)
                    {
                        theClone.PivotCategory_ExtractionInformation_ID = pivotCategory_extractionInformationID;
                        theClone.TimeCoverage_ExtractionInformation_ID = timeCoverage_extractionInformationID;
                        theClone.SaveToDatabase();
                    }
                }
                
                //have to commit here because for the final bits we will be using the normal CatalogueLibrary to generate stuff
                _destinationRepository.GetConnection().Transaction.Commit();

                ///////////////////JOINS AND LOOKUPS////////////////
                //Now for the tricky part, we can't clone all available joins and lookups or we could end up spiderwebbing all over the joint, so instead let the
                //QueryBuilder assemble the extraction SQL for the catalogue (Core and Supplemental only) and lets build only those joins/lookups in the clone
                //database so that the same procedure can be repeated there.

                QueryBuilder qb = new QueryBuilder(null,null);
                qb.AddColumnRange(toClone.GetAllExtractionInformation(ExtractionCategory.Core));
                qb.AddColumnRange(toClone.GetAllExtractionInformation(ExtractionCategory.Supplemental));

                try
                {
                    if (qb.ColumnCount() == 0)
                        return;
                }
                catch (Exception e)
                {
                    //it is possible the user has a screwed up Catalogue and it is not possible to generate Extraction SQl, this should stop him from
                    //Cloning the database.  This is the first point where SQL is generated by QueryBuilder (when you call a get property/method)
                    //so if it bombs here we just jump out and assume we have copied enough stuff.
                    throw new Exception("Catalogue " + toClone.Name + " is breaking when passed to QueryBuilder, it must be fixed before it can be cloned", e);
                }

                JoinInfo[] requiredJoins = qb.GetRequiredJoins();
                Lookup[] requiredLookups = qb.GetRequiredLookups();

                //clone joins
                foreach (JoinInfo join in requiredJoins)
                    _destinationRepository.JoinInfoFinder.AddJoinInfo(join.ForeignKey, join.PrimaryKey, join.ExtractionJoinType, join.Collation);

                foreach (Lookup lookup in requiredLookups)
                    new Lookup(_destinationRepository, lookup.Description, lookup.ForeignKey, lookup.PrimaryKey, lookup.ExtractionJoinType, lookup.Collation);
            }
            catch (MissingTableException ex)
            { throw ex;}
            catch (Exception ex)
            {
                try
                {
                    _destinationRepository.EndTransactedConnection(false);
                }
                catch (Exception)
                {
                    throw new Exception(ex.Message + "(Rollback Failed)" ,ex);
                }

                if (ex.Message.StartsWith("Violation of PRIMARY KEY"))
                {
                    throw new Exception("Attempted to clone an object that was already in the target database or at least an object of the same type with the same ID (Rollback Successful).", ex);
                }

                throw new Exception("Exception " + ex.Message + " occurred, transaction was rolled back", ex);
            }
        }

        private void CloneExternalSourceIfNotNullAndNotAlreadyExists(int? externalServerID)
        {
            if(externalServerID == null)
                return;

            var externalDatabaseServer = _sourceRepository.GetObjectByID<ExternalDatabaseServer>((int)externalServerID);
            _sourceRepository.CloneObjectInTableIfDoesntExist(externalDatabaseServer, _destinationRepository);
        }

        private void CheckTargetDatabaseHasRequiredTables()
        {
            var database = ((SqlConnectionStringBuilder) _destinationRepository.ConnectionStringBuilder).InitialCatalog;

            var db = _destinationRepository.DiscoveredServer.ExpectDatabase(database);
            if(!db.Exists())
                throw new Exception("Database " + db + " does not exist");

            DiscoveredTable[] tables = db.DiscoverTables(false);

            string[] expectedTables = new []
            {
                "Catalogue",
                "CatalogueItem",
                "SupportingDocument",
                "ExtractionInformation",
                "ExtractionFilter",
                "ExtractionFilterParameter",
                "TableInfo",
                "ColumnInfo",
                "JoinInfo",
                "Lookup",
                "LoadMetadata"
            };

            foreach (string expectedtable in expectedTables)
            {
                //invariant compare because mysql will often drop capitalization
                if (!tables.Any(t=>t.GetRuntimeName().ToLower().Equals(expectedtable.ToLower())))
                    throw new MissingTableException(expectedtable, database);    
            }
        }
        

        #endregion

        public void CreateNewEmptyCatalogueDatabase(string destinationDatabase, ICheckNotifier checksUI)
        {
            const string coreCreateFile = "CatalogueLibrary.Database.db.runAfterCreateDatabase.CreateCatalogue.sql";
            var assembly = typeof(Class1).Assembly;

            //get everything in the /up/ folder that are .sql
            var patches = Patch.GetAllPatchesInAssembly(assembly);

            StreamReader sr = new StreamReader(assembly.GetManifestResourceStream(coreCreateFile));
            //sr.ReadToEnd(), "1.0.0.0", patches
            var builder = (SqlConnectionStringBuilder) _destinationRepository.ConnectionStringBuilder;
            MasterDatabaseScriptExecutor creator = new MasterDatabaseScriptExecutor(builder.DataSource, destinationDatabase, builder.UserID, builder.Password);

            creator.CreateDatabase(sr.ReadToEnd(), "1.0.0.0", checksUI);
            creator.PatchDatabase(patches, checksUI, patch => true);
        }

        public void FullTreeDelete(Catalogue catalogue)
        {
            var coreAndSupplemental = catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                .Where(
                    ei =>
                        ei.ExtractionCategory == ExtractionCategory.Core ||
                        ei.ExtractionCategory == ExtractionCategory.Supplemental).ToArray();

            
            QueryBuilder qb = new QueryBuilder(null,null);
            qb.AddColumnRange(coreAndSupplemental);

            foreach (JoinInfo requiredJoin in qb.GetRequiredJoins())
                requiredJoin.DeleteInDatabase();

            foreach (Lookup requiredLookup in qb.GetRequiredLookups())
                requiredLookup.DeleteInDatabase();

            foreach (TableInfo tableInfo in catalogue.GetTableInfoList(false))
                tableInfo.DeleteInDatabase();

            foreach (
                ExtractionInformation extractionInformation in
                   catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
                extractionInformation.DeleteInDatabase();

            catalogue.DeleteInDatabase(); 
        }

     
    }
}
