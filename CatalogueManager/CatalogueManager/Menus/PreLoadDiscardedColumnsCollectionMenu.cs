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
    internal class PreLoadDiscardedColumnsCollectionMenu : ContextMenuStrip
    {
        private readonly IActivateItems _activator;
        private readonly PreLoadDiscardedColumnsNode _discardNode;

        public PreLoadDiscardedColumnsCollectionMenu(IActivateItems activator, PreLoadDiscardedColumnsNode discardNode)
        {
            _activator = activator;
            _discardNode = discardNode;

            AtomicCommandUIFactory uiFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);
            
            Items.Add(
                uiFactory.CreateMenuItem(new ExecuteCommandCreateNewPreLoadDiscardedColumn(activator,
                    _discardNode.TableInfo)));


        }
        
    }
}