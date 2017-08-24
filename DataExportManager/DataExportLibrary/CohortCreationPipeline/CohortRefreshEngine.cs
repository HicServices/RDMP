using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline
{
    public class CohortRefreshEngine
    {
        private readonly IDataLoadEventListener _listener;
        private readonly ExtractionConfiguration _configuration;
        private readonly MEF _mef;
        private CohortIdentificationConfiguration _cic;

        public CohortCreationRequest Request { get; private set; }

        public CohortRefreshEngine(IDataLoadEventListener listener, ExtractionConfiguration configuration, MEF mef)
        {
            _listener = listener;
            _configuration = configuration;
            _mef = mef;
            var origCohort = configuration.Cohort;
            var origCohortData = origCohort.GetExternalData();
            _cic = configuration.CohortIdentificationConfiguration;
            var project = (Project)configuration.Project;

            var definitionForNewCohort = new CohortDefinition(null, origCohortData.ExternalDescription, origCohortData.ExternalVersion + 1, (int)project.ProjectNumber, origCohort.ExternalCohortTable);
            Request = new CohortCreationRequest(project, definitionForNewCohort, (DataExportRepository)configuration.Repository, "Cohort Refresh");
        }

        public void Execute()
        {
            DataFlowPipelineEngineFactory<DataTable> pipelineFactory = new DataFlowPipelineEngineFactory<DataTable>(_mef, CohortCreationRequest.Context);
            
            var engine = pipelineFactory.Create(_configuration.CohortRefreshPipeline, _listener);
            
            var initializationObjects = new List<object>();

            if (_cic != null)
                initializationObjects.Add(_cic);

            initializationObjects.Add(Request);
            engine.Initialize(initializationObjects.ToArray());
            engine.ExecutePipeline(new GracefulCancellationToken());
        }
    }
}
