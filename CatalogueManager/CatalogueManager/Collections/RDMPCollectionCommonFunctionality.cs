using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Threading.Tasks;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Collections.Providers.Copying;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.TreeHelper;

namespace CatalogueManager.Collections
{
    public class RDMPCollectionCommonFunctionality : IRefreshBusSubscriber
    {
        private IActivateItems _activator;
        public TreeListView Tree;
        private TextBox _filterTextBox;

        public ICoreIconProvider CoreIconProvider { get; private set; }
        public ICoreChildProvider CoreChildProvider { get; set; }
        public RenameProvider RenameProvider { get; private set; }
        public DragDropProvider DragDropProvider { get; private set; }
        public CopyPasteProvider CopyPasteProvider { get; private set; }
        public FavouriteColumnProvider FavouriteColumnProvider { get; private set; }
        public TreeNodeParentFinder ParentFinder { get; private set; }

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        
        public OLVColumn FavouriteColumn { get; private set; }

        public bool IsSetup { get; private set; }

        HashSet<object> filterWhitelist = new HashSet<object>();
        private IModelFilter _secondaryFilter;

        /// <summary>
        /// Only valid if you have a filterTextBox too, this lets you specify an alternate filter that will be .All with the text/whitelist filter so that it must fulfil the requirements
        /// of the text search AND the SecondaryFilter (good for flags that always hide stuff)
        /// </summary>
        public IModelFilter SecondaryFilter
        {
            get { return _secondaryFilter; }
            set
            {
                _secondaryFilter = value;
                ApplyFilter();
            }
        }


        /// <summary>
        /// Sets up common functionality for an RDMPCollectionUI
        /// </summary>
        /// <param name="tree">The main tree in the collection UI</param>
        /// <param name="activator">The current activator, used to launch objects, register for refresh events etc </param>
        /// <param name="repositoryLocator">The object for finding the Catalogue/DataExport repository databases</param>
        /// <param name="commandFactory">A command factory for starting commands in appropriate circumstances e.g. when the user starts a drag of a Catalogue a CatalogueCommand should be created.  Try using a new RDMPCommandFactory </param>
        /// <param name="commandExecutionFactory">A command execution factory for completing a started ICommand in appropriate circumstances e.g. when the user completes a drop operation onto a second item.  Try using a new RDMPCommandExecutionFactory</param>
        /// <param name="iconColumn">The column of tree view which should contain the icon for each row object</param>
        /// <param name="iconProvider">The class that supplies images for the iconColumn, must return an Image very fast and must have an image for every object added to tree</param>
        /// <param name="filterTextBoxIfYouWantDefaultFilterBehaviour">A text box if you want to be able to filter by text string (also includes support for always showing newly expanded nodes)</param>
        /// <param name="renameableColumn">Nullable field for specifying which column supports renaming on F2</param>
        /// <param name="renameLabel">A Label for displaying the help text telling the user how to rename (F2)</param>
        public void SetUp(TreeListView tree, IActivateItems activator, OLVColumn iconColumn, TextBox filterTextBoxIfYouWantDefaultFilterBehaviour,OLVColumn renameableColumn, Label renameLabel)
        {
            IsSetup = true;
            _activator = activator;
            _activator.RefreshBus.Subscribe(this);

            RepositoryLocator = _activator.RepositoryLocator;

            Tree = tree;
            Tree.FullRowSelect = true;
            Tree.HideSelection = false;

            Tree.RevealAfterExpand = true;
            
            Tree.CanExpandGetter += CanExpandGetter;
            Tree.ChildrenGetter += ChildrenGetter;

            Tree.ItemActivate += CommonItemActivation;
            Tree.CellRightClick += CommonRightClick;

            iconColumn.ImageGetter += ImageGetter;
            Tree.RowHeight = 19;

            //what does this do to performance?
            Tree.UseNotifyPropertyChanged = true;

            ParentFinder = new TreeNodeParentFinder(Tree);

            DragDropProvider = new DragDropProvider(
                _activator.CommandFactory,
                _activator.CommandExecutionFactory,
                tree);
            
            if(renameableColumn != null)
            {
                RenameProvider = new RenameProvider(_activator.RefreshBus, tree, renameableColumn, renameLabel);
                RenameProvider.RegisterEvents();
            }
            
            FavouriteColumnProvider = new FavouriteColumnProvider(_activator, tree);
            FavouriteColumn = FavouriteColumnProvider.CreateColumn();
            FavouriteColumn.Sortable = false;

            CoreIconProvider = activator.CoreIconProvider;

            CopyPasteProvider = new CopyPasteProvider();
            CopyPasteProvider.RegisterEvents(tree);
            
            OnRefreshChildProvider(_activator.CoreChildProvider);

            _filterTextBox = filterTextBoxIfYouWantDefaultFilterBehaviour;
            if(filterTextBoxIfYouWantDefaultFilterBehaviour != null)
            {
                filterTextBoxIfYouWantDefaultFilterBehaviour.TextChanged += FilterTextChanged;
                tree.Expanding += TreeOnExpanding;
                tree.Collapsing += TreeOnCollapsing;
            }

            _activator.Emphasise += _activator_Emphasise;
        }

