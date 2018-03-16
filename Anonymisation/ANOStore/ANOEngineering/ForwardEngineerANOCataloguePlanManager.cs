using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Refactoring;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using Sharing.Sharing;

namespace ANOStore.ANOEngineering
{
    /// <summary>
    /// Configuration class for ForwardEngineerANOCatalogueEngine (See ForwardEngineerANOCatalogueEngine).  This class stores which anonymisation transforms/dilutions
    /// etc to apply to which columns, which TableInfos are to be mirated etc.  Also stores whether the LoadMetadata that is to be created should be a single one off
    /// load or should load in date based batches (e.g. 1 year at a time - use this option if you have too much data in the source table to be migrated in one go - e.g.
    /// tens of millions of records). 
    /// </summary>
    public class ForwardEngineerANOCataloguePlanManager : ICheckable
    {
        private readonly ShareManager _shareManager;
        public Catalogue Catalogue { get; private set; }
        private ExtractionInformation[] _allExtractionInformations;
        private CatalogueItem[] _allCatalogueItems;

        private readonly Dictionary<ColumnInfo, ColumnInfoANOPlan> Plans = new Dictionary<ColumnInfo, ColumnInfoANOPlan>();

        public List<IDilutionOperation>  DilutionOperations { get; private set; }

        public TableInfo[] TableInfos { get; private set; }

        public DiscoveredDatabase TargetDatabase { get; set; }
        public ColumnInfo DateColumn { get; set; }
        public DateTime? StartDate { get; set; }

        public HashSet<TableInfo> SkippedTables = new HashSet<TableInfo>();

        public ForwardEngineerANOCataloguePlanManager(ShareManager shareManager, Catalogue catalogue)
        {
            _shareManager = shareManager;
            Catalogue = catalogue;

            RefreshTableInfos();

            DilutionOperations = new List<IDilutionOperation>();
            
            ObjectConstructor constructor = new ObjectConstructor();

            foreach (var operationType in ((CatalogueRepository) catalogue.Repository).MEF.GetTypes<IDilutionOperation>())
                DilutionOperations.Add((IDilutionOperation) constructor.Construct(operationType));
        }

        public ColumnInfoANOPlan GetPlanForColumnInfo(ColumnInfo col)
        {
            if(!Plans.ContainsKey(col))
                throw new Exception("No plan found for column " + col);

            return Plans[col];
        }

        public IExternalDatabaseServer GetIdentifierDumpServer()
        {
            return new ServerDefaults((CatalogueRepository)Catalogue.Repository).GetDefaultFor(ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID);
        }

        
        public void Check(ICheckNotifier notifier)
        {
            if (TargetDatabase == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No TargetDatabase has been set", CheckResult.Fail));
            else
                if (!TargetDatabase.Exists())
                    notifier.OnCheckPerformed(new CheckEventArgs("TargetDatabase '"+TargetDatabase+"' does not exist", CheckResult.Fail));

            var toMigrateTables = TableInfos.Except(SkippedTables).ToArray();

            if (!toMigrateTables.Any())
                notifier.OnCheckPerformed(new CheckEventArgs("There are no TableInfos selected for anonymisation",CheckResult.Fail));

            try
            {
                var joinInfos = GetJoinInfosRequiredCatalogue();
                notifier.OnCheckPerformed(new CheckEventArgs("Generated Catalogue SQL succesfully", CheckResult.Success));

                foreach (JoinInfo joinInfo in joinInfos)
                    notifier.OnCheckPerformed(new CheckEventArgs("Found required JoinInfo '" + joinInfo + "' that will have to be migrated",CheckResult.Success));

                foreach (Lookup lookup in GetLookupsRequiredCatalogue())
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Found required Lookup '" + lookup + "' that will have to be migrated", CheckResult.Success));

                    //for each key involved in the lookup
                    foreach (ColumnInfo c in new[] { lookup.ForeignKey ,lookup.PrimaryKey,lookup.Description})
                    {
                        //lookup / table has already been migrated 
                        if(SkippedTables.Any(t=>t.ID == c.TableInfo_ID))
                            continue;

                        //make sure that the plan is sensible
                        if (GetPlanForColumnInfo(c).Plan != Plan.PassThroughUnchanged)
                            notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo '" + c + "' is part of a Lookup so must PassThroughUnchanged", CheckResult.Fail));
                            
                    }
                }
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to generate Catalogue SQL", CheckResult.Fail,ex));
            }
            
            if (DateColumn != null)
            {
                var dateColumnPlan = GetPlanForColumnInfo(DateColumn);
                if(dateColumnPlan.Plan != Plan.PassThroughUnchanged)
                    if(notifier.OnCheckPerformed(new CheckEventArgs("Plan for " + DateColumn + " must be PassThroughUnchanged",CheckResult.Fail,null,"Set plan to PassThroughUnchanged")))
                        dateColumnPlan.Plan = Plan.PassThroughUnchanged;

                var usedTables = TableInfos.Except(SkippedTables).Count();
                
                if (usedTables > 1)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "You cannot have a date based migration because you are trying to migrate " + usedTables +
                            " TableInfos at once", CheckResult.Fail));

            }
            
