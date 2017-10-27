using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddCachingSupportToLoadProgress : BasicUICommandExecution,IAtomicCommand
    {
        private readonly LoadProgress _loadProgress;

        public ExecuteCommandAddCachingSupportToLoadProgress(IActivateItems activator, LoadProgress loadProgress) : base(activator)
        {
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
                new CacheProgress(Activator.RepositoryLocator.CatalogueRepository, _loadProgress);
                _loadProgress.SaveToDatabase();
            }
            
            Publish(_loadProgress);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Add);
        }
    }
}