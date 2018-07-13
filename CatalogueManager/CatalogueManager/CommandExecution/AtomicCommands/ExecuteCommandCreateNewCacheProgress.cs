using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

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

        public override string GetCommandHelp()
        {
            return "Defines that the load requires data that is intensive/expensive to fetch and that this fetching and storing to disk should happen independently of the loading";
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