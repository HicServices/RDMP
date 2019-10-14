using System;
using System.Linq;
using System.Text;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandList : BasicCommandExecution
    {
        private Type _type;

        public ExecuteCommandList(IBasicActivateItems activator,string typename):base(activator)
        {
            _type = activator.RepositoryLocator.CatalogueRepository.MEF.GetType(typename);

            if(_type == null)
                SetImpossible($"Unknown Type '{typename}'");
        }

        public override void Execute()
        {
            base.Execute();

            StringBuilder sb = new StringBuilder();
            foreach (var m in BasicActivator.CoreChildProvider.GetAllObjects(_type,true))
            {
                sb.AppendLine(m.ID + ":" + m);
            }

            BasicActivator.Show(sb.ToString());
        }
    }
}