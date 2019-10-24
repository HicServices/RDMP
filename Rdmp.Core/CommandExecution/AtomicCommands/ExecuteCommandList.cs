using System.Text;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandList : BasicCommandExecution
    {
        private IMapsDirectlyToDatabaseTable[] _toList;

        public ExecuteCommandList(IBasicActivateItems activator,IMapsDirectlyToDatabaseTable[] toList):base(activator)
        {
            _toList = toList;
        }

        public override void Execute()
        {
            base.Execute();

            StringBuilder sb = new StringBuilder();
            foreach (var m in _toList)
                sb.AppendLine(m.ID + ":" + m);

            BasicActivator.Show(sb.ToString());
        }
    }
}