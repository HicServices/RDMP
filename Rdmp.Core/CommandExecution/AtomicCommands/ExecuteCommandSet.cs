using System;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandSet:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable _setOn;


        public ExecuteCommandSet(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable setOn):base(activator)
        {
            _setOn = setOn;
        }

        public override void Execute()
        {
            base.Execute();

            if (BasicActivator.TypeText("Property To Set", "Property Name", 1000, null, out string propName, false))
            {
                var prop = _setOn.GetType().GetProperty(propName);

                if (prop == null)
                {
                    Show($"No property found called '{propName}' on Type '{_setOn.GetType().Name}'");
                    return;
                }

                var invoker = new CommandInvoker(BasicActivator);

                var val = invoker.GetValueForParameterOfType(prop);
                
                prop.SetValue(_setOn,val);
                ((DatabaseEntity)_setOn).SaveToDatabase();
                
                Publish((DatabaseEntity) _setOn);
            }
        }
    }
}
