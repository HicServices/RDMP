using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDeprecate : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IMightBeDeprecated _o;
        private bool _desiredState;

        public ExecuteCommandDeprecate(IActivateItems itemActivator, IMightBeDeprecated o) : this(itemActivator,o,true)
        {
            
        }

        public ExecuteCommandDeprecate(IActivateItems itemActivator, IMightBeDeprecated o, bool desiredState) : base(itemActivator)
        {
            _o = o;
            _desiredState = desiredState;
        }

        public override string GetCommandName()
        {
            return _desiredState ? "Deprecate" : "UnDeprecate";
        }

        public override void Execute()
        {
            base.Execute();

            _o.IsDeprecated = _desiredState;
            _o.SaveToDatabase();
            Publish((DatabaseEntity)_o);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}