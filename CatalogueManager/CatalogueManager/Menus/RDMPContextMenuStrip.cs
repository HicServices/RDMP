using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [InheritedExport(typeof(RDMPContextMenuStrip))]
    [System.ComponentModel.DesignerCategory("")]
    public class RDMPContextMenuStrip:ContextMenuStrip
    {
        private readonly object _o;
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        protected IActivateItems _activator;
        
        protected ToolStripMenuItem DependencyViewingMenuItem { get; set; }

        private readonly AtomicCommandUIFactory AtomicCommandUIFactory;

        protected ToolStripMenuItem ActivateCommandMenuItem;
        private RDMPContextMenuStripArgs _args;

        public RDMPContextMenuStrip(RDMPContextMenuStripArgs args, object o)
        {
            _o = o;
            _args = args;

            _activator = _args.ItemActivator;

            _activator.Theme.ApplyTo(this);

            AtomicCommandUIFactory = new AtomicCommandUIFactory(_activator);
            
            RepositoryLocator = _activator.RepositoryLocator;

            if(o != null && !(o is RDMPCollection))
                ActivateCommandMenuItem = Add(new ExecuteCommandActivate(_activator,args.Masquerader?? o));
        }

        protected void ReBrandActivateAs(string newTextForActivate, RDMPConcept newConcept, OverlayKind overlayKind = OverlayKind.None)
        {
            //Activate is currently branded edit by parent lets tailor that
            ActivateCommandMenuItem.Image = _activator.CoreIconProvider.GetImage(newConcept, overlayKind);
            ActivateCommandMenuItem.Text = newTextForActivate;
        }
        protected ToolStripMenuItem Add(IAtomicCommand cmd, Keys shortcutKey = Keys.None, ToolStripMenuItem toAddTo = null)
        {
            var mi = AtomicCommandUIFactory.CreateMenuItem(cmd);

            if (shortcutKey != Keys.None)
                mi.ShortcutKeys = shortcutKey;

            if (toAddTo == null)
                Items.Add(mi);
            else
                toAddTo.DropDownItems.Add(mi);

            return mi;
        }


        public void AddCommonMenuItems(RDMPCollectionCommonFunctionality commonFunctionality)
        {
            var deletable = _o as IDeleteable;
            var nameable = _o as INamed;
            var databaseEntity = _o as DatabaseEntity;

            var treeMenuItem = new ToolStripMenuItem("Tree");
            var inspectionMenuItem = new ToolStripMenuItem("Inspection");

            //add plugin menu items
            foreach (var plugin in _activator.PluginUserInterfaces)
            {
                try
                {
                    ToolStripMenuItem[] toAdd = plugin.GetAdditionalRightClickMenuItems(_o);

                    if (toAdd != null && toAdd.Any())
                        Items.AddRange(toAdd);
                }
                catch (Exception ex)
                {
                    _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(ex.Message,
                        CheckResult.Fail, ex));
                }
            }

            if(Items.Count > 0)
                Items.Add(new ToolStripSeparator());

            //add delete/rename etc
            if (deletable != null)
            {
                if (_args.Masquerader is IDeleteable)
                    deletable = (IDeleteable)_args.Masquerader;

                Add(new ExecuteCommandDelete(_activator, deletable), Keys.Delete);
            }

            if (nameable != null)
                Add(new ExecuteCommandRename(_activator.RefreshBus, nameable), Keys.F2);

            //add seldom used submenus (pin, view dependencies etc)
            Items.Add(inspectionMenuItem);
            PopulateInspectionMenu(commonFunctionality, inspectionMenuItem);

            Items.Add(treeMenuItem);
            PopulateTreeMenu(commonFunctionality, treeMenuItem);

            //add refresh and then finally help
            if (databaseEntity != null)
                Add(new ExecuteCommandRefreshObject(_activator, databaseEntity), Keys.F5);

            Add(new ExecuteCommandShowKeywordHelp(_activator, _args));}

        private void PopulateTreeMenu(RDMPCollectionCommonFunctionality commonFunctionality, ToolStripMenuItem treeMenuItem)
        {
            var databaseEntity = _o as DatabaseEntity;

            if (databaseEntity != null)
            {
                if (databaseEntity.Equals(_args.CurrentlyPinnedObject))
                    Add(new ExecuteCommandUnpin(_activator, databaseEntity), Keys.None, treeMenuItem);
                else
                    Add(new ExecuteCommandPin(_activator, databaseEntity), Keys.None, treeMenuItem);
            }

            if (_args.Tree != null && !commonFunctionality.Settings.SuppressChildrenAdder)
            {
                Add(new ExecuteCommandExpandAllNodes(_activator, commonFunctionality, _args.Model), Keys.None, treeMenuItem);
                Add(new ExecuteCommandCollapseChildNodes(_activator, commonFunctionality, _args.Model), Keys.None, treeMenuItem);
            }
        }

        private void PopulateInspectionMenu(RDMPCollectionCommonFunctionality commonFunctionality, ToolStripMenuItem inspectionMenuItem)
        {
            var databaseEntity = _o as DatabaseEntity;

            if (commonFunctionality.CheckColumnProvider != null)
            {
                if (databaseEntity != null)
                    Add(new ExecuteCommandCheck(_activator, databaseEntity, commonFunctionality.CheckColumnProvider.RecordWorst), Keys.None, inspectionMenuItem);

                var checkAll = new ToolStripMenuItem("Check All", null, (s, e) => commonFunctionality.CheckColumnProvider.CheckCheckables());
                checkAll.Image = CatalogueIcons.TinyYellow;
                checkAll.Enabled = commonFunctionality.CheckColumnProvider.GetCheckables().Any();
                inspectionMenuItem.DropDownItems.Add(checkAll);
            }

            if(databaseEntity != null)
                Add(new ExecuteCommandViewDependencies(databaseEntity as IHasDependencies, _activator.GetLazyCatalogueObjectVisualisation()), Keys.None, inspectionMenuItem);
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
