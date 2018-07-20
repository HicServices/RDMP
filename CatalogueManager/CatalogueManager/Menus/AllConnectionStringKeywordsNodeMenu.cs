using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllConnectionStringKeywordsNodeMenu:RDMPContextMenuStrip
    {
        public AllConnectionStringKeywordsNodeMenu(RDMPContextMenuStripArgs args, AllConnectionStringKeywordsNode databaseEntity): base(args, databaseEntity)
        {
            Add(new ExecuteCommandCreateNewConnectionStringKeyword(_activator));
        }
    }
}
