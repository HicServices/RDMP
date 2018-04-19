using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DashboardTabs;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.Menus;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManagerLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.ItemActivation
{
    /// <summary>
    /// Central component class for handling all low level RDMP main user interface systems. This includes things like Tree object child provision, Icon provision,
    /// the publish system for notifying out of date objects etc.  Each function is segregated by a component class property e.g. RefreshBus, FavouritesProvider etc
    /// 
    /// <para>Also exposes the location of the Catalogue / Data Export repository databases via RepositoryLocator</para>
    /// </summary>
    public interface IActivateItems
    {
        /// <summary>
        /// Component for publishing the fact that an object has recently been put out of date by you.
        /// </summary>
        RefreshBus RefreshBus { get; }
        
        /// <summary>
        /// Component for telling you whether a given DatabaseEntity is one of the current users favourite objects and for toggling it
        /// </summary>
        FavouritesProvider FavouritesProvider { get;}
        
        /// <summary>
        /// Component for recording object tree inheritance (for RDMPCollectionUI primarily but also for anyone who wants to know children of objects or all objects quickly without having to go back to the database)
        /// </summary>
        ICoreChildProvider CoreChildProvider { get; }

        /// <summary>
        /// List of the currently loaded IPluginUserInterface classes (these allow injection of additional tree items, tailoring context menus etc).  This list will
        /// include some intrinsic RDMP IPluginUserInterfaces as part of its own internal design but most of these will be third party plugins.
        /// </summary>
        List<IPluginUserInterface> PluginUserInterfaces { get; }

        /// <summary>
        /// Component for closing and opening multiple windows at once for optimal user experience for achieving a given task (e.g. running a data load)
        /// </summary>
        IArrangeWindows WindowArranger { get;}


        Form ShowWindow(Control singleControlForm, bool asDocument = false);

        Form ShowRDMPSingleDatabaseObjectControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity objectOfTypeT);

        /// <summary>
        /// Stores the location of the Catalogue / Data Export repository databases and provides access to their objects
        /// </summary>
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <summary>
        /// Component for providing access to RDMPConcept icon images, these are 19x19 pixel icons representing specific objects/concepts in RDMP
        /// </summary>
        ICoreIconProvider CoreIconProvider { get; }

        /// <summary>
        /// Component for auditing errors that should be brought to the users attention subtly (e.g. if a plugin crashes while attempting to create menu items)
        /// </summary>
        ICheckNotifier GlobalErrorCheckNotifier { get; }
        
        /// <summary>
        /// Component for starting drag or copy operations
        /// </summary>
        ICommandFactory CommandFactory { get;}

        /// <summary>
        /// Component for suggesting completion options for an ongoing drag or paste
        /// </summary>
        ICommandExecutionFactory CommandExecutionFactory { get;}

        /// <summary>
        /// Launches a new instance of the specified RDMPSingleDatabaseObjectControl Type with the supplied DatabaseEntity.  If you already have
        /// an ICommandExecutionProposal which facilitates Activation then you can instead use CommandExecutionFactory.Activate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="databaseObject"></param>
        /// <returns></returns>
        T Activate<T, T2>(T2 databaseObject) where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity;

        /// <summary>
        /// Launches a new instance of the specified IObjectCollectionControl with the supplied collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        T Activate<T>(IPersistableObjectCollection collection) where T : IObjectCollectionControl, new();

        bool DeleteWithConfirmation(object sender, IDeleteable deleteable,string overrideConfirmationText=null);
        bool DeleteControlFromDashboardWithConfirmation(object sender, DashboardControl controlToDelete);

        IFilter AdvertiseCatalogueFiltersToUser(IContainer containerToImportOneInto, IFilter[] filtersThatCouldBeImported);
        void ActivateCatalogueItemIssue(object sender, CatalogueItemIssue catalogueItemIssue);

        void ActivateConvertColumnInfoIntoANOColumnInfo(ColumnInfo columnInfo);
        void ActivateSupportingDocument(object sender, SupportingDocument supportingDocument);
        void ActivateSupportingSqlTable(object sender, SupportingSQLTable supportingSQLTable);
        void ActivateDataAccessCredentials(object sender, DataAccessCredentials dataAccessCredentials);
        
        void ViewDataSample(IViewSQLAndResultsCollection collection);

        DashboardLayoutUI ActivateDashboard(object sender, DashboardLayout dashboard);

        event EmphasiseItemHandler Emphasise;
        
        /// <summary>
        /// Requests that the activator highlight or otherwise emphasise the supplied item.  Depending on who is subscribed to this event nothing may actually happen
        /// </summary>
        void RequestItemEmphasis(object sender, EmphasiseRequest request);
        
        void ActivateLookupConfiguration(object sender, Catalogue catalogue,TableInfo optionalLookupTableInfo=null);
        void ActivateJoinInfoConfiguration(object sender, TableInfo tableInfo);

        void ActivateReOrderCatalogueItems(Catalogue catalogue);

        void ActivateConfigureValidation(object sender, Catalogue catalogue);
        void ViewFilterGraph(object sender,FilterGraphObjectCollection collection);

        void ActivateViewCohortIdentificationConfigurationSql(object sender, CohortIdentificationConfiguration cic);
        void ActivateViewLog(ExternalDatabaseServer loggingServer, int dataLoadRunID);

        IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata);
        void ActivateExternalDatabaseServer(object sender, ExternalDatabaseServer externalDatabaseServer);
        void ActivateTableInfo(object sender, TableInfo tableInfo);
        void ActivatePreLoadDiscardedColumn(object sender, PreLoadDiscardedColumn preLoadDiscardedColumn);
        void ActivatePermissionWindow(object sender, PermissionWindow permissionWindow);
        bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject);
        bool HasProblem(object model);
        string DescribeProblemIfAny(object model);
        object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise);
    }
}
