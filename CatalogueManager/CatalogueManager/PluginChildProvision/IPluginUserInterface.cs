using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPStartup;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.PluginChildProvision
{
    [InheritedExport(typeof(IChildProvider))]
    [InheritedExport(typeof(IPluginUserInterface))]
    public interface IPluginUserInterface:IChildProvider,IIconProvider
    {
        ToolStripMenuItem[] GetAdditionalRightClickMenuItems(DatabaseEntity databaseEntity);
        void Activate(object sender, object model);
    }
}
