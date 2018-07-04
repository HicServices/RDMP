using System.Drawing;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShow : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IMapsDirectlyToDatabaseTable _objectToShow;
        private readonly int _expansionDepth;

        public ExecuteCommandShow(IActivateItems activator,IMapsDirectlyToDatabaseTable objectToShow, int expansionDepth):base(activator)
        {
            _objectToShow = objectToShow;
            _expansionDepth = expansionDepth;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_objectToShow,_expansionDepth));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}