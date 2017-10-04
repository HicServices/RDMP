using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetPermissionWindow : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly CacheProgress _cacheProgress;
        private readonly PermissionWindow _window;

        public ExecuteCommandSetPermissionWindow(IActivateItems activator, CacheProgress cacheProgress,PermissionWindow window)
        {
            _activator = activator;
            _cacheProgress = cacheProgress;
            _window = window;
        }

        public override string GetCommandName()
        {
            return _window.Name;
        }

        public override void Execute()
        {
            base.Execute();

            _cacheProgress.PermissionWindow_ID = _window.ID;
            _cacheProgress.SaveToDatabase();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_cacheProgress));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Link);
        }
    }
}