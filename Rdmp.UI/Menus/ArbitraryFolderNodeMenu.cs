using Rdmp.Core.Providers.Nodes;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Menus
{
    class ArbitraryFolderNodeMenu : RDMPContextMenuStrip
    {
        public ArbitraryFolderNodeMenu(RDMPContextMenuStripArgs args, ArbitraryFolderNode folder) : base(args, folder)
        {
            if(folder.CommandGetter != null)
                foreach(IAtomicCommand cmd in folder.CommandGetter())
                    Add(cmd);
        }
        
    }
}
