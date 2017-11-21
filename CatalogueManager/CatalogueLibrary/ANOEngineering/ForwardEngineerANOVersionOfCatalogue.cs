using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.ANOEngineering
{
    public class ForwardEngineerANOVersionOfCatalogue
    {
        private readonly Catalogue _catalogue;
        private ExtractionInformation[] _allExtractionInformations;
        private CatalogueItem[] _allCatalogueItems;
        
        private readonly Dictionary<ColumnInfo,Plan> Plans = new Dictionary<ColumnInfo, Plan>();
        private readonly Dictionary<ColumnInfo, ANOTable> PlannedANOTables = new Dictionary<ColumnInfo, ANOTable>();
        private readonly Dictionary<ColumnInfo, IDilutionOperation> PlannedDilution = new Dictionary<ColumnInfo, IDilutionOperation>();
        private IQuerySyntaxHelper _querySyntaxHelper;

        public List<IDilutionOperation>  DilutionOperations { get; private set; }

        public TableInfo[] TableInfos { get; private set; }
        
        public ForwardEngineerANOVersionOfCatalogue(Catalogue catalogue)
        {
            _catalogue = catalogue;
            _allExtractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            _allCatalogueItems = _catalogue.CatalogueItems.Where(ci=>ci.ColumnInfo_ID != null).ToArray();

            TableInfos =
                _allCatalogueItems.Where(ci => IsMandatoryForMigration(ci.ColumnInfo))
                    .Select(ci => ci.ColumnInfo.TableInfo)
                    .Distinct()
                    .ToArray();

            foreach (ColumnInfo col in TableInfos.SelectMany(t => t.ColumnInfos))
                Plans.Add(col, IsMandatoryForMigration(col) ? Plan.PassThroughUnchanged : Plan.Drop);

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
                throw new ArgumentException("Cannot drop column because it is Mandatory", "col");

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
            if(GetPlanForColumnInfo(col) != Plan.ANO)
                throw new ArgumentException("ColumnInfo '" + col + "' is not planned to be ANO. First call SetPlan ANO", "col");

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
            if (GetPlanForColumnInfo(col) != Plan.Dillute)
                throw new ArgumentException("ColumnInfo '" + col + "' is not planned to be Dilluted. First call SetPlan Dillute", "col");

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
    }
}
