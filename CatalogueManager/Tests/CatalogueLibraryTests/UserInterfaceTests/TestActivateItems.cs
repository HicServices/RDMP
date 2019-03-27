using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Providers;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.Dependencies.Models;
using ReusableUIComponents.Theme;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class TestActivateItems:IActivateItems, ITheme
    {
        public ITheme Theme { get {return this;}}
        public IServerDefaults ServerDefaults { get; private set; }
        public RefreshBus RefreshBus { get; private set; }
        public FavouritesProvider FavouritesProvider { get; private set; }
        public ICoreChildProvider CoreChildProvider { get; private set; }
        public List<IPluginUserInterface> PluginUserInterfaces { get; private set; }
        public IArrangeWindows WindowArranger { get; private set; }

        public TestActivateItems(MemoryDataExportRepository repo)
        {
            RepositoryLocator = new RepositoryProvider(repo);
            RefreshBus = new RefreshBus();
            CommentStore = new CommentStore();
            CommentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
            CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier());
        }

        public Form ShowWindow(Control singleControlForm, bool asDocument = false)
        {
            throw new NotImplementedException();
        }

        public Form ShowRDMPSingleDatabaseObjectControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity objectOfTypeT)
        {
            throw new NotImplementedException();
        }

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        public ICoreIconProvider CoreIconProvider { get; private set; }
        public ICheckNotifier GlobalErrorCheckNotifier { get; private set; }
        public ICommandFactory CommandFactory { get; private set; }
        public ICommandExecutionFactory CommandExecutionFactory { get; private set; }
        public CommentStore CommentStore { get; private set; }
        public Lazy<IObjectVisualisation> GetLazyCatalogueObjectVisualisation()
        {
            throw new NotImplementedException();
        }

        public T Activate<T, T2>(T2 databaseObject) where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity
        {
            throw new NotImplementedException();
        }

        public T Activate<T>(IPersistableObjectCollection collection) where T : Control, IObjectCollectionControl, new()
        {
            throw new NotImplementedException();
        }

        public bool DeleteWithConfirmation(object sender, IDeleteable deleteable)
        {
            throw new NotImplementedException();
        }

        public void ViewDataSample(IViewSQLAndResultsCollection collection)
        {
            throw new NotImplementedException();
        }

        public event EmphasiseItemHandler Emphasise;
        public void RequestItemEmphasis(object sender, EmphasiseRequest request)
        {
            throw new NotImplementedException();
        }

        public void ActivateLookupConfiguration(object sender, Catalogue catalogue, TableInfo optionalLookupTableInfo = null)
        {
            throw new NotImplementedException();
        }

        public void ViewFilterGraph(object sender, FilterGraphObjectCollection collection)
        {
            throw new NotImplementedException();
        }

        public void ActivateViewCohortIdentificationConfigurationSql(object sender, CohortIdentificationConfiguration cic)
        {
            throw new NotImplementedException();
        }

        public void ActivateViewLog(LoadMetadata loadMetadata)
        {
            throw new NotImplementedException();
        }

        public IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata)
        {
            throw new NotImplementedException();
        }

        public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject)
        {
            throw new NotImplementedException();
        }

        public bool HasProblem(object model)
        {
            throw new NotImplementedException();
        }

        public string DescribeProblemIfAny(object model)
        {
            throw new NotImplementedException();
        }

        public object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise)
        {
            throw new NotImplementedException();
        }

        public string GetDocumentation(Type type)
        {
            throw new NotImplementedException();
        }

        public string CurrentDirectory { get { return TestContext.CurrentContext.TestDirectory; }}

        public void ApplyTo(ToolStrip item)
        {
            
        }

        public bool ApplyThemeToMenus { get; set; }
    }
}
