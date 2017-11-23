using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.ANOEngineering
{
    public class ForwardEngineerANOCataloguePlanManager : ICheckable
    {
        public Catalogue Catalogue { get; private set; }
        private ExtractionInformation[] _allExtractionInformations;
        private CatalogueItem[] _allCatalogueItems;
        
        private readonly Dictionary<ColumnInfo,Plan> Plans = new Dictionary<ColumnInfo, Plan>();
        private readonly Dictionary<ColumnInfo, ANOTable> PlannedANOTables = new Dictionary<ColumnInfo, ANOTable>();
        private readonly Dictionary<ColumnInfo, IDilutionOperation> PlannedDilution = new Dictionary<ColumnInfo, IDilutionOperation>();
        private IQuerySyntaxHelper _querySyntaxHelper;

        public List<IDilutionOperation>  DilutionOperations { get; private set; }

        public TableInfo[] TableInfos { get; private set; }

        public DiscoveredDatabase TargetDatabase { get; set; }

        public HashSet<TableInfo> SkippedTables = new HashSet<TableInfo>();

        public ForwardEngineerANOCataloguePlanManager(Catalogue catalogue)
        {
            Catalogue = catalogue;

            RefreshTableInfos();

            DilutionOperations = new List<IDilutionOperation>();
            
            ObjectConstructor constructor = new ObjectConstructor();

            foreach (var operationType in ((CatalogueRepository) catalogue.Repository).MEF.GetTypes<IDilutionOperation>())
                DilutionOperations.Add((IDilutionOperation) constructor.Construct(operationType));
            
            _querySyntaxHelper = TableInfos.Select(t => t.GetQuerySyntaxHelper()).FirstOrDefault();
        }

        public string GetEndpointDataType(ColumnInfo col)
        {
            switch (GetPlanForColumnInfo(col))
            {
                case Plan.Drop:
                    return null;
                case Plan.ANO:
                    var anoTable = GetPlannedANOTable(col);

                    if (anoTable == null)
                        return "Unknown";

                    return anoTable.GetRuntimeDataType(LoadStage.PostLoad);
                case Plan.Dillute:
                    var dilution = GetPlannedDilution(col);

                    if (dilution == null)
                        return "Unknown";
                    
                    return _querySyntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(dilution.ExpectedDestinationType);

                case Plan.PassThroughUnchanged:
                    return col.Data_type;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Plan GetPlanForColumnInfo(ColumnInfo col)
        {
            if(!Plans.ContainsKey(col))
                return Plan.Drop;

            return Plans[col];
        }
        
        public void SetPlan(ColumnInfo col, Plan plan)
        {
            var oldPlan = GetPlanForColumnInfo(col);

            //no change
            if(oldPlan == plan)
                return;

            if (IsMandatoryForMigration(col) && plan == Plan.Drop)
                throw new ArgumentException("Cannot drop column '" + col + "' because it is Mandatory (An ExtractionInformation exists for it in Catalogue(s))", "col");

            //change plan
            Plans[col] = plan;

            //Set diluteness dictionary key depending on plan
            if(plan == Plan.Dillute)
                PlannedDilution.Add(col,null);
            else if (PlannedDilution.ContainsKey(col))
                PlannedDilution.Remove(col);
            
            //Set ANO dictionary key depending on plan
            if(plan == Plan.ANO)
                PlannedANOTables.Add(col,null);
            else
                if(PlannedANOTables.ContainsKey(col)) //plan is not to ANO
                    PlannedANOTables.Remove(col);
        }

        public ANOTable GetPlannedANOTable(ColumnInfo col)
        {
            if (GetPlanForColumnInfo(col) == Plan.ANO)
                return PlannedANOTables[col];

            return null;
        }

        public void SetPlannedANOTable(ColumnInfo col, ANOTable anoTable)
        {
            SetPlan(col,Plan.ANO);
            
            PlannedANOTables[col] = anoTable;
        }

        public IDilutionOperation GetPlannedDilution(ColumnInfo ci)
        {
            if (PlannedDilution.ContainsKey(ci))
                return PlannedDilution[ci];

            return null;
        }

        public void SetPlannedDilution(ColumnInfo col, IDilutionOperation operation)
        {
            SetPlan(col,Plan.Dillute);
            
            PlannedDilution[col] = operation;
        }
        
        public bool IsMandatoryForMigration(ColumnInfo col)
        {
            return 
                //ColumnInfo is part of the table primary key
                col.IsPrimaryKey ||

                //there are CatalogueItems that reference the ColumnInfo and those CatalogueItems are extractable (have associated ExtractionInformation)
                   _allCatalogueItems.Any(
                       ci =>
                           ci.ColumnInfo_ID == col.ID && 
                           _allExtractionInformations.Any(ei => ei.CatalogueItem_ID == ci.ID));
        }

        public enum Plan
        {
            Drop,
            ANO,
            Dillute,
            PassThroughUnchanged
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

            foreach (TableInfo tableInfo in toMigrateTables)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Evaluating TableInfo '" + tableInfo + "'",CheckResult.Success));

                if (TargetDatabase != null && TargetDatabase.ExpectTable(tableInfo.GetRuntimeName()).Exists())
                    notifier.OnCheckPerformed(new CheckEventArgs("Table '" + tableInfo + "' already exists in Database '" + TargetDatabase + "'",CheckResult.Fail));

                var pks = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).ToArray();

                if(!pks.Any())
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo '" + tableInfo + "' does not have any Primary Keys, it cannot be anonymised", CheckResult.Fail));

                foreach (ColumnInfo pk in pks)
                {
                    var plan = GetPlanForColumnInfo(pk);

                    if (plan == Plan.Dillute || plan == Plan.Drop)
                        notifier.OnCheckPerformed(new CheckEventArgs("Current plan for column '" + pk + "' ('" + plan +"') is invalid because it is a primary key", CheckResult.Fail));
                }

                if (tableInfo.IsTableValuedFunction)
                    notifier.OnCheckPerformed(new CheckEventArgs("TableInfo '" + tableInfo + "' is an IsTableValuedFunction so cannot be anonymised",CheckResult.Fail));
            }

            foreach (KeyValuePair<ColumnInfo, ANOTable> kvp in PlannedANOTables.Where(k=>k.Value == null))
                notifier.OnCheckPerformed(new CheckEventArgs("No ANOTable has been picked for ColumnInfo '" + kvp.Key + "'",CheckResult.Fail));

            foreach (KeyValuePair<ColumnInfo, IDilutionOperation> kvp in PlannedDilution.Where(k => k.Value == null))
                notifier.OnCheckPerformed(new CheckEventArgs("No Dilution Operation has been picked for ColumnInfo '" + kvp.Key + "'", CheckResult.Fail));
                
            foreach (KeyValuePair<ColumnInfo, Plan> kvp in Plans)
            {
                if (kvp.Value != Plan.Drop)
                {
                    try
                    {
                        var datatype = GetEndpointDataType(kvp.Key);
                        notifier.OnCheckPerformed(new CheckEventArgs("Determined endpoint data type '"+datatype+"' for ColumnInfo '" + kvp.Key + "'", CheckResult.Success));

                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not determine endpoint data type for ColumnInfo '" + kvp.Key + "'", CheckResult.Fail,e));
                    }
                }
            }
        }

        /// <summary>
        /// Re checks the TableInfos associated with the Catalogue incase some have changed
        /// </summary>
        public void RefreshTableInfos()
        {
            _allExtractionInformations = Catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            _allCatalogueItems = Catalogue.CatalogueItems.Where(ci => ci.ColumnInfo_ID != null).ToArray();

            TableInfos =
                _allCatalogueItems.Where(ci => IsMandatoryForMigration(ci.ColumnInfo))
                    .Select(ci => ci.ColumnInfo.TableInfo)
                    .Distinct()
                    .ToArray();

            //Remove unplanned columns
            foreach (var col in Plans.Keys.ToArray())
                if (!IsStillNeeded(col))
                    Plans.Remove(col);

            foreach (var col in PlannedANOTables.Keys.ToArray())
                if (!IsStillNeeded(col))
                    PlannedANOTables.Remove(col);

            foreach (var col in PlannedDilution.Keys.ToArray())
                if (!IsStillNeeded(col))
                    PlannedDilution.Remove(col);

            //Add new column infos
            foreach (ColumnInfo col in TableInfos.SelectMany(t => t.ColumnInfos))
                if(!Plans.ContainsKey(col))
                    Plans.Add(col, IsMandatoryForMigration(col) ? Plan.PassThroughUnchanged : Plan.Drop);
        }

        private bool IsStillNeeded(ColumnInfo columnInfo)
        {
            return TableInfos.Any(t => t.ID == columnInfo.TableInfo_ID);
        }
    }
}
