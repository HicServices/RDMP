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
        private readonly PreLoadDiscardedColumnsCollection _discardCollection;

        public PreLoadDiscardedColumnsCollectionMenu(IActivateItems activator, PreLoadDiscardedColumnsCollection discardCollection)
        {
            _activator = activator;
            _discardCollection = discardCollection;

            AtomicCommandUIFactory uiFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);
            
            Items.Add(new SetDumpServerMenuItem(activator, discardCollection.TableInfo));

            Items.Add(
                uiFactory.CreateMenuItem(new ExecuteCommandCreateNewPreLoadDiscardedColumn(activator,
                    _discardCollection.TableInfo)));


        }
        
    }
}