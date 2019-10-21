using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandNewObject:BasicCommandExecution
    {
        private Type _type;

        public ExecuteCommandNewObject(IBasicActivateItems activator,Type type):base(activator)
        {
            if(!typeof(DatabaseEntity).IsAssignableFrom(type))
                SetImpossible("Type must be derived from DatabaseEntity");
            _type = type;
        }

        public override void Execute()
        {
            base.Execute();

            var objectConstructor = new ObjectConstructor();

            var constructor = objectConstructor.GetRepositoryConstructor(_type);

            var invoker = new CommandInvoker(BasicActivator);

            var instance = objectConstructor.ConstructIfPossible(_type,
                constructor.GetParameters().Select(invoker.GetValueForParameterOfType).ToArray());

            if(instance == null)
                throw new Exception("Failed to construct object with provided parameters");

            Publish((DatabaseEntity) instance);
        }
    }
}
