using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced.Options;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public class AggregateFilterUIOptions : FilterUIOptions
    {
        private ISqlParameter[] _globals;
        private TableInfo[] _tables;
        private IColumn[] _columns;

        public AggregateFilterUIOptions(AggregateFilter aggregateFilter):base(aggregateFilter)
        {
            var aggregateConfiguration = aggregateFilter.GetAggregate();

            if(aggregateConfiguration == null)
                throw new Exception("AggregateFilter '" + aggregateFilter + "' (ID="+aggregateFilter.ID+") does not belong to any AggregateConfiguration, is it somehow an orphan?");
            
            //it part of an AggregateConfiguration so get the same factory that is used by AggregateEditorUI to tell us about the globals and the columns
            var options = new AggregateEditorOptionsFactory().Create(aggregateConfiguration);
            _globals = options.GetAllParameters(aggregateConfiguration);

            //get all the tables 
            _tables = aggregateConfiguration.Catalogue.GetTableInfoList(true);

            //but also add the ExtractionInformations and AggregateDimensions - in the case of PatientIndex table join usages (duplicates are ignored by _autoCompleteProvider)
            _columns = options.GetAvailableWHEREColumns(aggregateConfiguration);
        }

        public override TableInfo[] GetTableInfos()
        {
            return _tables;
        }

        public override ISqlParameter[] GetGlobalParametersInFilterScope()
        {
            return _globals;
        }

        public override IColumn[] GetIColumnsInFilterScope()
        {
            return _columns;
        }
    }
}