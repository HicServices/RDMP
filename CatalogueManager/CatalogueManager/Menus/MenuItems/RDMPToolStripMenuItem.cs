using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Menus.MenuItems
{
    [System.ComponentModel.DesignerCategory("")]
    public abstract class RDMPToolStripMenuItem : ToolStripMenuItem
    {
        protected AtomicCommandUIFactory AtomicCommandUIFactory;
        protected IActivateItems _activator;

        protected RDMPToolStripMenuItem(IActivateItems activator,string text):base(text)
        {
            _activator = activator;
            AtomicCommandUIFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);
        }
        
        protected void Activate(DatabaseEntity o)
        {
            var cmd = new ExecuteCommandActivate(_activator, o);
            cmd.Execute();
        }

        protected void Publish(DatabaseEntity o)
        {
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(o));
        }

        /// <summary>
        /// Adds the given command to the drop down item list of this tool strip menu item
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="shortcutKey"></param>
        /// <returns></returns>
        protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None)
        {
            var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);

            if (shortcutKey != Keys.None)
                mi.ShortcutKeys = shortcutKey;

            DropDownItems.Add(mi);
            return mi;
        }
    }
}
