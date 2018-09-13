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
    public sealed class CreateTableFromAggregateUseCase:PipelineUseCase
    {
        /// <summary>
        /// Defines a new use case in which the given <see cref="AggregateConfiguration"/> will be turned into an SQL query and used to generate rows
        /// that will be released into the pipeline.  The source is fixed the destination and middle components are open.
        /// </summary>
        /// <param name="aggregateConfiguration">The aggregate query that will be run to generate the rows</param>
        /// <param name="constrainByCohort">Only applies if <see cref="AggregateConfiguration"/> is a patient index table, specifying a cohort will only commit rows 
        /// in which the patient id appears in the cohort</param>
        /// <param name="table">The destination table in which to put the matched records.
        /// <para> (table does not have to exist yet, you can use <see cref="DiscoveredDatabase.ExpectTable"/> to obtain a reference to a non existant table)</para></param>
        public CreateTableFromAggregateUseCase(AggregateConfiguration aggregateConfiguration, ExtractableCohort constrainByCohort, DiscoveredTable table)
        {
            if (constrainByCohort == null)
            {
                var src = new AggregateConfigurationTableSource();
                src.PreInitialize(aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                src.TableName = table.GetRuntimeName();
                ExplicitSource = src;
            }
            else
            {
                AddInitializationObject(constrainByCohort);

                var src = new PatientIndexTableSource();
                src.PreInitialize(aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                src.PreInitialize(constrainByCohort, new ThrowImmediatelyDataLoadEventListener());
                src.TableName = table.GetRuntimeName();
                ExplicitSource = src;
            }
            
            AddInitializationObject(aggregateConfiguration);
            AddInitializationObject(aggregateConfiguration.Repository);
            AddInitializationObject(table.Database);

            GenerateContext();
        }

        protected override IDataFlowPipelineContext GenerateContextImpl()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedSource);
            context.MustHaveDestination = typeof(DataTableUploadDestination);
            
            return context;
        }

        /// <summary>
        /// Design time types
        /// </summary>
        private CreateTableFromAggregateUseCase()
            : base(new[]{typeof(AggregateConfiguration),
                typeof(ExtractableCohort),
                typeof(DiscoveredDatabase),
                typeof(ICatalogueRepository)})
        {
            ExplicitSource = new AggregateConfigurationTableSource();
            GenerateContext();
        }

        public static PipelineUseCase DesignTime(CatalogueRepository catalogueRepository)
        {
            return new CreateTableFromAggregateUseCase();
        }
    }
}