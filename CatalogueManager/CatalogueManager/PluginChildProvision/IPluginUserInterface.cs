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
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.PluginChildProvision
{
    /// <summary>
    /// Interface for declaring plugins which interact with the RDMP user interface.  Supports injecting custom objects into RDMPCollectionUI trees and inject new
    /// menu items under existing objects e.g. add a new option to the Catalogue right click menu.  See the abstract base for how to do this easily.
    /// </summary>
    [InheritedExport(typeof(IChildProvider))]
    [InheritedExport(typeof(IPluginUserInterface))]
    public interface IPluginUserInterface:IChildProvider,IIconProvider
    {
        /// <summary>
        /// Return a list of new menu items that should appear under the given treeObject (that was right clicked in a RDMPCollectionUI)
        /// </summary>
        /// <param name="treeObject"></param>
        /// <returns></returns>
        ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object treeObject);
        
        /// <summary>
        /// Return a list of commands that should be exposed on the given user interface tab control (<paramref name="control"/>) when displaying the
        /// given <see cref="databaseEntity"/> object.  These will be shown as buttons on the control (where the control invokes
        ///  <see cref="RDMPSingleDatabaseObjectControl{T}.AddPluginCommands"/>)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="databaseEntity"></param>
        /// <returns></returns>
        IEnumerable<IAtomicCommand> GetAdditionalCommandsForControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity databaseEntity);
    }
}
