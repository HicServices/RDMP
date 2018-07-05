using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

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

        public ContextMenuStrip CreateMenu(params IAtomicCommand[] commands)
        {
            var toReturn = new ContextMenuStrip();
            
            foreach (IAtomicCommand command in commands)
                toReturn.Items.Add(CreateMenuItem(command));
            
            return toReturn;
        }
    }
}
