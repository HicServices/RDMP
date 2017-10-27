using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public abstract class BasicUICommandExecution:BasicCommandExecution
    {
        protected readonly IActivateItems Activator;

        protected BasicUICommandExecution(IActivateItems activator)
        {
            Activator = activator;
        }

        protected void Publish(DatabaseEntity o)
        {
            Activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(o));
        }

        protected void Activate(DatabaseEntity o)
        {
            var cmd = new ExecuteCommandActivate(Activator, o);
            cmd.Execute();
        }
    }
}
