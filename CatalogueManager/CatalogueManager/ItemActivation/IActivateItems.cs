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
    public interface IActivateItems
    {
        RefreshBus RefreshBus { get; }
        
        FavouritesProvider FavouritesProvider { get;}
        
        ICoreChildProvider CoreChildProvider { get; }

        List<IPluginUserInterface> PluginUserInterfaces { get; }

        IArrangeWindows WindowArranger { get;}

        Form ShowWindow(Control singleControlForm, bool asDocument = false);
        Form ShowRDMPSingleDatabaseObjectControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity objectOfTypeT);

        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        ICoreIconProvider CoreIconProvider { get; }

        ICheckNotifier GlobalErrorCheckNotifier { get; }
        
        ICommandFactory CommandFactory { get;}
        ICommandExecutionFactory CommandExecutionFactory { get;}

        /// <summary>
        /// You might want to use CommandExecutionFactory.Activate instead unless you have a specific combination in mind
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="databaseObject"></param>
        /// <returns></returns>
        T Activate<T, T2>(T2 databaseObject) where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity;
        
        bool DeleteWithConfirmation(object sender, IDeleteable deleteable,string overrideConfirmationText=null);
        bool DeleteControlFromDashboardWithConfirmation(object sender, DashboardControl controlToDelete);

        void ActivateViewLoadMetadataLog(object sender,LoadMetadata loadMetadata);

        void ActivateExtractionFilterParameterSet(object sender,ExtractionFilterParameterSet parameterSet);

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
        /// <param name="o"></param>
        void RequestItemEmphasis(object sender, EmphasiseRequest request);
        
        void ActivateLookupConfiguration(object sender, Catalogue catalogue,TableInfo optionalLookupTableInfo=null);
        void ActivateJoinInfoConfiguration(object sender, TableInfo tableInfo);

        void ActivateReOrderCatalogueItems(Catalogue catalogue);

        void ActivateConfigureValidation(object sender, Catalogue catalogue);
        void ViewFilterGraph(object sender,FilterGraphObjectCollection collection);

        void ActivateViewCohortIdentificationConfigurationSql(object sender, CohortIdentificationConfiguration cic);
        void ActivateViewLog(ExternalDatabaseServer loggingServer, int dataLoadRunID);

        void ActivateLoadProgress(object sender, LoadProgress loadProgress);
        IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata);
        void ActivateExternalDatabaseServer(object sender, ExternalDatabaseServer externalDatabaseServer);
        void ActivateTableInfo(object sender, TableInfo tableInfo);
        void ActivatePreLoadDiscardedColumn(object sender, PreLoadDiscardedColumn preLoadDiscardedColumn);
        void ActivatePermissionWindow(object sender, PermissionWindow permissionWindow);
    }
}
