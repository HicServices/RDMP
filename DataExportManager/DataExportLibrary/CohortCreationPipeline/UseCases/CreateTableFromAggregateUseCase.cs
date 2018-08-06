using System.Collections.Generic;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.CohortCreationPipeline.Sources;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.DataFlowPipeline.Destinations;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.UseCases
{
    /// <summary>
    /// Use case which describes creating a new table in the database containing all rows matched by the <see cref="AggregateConfiguration"/>.
    /// The source is fixed the destination and middle components are open.
    /// </summary>
    public class CreateTableFromAggregateUseCase:PipelineUseCase
    {
        private DataFlowPipelineContext<DataTable> _context;
        private object[] _initializationObjects;

        /// <summary>
        /// Defines a new use case in which the given <see cref="AggregateConfiguration"/> will be turned into an SQL query and used to generate rows
        /// that will be released into the pipeline.  The source is fixed the destination and middle components are open.
        /// </summary>
        /// <param name="aggregateConfiguration">The aggregate query that will be run to generate the rows</param>
        /// <param name="constrainByCohort">Only applies if <see cref="aggregateConfiguration"/> is a patient index table, specifying a cohort will only commit rows 
        /// in which the patient id appears in the cohort</param>
        /// <param name="table">The destination table in which to put the matched records.
        /// <para> (table does not have to exist yet, you can use <see cref="DiscoveredDatabase.ExpectTable"/> to obtain a reference to a non existant table)</para></param>
        public CreateTableFromAggregateUseCase(AggregateConfiguration aggregateConfiguration, ExtractableCohort constrainByCohort, DiscoveredTable table)
        {
            var initializationObjects = new List<object>();
            initializationObjects.Add(aggregateConfiguration);
            
            GenerateContext();

            if (constrainByCohort == null)
            {
                var src = new AggregateConfigurationTableSource();
                src.PreInitialize(aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                src.TableName = table.GetRuntimeName();
                ExplicitSource = src;
            }
            else
            {
                initializationObjects.Add(constrainByCohort);

                var src = new PatientIndexTableSource();
                src.PreInitialize(aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                src.PreInitialize(constrainByCohort, new ThrowImmediatelyDataLoadEventListener());
                src.TableName = table.GetRuntimeName();
                ExplicitSource = src;
            }

            initializationObjects.Add(aggregateConfiguration.Repository);
            initializationObjects.Add(table.Database);

            _initializationObjects = initializationObjects.ToArray();
        }

        private void GenerateContext()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            _context = contextFactory.Create(PipelineUsage.FixedSource);
            _context.MustHaveDestination = typeof(DataTableUploadDestination);
        }

        public override object[] GetInitializationObjects()
        {
            return _initializationObjects;
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }

        private CreateTableFromAggregateUseCase()
        {
            IsDesignTime = true;
            
            GenerateContext();

            ExplicitSource = new AggregateConfigurationTableSource();

            _initializationObjects = new object[]
            {
                typeof(AggregateConfiguration),
                typeof(ExtractableCohort),
                typeof(DiscoveredDatabase),
                typeof(ICatalogueRepository)

            };
        }

        public static PipelineUseCase DesignTime(CatalogueRepository catalogueRepository)
        {
            return new CreateTableFromAggregateUseCase();
        }
    }
}