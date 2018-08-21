using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Collections;
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

            AtomicCommandUIFactory = new AtomicCommandUIFactory(_activator);
            
            RepositoryLocator = _activator.RepositoryLocator;

            if(o != null)
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

        /// <summary>
        /// Adds all commands (usually plugins) that are derrived from T e.g. PluginDatabaseAtomicCommand.  This will only add Types that are exposed
        /// via MEF Export decorations so if you define a new base Type T make sure to inherit from PluginAtomicCommand to pick up the [InheritedExport].
        /// 
        /// <para>All derrived classes must have either a blank constructor or one taking either an <see cref="IActivateItems"/> or <see cref="IRDMPPlatformRepositoryServiceLocator"/></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void AddAll<T>()
        {
            var types = _activator.RepositoryLocator.CatalogueRepository.MEF
                .GetTypes<IAtomicCommand>().Where(t =>
                    typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                var constructor = new ObjectConstructor();

                var instance =
                    constructor.ConstructIfPossible(type, _activator)?? //what about one that takes an IActivateItems?
                    constructor.ConstructIfPossible(type, _activator.RepositoryLocator)?? //maybe it takes a repo locator
                    constructor.ConstructIfPossible(type);  //does it maybe have a blank constructor?

                if(instance == null)
                    throw new NotSupportedException("Type " + type + " did not have any valid constructors");

                Add((IAtomicCommand)instance);
            }
        }

        public void AddCommonMenuItems(RDMPCollectionCommonFunctionality commonFunctionality)
        {
            var deletable = _o as IDeleteable;
            var nameable = _o as INamed;
            var databaseEntity = _o as DatabaseEntity;

            if(Items.Count > 0)
                Items.Add(new ToolStripSeparator());
            
            if (databaseEntity != null)
                Add(new ExecuteCommandRefreshObject(_activator, databaseEntity), Keys.F5);
            
            if (deletable != null)
            {
                if (_args.Masquerader is IDeleteable)
                    deletable = (IDeleteable)_args.Masquerader;

                Add(new ExecuteCommandDelete(_activator, deletable),Keys.Delete);
            }

            if (nameable != null)
                Add(new ExecuteCommandRename(_activator.RefreshBus, nameable),Keys.F2);

            Add(new ExecuteCommandShowKeywordHelp(_activator, _args));

            if (databaseEntity != null)
            {
                if (databaseEntity.Equals(_args.CurrentlyPinnedObject))
                    Add(new ExecuteCommandUnpin(_activator, databaseEntity));
                else
                    Add(new ExecuteCommandPin(_activator, databaseEntity));

                Add(new ExecuteCommandViewDependencies(databaseEntity as IHasDependencies, new CatalogueObjectVisualisation(_activator.CoreIconProvider)));
            }
            
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

            if (_args.Tree != null && !commonFunctionality.Settings.SuppressChildrenAdder)
            {
                Add(new ExecuteCommandExpandAllNodes(_activator, commonFunctionality, _args.Model));
                Add(new ExecuteCommandCollapseChildNodes(_activator, commonFunctionality, _args.Model));
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
