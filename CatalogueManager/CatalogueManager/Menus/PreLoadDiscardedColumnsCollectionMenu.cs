using System;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class PreLoadDiscardedColumnsCollectionMenu : RDMPContextMenuStrip
    {
        public PreLoadDiscardedColumnsCollectionMenu(IActivateItems activator, PreLoadDiscardedColumnsNode discardNode) : base(activator,null)
        {
            Add(new ExecuteCommandCreateNewPreLoadDiscardedColumn(activator, discardNode.TableInfo));
        }
        
    }
}