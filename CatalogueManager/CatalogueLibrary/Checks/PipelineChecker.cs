using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks
{
    public class PipelineChecker : ICheckable
    {
        private readonly IPipeline _pipeline;

        public PipelineChecker(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }
        
        public void Check(ICheckNotifier notifier)
        {
            var mef = ((CatalogueRepository) _pipeline.Repository).MEF;
            
            foreach (var component in _pipeline.PipelineComponents)
            {
                var copy = component;
                var mefChecker = new MEFChecker(mef, component.Class, delegate(string s) { copy.Class = s; copy.SaveToDatabase(); });
                mefChecker.Check(notifier);
            }
        }
    }
}
