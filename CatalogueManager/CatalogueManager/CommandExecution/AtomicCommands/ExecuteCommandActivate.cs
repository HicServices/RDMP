using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandActivate : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly object _o;

        public ExecuteCommandActivate(IActivateItems activator, object o)
        {
            _activator = activator;
            _o = o;

            if(!activator.CommandExecutionFactory.CanActivate(o))
                SetImpossible("Object cannot be Activated");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(_o, OverlayKind.Edit);
        }

        public override string GetCommandName()
        {
            return "Edit";
        }

        public override void Execute()
        {
            base.Execute();

            _activator.CommandExecutionFactory.Activate(_o);
        }
    }
}