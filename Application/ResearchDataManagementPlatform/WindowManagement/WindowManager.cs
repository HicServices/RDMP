// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI;
using Rdmp.UI.Collections;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Events;
using ResearchDataManagementPlatform.WindowManagement.HomePane;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WeifenLuo.WinFormsUI.Docking;
using Image = SixLabors.ImageSharp.Image;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
/// Handles creating and tracking the main RDMPCollectionUIs tree views
/// </summary>
public class WindowManager
{
    private readonly Dictionary<RDMPCollection, PersistableToolboxDockContent> _visibleToolboxes = new();
    private readonly List<RDMPSingleControlTab> _trackedWindows = new();
    private readonly List<DockContent> _trackedAdhocWindows = new();

    public NavigationTrack<INavigation> Navigation { get; private set; }
    public event TabChangedHandler TabChanged;

    private readonly DockPanel _mainDockPanel;

    public RDMPMainForm MainForm { get; set; }

    /// <summary>
    /// The location finder for the Catalogue and optionally Data Export databases
    /// </summary>
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

    public ActivateItems ActivateItems;
    private readonly WindowFactory _windowFactory;

    public event RDMPCollectionCreatedEventHandler CollectionCreated;

    private HomeUI _home;
    private DockContent _homeContent;

    public WindowManager(ITheme theme, RDMPMainForm mainForm, RefreshBus refreshBus, DockPanel mainDockPanel,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
    {
        _windowFactory = new WindowFactory(repositoryLocator, this);
        ActivateItems = new ActivateItems(theme, refreshBus, mainDockPanel, repositoryLocator, _windowFactory, this,
            globalErrorCheckNotifier);

        GlobalExceptionHandler.Instance.Handler = e =>
            globalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));

        _mainDockPanel = mainDockPanel;

        MainForm = mainForm;
        RepositoryLocator = repositoryLocator;