            if (Plans.Any(p=>p.Value.Plan == Plan.Dilute))
                if (GetIdentifierDumpServer() == null)
                    notifier.OnCheckPerformed(new CheckEventArgs("No default Identifier Dump server has been configured", CheckResult.Fail));
            
            var refactorer = new SelectSQLRefactorer();

            foreach (ExtractionInformation e in _allExtractionInformations)
                if (!refactorer.IsRefactorable(e))
                    notifier.OnCheckPerformed(new CheckEventArgs("ExtractionInformation '" + e + "' is a not refactorable due to reason:" + refactorer.GetReasonNotRefactorable(e), CheckResult.Fail));
            
            foreach (TableInfo tableInfo in toMigrateTables)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Evaluating TableInfo '" + tableInfo + "'", CheckResult.Success));

                if (TargetDatabase != null && TargetDatabase.ExpectTable(tableInfo.GetRuntimeName()).Exists())
                    notifier.OnCheckPerformed(new CheckEventArgs("Table '" + tableInfo + "' already exists in Database '" + TargetDatabase + "'", CheckResult.Fail));

                var pks = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).ToArray();

                if (!pks.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo '" + tableInfo + "' does not have any Primary Keys, it cannot be anonymised", CheckResult.Fail));
                
                if (tableInfo.IsTableValuedFunction)
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo '" + tableInfo + "' is an IsTableValuedFunction so cannot be anonymised", CheckResult.Fail));

                EnsureNotAlreadySharedLocally(notifier, tableInfo);
                EnsureNotAlreadySharedLocally(notifier, Catalogue);
            }

            //check the column level plans
            foreach (var p in Plans.Values)
                p.Check(notifier);
        }

        private void EnsureNotAlreadySharedLocally<T>(ICheckNotifier notifier,T m) where T:IMapsDirectlyToDatabaseTable
        {
            if (_shareManager.IsExportedObject(m))
            {
                var existingExport = _shareManager.GetExportFor(m);
                var existingImportReference = _shareManager.GetExistingImport(existingExport.SharingUID);

                if (existingImportReference != null)
                {
                    T existingImportInstance = m.Repository.GetObjectByID<T>(existingImportReference.LocalObjectID);
                    notifier.OnCheckPerformed(new CheckEventArgs(typeof(T) + " '" + m + "' is already locally shared as '" + existingImportInstance + "'", CheckResult.Fail));
                }
            }
        }

        /// <summary>
        /// Re checks the TableInfos associated with the Catalogue incase some have changed
        /// </summary>
        public void RefreshTableInfos()
        {
            var allColumnInfosSystemWide = Catalogue.Repository.GetAllObjects<ColumnInfo>();

            _allExtractionInformations = Catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            _allCatalogueItems = Catalogue.CatalogueItems.Where(ci => ci.ColumnInfo_ID != null).ToArray();
            var joins = GetJoinInfosRequiredCatalogue();
            var lookups = GetLookupsRequiredCatalogue();

            TableInfos = Catalogue.GetTableInfoList(true);

            //generate plans for novel ColumnInfos
            foreach (TableInfo tableInfo in TableInfos)
            {
                var syntaxHelper = tableInfo.GetQuerySyntaxHelper();
                
                foreach (ColumnInfo columnInfo in tableInfo.ColumnInfos)
                    if (!Plans.ContainsKey(columnInfo))
                        Plans.Add(columnInfo, new ColumnInfoANOPlan(columnInfo, _allExtractionInformations, _allCatalogueItems, joins, lookups, allColumnInfosSystemWide, syntaxHelper,this));
            }
            

            //Remove unplanned columns
            foreach (var col in Plans.Keys.ToArray())
                if (!IsStillNeeded(col))
                    Plans.Remove(col);
        }

        public List<JoinInfo> GetJoinInfosRequiredCatalogue()
        {
            var qb = new QueryBuilder(null, null);
            qb.AddColumnRange(Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
            qb.RegenerateSQL();
            return qb.JoinsUsedInQuery;
        }

        public List<Lookup> GetLookupsRequiredCatalogue()
        {
            var qb = new QueryBuilder(null, null);
            qb.AddColumnRange(Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
            qb.RegenerateSQL();

            return qb.GetDistinctRequiredLookups().ToList();
        }

        private bool IsStillNeeded(ColumnInfo columnInfo)
        {
            return TableInfos.Any(t => t.ID == columnInfo.TableInfo_ID);
        }
    }
}
