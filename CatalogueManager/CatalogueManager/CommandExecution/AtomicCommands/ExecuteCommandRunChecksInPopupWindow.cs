using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunChecksInPopupWindow : BasicUICommandExecution, IAtomicCommand
    {
        private ICheckable _checkable;

        public ExecuteCommandRunChecksInPopupWindow(IActivateItems activator, ICheckable checkable) : base(activator)
        {
            _checkable = checkable;
        }

        public override string GetCommandName()
        {
            return "Run Checks ("+ _checkable.GetType().Name +")";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Warning;
        }

        public override void Execute()
        {
            base.Execute();
            var p = new PopupChecksUI(GetCommandName(),false);
            p.StartChecking(_checkable);
        }
    }
}