        void _activator_Emphasise(object sender, ItemActivation.Emphasis.EmphasiseEventArgs args)
        {
            //get the parental hierarchy
            var decendancyList = CoreChildProvider.GetDescendancyListIfAnyFor(args.Request.ObjectToEmphasise);
            
            if (decendancyList != null)
            {
                //for each parent in the decendandy list
                foreach (var parent in decendancyList.Parents)
                {
                    //parent isn't in our tree
                    if (Tree.IndexOf(parent) == -1)
                        return;

                    //parent is in our tree so make sure it's expanded
                    Tree.Expand(parent);
                }
            }

            //tree doesn't contain object even after expanding parents
            int index = Tree.IndexOf(args.Request.ObjectToEmphasise);

            if(index == -1)
                return;

            if (args.Request.ExpansionDepth > 0)
                ExpandToDepth(args.Request.ExpansionDepth, args.Request.ObjectToEmphasise);

            //select the object and ensure it's visible
            Tree.SelectedObject = args.Request.ObjectToEmphasise;
            Tree.EnsureVisible(index);
            
            args.FormRequestingActivation = Tree.FindForm();
        }

        private void ExpandToDepth(int expansionDepth, object currentObject)
        {
            if(expansionDepth == 0)
                return;

            Tree.Expand(currentObject);

            foreach (object o in ChildrenGetter(currentObject))
                ExpandToDepth(expansionDepth -1,o);
        }

        private void FilterTextChanged(object sender, EventArgs e)
        {
            //text changed so clear the whitelist
            filterWhitelist = new HashSet<object>();
            ApplyFilter();
        }

        private void TreeOnCollapsing(object sender, TreeBranchCollapsingEventArgs e)
        {
            filterWhitelist.Add(e.Model);
            ApplyFilter();
        }

        private void TreeOnExpanding(object sender, TreeBranchExpandingEventArgs treeBranchExpandingEventArgs)
        {
            AddAllChildrenToFilterRecursively(treeBranchExpandingEventArgs.Model);
            ApplyFilter();
        }
        private void AddAllChildrenToFilterRecursively(object model)
        {
            if(model == null)
                return;

            filterWhitelist.Add(model);

            var children = CoreChildProvider.GetChildren(model);
            
            foreach (var child in children)
                AddAllChildrenToFilterRecursively(child);
        }
        
        private void ApplyFilter()
        {
            //create new filter
            if (!string.IsNullOrWhiteSpace(_filterTextBox.Text))
            {
                Tree.UseFiltering = true;

                var textFilter = new TextMatchFilterWithWhiteList(filterWhitelist, Tree, _filterTextBox.Text, StringComparison.CurrentCultureIgnoreCase);

                if (_secondaryFilter == null)
                    Tree.ModelFilter = textFilter;
                else 
                    Tree.ModelFilter = new CompositeAllFilter(new List<IModelFilter>(new[]{textFilter,_secondaryFilter}));
            }
            else
            {
                if(SecondaryFilter == null)
                {
                    Tree.UseFiltering = false;
                    Tree.ModelFilter = null;
                }
                else
                {
                    Tree.ModelFilter = SecondaryFilter;
                    Tree.UseFiltering = true;
                }
            }
        }



        private IEnumerable ChildrenGetter(object model)
        {
            return CoreChildProvider.GetChildren(model);
        }

        private bool CanExpandGetter(object model)
        {
            return CoreChildProvider.GetChildren(model).Any();
        }

        private object ImageGetter(object rowObject)
        {
            return CoreIconProvider.GetImage(rowObject);
        }
        
