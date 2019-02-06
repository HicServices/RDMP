using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandActivate : BasicUICommandExecution,IAtomicCommand
    {
        private readonly object _o;
        
        public ExecuteCommandActivate(IActivateItems activator, object o) : base(activator)
        {    
            _o = o;

            var masquerader = _o as IMasqueradeAs;

            //if we have a masquerader and we cannot activate the masquerader, maybe we can activate what it is masquerading as?
            if (masquerader != null && !Activator.CommandExecutionFactory.CanActivate(masquerader))
                _o = masquerader.MasqueradingAs();

            if(!Activator.CommandExecutionFactory.CanActivate(_o))
                SetImpossible("Object cannot be Activated");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            if (_o == null)
                return null;

            return iconProvider.GetImage(_o, OverlayKind.Edit);
        }

        public override string GetCommandName()
        {
            return "Edit";
        }

        public override void Execute()
        {
            base.Execute();

            Activator.CommandExecutionFactory.Activate(_o);
        }
    }
}