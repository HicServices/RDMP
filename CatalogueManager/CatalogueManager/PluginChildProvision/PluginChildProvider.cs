using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.PluginChildProvision
{
    public abstract class PluginUserInterface:IPluginUserInterface
    {
        protected readonly IActivateItems ItemActivator;
        
        protected PluginUserInterface(IActivateItems itemActivator)
        {
            ItemActivator = itemActivator;
        }

        public virtual object[] GetChildren(object model)
        {
            return null;
        }

        public virtual ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            return null;
        }

        private AtomicCommandUIFactory _atomicCommandUIFactory  = null;
        protected ToolStripMenuItem GetMenuItem(IAtomicCommand cmd)
        {
            if(_atomicCommandUIFactory == null)
                _atomicCommandUIFactory = new AtomicCommandUIFactory(ItemActivator.CoreIconProvider);

            return _atomicCommandUIFactory.CreateMenuItem(cmd);
        }

        protected ToolStripMenuItem[] GetMenuArray(params IAtomicCommand[] commands)
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();

            foreach (IAtomicCommand command in commands)
                items.Add(GetMenuItem(command));

            return items.ToArray();
        }

        public virtual Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            return null;
        }
    }
}