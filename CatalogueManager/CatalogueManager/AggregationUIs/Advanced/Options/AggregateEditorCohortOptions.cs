using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode;

namespace CatalogueManager.AggregationUIs.Advanced.Options
{
    public class AggregateEditorCohortOptions: IAggregateEditorOptions
    {
        private readonly ISqlParameter[] _globals;

        public AggregateEditorCohortOptions(ISqlParameter[] globals)
        {
            _globals = globals;
        }

        public string GetTitleTextPrefix(AggregateConfiguration aggregate)
        {
            if (aggregate.IsJoinablePatientIndexTable())
                return "Patient Index Table:";

            return "Cohort Identification Set:";
        }

        public string GetRefreshSQL(AggregateConfiguration aggregate)
        {
            return new CohortQueryBuilder(aggregate, _globals, aggregate.IsJoinablePatientIndexTable()).SQL;
        }

        public IColumn[] GetAvailableSELECTColumns(AggregateConfiguration aggregate)
        {
            //get the existing dimensions
            var alreadyExisting = aggregate.AggregateDimensions.ToArray();
            
            //get novel ExtractionInformations from the catalogue for which there are not already any Dimensions
            var candidates = aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(e => alreadyExisting.All(d => d.ExtractionInformation_ID != e.ID)).ToArray();

            //patient index tables can have any columns
            if (aggregate.IsJoinablePatientIndexTable())
                return candidates;

            //otherwise only return the patient identifier column(s) - for example Marriages dataset would have Partner1Identifier and Partner2Identifier
            return candidates.Where(c => c.IsExtractionIdentifier).ToArray();
        }

        public IColumn[] GetAvailableWHEREColumns(AggregateConfiguration aggregate)
        {
            var toReturn = new List<IColumn>();
            
            toReturn.AddRange(aggregate.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            //for each joined PatientIdentifier table
            foreach (JoinableCohortAggregateConfigurationUse usedJoinable in aggregate.PatientIndexJoinablesUsed)
            {
                var tableAlias = usedJoinable.GetJoinTableAlias();
                IColumn[] hackedDimensions = usedJoinable.JoinableCohortAggregateConfiguration.AggregateConfiguration.AggregateDimensions.Cast<IColumn>().ToArray();

                //change the SelectSQL to the table alias of the joinable used (see CohortQueryBuilder.AddJoinablesToBuilder)
                foreach (var dimension in hackedDimensions)
                    dimension.SelectSQL = tableAlias + "." + dimension.GetRuntimeName();
                
                toReturn.AddRange(hackedDimensions);
            }

            return toReturn.ToArray();
        }

        public bool ShouldBeEnabled(AggregateEditorSection section, AggregateConfiguration aggregate)
        {
            switch (section)
            {
                case AggregateEditorSection.ExtractableTickBox:
                    return false;
                case AggregateEditorSection.SELECT:
                    return true;
                case AggregateEditorSection.TOPX:
                    return false;
                case AggregateEditorSection.FROM:
                    return true;
                case AggregateEditorSection.WHERE:
                    return true;
                case AggregateEditorSection.HAVING:
                    return true;
                case AggregateEditorSection.PIVOT:
                    return false;
                case AggregateEditorSection.AXIS:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }
        }

        public IMapsDirectlyToDatabaseTable[] GetAvailableJoinables(AggregateConfiguration aggregate)
        {
            var existingForcedJoinTables = aggregate.ForcedJoins;
            
            var existingDimensions = aggregate.AggregateDimensions;
            var existingTablesAlreadyReferenced = existingDimensions.Select(d => d.ColumnInfo.TableInfo).Distinct();

            var availableTableInfos = aggregate.Catalogue.GetTableInfoList(true);

            List<IMapsDirectlyToDatabaseTable> toReturn = new List<IMapsDirectlyToDatabaseTable>();
            
            //They can add TableInfos that have not been referenced yet by the columns or already been configured as an explicit force join
            toReturn.AddRange(availableTableInfos.Except(existingTablesAlreadyReferenced.Union(existingForcedJoinTables)));

            //if it is a patient index table itself then that's all folks
            if (aggregate.IsJoinablePatientIndexTable())
                return toReturn.ToArray();

            //it's not a patient index table itself so it can reference other patient index tables in the configuration
            var config = aggregate.GetCohortIdentificationConfigurationIfAny();

            //If this returns null then it means someone deleted it out of the configuration while you were editing it?
            if(config == null)
                throw new NotSupportedException("Aggregate " + aggregate + " did not return it's CohortIdentificationConfiguration correctly, did someone delete the configuration or Orphan this AggregateConfiguration while you weren't looking?");

            //find those that are already referenced
            var existingJoinables = aggregate.PatientIndexJoinablesUsed.Select(u=>u.JoinableCohortAggregateConfiguration);

            //return also these which are available for use but not yet linked in
            toReturn.AddRange(config.GetAllJoinables().Except(existingJoinables));

            return toReturn.ToArray();
        }

        public ISqlParameter[] GetAllParameters(AggregateConfiguration aggregate)
        {
            var parameterManager = new ParameterManager();
            foreach (var p in _globals)
                parameterManager.AddGlobalParameter(p);
            
            parameterManager.AddParametersFor(aggregate, ParameterLevel.QueryLevel);

            return parameterManager.GetFinalResolvedParametersList().ToArray();
        }

        public CountColumnRequirement GetCountColumnRequirement(AggregateConfiguration aggregate)
        {
            return aggregate.IsJoinablePatientIndexTable()
                ? CountColumnRequirement.CanOptionallyHaveOne
                : CountColumnRequirement.CannotHaveOne;
        }
    }
}
