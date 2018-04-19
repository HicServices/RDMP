using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCacheProgress : BasicUICommandExecution,IAtomicCommand
    {
        private readonly LoadProgress _loadProgress;

        public ExecuteCommandCreateNewCacheProgress(IActivateItems activator, LoadProgress loadProgress) : base(activator)
        {
            _loadProgress = loadProgress;

            if(_loadProgress.CacheProgress != null)
                SetImpossible("LoadProgress already has a CacheProgress associated with it");
        }

        public override void Execute()
        {
            base.Execute();

            // If the LoadProgress doesn't have a corresponding CacheProgress, create it
            var cp = new CacheProgress(Activator.RepositoryLocator.CatalogueRepository, _loadProgress);
            
            Publish(_loadProgress);
            Emphasise(cp);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Add);
        }
    }
}