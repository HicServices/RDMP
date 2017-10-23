using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.ObjectVisualisation;
using DataLoadEngine.DataProvider.FromCache;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Dependencies;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public abstract class RDMPContextMenuStrip:ContextMenuStrip
    {
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        protected IActivateItems _activator;
        private readonly DatabaseEntity _databaseEntity;

        protected ToolStripMenuItem DependencyViewingMenuItem { get; set; }

        private AtomicCommandUIFactory AtomicCommandUIFactory;

        public RDMPContextMenuStrip(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _activator = activator;
            _databaseEntity = databaseEntity;

            AtomicCommandUIFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);

            if(databaseEntity != null)
                Add(new ExecuteCommandActivate(activator,databaseEntity));

            RepositoryLocator = _activator.RepositoryLocator;
            
            var dependencies = databaseEntity as IHasDependencies;

            if (dependencies != null)
                DependencyViewingMenuItem = new ViewDependenciesToolStripMenuItem(dependencies, new CatalogueObjectVisualisation(activator.CoreIconProvider));
        }
        protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None)
        {
            var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);

            if (shortcutKey != Keys.None)
                mi.ShortcutKeys = shortcutKey;
            
            Items.Add(mi);
            return mi;
        }

        protected void AddCommonMenuItems()
        {
            var deletable = _databaseEntity as IDeleteable;
            var nameable = _databaseEntity as INamed;

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandRefreshObject(_activator, _databaseEntity),Keys.F5);
            
            if (deletable != null)
                Add(new ExecuteCommandDelete(_activator, deletable),Keys.Delete);

            if (nameable != null)
                Add(new ExecuteCommandRename(_activator.RefreshBus, nameable),Keys.F2);
            
            if(DependencyViewingMenuItem != null)
                Items.Add(DependencyViewingMenuItem);

            if(_databaseEntity != null)
            {
                foreach (var plugin in _activator.PluginUserInterfaces)
                {
                    try
                    {
                        var toAdd = plugin.GetAdditionalRightClickMenuItems(_databaseEntity);

                        if (toAdd != null && toAdd.Any())
                        {
                            Items.Add(new ToolStripSeparator());
                            Items.AddRange(toAdd);
                        }
                    }
                    catch (Exception ex)
                    {
                        _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(ex.Message,CheckResult.Fail, ex));
                    }

                }

                Items.Add(new ExpandAllTreeNodesMenuItem(_activator, _databaseEntity));
            }
        }
    }
}
