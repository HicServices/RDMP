using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandPin : BasicUICommandExecution,IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandPin(IActivateItems activator, DatabaseEntity databaseEntity) : base(activator)
        {
            _databaseEntity = databaseEntity;

            if(!CollectionPinFilterUI.IsPinnableType(databaseEntity))
                SetImpossible("Object is not a Pinnable Type");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_databaseEntity,1){Pin = true});
        }
    }
}