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

        public abstract object[] GetChildren(object model);
        public abstract ToolStripMenuItem[] GetAdditionalRightClickMenuItems(DatabaseEntity databaseEntity);

        public abstract Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None);
    }
}