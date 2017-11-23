using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class AllANOTablesNodeMenu:RDMPContextMenuStrip
    {
        public AllANOTablesNodeMenu(IActivateItems activator, AllANOTablesNode node)
            : base(activator, null)
        {
            Add(new ExecuteCommandCreateNewANOTable(activator));
            
            AddCommonMenuItems(node);
        }
    }
}
