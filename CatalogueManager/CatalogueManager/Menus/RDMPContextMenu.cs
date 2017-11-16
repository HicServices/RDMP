using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using DataLoadEngine.DataProvider.FromCache;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Dependencies;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [InheritedExport(typeof(RDMPContextMenuStrip))]
    [System.ComponentModel.DesignerCategory("")]
    public abstract class RDMPContextMenuStrip:ContextMenuStrip
    {
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        protected IActivateItems _activator;
        private readonly DatabaseEntity _databaseEntity;

        protected ToolStripMenuItem DependencyViewingMenuItem { get; set; }

        private AtomicCommandUIFactory AtomicCommandUIFactory;

        protected ToolStripMenuItem ActivateCommandMenuItem;

        protected RDMPContextMenuStrip(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _activator = activator;
            _databaseEntity = databaseEntity;

            AtomicCommandUIFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);

            if(databaseEntity != null)
                ActivateCommandMenuItem = Add(new ExecuteCommandActivate(activator, databaseEntity));

            RepositoryLocator = _activator.RepositoryLocator;
        }

        protected void ReBrandActivateAs(string newTextForActivate, RDMPConcept newConcept, OverlayKind overlayKind = OverlayKind.None)
        {
            //Activate is currently branded edit by parent lets tailor that
            ActivateCommandMenuItem.Image = _activator.CoreIconProvider.GetImage(newConcept, overlayKind);
            ActivateCommandMenuItem.Text = newTextForActivate;
        }
        protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None)
        {
            var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);
            
            if (shortcutKey != Keys.None)
                mi.ShortcutKeys = shortcutKey;
            
            Items.Add(mi);
            return mi;
        }

        /// <summary>
        /// Adds right click options that relate to the DatabaseEntity you passed into the constructor (which might be null).  This includes activate, delete, rename
        /// etc where appropriate to that object.  Also all PluginUserInterfaces will be asked for additional menu items for the supplied DatabaseEntity (if any).
        /// 
        /// If you want to expose a non DatabaseEntity object (or multiple) to PluginUserInterfaces then pass that in as argument  additionalObjectsToExposeToPluginUserInterfaces
        /// </summary>
        /// <param name="additionalObjectsToExposeToPluginUserInterfaces">Optional additional objects, do not pass the same object in twice or you will get duplication in your menu</param>
        protected void AddCommonMenuItems(params object[] additionalObjectsToExposeToPluginUserInterfaces)
        {
            var deletable = _databaseEntity as IDeleteable;
            var nameable = _databaseEntity as INamed;

            if(Items.Count > 0)
                Items.Add(new ToolStripSeparator());
            
            if (_databaseEntity != null)
                Add(new ExecuteCommandRefreshObject(_activator, _databaseEntity),Keys.F5);
            
            if (deletable != null)
                Add(new ExecuteCommandDelete(_activator, deletable),Keys.Delete);

            if (nameable != null)
                Add(new ExecuteCommandRename(_activator.RefreshBus, nameable),Keys.F2);

            if(_databaseEntity != null)
            {
                Add(new ExecuteCommandShowKeywordHelp(_activator, _databaseEntity));
                Add(new ExecuteCommandViewDependencies(_databaseEntity as IHasDependencies, new CatalogueObjectVisualisation(_activator.CoreIconProvider)));
                Add(new ExecuteCommandPin(_activator, _databaseEntity));
            }
            
            List<object> askPluginsAbout = new List<object>(additionalObjectsToExposeToPluginUserInterfaces);
            
            if(_databaseEntity != null)
                askPluginsAbout.Add(_databaseEntity);

            if(askPluginsAbout.Any())
            {
                foreach (var plugin in _activator.PluginUserInterfaces)
                {
                    foreach (var askAbout in askPluginsAbout)
                    {

                        try
                        {
                            ToolStripMenuItem[] toAdd;
                            if (askAbout is DatabaseEntity)
                                toAdd = plugin.GetAdditionalRightClickMenuItems((DatabaseEntity) askAbout);
                            else
                                toAdd = plugin.GetAdditionalRightClickMenuItems(askAbout);

                            if (toAdd != null && toAdd.Any())
                                Items.AddRange(toAdd);
                        }
                        catch (Exception ex)
                        {
                            _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(ex.Message,
                                CheckResult.Fail, ex));
                        }
                    }

                }
            }
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

        protected Image GetImage(object concept, OverlayKind shortcut = OverlayKind.None)
        {
            return _activator.CoreIconProvider.GetImage(concept, shortcut);
        }

        protected void Emphasise(DatabaseEntity o, int expansionDepth = 0)
        {
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, expansionDepth));
        }
    }
}
