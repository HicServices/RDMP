using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.UIFactory
{
    public class AtomicCommandUIFactory
    {
        private readonly IActivateItems _activator;
        private readonly IIconProvider _iconProvider;

        public AtomicCommandUIFactory(IActivateItems activator)
        {
            _activator = activator;
            _iconProvider = activator.CoreIconProvider;
        }

        public ToolStripMenuItem CreateMenuItem(IAtomicCommand command)
        {
            return new AtomicCommandMenuItem(command, _activator);
        }

        public AtomicCommandLinkLabel CreateLinkLabel(IAtomicCommand command)
        {
            return new AtomicCommandLinkLabel(_iconProvider,command);
        }

        public AtomicCommandWithTargetUI<T> CreateLinkLabelWithSelection<T>(IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector)
        {
            return new AtomicCommandWithTargetUI<T>(_iconProvider, command, selection, propertySelector);
        }

        public RDMPContextMenuStrip CreateMenu(IActivateItems activator,TreeListView tree, RDMPCollection collection, params IAtomicCommand[] commands)
        {
            var toReturn = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(activator,tree,collection),collection);
            
            foreach (IAtomicCommand command in commands)
                toReturn.Items.Add(CreateMenuItem(command));

            return toReturn;
        }

        public ToolStripItem CreateToolStripItem(IAtomicCommand command)
        {
            return new AtomicCommandToolStripItem(command, _activator);
        }
    }

}
