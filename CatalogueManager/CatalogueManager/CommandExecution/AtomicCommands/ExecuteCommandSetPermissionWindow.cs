using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetPermissionWindow : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private readonly CacheProgress _cacheProgress;
        private PermissionWindow _window;
        
        public ExecuteCommandSetPermissionWindow(IActivateItems activator, CacheProgress cacheProgress) : base(activator)
        {
            _cacheProgress = cacheProgress;
            _window = null;

            if(!activator.CoreChildProvider.AllPermissionWindows.Any())
                SetImpossible("There are no PermissionWindows created yet");
        }

        public override void Execute()
        {
            base.Execute();

            if(_window == null)
                _window = SelectOne<PermissionWindow>(Activator.RepositoryLocator.CatalogueRepository);

            if(_window == null)
                return;

            _cacheProgress.PermissionWindow_ID = _window.ID;
            _cacheProgress.SaveToDatabase();

            Publish(_cacheProgress);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var window = target as PermissionWindow;
            if (window != null)
                _window = window;

            return this;
        }
    }
}