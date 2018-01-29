using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUnpin : BasicUICommandExecution, IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;

        public ExecuteCommandUnpin(IActivateItems activator, DatabaseEntity databaseEntity)
            : base(activator)
        {
            _databaseEntity = databaseEntity;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_databaseEntity, int.MaxValue) { Pin = false });
        }
    }
}