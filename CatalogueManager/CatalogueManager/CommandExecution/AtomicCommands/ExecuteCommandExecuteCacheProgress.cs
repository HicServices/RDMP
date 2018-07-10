using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LoadExecutionUIs;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteCacheProgress:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private CacheProgress _cp;

        [ImportingConstructor]
        public ExecuteCommandExecuteCacheProgress(IActivateItems activator, CacheProgress cp) : base(activator)
        {
            _cp = cp;
        }
        
        public ExecuteCommandExecuteCacheProgress(IActivateItems activator)
            : base(activator)
        {

        }

        public override string GetCommandHelp()
        {
            return "Runs the caching activity.  This usually involves long term incremental fetching and storing data ready for load";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _cp = (CacheProgress) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_cp == null)
                _cp = SelectOne<CacheProgress>(Activator.RepositoryLocator.CatalogueRepository);
            
            if(_cp == null)
                return;
            
            Activator.Activate<ExecuteCacheProgressUI, CacheProgress>(_cp);
        }
    }
}
