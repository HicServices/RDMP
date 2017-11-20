using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.ANOEngineering
{
    public class ForwardEngineerANOVersionOfCatalogue
    {
        private readonly Catalogue _catalogue;
        private ExtractionInformation[] _allExtractionInformations;
        private CatalogueItem[] _allCatalogueItems;
        
        private readonly Dictionary<ColumnInfo,Plan> Plans = new Dictionary<ColumnInfo, Plan>();
        private readonly Dictionary<ColumnInfo, ANOTable> PlannedANOTables = new Dictionary<ColumnInfo, ANOTable>();
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
        }

        public Plan GetPlanForColumnInfo(ColumnInfo col)
        {
            if(!Plans.ContainsKey(col))
                return Plan.Drop;

            return Plans[col];
        }

        public ANOTable GetPlannedANOTable(ColumnInfo col)
        {
            if (GetPlanForColumnInfo(col) == Plan.ANO)
                return PlannedANOTables[col];

            return null;
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
            
            //no longer planning to ANO
            if (oldPlan == Plan.ANO)
                PlannedANOTables.Remove(col);
            
            //new plan is to ANO
            if(plan == Plan.ANO)
                PlannedANOTables.Add(col,null);
        }

        public void SetPlannedANOTable(ColumnInfo col, ANOTable anoTable)
        {
            if(GetPlanForColumnInfo(col) != Plan.ANO)
                throw new ArgumentException("ColumnInfo '" + col + "' is not planned to be ANO. First call SetPlan ANO", "col");

            PlannedANOTables[col] = anoTable;

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