        Navigation = new NavigationTrack<INavigation>(c => c.IsAlive, c => c.Activate(ActivateItems));
        mainDockPanel.ActiveDocumentChanged += mainDockPanel_ActiveDocumentChanged;
        ActivateItems.Emphasise += RecordEmphasis;
    }

    private void RecordEmphasis(object sender, EmphasiseEventArgs args)
    {
        if (args.Request.ObjectToEmphasise is IMapsDirectlyToDatabaseTable m)
            Navigation.Append(new CollectionNavigation(m));
    }

    /// <summary>
    /// Creates a new instance of the given RDMPCollectionUI specified by the Enum collectionToCreate at the specified dock position
    /// </summary>
    /// <param name="collectionToCreate"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public PersistableToolboxDockContent Create(RDMPCollection collectionToCreate,
        DockState position = DockState.DockLeft)
    {
        PersistableToolboxDockContent toReturn;
        RDMPCollectionUI collection;

        switch (collectionToCreate)
        {
            case RDMPCollection.Catalogue:
                collection = new CatalogueCollectionUI();
                toReturn = Show(RDMPCollection.Catalogue, collection, "Catalogues",
                    Image.Load<Rgba32>(CatalogueIcons.Catalogue));
                break;

            case RDMPCollection.DataLoad:
                collection = new LoadMetadataCollectionUI();
                toReturn = Show(RDMPCollection.DataLoad, collection, "Load Configurations",
                    Image.Load<Rgba32>(CatalogueIcons.LoadMetadata));
                break;

            case RDMPCollection.Tables:
                collection = new TableInfoCollectionUI();
                toReturn = Show(RDMPCollection.Tables, collection, "Tables",
                    Image.Load<Rgba32>(CatalogueIcons.TableInfo));
                break;

            case RDMPCollection.DataExport:
                if (RepositoryLocator.DataExportRepository == null)
                {
                    WideMessageBox.Show("Data export database unavailable",
                        "Cannot create DataExport Toolbox because DataExportRepository has not been set/created yet");
                    return null;
                }

                collection = new DataExportCollectionUI();
                toReturn = Show(RDMPCollection.DataExport, collection, "Projects",
                    Image.Load<Rgba32>(CatalogueIcons.Project));
                break;

            case RDMPCollection.Cohort:
                collection = new CohortIdentificationCollectionUI();
                toReturn = Show(RDMPCollection.Cohort, collection, "Cohort Builder",
                    Image.Load<Rgba32>(CatalogueIcons.CohortIdentificationConfiguration));
                break;
            case RDMPCollection.SavedCohorts:
                collection = new SavedCohortsCollectionUI();
                toReturn = Show(RDMPCollection.SavedCohorts, collection, "Saved Cohorts",
                    Image.Load<Rgba32>(CatalogueIcons.AllCohortsNode));
                break;
            case RDMPCollection.Favourites:
                collection = new FavouritesCollectionUI();
                toReturn = Show(RDMPCollection.Favourites, collection, "Favourites",
                    Image.Load<Rgba32>(CatalogueIcons.Favourite));
                break;

            case RDMPCollection.Configurations:
                collection = new ConfigurationsCollectionUI();
                toReturn = Show(RDMPCollection.Configurations, collection, "Configurations",
                     Image.Load<Rgba32>(FamFamFamIcons.pencil_go));
                break;

            default: throw new ArgumentOutOfRangeException(nameof(collectionToCreate));
        }

        toReturn.DockState = position;

        collection.SetItemActivator(ActivateItems);

        CollectionCreated?.Invoke(this, new RDMPCollectionCreatedEventHandlerArgs(collectionToCreate));

        collection.CommonTreeFunctionality.Tree.SelectionChanged += (s, e) =>
        {
            if (collection.CommonTreeFunctionality.Tree.SelectedObject is IMapsDirectlyToDatabaseTable im)
                Navigation.Append(new CollectionNavigation(im));
        };

        return toReturn;
    }


    private PersistableToolboxDockContent Show(RDMPCollection collection, RDMPCollectionUI control, string label,
        Image<Rgba32> image)
    {
        var content =
            _windowFactory.Create(ActivateItems, control, label, image,
                collection); //these are collections so are not tracked with a window tracker.
        content.Closed += (s, e) => content_Closed(collection);

        _visibleToolboxes.Add(collection, content);
        content.Show(_mainDockPanel, DockState.DockLeft);

        return content;
    }

    private void content_Closed(RDMPCollection collection)
    {
        //no longer visible
        _visibleToolboxes.Remove(collection);
    }

    /// <summary>
    /// Closes the specified RDMPCollectionUI (must be open - use IsVisible to check this)
    /// </summary>
    /// <param name="collection"></param>
    public void Destroy(RDMPCollection collection)
    {
        _visibleToolboxes[collection].Close();
    }

    /// <summary>
    /// Brings the specified collection to the front (must already be visible)
    /// </summary>
    /// <param name="collection"></param>
    public void Pop(RDMPCollection collection)
    {
        if (!_visibleToolboxes.TryGetValue(collection, out var content)) return;
        content.DockState = content.DockState switch
        {
            DockState.DockLeftAutoHide => DockState.DockLeft,
            DockState.DockRightAutoHide => DockState.DockRight,
            DockState.DockTopAutoHide => DockState.DockTop,
            DockState.DockBottomAutoHide => DockState.DockBottom,
            _ => _visibleToolboxes[collection].DockState
        };

        content.Activate();
    }

    /// <summary>
    /// Returns true if the corresponding RDMPCollectionUI is open (even if it is buried under other windows).
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public bool IsVisible(RDMPCollection collection) => _visibleToolboxes.ContainsKey(collection);

    public RDMPCollection GetFocusedCollection()
    {
        return _visibleToolboxes.Where(static t => t.Value.ContainsFocus).Select(static t => t.Key).FirstOrDefault();
    }

    internal void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
    {
        foreach (var c in _trackedWindows)
            if (c.Control is IConsultableBeforeClosing consult)
            {
                consult.ConsultAboutClosing(this, e);

                if (e.Cancel) return;
            }
    }


    /// <summary>
    /// Attempts to ensure that a compatible RDMPCollectionUI is made visible for the supplied object which must be one of the expected root Tree types of
    /// an RDMPCollectionUI.  For example Project is the a root object of DataExportCollectionUI.  If a matching collection is already visible or no collection
    /// supports the supplied object as a root object then nothing will happen.  Otherwise the corresponding collection will be shown
    /// </summary>
    /// <param name="root"></param>
    public void ShowCollectionWhichSupportsRootObjectType(object root)
    {
        var collection = GetCollectionForRootObject(root);

        if (collection == RDMPCollection.None)
            return;

        if (IsVisible(collection))
        {
            Pop(collection);
            return;
        }

        Create(collection);
    }

    public RDMPCollection GetCollectionForRootObject(object root)
    {
        if (FavouritesCollectionUI.IsRootObject(ActivateItems, root))
            return RDMPCollection.Favourites;

        if (CatalogueCollectionUI.IsRootObject(root))
            return RDMPCollection.Catalogue;

        if (CohortIdentificationCollectionUI.IsRootObject(root))
            return RDMPCollection.Cohort;

        if (DataExportCollectionUI.IsRootObject(root))
            return RDMPCollection.DataExport;

        if (LoadMetadataCollectionUI.IsRootObject(root))
            return RDMPCollection.DataLoad;

        if (TableInfoCollectionUI.IsRootObject(root))
            return RDMPCollection.Tables;

        return SavedCohortsCollectionUI.IsRootObject(root) ? RDMPCollection.SavedCohorts : RDMPCollection.None;
    }

    /// <summary>
    /// Displays the HomeUI tab or brings it to the front if it is already open
    /// </summary>
    public void PopHome()
    {
        if (_home == null)
        {
            _home = new HomeUI(ActivateItems);

            _homeContent = _windowFactory.Create(ActivateItems, _home, "Home",
                Image.Load<Rgba32>(FamFamFamIcons.application_home));
            _homeContent.Closed += (s, e) => _home = null;
            _homeContent.Show(_mainDockPanel, DockState.Document);
        }
        else
        {
            _homeContent.Activate();
        }
    }

    /// <summary>
    /// Closes all currently open RDMPCollectionUI tabs
    /// </summary>
    public void CloseAllToolboxes()
    {
        foreach (RDMPCollection collection in Enum.GetValues(typeof(RDMPCollection)))
            if (IsVisible(collection))
                Destroy(collection);
    }

    /// <summary>
    /// Closes all content window tabs (i.e. anything that isn't an RDMPCollectionUI tab - see CloseAllToolboxes)
    /// </summary>
    public void CloseAllWindows()
    {
        CloseAllWindows(null);
    }


    /// <summary>
    /// Closes all Tracked windows
    /// </summary>
    /// <param name="tab"></param>
    public void CloseAllWindows(RDMPSingleControlTab tab)
    {
        if (tab != null)
        {
            CloseAllButThis(tab);
            tab.Close();
        }
        else
        {
            foreach (var trackedWindow in _trackedWindows.ToArray())
                trackedWindow.Close();

            foreach (var adhoc in _trackedAdhocWindows.ToArray())
                adhoc.Close();
        }
    }

    private void mainDockPanel_ActiveDocumentChanged(object sender, EventArgs e)
    {
        var newTab = (DockContent)_mainDockPanel.ActiveDocument;

        if (newTab?.ParentForm != null)
        {
            Navigation.Append(new TabNavigation(newTab));
            newTab.ParentForm.Text = $"{newTab.TabText} - RDMP";
        }


        TabChanged?.Invoke(sender, newTab);
    }


    /// <summary>
    /// Records the fact that a new single object editing tab has been opened.  .
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if another instance of the Control Type is already active with the same DatabaseObject</exception>
    /// <param name="window"></param>
    public void AddWindow(RDMPSingleControlTab window)
    {
        if (window is PersistableSingleDatabaseObjectDockContent singleObjectUI)
            if (AlreadyActive(singleObjectUI.Control.GetType(), singleObjectUI.DatabaseObject))
                throw new ArgumentOutOfRangeException(
                    $"Cannot create another window for object {singleObjectUI.DatabaseObject} of type {singleObjectUI.Control.GetType()} because there is already a window active for that object/window type");

        _trackedWindows.Add(window);

        window.FormClosed += (s, e) => Remove(window);
    }

    /// <summary>
    /// Records the fact that a new impromptu/adhoc tab has been shown.  These windows are not checked for duplication.
    /// </summary>
    /// <param name="adhocWindow"></param>
    public void AddAdhocWindow(DockContent adhocWindow)
    {
        _trackedAdhocWindows.Add(adhocWindow);
        adhocWindow.FormClosed += (s, e) => _trackedAdhocWindows.Remove(adhocWindow);
    }

    private void Remove(RDMPSingleControlTab window)
    {
        _trackedWindows.Remove(window);
    }

    public PersistableSingleDatabaseObjectDockContent GetActiveWindowIfAnyFor(Type windowType,
        IMapsDirectlyToDatabaseTable databaseObject)
    {
        return _trackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().SingleOrDefault(t =>
            t.Control.GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
    }

    public PersistableObjectCollectionDockContent GetActiveWindowIfAnyFor(Type windowType,
        IPersistableObjectCollection collection)
    {
        return _trackedWindows.OfType<PersistableObjectCollectionDockContent>()
            .SingleOrDefault(t => t.Control.GetType() == windowType && t.Collection.Equals(collection));
    }

    /// <summary>
    /// Check whether a given RDMPSingleControlTab is already showing with the given DatabaseObject (e.g. is user currently editing Catalogue bob in CatalogueUI)
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <param name="windowType">A Type derived from RDMPSingleControlTab</param>
    /// <param name="databaseObject">An instance of an object which matches the windowType</param>
    /// <returns></returns>
    public bool AlreadyActive(Type windowType, IMapsDirectlyToDatabaseTable databaseObject)
    {
        return !typeof(IRDMPSingleDatabaseObjectControl).IsAssignableFrom(windowType)
            ? throw new ArgumentException("windowType must be a Type derived from RDMPSingleControlTab")
            : _trackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().Any(t =>
                t.Control.GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
    }

    /// <summary>
    /// Closes all Tracked windows except the specified tab
    /// </summary>
    public void CloseAllButThis(DockContent content)
    {
        var trackedWindowsToClose = _trackedWindows.ToArray().Where(t => t != content);

        foreach (var trackedWindow in trackedWindowsToClose)
            CloseWindowIfInSameScope(trackedWindow, content);

        foreach (var adhoc in _trackedAdhocWindows.ToArray().Where(t => t != content))
            CloseWindowIfInSameScope(adhoc, content);
    }

    private static void CloseWindowIfInSameScope(DockContent toClose, DockContent tabInSameScopeOrNull)
    {
        var parent = tabInSameScopeOrNull?.Parent;

        if (toClose != null && (parent == null || toClose.Parent == parent))
            toClose.Close();
    }

    public void CloseCurrentTab()
    {
        //nothing to close
        if (Navigation.Current == null)
            return;

        Navigation.Suspend();
        try
        {
            Navigation.Current.Close();

            Navigation.Current?.Activate(ActivateItems);
        }
        finally
        {
            Navigation.Resume();
            if (_mainDockPanel.ActiveDocument is DockContent dc)
                Navigation.Append(new TabNavigation(dc));
        }
    }

    /// <summary>
    /// Returns all tracked tabs currently open of the Type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> GetAllWindows<T>()
    {
        return _trackedWindows.OfType<RDMPSingleControlTab>().Select(t => t.Control).OfType<T>();
    }
}