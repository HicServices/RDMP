// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
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
using CatalogueManager.CommandExecution;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Arranging;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.Rules;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Providers;
using DataExportLibrary.Repositories;
using DataExportManager.Icons.IconProvision;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.Dependencies.Models;
using ReusableUIComponents.Theme;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class TestActivateItems:IActivateItems, ITheme
    {
        private readonly UITests _uiTests;
        private static CommentStore _commentStore;
        private List<IProblemProvider> _problemProviders;

        public ITheme Theme { get {return this;}}
        public IServerDefaults ServerDefaults { get; private set; }
        public RefreshBus RefreshBus { get; private set; }
        public FavouritesProvider FavouritesProvider { get; private set; }
        public ICoreChildProvider CoreChildProvider { get; private set; }
        public List<IPluginUserInterface> PluginUserInterfaces { get; private set; }
        public IArrangeWindows WindowArranger { get; private set; }

        /// <summary>
        /// All the activities that you might want to know happened during tests.  (not a member of <see cref="IActivateItems"/>)
        /// </summary>
        public TestActivateItemsResults Results { get; private set; }

        public TestActivateItems(UITests uiTests,MemoryDataExportRepository repo)
        {
            _uiTests = uiTests;
            Results = new TestActivateItemsResults();

            RepositoryLocator = new RepositoryProvider(repo);
            RefreshBus = new RefreshBus();

            //don't load the comment store for every single test
            if (_commentStore == null)
            {
                _commentStore = new CommentStore();
                _commentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
            }

            CommentStore = _commentStore;

            CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,new ThrowImmediatelyCheckNotifier());
            CoreIconProvider = new DataExportIconProvider(null);
            FavouritesProvider = new FavouritesProvider(this,repo.CatalogueRepository);

            _problemProviders = new List<IProblemProvider>(new IProblemProvider[]
            {
                new CatalogueProblemProvider(),
                new DataExportProblemProvider()
            });

            CommandExecutionFactory = new RDMPCommandExecutionFactory(this);
            PluginUserInterfaces = new List<IPluginUserInterface>();
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
            return _uiTests.AndLaunch<T>(databaseObject);
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
            return _problemProviders.Any(p=>p.HasProblem(model));
        }

        public string DescribeProblemIfAny(object model)
        {
            return _problemProviders.Select(p => p.DescribeProblem(model)).SingleOrDefault(prob=>prob != null);
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
        public DialogResult ShowDialog(Form form)
        {
            Results.WindowsShown.Add(form);
            return DialogResult.OK;
        }

        
        public void KillForm(Form f, Exception reason)
        {
            Results.KilledForms.Add(f,reason);
        }

        public void OnRuleRegistered(IBinderRule rule)
        {
            Results.RegisteredRules.Add(rule);
        }

        public void ApplyTo(ToolStrip item)
        {
            
        }

        public bool ApplyThemeToMenus { get; set; }

        
    }

    public class TestActivateItemsResults
    {
        public List<Control> WindowsShown = new List<Control>();
        public Dictionary<Form, Exception> KilledForms = new Dictionary<Form, Exception>();
        public List<IBinderRule> RegisteredRules = new List<IBinderRule>();
        public List<CheckEventArgs> FatalCalls = new List<CheckEventArgs>();

        public void Clear()
        {
            WindowsShown = new List<Control>();
            KilledForms = new Dictionary<Form, Exception>();
            RegisteredRules = new List<IBinderRule>();
            FatalCalls = new List<CheckEventArgs>();
        }
    }
}