        public void CommonRightClick(object sender, CellRightClickEventArgs e)
        {
            var o = e.Model;
            if (o is AggregateConfiguration)
                e.MenuStrip = new AggregateConfigurationMenu(RepositoryLocator, _activator, (AggregateConfiguration)o, CoreIconProvider);

            var container = o as AggregateFilterContainer;
            if (container != null)
                e.MenuStrip = new AggregateFilterContainerMenu(_activator, container, ParentFinder.GetFirstOrNullParentRecursivelyOfType<AggregateConfiguration>(container), CoreIconProvider);

            if (o is IFilter)
                e.MenuStrip = new FilterMenu( _activator, (IFilter) o, CoreIconProvider);

            if (o is ParametersNode)
                e.MenuStrip = new FiltersParametersNodeMenu( _activator, (ParametersNode)o, CoreIconProvider);

            //if user mouses down on one object then mouses up over another then the cell right click event is for the mouse up so select the row so the user knows whats happening
            if(o != null)
                Tree.SelectedObject = o;
        }

        public void CommonItemActivation(object sender, EventArgs eventArgs)
        {
            var o = Tree.SelectedObject;

            //also actually handles ExtractionFilters as well as AggregateFilters
            var parameters = o as ParametersNode;
            
            if (parameters != null)
                _activator.ActivateParameterNode(this, parameters);

            var d = o as DatabaseEntity;
            if(d != null)
            {
                var cmd = new ExecuteCommandActivate(_activator, d);
                if(!cmd.IsImpossible)
                    cmd.Execute();
            }

            foreach (IPluginUserInterface pluginUserInterface in _activator.PluginUserInterfaces)
                pluginUserInterface.Activate(sender, o);
        }


        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            OnRefreshChildProvider(_activator.CoreChildProvider);
            
            //now tell tree view to refresh the object
            object parent = null; 

            //or from known descendancy
            var knownDescendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);
            if (knownDescendancy != null)
                parent = knownDescendancy.Last();
            
            //item deleted?
            if (!e.Object.Exists())
            {

                //item was deleted so remove it
                Tree.RemoveObject(e.Object);

                //if we have a parent it might be a node category that should now disapear too
                if (parent != null)
                {
                    //it's a Node (e.g. SupportingDocumentsNode) but not a SingletonNode (e.g. ANOTablesNode)
                    if (parent.GetType().Name.EndsWith("Node") && ! (parent is SingletonNode))
                    {
                        //if we are the only child
                        if (Tree.GetChildren(parent).Cast<object>().Count() <= 1)
                        {
                            Tree.RemoveObject(parent);
                            return;
                        }

                        //there are other siblings so removing e.Object will not result in the node disapearing
                        Tree.RefreshObject(parent);
                    }
                }
            }
            else
            {
                //but the filter is currently hiding the object?
                if (IsHiddenByFilter(e.Object))
                    return;

                //is parent in tree?
                if(parent != null && Tree.IndexOf(parent) != -1)
                    Tree.RefreshObject(parent);//refresh parent
                else
                //parent isn't in tree, could be a root object? try to refresh the object anyway
                if(Tree.IndexOf(e.Object) != -1)
                    Tree.RefreshObject(e.Object);


            }
        }

        private bool IsHiddenByFilter(object o)
        {
            return Tree.IsFiltering && !Tree.FilteredObjects.Cast<object>().Contains(o);
        }

        private void OnRefreshChildProvider(ICoreChildProvider coreChildProvider)
        {
            CoreChildProvider = coreChildProvider;
            CoreIconProvider.SetClassifications(CoreChildProvider.CatalogueItemClassifications);
        }

        public void TearDown()
        {
            if(IsSetup)
            {

                _activator.RefreshBus.Unsubscribe(this);
                _activator.Emphasise -= _activator_Emphasise;
            }
        }

        private bool expand = true;

        /// <summary>
        /// Expands or collapses the tree view.  Returns true if the tree is now expanded, returns false if the tree is now collapsed
        /// </summary>
        /// <param name="btnExpandOrCollapse"></param>
        /// <returns></returns>
        public bool ExpandOrCollapse(Button btnExpandOrCollapse)
        {
            Tree.UseFiltering = false;

            if (expand)
            {
                Tree.ExpandAll();
                expand = false;

                if(btnExpandOrCollapse != null)
                    btnExpandOrCollapse.Text = "Collapse";
                
            }
            else
            {
                Tree.CollapseAll();
                expand = true;
                if(btnExpandOrCollapse != null)
                    btnExpandOrCollapse.Text = "Expand";
            }

            Tree.UseFiltering = true;
            Tree.EnsureVisible(0);
            
            return !expand;
        }
    }
}
