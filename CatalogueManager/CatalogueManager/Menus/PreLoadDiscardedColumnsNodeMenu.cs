using System;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class PreLoadDiscardedColumnsNodeMenu : RDMPContextMenuStrip
    {
        public PreLoadDiscardedColumnsNodeMenu(RDMPContextMenuStripArgs args, PreLoadDiscardedColumnsNode discardNode) : base(args,null)
        {
            Add(new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator, discardNode.TableInfo));
        }

        public PreLoadDiscardedColumnsNodeMenu(RDMPContextMenuStripArgs args, IdentifierDumpServerUsageNode discardUsageNode): base(args, null)
        {
            Add(new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator, discardUsageNode.TableInfo));
        }
        
    }
}