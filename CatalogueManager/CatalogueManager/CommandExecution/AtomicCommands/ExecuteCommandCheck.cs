using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCheck : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ICheckable _checkable;

        public ExecuteCommandCheck(IActivateItems activator, ICheckable checkable): base(activator)
        {
            _checkable = checkable;
        }

        public override void Execute()
        {
            base.Execute();

            var popupChecksUI = new PopupChecksUI("Checking " + _checkable, false);
            popupChecksUI.StartChecking(_checkable);
        }


        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.TinyYellow;
        }
    }
}