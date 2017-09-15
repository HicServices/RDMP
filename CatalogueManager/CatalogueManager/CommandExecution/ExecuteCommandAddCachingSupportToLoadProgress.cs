using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddCachingSupportToLoadProgress : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly LoadProgress _loadProgress;

        public ExecuteCommandAddCachingSupportToLoadProgress(IActivateItems activator, LoadProgress loadProgress)
        {
            _activator = activator;
            _loadProgress = loadProgress;

            if(_loadProgress.CacheProgress != null)
                SetImpossible("LoadProgress already has a CacheProgress associated with it");
        }

        public override void Execute()
        {
            base.Execute();

            var cacheProgress = _loadProgress.CacheProgress;

            // If the LoadProgress doesn't have a corresponding CacheProgress, create it
            if (cacheProgress == null)
            {
                cacheProgress = new CacheProgress(_activator.RepositoryLocator.CatalogueRepository, _loadProgress);
                _loadProgress.SaveToDatabase();
            }
            
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_loadProgress));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Add);
        }
    }
}