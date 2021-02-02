using ReusableLibraryCode.Settings;
using System.Reflection;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandListUserSettings : BasicCommandExecution
    {
        public ExecuteCommandListUserSettings(IBasicActivateItems activator):base(activator)
        {

        }
        public override void Execute()
        {
            base.Execute();

            var sb = new StringBuilder();

            foreach(var prop in typeof(UserSettings).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                sb.AppendLine($"{prop.Name}:{prop.GetValue(null)}");
            }

            BasicActivator.Show(sb.ToString());
        }

    }
}
