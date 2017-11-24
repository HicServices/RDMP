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
            
            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                typeof(ANOStore.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

            AddCommonMenuItems(node);
        }
    }
}
