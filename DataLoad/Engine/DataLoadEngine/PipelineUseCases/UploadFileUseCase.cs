using System.Data;
using System.IO;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.DataFlowPipeline.Destinations;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.PipelineUseCases
{
    public class UploadFileUseCase:PipelineUseCase
    {
        private object[] _initializationObjects;

        private DataFlowPipelineContext<DataTable> _context = new DataFlowPipelineContext<DataTable>();

        public UploadFileUseCase(FileInfo file, DiscoveredDatabase targetDatabase)
        {
            _initializationObjects = new object[]
            {
                new FlatFileToLoad(file),
                targetDatabase
            };
            
            GenerateContext();

            _context.MustHaveDestination = typeof (DataTableUploadDestination);
        }

        private void GenerateContext()
        {
            _context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
        }


        public override object[] GetInitializationObjects()
        {
            return _initializationObjects;
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }

        private UploadFileUseCase()
        {
            GenerateContext();

            _initializationObjects = new[]
            {
                typeof (FlatFileToLoad),
                typeof (DiscoveredDatabase)
            };

            IsDesignTime = true;
        }

        public static PipelineUseCase DesignTime()
        {
            return new UploadFileUseCase();
        }
    }
}