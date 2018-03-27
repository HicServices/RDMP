using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks
{
    /// <summary>
    /// Checks an IPipeline (persisted data flow pipeline configuration) to see if all it's components are constructable (using MEFChecker)
    /// </summary>
    public class PipelineChecker : ICheckable
    {
        private readonly IPipeline _pipeline;

        /// <summary>
        /// Sets up the checker to check the supplied pipeline
        /// </summary>
        /// <param name="pipeline"></param>
        public PipelineChecker(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }
        
        /// <summary>
        /// Checks that all the components defined in the pipeline are found using a MEFChecker.  This will also handle classes changing namespaces by updating
        /// class name reference.
        /// </summary>
        /// <param name="notifier"></param>
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
