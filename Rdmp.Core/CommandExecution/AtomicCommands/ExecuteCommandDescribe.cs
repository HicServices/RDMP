using System.Reflection;
using System.Text;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDescribe:BasicCommandExecution
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toDescribe;

        public ExecuteCommandDescribe(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable[] toDescribe):base(activator)
        {
            _toDescribe = toDescribe;
        }

        public override void Execute()
        {
            base.Execute();

            var sb = new StringBuilder();

            foreach (IMapsDirectlyToDatabaseTable o in _toDescribe)
            {
                foreach (PropertyInfo p in o.GetType().GetProperties())
                {
                    sb.AppendLine(p.Name + ":" + (p.GetValue(o)?.ToString() ?? "NULL"));
                }

                sb.AppendLine("-----------------------------------------");
            }

            if(sb.Length > 0)
                Show(sb.ToString());
        }
    }
}
