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
    internal class ExecuteCommandSetPermissionWindow : BasicUICommandExecution,IAtomicCommand
    {
        private readonly CacheProgress _cacheProgress;
        private readonly PermissionWindow _window;

        public ExecuteCommandSetPermissionWindow(IActivateItems activator, CacheProgress cacheProgress,PermissionWindow window) : base(activator)
        {
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

            Publish(_cacheProgress);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PermissionWindow, OverlayKind.Link);
        }
    }
}