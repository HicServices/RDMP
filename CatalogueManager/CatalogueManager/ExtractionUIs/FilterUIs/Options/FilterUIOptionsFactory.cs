using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using DataExportLibrary.Data.DataTables;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public class FilterUIOptionsFactory
    {
        public FilterUIOptions Create(IFilter filter)
        {
            var aggregateFilter = filter as AggregateFilter;
            var deployedExtractionFilter = filter as DeployedExtractionFilter;
            var masterCatalogueFilter = filter as ExtractionFilter;

            if (aggregateFilter != null)
                return new AggregateFilterUIOptions(aggregateFilter);

            if (deployedExtractionFilter != null)
                return new DeployedExtractionFilterUIOptions(deployedExtractionFilter);

            if (masterCatalogueFilter != null)
                return new ExtractionFilterUIOptions(masterCatalogueFilter);

            throw new Exception("Expected IFilter '" + filter +
                                    "' to be either an AggregateFilter, DeployedExtractionFilter or a master ExtractionFilter but it was " +
                                    filter.GetType().Name);
        }
    }
}
