// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.Collections.Providers.Copying;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Theme;
using Rdmp.UI.TreeHelper;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Provides centralised functionality for all RDMPCollectionUI classes.  This includes configuring TreeListView to use the correct icons, have the correct row 
    /// height, child nodes etc.
    /// </summary>
    public partial class RDMPCollectionCommonFunctionality : IRefreshBusSubscriber
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
        
        public Func<IActivateItems,IAtomicCommand[]> WhitespaceRightClickMenuCommandsGetter { get; set; }
        
        public IDColumnProvider IDColumnProvider { get; set; }
        public OLVColumn IDColumn { get; set; }
        public CheckColumnProvider CheckColumnProvider { get; set; }
        public OLVColumn CheckColumn { get; set; }

        /// <summary>
        /// List of Types for which the children should not be returned.  By default the IActivateItems child provider knows all objects children all the way down
        /// You can cut off any branch with this property, just specify the Types to stop descending at and you will get that object Type (assuming you normally would)
        /// but no further children.
        /// </summary>
        public Type[] AxeChildren { get; set; }

        public Type[] MaintainRootObjects { get; set; }
        
        public RDMPCollectionCommonFunctionalitySettings Settings { get; private set; }

        public event EventHandler<MenuBuiltEventArgs> MenuBuilt;
         
        private static readonly Dictionary<RDMPCollection,Guid> TreeGuids = new Dictionary<RDMPCollection, Guid>()
        {
            {RDMPCollection.Tables,new Guid("8f24d624-acad-45dd-862b-01b18dfdd9a2")},
            {RDMPCollection.Catalogue,new Guid("d0f72b03-63f1-487e-9afa-51c03afa7819")},
            {RDMPCollection.DataExport,new Guid("9fb651f6-3e4f-4629-b64e-f61551ae009e")},
            {RDMPCollection.SavedCohorts,new Guid("6d0e4560-9357-4ee1-91b6-a182a57f7a6f")},
            {RDMPCollection.Cohort,new Guid("5c7cceb3-4202-47b1-b271-e2eed869d9ef")},
            {RDMPCollection.Favourites,new Guid("39d37439-ac7a-4346-8c79-9867384db92e")},
            {RDMPCollection.DataLoad,new Guid("600aad33-df6c-4013-ad92-65de19d494cf")},
        };

        /// <summary>
        /// Sets up width and visibility tracking on the given <paramref name="col"/>.  Each logically different
        /// column should have it's own unique Guid.  But it is ok for the same column (e.g. ID) in two different
        /// collection windows to share the same Guid in order to persist user preference of visibility of the concept.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="g"></param>
        public void SetupColumnTracking(OLVColumn col, Guid g)
        {
            SetupColumnTracking(Tree, col, g);
        }
        public void SetupColumnTracking(OLVColumn col, string columnUniqueIdentifier)
        {
            SetupColumnTracking(Tree, col, columnUniqueIdentifier);
        }

        /// <inheritdoc cref="SetupColumnTracking(OLVColumn, Guid)"/>
        public static void SetupColumnTracking(ObjectListView view, OLVColumn col, Guid g)
        {
            col.Width = UserSettings.GetColumnWidth(g);
            view.ColumnWidthChanged += (s, e) => UserSettings.SetColumnWidth(g, col.Width);

            col.IsVisible = UserSettings.GetColumnVisible(g);
            col.VisibilityChanged += (s, e) => UserSettings.SetColumnVisible(g, ((OLVColumn)s).IsVisible);

            view.RebuildColumns();
        }
        public static void SetupColumnTracking(ObjectListView view, OLVColumn col,string columnUniqueIdentifier)
        {
            col.Width = UserSettings.GetColumnWidth(columnUniqueIdentifier);
            view.ColumnWidthChanged += (s, e) => UserSettings.SetColumnWidth(columnUniqueIdentifier, col.Width);

            col.IsVisible = UserSettings.GetColumnVisible(columnUniqueIdentifier);
            col.VisibilityChanged += (s, e) => UserSettings.SetColumnVisible(columnUniqueIdentifier, ((OLVColumn)s).IsVisible);

            view.RebuildColumns();
        }

        public static void SetupColumnSortTracking(ObjectListView tree, Guid collectionGuid)
        {
            tree.PrimarySortColumn ??= tree.AllColumns.FirstOrDefault(c => c.IsVisible && c.Sortable);

            //persist user sort orders
            if (collectionGuid == Guid.Empty) return;
            
            //if we know the sort order for this collection last time
            var lastSort = UserSettings.GetLastColumnSortForCollection(collectionGuid);

            //reestablish that sort order
            if (lastSort != null && tree.AllColumns.Any(c => c.Text == lastSort.Item1))
            {
                tree.PrimarySortColumn = tree.GetColumn(lastSort.Item1);
                tree.PrimarySortOrder = lastSort.Item2 ? SortOrder.Ascending : SortOrder.Descending;
            }

            //and track changes to the sort order
            tree.AfterSorting += (s, e) => TreeOnAfterSorting(s, e, collectionGuid);
        }

        /// <summary>
        /// Sets up common functionality for an RDMPCollectionUI with the default settings
        /// </summary>
        /// <param name="collection"></param>
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
        /// <param name="collection"></param>
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
            Tree.KeyPress += Tree_KeyPress;

            Tree.CellToolTip.InitialDelay = UserSettings.TooltipAppearDelay;
            Tree.CellToolTipShowing += (s,e)=>Tree_CellToolTipShowing(activator,e);

            Tree.RevealAfterExpand = true;

            if (!Settings.SuppressChildrenAdder)
            {
                Tree.CanExpandGetter += CanExpandGetter;
                Tree.ChildrenGetter += ChildrenGetter;
            }

            if(!Settings.SuppressActivate)
                Tree.ItemActivate += CommonItemActivation;

            Tree.CellRightClick += CommonRightClick;
            Tree.KeyUp += CommonKeyPress;
            
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
                RenameProvider = new RenameProvider(_activator, tree, renameableColumn);
                RenameProvider.RegisterEvents();
            }

            if (Settings.AddFavouriteColumn)
            {
                FavouriteColumnProvider = new FavouriteColumnProvider(_activator, tree);
                FavouriteColumn = FavouriteColumnProvider.CreateColumn();

                SetupColumnTracking(FavouriteColumn, new Guid("ab25aa56-957c-4d1b-b395-48299be8e467"));
            }

            if (settings.AddIDColumn)
            {
                IDColumnProvider = new IDColumnProvider(tree);
                IDColumn = IDColumnProvider.CreateColumn();

                Tree.AllColumns.Add(IDColumn);
                SetupColumnTracking(IDColumn, new Guid("9d567d9c-06f5-41b6-9f0d-e630a0e23f3a"));
                Tree.RebuildColumns();
            }

            if (Settings.AddCheckColumn)
            {
                CheckColumnProvider = new CheckColumnProvider(tree, _activator.CoreIconProvider);
                CheckColumn = CheckColumnProvider.CreateColumn();

                SetupColumnTracking(CheckColumn, new Guid("8d9c6a0f-82a8-4f4e-b058-e3017d8d60e0"));

                Tree.AllColumns.Add(CheckColumn);
                Tree.RebuildColumns();
            }
            CoreIconProvider = activator.CoreIconProvider;

            CopyPasteProvider = new CopyPasteProvider();
            CopyPasteProvider.RegisterEvents(tree);
            
            CoreChildProvider = _activator.CoreChildProvider;
            
            _activator.Emphasise += _activator_Emphasise;

            Tree.TreeFactory = TreeFactoryGetter;
            Tree.RebuildAll(true);
            
            Tree.FormatRow += Tree_FormatRow;
            Tree.KeyDown += Tree_KeyDown;

            if(Settings.AllowSorting)
            {
                SetupColumnSortTracking(Tree, TreeGuids.ContainsKey(collection) ? TreeGuids[collection] : Guid.Empty);
            }
            else
                foreach (OLVColumn c in Tree.AllColumns)
                    c.Sortable = false;
        }

        public static void Tree_CellToolTipShowing(IActivateItems activator, ToolTipShowingEventArgs e)
        {            
            var model = e.Model;

            if (model is IMasqueradeAs m)
                model = m.MasqueradingAs();

            e.AutoPopDelay = 32767;

            string problem = activator.DescribeProblemIfAny(model);

            if (!string.IsNullOrWhiteSpace(problem))
            {
                e.StandardIcon = ToolTipControl.StandardIcons.Error;
                e.Title = model.ToString();

                e.Text = problem;
                e.IsBalloon = true;

            }
            else
            if (model is ICanBeSummarised sum)
            {
                e.StandardIcon = ToolTipControl.StandardIcons.Info;
                
                if(model is IMapsDirectlyToDatabaseTable d)
                {
                    e.Title = $"{model} (ID: {d.ID})";
                }
                else
                {
                    e.Title = model.ToString();
                }

                e.Text = GenerateSummary(activator,sum);
                e.IsBalloon = true;
            }

        }

        static DateTime lastInvalidatedCache = DateTime.Now;
        static Dictionary<object, string> cache = new Dictionary<object, string>();

        private static string GenerateSummary(IActivateItems activator, ICanBeSummarised sum)
        {
            if(DateTime.Now.Subtract(lastInvalidatedCache).TotalSeconds > 10)
            {
                cache.Clear();
                lastInvalidatedCache = DateTime.Now;
            }

            if(cache.ContainsKey(sum))
                return cache[sum];

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(sum.GetSummary(false, false));

            var gotoF = new GoToCommandFactory(activator);
            try
            {
                var associatedObjects = gotoF.GetCommands(sum).OfType<ExecuteCommandShow>()
                    .ToDictionary(cmd => cmd.GetCommandName(), cmd => cmd.GetObjects().Where(o => o != null));

                foreach (var kvp in associatedObjects)
                {
                    if (!kvp.Value.Any())
                        continue;

                    var val = string.Join(", ", kvp.Value.Select(o => o.ToString()).ToArray());

                    if(val.Length>100)
                    {
                        val = val.Substring(0,100) + "...";
                    }

                    sb.AppendLine($"{kvp.Key}: {val}");
                }
            }
            catch (Exception)
            {
                // couldn't get all the things you can go to, nevermind
                return sb.ToString();
            }
            finally
            {
                cache.Add(sum, sb.ToString());
            }

            return sb.ToString();
        }

        private void Tree_KeyDown(object sender, KeyEventArgs e)
        {
            if(_shortcutKeys.Contains(e.KeyCode))
            {

                // Prevents bong sound
                e.SuppressKeyPress = true;
            }
        }

        void Tree_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Prevents keyboard 'bong' sound occuring when using Enter to activate an object
            if (e.KeyChar == (char)Keys.Enter)
                e.Handled = true;
        }

        private static void TreeOnAfterSorting(object sender, AfterSortingEventArgs e, Guid collectionGuid)
        {
            UserSettings.SetLastColumnSortForCollection(collectionGuid, e.ColumnToSort == null ? null:e.ColumnToSort.Text, e.SortOrder == SortOrder.Ascending);
        }

        private void CreateColorIndicator(TreeListView tree, RDMPCollection collection)
        {
            if(Tree.Parent == null || collection == RDMPCollection.None)
                return;

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

        void Tree_FormatRow(object sender, FormatRowEventArgs e)
        {
            bool hasProblems = _activator.HasProblem(e.Model);

            var disableable = e.Model as IDisableable;

            if (disableable != null && disableable.IsDisabled)
            {
                e.Item.ForeColor = Color.FromArgb(152,152,152);
                
                //make it italic
                if(!e.Item.Font.Italic)
                    e.Item.Font = new Font(e.Item.Font,FontStyle.Italic);

                e.Item.BackColor = Color.FromArgb(225,225,225);
            }
            else
            {
                //make it not italic
                if(e.Item.Font.Italic)
                    e.Item.Font = new Font(e.Item.Font,FontStyle.Regular);

                e.Item.ForeColor = hasProblems ? Color.Red : Color.Black;
                e.Item.BackColor = hasProblems ? Color.FromArgb(255,220,220) : Color.White;
            }
        }

        private TreeListView.Tree TreeFactoryGetter(TreeListView view)
        {
            return new RDMPCollectionCommonFunctionalityTreeHijacker(view);
        }
        
        // Tracks when RefreshContextMenuStrip is called to prevent rebuilding on select and right click in rapid succession
        private object _lastMenuObject;
        private DateTime _lastMenuBuilt = DateTime.Now;
        private ContextMenuStrip _menu;
        HashSet<Keys> _shortcutKeys = new HashSet<Keys>(){
            Keys.I,
            Keys.Delete,
            Keys.F1,
            Keys.F5
        };

        private void RefreshContextMenuStrip()
        {
            // appropriate menu has already been created recently
            if(_lastMenuObject == Tree.SelectedObject && DateTime.Now.Subtract(_lastMenuBuilt) < TimeSpan.FromSeconds(2))
                return;

            //clear the old menu strip first so old shortcuts cannot be activated during 
            if(_menu != null)
                _menu.Dispose();

            if(Tree.SelectedObjects.Count <= 1)
                _menu = GetMenuIfExists(_lastMenuObject = Tree.SelectedObject);
            else
                _menu = GetMenuIfExists(_lastMenuObject = Tree.SelectedObjects);

            _lastMenuBuilt = DateTime.Now;
        }

        public void CommonRightClick(object sender, CellRightClickEventArgs e)
        {
            //if we aren't doing a multi select
            if(Tree.SelectedObjects.Count <= 1)
            {
                Tree.SelectedObject = e.Model;
            }

            RefreshContextMenuStrip();
            _menu?.Show(Tree.PointToScreen(e.Location));
        }

        public void CommonKeyPress(object sender , KeyEventArgs e)
        {
            if(_shortcutKeys.Contains(e.KeyCode))
            {
                RefreshContextMenuStrip();

                if(_menu != null)
                {
                    var options = _menu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(mi=>mi.ShortcutKeys == (e.Control ? Keys.Control | e.KeyCode : e.KeyCode));
                    options?.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        void _activator_Emphasise(object sender, EmphasiseEventArgs args)
        {
            var rootObject = _activator.GetRootObjectOrSelf(args.Request.ObjectToEmphasise);
                        
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
                try
                {
                    Tree.BeginUpdate();
                    ExpandToDepth(args.Request.ExpansionDepth, args.Request.ObjectToEmphasise);
                }
                finally
                {
                    Tree.EndUpdate();
                }
                
            //update index now pin filter is applied
            index = Tree.IndexOf(args.Request.ObjectToEmphasise);

            //select the object and ensure it's visible
            Tree.SelectedObject = args.Request.ObjectToEmphasise;
            Tree.EnsureVisible(index);

            
            args.Sender = Tree.FindForm();
        }

        /// <summary>
        /// Expands the current object (which must exist/be visible in the UI) to the given depth
        /// </summary>
        /// <param name="expansionDepth"></param>
        /// <param name="currentObject"></param>
        public void ExpandToDepth(int expansionDepth, object currentObject)
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

        /// <summary>
        /// Creates a menu compatible with object <paramref name="o"/>.  Returns null if no compatible menu exists.
        /// Errors are reported to <see cref="IBasicActivateItems.GlobalErrorCheckNotifier"/> (if set up).
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public ContextMenuStrip GetMenuIfExists(object o)
        {
            try
            {
                if (o is ICollection many)
                {
                    var menu = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(_activator,Tree,many),many);

                    var factory = new AtomicCommandUIFactory(_activator);

                    if (many.Cast<object>().All(d => d is IMapsDirectlyToDatabaseTable))
                    {
                        menu.Items.Add(factory.CreateMenuItem(new ExecuteCommandStartSession(_activator, many.Cast<IMapsDirectlyToDatabaseTable>().ToArray(),null)));
                        menu.Items.Add(factory.CreateMenuItem(new ExecuteCommandAddToSession(_activator, many.Cast<IMapsDirectlyToDatabaseTable>().ToArray(),null)));
                    }

                    var cmdFactory = new AtomicCommandFactory(_activator);
                    foreach(var p in cmdFactory.CreateManyObjectCommands(many))
                    {
                        menu.Add(p);
                    }

                    MenuBuilt?.Invoke(this,new MenuBuiltEventArgs(menu,o));
                    return Sort(menu);
                }

                if (o != null)
                {
                    //is o masquerading as someone else?
                    IMasqueradeAs masquerader = o as IMasqueradeAs;

                    //if so this is who he is pretending to be
                    object masqueradingAs = null;

                    if (masquerader != null)
                        masqueradingAs = masquerader.MasqueradingAs(); //yes he is masquerading!

                    var menu = GetMenuWithCompatibleConstructorIfExists(o);

                    //If no menu takes the object o try checking the object it is masquerading as as a secondary preference
                    if (menu == null && masqueradingAs != null)
                        menu = GetMenuWithCompatibleConstructorIfExists(masqueradingAs, masquerader);

                    //found a menu with compatible constructor arguments
                    if (menu != null)
                    {                        
                        MenuBuilt?.Invoke(this,new MenuBuiltEventArgs(menu,o));
                        return Sort(menu);
                    }

                    //no compatible menus so just return default menu
                    var defaultMenu = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(_activator, Tree, o), o);
                    defaultMenu.AddCommonMenuItems(this);
                    
                    MenuBuilt?.Invoke(this,new MenuBuiltEventArgs(defaultMenu,o));
                    return Sort(defaultMenu);
                }
                else
                {
                    //it's a right click in whitespace (nothing right clicked)

                    AtomicCommandUIFactory factory = new AtomicCommandUIFactory(_activator);

                    if (WhitespaceRightClickMenuCommandsGetter != null)
                    {
                        var toReturn = new RDMPContextMenuStrip(new RDMPContextMenuStripArgs(_activator, Tree, this), this);

                        foreach(var cmd in WhitespaceRightClickMenuCommandsGetter(_activator))
                        {
                            toReturn.Add(cmd);
                        }

                        toReturn.AddCommonMenuItems(this);
                        return Sort(toReturn);

                    }
                }

                return null;
            }
            catch(Exception ex)
            {
                if(_activator?.GlobalErrorCheckNotifier == null)
                    throw;

                _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs($"Failed to build menu for {o} of Type {o?.GetType()}",CheckResult.Fail,ex));
                return null;
            }           
        }

        //once we find the best menu for object of Type x then we want to cache that knowledge and go directly to that menu every time
        Dictionary<Type,Type> _cachedMenuCompatibility = new Dictionary<Type, Type>();
        
        private ContextMenuStrip GetMenuWithCompatibleConstructorIfExists(object o, IMasqueradeAs oMasquerader = null)
        {
            RDMPContextMenuStripArgs args = new RDMPContextMenuStripArgs(_activator,Tree,o);
            args.Masquerader = oMasquerader ?? o as IMasqueradeAs;

            var objectConstructor = new ObjectConstructor();

            Type oType = o.GetType();

            //if we have encountered this object type before
            if (_cachedMenuCompatibility.ContainsKey(oType))
            {
                Type compatibleMenu = _cachedMenuCompatibility[oType];
                
                //we know there are no menus compatible with o
                if (compatibleMenu == null)
                    return null;

                return ConstructMenu(objectConstructor, _cachedMenuCompatibility[oType], args, o);
            }
                

            //now find the first RDMPContextMenuStrip with a compatible constructor
            foreach (Type menuType in _activator.RepositoryLocator.CatalogueRepository.MEF.GetTypes<RDMPContextMenuStrip>())
            {
                if (menuType.IsAbstract || menuType.IsInterface || menuType == typeof(RDMPContextMenuStrip))
                    continue;

                //try constructing menu with:
                var menu = ConstructMenu(objectConstructor,menuType,args,o);

                //find first menu that's compatible
                if (menu != null)
                {
                    if (!_cachedMenuCompatibility.ContainsKey(oType))
                        _cachedMenuCompatibility.Add(oType, menu.GetType());

                    return menu;
                }
            }

            //we know there are no menus compatible with this type
            if (!_cachedMenuCompatibility.ContainsKey(oType))
                _cachedMenuCompatibility.Add(oType, null);

            //there are no derrived classes with compatible constructors
            return null;
        }

        private RDMPContextMenuStrip ConstructMenu(ObjectConstructor objectConstructor, Type type, RDMPContextMenuStripArgs args, object o)
        {
            //there is a compatible menu Type known
            
            //parameter 1 must be args
            //parameter 2 must be object compatible Type

            var menu = (RDMPContextMenuStrip)objectConstructor.ConstructIfPossible(type, args, o);
            
            if(menu != null)
                menu.AddCommonMenuItems(this);

            return menu;
        }

        private ContextMenuStrip Sort(ContextMenuStrip menu)
        {
            if (menu != null)
            {
                OrderMenuItems(menu.Items);
            }
            return menu;
        }

        private void OrderMenuItems(ToolStripItemCollection coll)
        {
            Dictionary<int, List<ToolStripItem>> itemsByBucket = new Dictionary<int, List<ToolStripItem>>();

            // Create buckets for every item in every context menu
            foreach(ToolStripItem oItem in coll)
            {
                int bucket = (int)GetWeight(oItem);

                if (!itemsByBucket.ContainsKey(bucket))
                {
                    itemsByBucket.Add(bucket, new List<ToolStripItem>());
                }

                itemsByBucket[bucket].Add(oItem);
            }

            coll.Clear();
            
            var buckets = itemsByBucket.OrderBy(kvp => kvp.Key).ToArray();

            for(int i =0;i< buckets.Length;i++)
            {
                // add all the items
                foreach(var item in buckets[i].Value.OrderBy(i=>GetWeight(i)))
                {
                    coll.Add(item);

                    if (item is ToolStripMenuItem mi)
                    {
                        // if menu item has submenus
                        if (mi.DropDownItems.Count > 0)
                        {
                            // sort those too - recurisvely
                            OrderMenuItems(mi.DropDownItems);
                        }
                    }
                }

                // if there are more buckets to come
                if(i != buckets.Length - 1)
                {
                    coll.Add(new ToolStripSeparator());
                }
            }
        }

        private float GetWeight(ToolStripItem oItem)
        {
            if (oItem.Tag is IAtomicCommand cmd)
            {
                return cmd.Weight;
            }

            if (oItem is ToolStripMenuItem mi)
            {
                if(mi.DropDownItems.Count > 0)
                    return mi.DropDownItems.Cast<ToolStripItem>().Min(GetWeight);

                if (mi.Tag is float f)
                    return f;
            }
            return 0;
        }

        public void CommonItemActivation(object sender, EventArgs eventArgs)
        {
            var o = Tree.SelectedObject;
            
            if(o == null)
                return;

            if (UserSettings.DoubleClickToExpand)
            {
                if (Tree.CanExpand(o) && !Tree.IsExpanded(o))
                {
                    Tree.Expand(o);
                    return;
                }
                if (Tree.IsExpanded(o))
                {
                    Tree.Collapse(o);
                    return;
                }
            }

            var cmd = new ExecuteCommandActivate(_activator, o);
            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            RefreshObject(e.Object,e.Exists);

            //now tell tree view to refresh the object
            
            RefreshContextMenuStrip();

            //also refresh anyone who is masquerading as e.Object
            foreach (IMasqueradeAs masquerader in _activator.CoreChildProvider.GetMasqueradersOf(e.Object))
                RefreshObject(masquerader, e.Exists);
            
        }

        private void RefreshObject(object o, bool exists)
        {
            Tree.RebuildAll(true);
        }

        public void TearDown()
        {
            if(IsSetup)
            {
                _activator.RefreshBus.Unsubscribe(this);
                _activator.Emphasise -= _activator_Emphasise;
            }
        }

    }
}
