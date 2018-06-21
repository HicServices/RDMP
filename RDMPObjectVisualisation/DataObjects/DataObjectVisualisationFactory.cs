using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace RDMPObjectVisualisation.DataObjects
{
    public class DataObjectVisualisationFactory
    {
        public Control Create(object value)
        {
            var type = value.GetType();
            if(type == typeof(FlatFileToLoad))
                return new FlatFileToLoadVisualisation((FlatFileToLoad)value);

            if (type == typeof(ExtractableCohort))
                return new ExtractableCohortVisualisation((ExtractableCohort)value);

            if (IsGenericType(value.GetType(), typeof (IDataFlowSource<>)))
                return new DataFlowComponentVisualisation(DataFlowComponentVisualisation.Role.Source, value,null);

            if (IsGenericType(value.GetType(), typeof(IDataFlowDestination<>)))
                return new DataFlowComponentVisualisation(DataFlowComponentVisualisation.Role.Destination, value, null);

            if (IsGenericType(value.GetType(), typeof(IDataFlowComponent<>)))
                return new DataFlowComponentVisualisation(DataFlowComponentVisualisation.Role.Middle, value, null);
            
            if (type == typeof(DiscoveredDatabase))
                return new DiscoveredDatabaseVisualisation((DiscoveredDatabase)value);

            if (type == typeof (CohortCreationRequest))
                return new CohortCreationRequestVisualisation((CohortCreationRequest)value);
            
            if(type == typeof(CohortIdentificationConfiguration))
                return new CohortIdentificationConfigurationVisualisation((CohortIdentificationConfiguration) value);

            if(type == typeof(AggregateConfiguration))
                return new AggregateConfigurationVisualisation((AggregateConfiguration) value);//if we expand into visualizing non patient index tables then we will have to evaluate the value to see what type of aggregate it is (identifier list, graphable, patient index table etc)

            if (type == typeof (ExtractionInformation))
                return new ExtractionInformationVisualisation((ExtractionInformation) value);

            return new UnknownObjectVisualisation(value);
        }

        private bool IsGenericType(Type toCheck, Type genericType)
        {
            return toCheck.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
        }
    }
}
