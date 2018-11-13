using System;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using RDMPStartup;

namespace CatalogueManager.Menus
{
    class AggregatesNodeMenu : RDMPContextMenuStrip
    {
        public AggregatesNodeMenu(RDMPContextMenuStripArgs args, AggregatesNode aggregatesNode):base(args,aggregatesNode)
        {
            Add(new ExecuteCommandAddNewAggregateGraph(_activator, aggregatesNode.Catalogue));
        }
    }
}