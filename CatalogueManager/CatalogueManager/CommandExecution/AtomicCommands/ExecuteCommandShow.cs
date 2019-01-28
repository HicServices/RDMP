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
        private readonly bool _useIconAndTypeName;

        public ExecuteCommandShow(IActivateItems activator, IMapsDirectlyToDatabaseTable objectToShow, int expansionDepth, bool useIconAndTypeName=false):base(activator)
        {
            _objectToShow = objectToShow;
            _expansionDepth = expansionDepth;
            _useIconAndTypeName = useIconAndTypeName;
        }

        public override string GetCommandName()
        {
            return _useIconAndTypeName? "Show " + _objectToShow.GetType().Name :base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_objectToShow,_expansionDepth));
        }

        public override string GetCommandHelp()
        {
            return "Opens the containing toolbox collection and shows the object";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return _useIconAndTypeName? iconProvider.GetImage(_objectToShow):null;
        }
    }
}