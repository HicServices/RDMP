using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandDisableOrEnable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IDisableable _target;

        public ExecuteCommandDisableOrEnable(IActivateItems itemActivator, IDisableable target):base(itemActivator)
        {
            _target = target;
        }

        public override void Execute()
        {
            base.Execute();

            _target.IsDisabled = !_target.IsDisabled;
            _target.SaveToDatabase();
            Publish((DatabaseEntity)_target);
        }

        public override string GetCommandName()
        {
            return _target.IsDisabled ? "Enable" : "Disable";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}