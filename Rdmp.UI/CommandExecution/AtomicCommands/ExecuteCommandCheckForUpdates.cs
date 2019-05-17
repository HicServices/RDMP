using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCheckForUpdates : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandCheckForUpdates(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();
            
            using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/HicServices/RDMP"))
            {
                var entry = mgr.Result.CheckForUpdate(false, (int f) => Debug.Print("progress: " + f)).Result;
                if (entry.IsBootstrapping)
                {
                    Console.WriteLine("Ehy I am bootstrapping... whoooo");
                }
            }
        }
    }
}
