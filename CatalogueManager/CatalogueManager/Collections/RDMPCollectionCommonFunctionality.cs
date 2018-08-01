using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Collections.Providers.Copying;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;
using CatalogueManager.Theme;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.TreeHelper;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Provides centralised functionality for all RDMPCollectionUI classes.  This includes configuring TreeListView to use the correct icons, have the correct row 
    /// height, child nodes etc.  Also centralises functionality like applying a CollectionPinFilterUI to an RDMPCollectionUI, keeping trees up to date during object
    /// refreshes / deletes etc.
    /// </summary>
    public class RDMPCollectionCommonFunctionality : IRefreshBusSubscriber
    {
        private RDMPCollection _collection;

        private IActivateItems _activator;
        public TreeListView Tree;

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
        
        public IAtomicCommand[] WhitespaceRightClickMenuCommands { get; set; }
        
        private CollectionPinFilterUI _pinFilter;
        private object _currentlyPinned;

        public IDColumnProvider IDColumnProvider { get; set; }
        public OLVColumn IDColumn { get; set; }

        /// <summary>
        /// List of Types for which the children should not be returned.  By default the IActivateItems child provider knows all objects children all the way down
        /// You can cut off any branch with this property, just specify the Types to stop descending at and you will get that object Type (assuming you normally would)
        /// but no further children.
        /// </summary>
        public Type[] AxeChildren { get; set; }

        public Type[] MaintainRootObjects { get; set; }

        public RDMPCollectionCommonFunctionalitySettings Settings { get; private set; }

        /// <summary>
        /// Sets up common functionality for an RDMPCollectionUI with the default settings
        /// </summary>
        /// <param name="tree">The main tree in the collection UI</param>
        /// <param name="activator">The current activator, used to launch objects, register for refresh events etc </param>
        /// <param name="iconColumn">The column of tree view which should contain the icon for each row object</param>
        /// <param name="renameableColumn">Nullable field for specifying which column supports renaming on F2</param>
        public void SetUp(RDMPCollection collection, TreeListView tree, IActivateItems activator, OLVColumn iconColumn, OLVColumn renameableColumn)
        {
            SetUp(collection,tree,activator,iconColumn,renameableColumn,new RDMPCollectionCommonFunctionalitySettings());
        }

        /// <summary>
        /// Sets up common functionality for an RDMPCollectionUI
        /// </summary>
        /// <param name="tree">The main tree in the collection UI</param>
        /// <param name="activator">The current activator, used to launch objects, register for refresh events etc </param>
        /// <param name="iconColumn">The column of tree view which should contain the icon for each row object</param>
        /// <param name="renameableColumn">Nullable field for specifying which column supports renaming on F2</param>
        /// <param name="settings">Customise which common behaviorurs are turned on</param>
        public void SetUp(RDMPCollection collection, TreeListView tree, IActivateItems activator, OLVColumn iconColumn, OLVColumn renameableColumn,RDMPCollectionCommonFunctionalitySettings settings)
        {
            Settings = settings;
            _collection = collection;
            IsSetup = true;
            _activator = activator;
            _activator.RefreshBus.Subscribe(this);

            RepositoryLocator = _activator.RepositoryLocator;

            Tree = tree;
            Tree.FullRowSelect = true;
            Tree.HideSelection = false;

            Tree.RevealAfterExpand = true;

            if (!Settings.SuppressChildrenAdder)
            {
                Tree.CanExpandGetter += CanExpandGetter;
                Tree.ChildrenGetter += ChildrenGetter;
            }

            if(!Settings.SuppressActivate)
                Tree.ItemActivate += CommonItemActivation;

            Tree.CellRightClick += CommonRightClick;
            Tree.SelectionChanged += (s,e)=>RefreshContextMenuStrip();
            
            if(iconColumn != null)
                iconColumn.ImageGetter += ImageGetter;
            
            if(Tree.RowHeight != 19)
                Tree.RowHeight = 19;

            //add colour indicator bar
            Tree.Location = new Point(Tree.Location.X, tree.Location.Y+3);
            Tree.Height -= 3;

            CreateColorIndicator(Tree,collection);

            //what does this do to performance?
            Tree.UseNotifyPropertyChanged = true;

            ParentFinder = new TreeNodeParentFinder(Tree);

            DragDropProvider = new DragDropProvider(
                _activator.CommandFactory,
                _activator.CommandExecutionFactory,
                tree);

            if(renameableColumn != null)
            {
                RenameProvider = new RenameProvider(_activator.RefreshBus, tree, renameableColumn);
                RenameProvider.RegisterEvents();
            }

            if (Settings.AddFavouriteColumn)
            {
                FavouriteColumnProvider = new FavouriteColumnProvider(_activator, tree);
                FavouriteColumn = FavouriteColumnProvider.CreateColumn();
            }

            if (settings.AddIDColumn)
            {
                IDColumnProvider = new IDColumnProvider(tree);
                IDColumn = IDColumnProvider.CreateColumn();

                Tree.AllColumns.Add(IDColumn);
                Tree.RebuildColumns();
            }

            CoreIconProvider = activator.CoreIconProvider;

            CopyPasteProvider = new CopyPasteProvider();
            CopyPasteProvider.RegisterEvents(tree);
            
            OnRefreshChildProvider(_activator.CoreChildProvider);
            
            _activator.Emphasise += _activator_Emphasise;

            Tree.TreeFactory = TreeFactoryGetter;
            Tree.RebuildAll(true);
            
            Tree.FormatRow += Tree_FormatRow;
            Tree.CellToolTipGetter += Tree_CellToolTipGetter;
        }

        private void CreateColorIndicator(TreeListView tree, RDMPCollection collection)
        {
            var indicatorHeight = BackColorProvider.IndiciatorBarSuggestedHeight;

            BackColorProvider p = new BackColorProvider();
            var ctrl = new Control();
            ctrl.BackColor = p.GetColor(collection);
            ctrl.Location = new Point(Tree.Location.X, tree.Location.Y - indicatorHeight);
            ctrl.Height = indicatorHeight;
            ctrl.Width = Tree.Width;

            if (Tree.Dock != DockStyle.None)
                ctrl.Dock = DockStyle.Top;
            else
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            Tree.Parent.Controls.Add(ctrl);
        }

        private string Tree_CellToolTipGetter(OLVColumn column, object modelObject)
        {
            return  _activator.DescribeProblemIfAny(modelObject);
        }

        void Tree_FormatRow(object sender, FormatRowEventArgs e)
        {
            bool hasProblems = _activator.HasProblem(e.Model);

            e.Item.ForeColor = hasProblems ? Color.Red : Color.Black;
            e.Item.BackColor = hasProblems ? Color.FromArgb(255,220,220) : Color.White;

        }

        private TreeListView.Tree TreeFactoryGetter(TreeListView view)
        {
            return new RDMPCollectionCommonFunctionalityTreeHijacker(view);
        }
        
        private void RefreshContextMenuStrip()
        {
            Tree.ContextMenuStrip = GetMenuIfExists(Tree.SelectedObject);
        }

        public void CommonRightClick(object sender, CellRightClickEventArgs e)
        {
            Tree.SelectedObject = e.Model;
            RefreshContextMenuStrip();
        }

        void _activator_Emphasise(object sender, ItemActivation.Emphasis.EmphasiseEventArgs args)
        {
            var rootObject = _activator.GetRootObjectOrSelf(args.Request.ObjectToEmphasise);

            // unpin first if there is somthing pinned, so we find our object!
            if (_pinFilter != null && _activator.IsRootObjectOfCollection(_collection,rootObject))
                _pinFilter.UnApplyToTree();
            
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

            if (args.Request.Pin && Settings.AllowPinning)
                Pin(args.Request.ObjectToEmphasise, decendancyList);

            //update index now pin filter is applied
            index = Tree.IndexOf(args.Request.ObjectToEmphasise);

            //select the object and ensure it's visible
            Tree.SelectedObject = args.Request.ObjectToEmphasise;
            Tree.EnsureVisible(index);

            
            args.FormRequestingActivation = Tree.FindForm();
        }

        private void Pin(IMapsDirectlyToDatabaseTable objectToPin, DescendancyList descendancy)
        {
            if (_pinFilter != null)
                _pinFilter.UnApplyToTree();
            
            _pinFilter = new CollectionPinFilterUI();
            _pinFilter.ApplyToTree(_activator.CoreChildProvider, Tree, objectToPin, descendancy);
            _currentlyPinned = objectToPin;

            _pinFilter.UnApplied += (s, e) =>
            {
                _pinFilter = null;
                _currentlyPinned = null;
            };
        }

        private void ExpandToDepth(int expansionDepth, object currentObject)
        {
            if(expansionDepth == 0)
                return;

            Tree.Expand(currentObject);

            foreach (object o in ChildrenGetter(currentObject))
                ExpandToDepth(expansionDepth -1,o);
        }

        private IEnumerable ChildrenGetter(object model)
        {
            if (AxeChildren != null && AxeChildren.Contains(model.GetType()))
                return new object[0];

            return CoreChildProvider.GetChildren(model);
        }

        private bool CanExpandGetter(object model)
        {
            var result = ChildrenGetter(model);

            if (result == null)
                return false;
            
            return result.Cast<object>().Any();
        }

        private object ImageGetter(object rowObject)
        {
            bool hasProblems = _activator.HasProblem(rowObject);
            
            return CoreIconProvider.GetImage(rowObject,hasProblems?OverlayKind.Problem:OverlayKind.None);
        }
        

        private ContextMenuStrip GetMenuIfExists(object o)
        {
            if (o != null)
            {
                //if user mouses down on one object then mouses up over another then the cell right click event is for the mouse up so select the row so the user knows whats happening
                Tree.SelectedObject = o;

                object masqueradingAs = null;
                if (o is IMasqueradeAs)
                    masqueradingAs = ((IMasqueradeAs)o).MasqueradingAs();

                var menu = GetMenuWithCompatibleConstructorIfExists(o);

                //If no menu takes the object o try checking the object it is masquerading as as a secondary preference
                if (menu == null && masqueradingAs != null)
                    menu = GetMenuWithCompatibleConstructorIfExists(masqueradingAs, o);

                //found a menu with compatible constructor arguments
                if (menu != null)
                {
                    if (!Settings.AllowPinning)
                    {
                        var miPin = menu.Items.OfType<AtomicCommandMenuItem>().SingleOrDefault(mi => mi.Tag is ExecuteCommandPin);

                        if (miPin != null)
                        {
                            miPin.Enabled = false;
                            miPin.ToolTipText = "Pinning is disabled in this collection";
                        }
                    }

                    return menu;
                }

                //no compatible menus so just return default menu
                var defaultMenu = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(_activator, Tree, o), o);
                defaultMenu.AddCommonMenuItems(Settings);
                return defaultMenu;
            }
            else
            {
                //it's a right click in whitespace (nothing right clicked)

                AtomicCommandUIFactory factory = new AtomicCommandUIFactory(_activator);

                if (WhitespaceRightClickMenuCommands != null)
                    return factory.CreateMenu(WhitespaceRightClickMenuCommands);
            }

            return null;
        }

        private ContextMenuStrip GetMenuWithCompatibleConstructorIfExists(object o, object oMasquerader = null)
        {
            RDMPContextMenuStripArgs args = new RDMPContextMenuStripArgs(_activator,Tree,o);
            args.CurrentlyPinnedObject = _currentlyPinned;
            args.Masquerader = oMasquerader ?? o as IMasqueradeAs;

            var objectConstructor = new ObjectConstructor();

            //now find the first RDMPContextMenuStrip with a compatible constructor
            foreach (Type menuType in _activator.RepositoryLocator.CatalogueRepository.MEF.GetTypes<RDMPContextMenuStrip>())
            {
                if (menuType.IsAbstract || menuType.IsInterface || menuType == typeof(RDMPContextMenuStrip))
                    continue;

                //try constructing menu with:
                var menu = (RDMPContextMenuStrip)objectConstructor.ConstructIfPossible(menuType,
                    args,//parameter 1 must be args
                    o //parameter 2 must be object compatible Type
                    );

                //find first menu that's compatible
                if (menu != null)
                {
                    menu.AddCommonMenuItems(Settings);
                    return menu;
                }
            }
            
            //there are no derrived classes with compatible constructors
            return null;
        }


        public void CommonItemActivation(object sender, EventArgs eventArgs)
        {
            var o = Tree.SelectedObject;
            
            var cmd = new ExecuteCommandActivate(_activator, o);
            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            OnRefreshChildProvider(_activator.CoreChildProvider);
            
            //now tell tree view to refresh the object
            
            //or from known descendancy
            var knownDescendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);
            
            //if the descendancy is known 
            if (_pinFilter != null)
                _pinFilter.OnRefreshObject(_activator.CoreChildProvider,e);

            //if it is a root object maintained by this tree and it exists
            if (MaintainRootObjects != null && MaintainRootObjects.Contains(e.Object.GetType()) && e.Object.Exists())
                    //if tree doesn't yet contain the object
                    if (!Tree.Objects.Cast<object>().Contains(e.Object))
                        Tree.AddObject(e.Object); //add it

            //item deleted?
            if (!e.Object.Exists())
            {
                //if we have the object
                if (Tree.IndexOf(e.Object) != -1)
                {
                    var parent = Tree.GetParent(e.Object);
                    //item was deleted so remove it
                    Tree.RemoveObject(e.Object);

                    //if we have a parent it might be a node category that should now disapear too
                    if (parent != null)
                    {
                        //it's a Node (e.g. SupportingDocumentsNode) but not a SingletonNode (e.g. ANOTablesNode)
                        if (parent.GetType().Name.EndsWith("Node") && !(parent is SingletonNode))
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
                
            }
            else
            {
                try
                {
                    //but the filter is currently hiding the object?
                    if (!IsHiddenByFilter(e.Object))
                    {
                        //Do we have the object itself?
                        if (Tree.IndexOf(e.Object) != -1)
                            Tree.RefreshObject(e.Object);
                        else
                        if(knownDescendancy != null) //we don't have the object but do we have something in it's descendancy?
                        {
                            var lastParent = knownDescendancy.Parents.LastOrDefault(p => Tree.IndexOf(p) != -1);
                            
                            if (lastParent != null)
                                Tree.RefreshObject(lastParent); //refresh parent
                        }
                    }
                }
                catch (ArgumentException)
                {
                    
                }
                catch (IndexOutOfRangeException)
                {
                        
                }
            }

            RefreshContextMenuStrip();
        }

        private bool IsHiddenByFilter(object o)
        {
            return Tree.IsFiltering && !Tree.FilteredObjects.Cast<object>().Contains(o);
        }

        private void OnRefreshChildProvider(ICoreChildProvider coreChildProvider)
        {
            CoreChildProvider = coreChildProvider;
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
