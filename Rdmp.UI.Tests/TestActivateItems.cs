// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.ItemActivation.Emphasis;
using Rdmp.UI.PluginChildProvision;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace Rdmp.UI.Tests
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

        public Func<bool> ShouldReloadFreshCopyDelegate;

        /// <summary>
        /// All the activities that you might want to know happened during tests.  (not a member of <see cref="IActivateItems"/>)
        /// </summary>
        public TestActivateItemsResults Results { get; private set; }

        public TestActivateItems(UITests uiTests,MemoryDataExportRepository repo)
        {
            _uiTests = uiTests;
            Results = new TestActivateItemsResults();
            GlobalErrorCheckNotifier = new ToMemoryCheckNotifier();

            RepositoryLocator = new RepositoryProvider(repo);
            RefreshBus = new RefreshBus();

            //don't load the comment store for every single test
            if (_commentStore == null)
            {
                _commentStore = new CommentStore();
                _commentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
            }

            CommentStore = _commentStore;

            CoreChildProvider = new DataExportChildProvider(RepositoryLocator,null,Results);
            CoreIconProvider = new DataExportIconProvider(RepositoryLocator,null);
            FavouritesProvider = new FavouritesProvider(this,repo.CatalogueRepository);

            _problemProviders = new List<IProblemProvider>(new IProblemProvider[]
            {
                new CatalogueProblemProvider(),
                new DataExportProblemProvider()
            });

            PluginUserInterfaces = new List<IPluginUserInterface>();
        }

        public Form ShowWindow(Control singleControlForm, bool asDocument = false)
        {
            _uiTests.AndLaunch(singleControlForm);
            return singleControlForm.FindForm();
        }


        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        public ICoreIconProvider CoreIconProvider { get; private set; }
        public ICheckNotifier GlobalErrorCheckNotifier { get; private set; }
        public void Publish(DatabaseEntity databaseEntity)
        {
            RefreshBus.Publish(this,new RefreshObjectEventArgs(databaseEntity));
        }

        public void Show(string message)
        {
            Assert.Fail("Did not expect a MessageBox to be shown");
        }

        public ICommandFactory CommandFactory { get; private set; }
        public ICommandExecutionFactory CommandExecutionFactory { get; set; }
        public CommentStore CommentStore { get; private set; }

        public T Activate<T, T2>(T2 databaseObject) where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity
        {
            return _uiTests.AndLaunch<T>(databaseObject);
        }

        public T Activate<T>(IPersistableObjectCollection collection) where T : Control, IObjectCollectionControl, new()
        {
            T t = new T();
            _uiTests.AndLaunch(t);
            t.SetCollection(this, collection);
            return t;
        }

        public bool DeleteWithConfirmation(IDeleteable deleteable)
        {
            if(deleteable is DatabaseEntity d && !d.Exists())
                throw new Exception("Attempt made to delete an object which didn't exist");

            deleteable.DeleteInDatabase();
            RefreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity)deleteable));
            return true;
        }

        public event EmphasiseItemHandler Emphasise;
        public void RequestItemEmphasis(object sender, EmphasiseRequest request)
        {
            Emphasise?.Invoke(sender, new EmphasiseEventArgs(request));
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
            return CoreChildProvider.GetRootObjectOrSelf(objectToEmphasise);
        }

        public string GetDocumentation(Type type)
        {
            return RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(type);
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
        public void KillForm(Form f, string reason)
        {
            Results.KilledForms.Add(f,new Exception(reason));
        }

        public void OnRuleRegistered(IBinderRule rule)
        {
            Results.RegisteredRules.Add(rule);
        }

        public bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity)
        {
            if(ShouldReloadFreshCopyDelegate == null)
            {
                Assert.Fail("Object " + databaseEntity + " was out of date with the database, normally user would be asked to load a new copy but since this is a test the test will be failed.  Solve this either by calling SaveToDatabase before launching your UI or by setting the ShouldReloadFreshCopyDelegate delegate (if the MessageBox showing is how the live system should respond)");
                return false;
            }

            return ShouldReloadFreshCopyDelegate();
        }

        public void ApplyTo(ToolStrip item)
        {
            
        }

        public bool ApplyThemeToMenus { get; set; }

        
        /// <summary>
        /// The answer to give when asked <see cref="YesNo(string, string)"/>
        /// </summary>
        public bool? YesNoResponse { get;set;}

        public bool YesNo(string text, string caption)
        {
            if(YesNoResponse.HasValue)
                return YesNoResponse.Value;

            throw new Exception("Did not expect to be asked a question but we were asked :" + text);
        }


        /// <summary>
        /// The answer to give when asked to <see cref="TypeText(string, string, int, string, out string, bool)"/>
        /// </summary>
        public string TypeTextResponse { get; set; }

        public bool TypeText(string header, string prompt, int maxLength, string initialText, out string text, bool requireSaneHeaderText)
        {
            text = TypeTextResponse;
            return !string.IsNullOrWhiteSpace(TypeTextResponse);
        }

        public DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            throw new NotImplementedException();
        }

        public DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            throw new NotImplementedException();
        }

        public void Wait(string title, Task task, CancellationTokenSource cts)
        {
            task.Wait(cts.Token);
        }

        public List<KeyValuePair<Type, Func<ParameterInfo, object>>> GetDelegates()
        {
            return new List<KeyValuePair<Type, Func<ParameterInfo, object>>>
            {
                new KeyValuePair<Type, Func<ParameterInfo, object>>(typeof(IActivateItems),(p)=>this)
            };
        }

        public IEnumerable<Type> GetIgnoredCommands()
        {
            return new List<Type>();
        }

        public object PickMany(ParameterInfo parameterInfo, Type arrayElementType, IMapsDirectlyToDatabaseTable[] availableObjects)
        {
            throw new NotImplementedException();
        }

        public object SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null, bool allowAutoSelect = false)
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo PickDirectory(ParameterInfo parameterInfo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAll<T>()
        {
            throw new NotImplementedException();
        }

        public object PickValueType(ParameterInfo parameterInfo, Type paramType)
        {
            throw new NotImplementedException();
        }
    }

    public class TestActivateItemsResults:ICheckNotifier
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

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if(args.Result >= CheckResult.Fail)
                FatalCalls.Add(args);

            return false;
        }
    }
}
