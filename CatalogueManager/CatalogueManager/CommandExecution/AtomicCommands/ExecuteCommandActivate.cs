using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandActivate : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandActivate(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _activator = activator;
            _databaseEntity = databaseEntity;

            if(!activator.CommandExecutionFactory.CanActivate(databaseEntity))
                SetImpossible("Object cannot be Activated");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(_databaseEntity, OverlayKind.Edit);
        }

        public override void Execute()
        {
            base.Execute();

            _activator.CommandExecutionFactory.Activate(_databaseEntity);
        }
    }
}