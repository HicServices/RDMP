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

        public CohortCreationRequest Request { get; private set; }

        public CohortRefreshEngine(IDataLoadEventListener listener, ExtractionConfiguration configuration)
        {
            _listener = listener;
            _configuration = configuration;
            Request = new CohortCreationRequest(configuration);
        }

        public void Execute()
        {
            Request.GetEngine(_configuration.CohortRefreshPipeline,_listener).ExecutePipeline(new GracefulCancellationToken());
        }
    }
}
