using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandListSupportedCommands:BasicCommandExecution
    {
        public ExecuteCommandListSupportedCommands(IBasicActivateItems basicActivator):base(basicActivator)
        {
            
        }

        public override void Execute()
        {
            var commandCaller = new CommandInvoker(BasicActivator);

            string commands = string.Join(Environment.NewLine,
                commandCaller.GetSupportedCommands()
                    .Select(t => BasicCommandExecution.GetCommandName(t.Name))
                        .OrderBy(s=>s));

            BasicActivator.Show(commands);

            base.Execute();
        }
    }
}
