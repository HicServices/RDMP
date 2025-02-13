// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;

namespace Rdmp.UI.Tests;

public class TestActivateItems : BasicActivateItems, IActivateItems, ITheme
{
    private readonly UITests _uiTests;
    private static CommentStore _commentStore;
    private List<IProblemProvider> _problemProviders;

    public ITheme Theme => this;
    public RefreshBus RefreshBus { get; private set; }
    public IArrangeWindows WindowArranger { get; private set; }

    public Func<bool> ShouldReloadFreshCopyDelegate;

    /// <summary>
    /// All the activities that you might want to know happened during tests.  (not a member of <see cref="IActivateItems"/>)
    /// </summary>
    public TestActivateItemsResults Results { get; private set; }

    public TestActivateItems(UITests uiTests, MemoryDataExportRepository repo) : base(new RepositoryProvider(repo),
        new ToMemoryCheckNotifier())
    {
        _uiTests = uiTests;
        Results = new TestActivateItemsResults();
        RefreshBus = new RefreshBus();

        //don't load the comment store for every single test
        if (_commentStore == null)
        {
            _commentStore = new CommentStore();
            _commentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
        }

        CommentStore = _commentStore;

        HistoryProvider = new HistoryProvider(RepositoryLocator);

        _problemProviders = new List<IProblemProvider>(new IProblemProvider[]
        {
            new CatalogueProblemProvider(),
            new DataExportProblemProvider()
        });
    }

    public Form ShowWindow(Control singleControlForm, bool asDocument = false)
    {
        _uiTests.AndLaunch(singleControlForm);
        return singleControlForm.FindForm();
    }

    public override void Publish(IMapsDirectlyToDatabaseTable o)
    {
        base.Publish(o);

        if (o is DatabaseEntity e)
            RefreshBus.Publish(this, new RefreshObjectEventArgs(e));
    }

    public override void Show(string title, string message)
    {
        Assert.Fail($"Did not expect a MessageBox to be shown but it was '{message}'");
    }

    public ICombineableFactory CommandFactory { get; private set; }
    public ICommandExecutionFactory CommandExecutionFactory { get; set; }
    public new CommentStore CommentStore { get; private set; } // Hide normal CommentStore so we can overwrite it later
    public HistoryProvider HistoryProvider { get; }

    public T Activate<T, T2>(T2 databaseObject)
        where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity =>
        _uiTests.AndLaunch<T>(databaseObject);

    public T Activate<T>(IPersistableObjectCollection collection) where T : Control, IObjectCollectionControl, new()
    {
        var t = new T();
        _uiTests.AndLaunch(t);
        t.SetCollection(this, collection);
        return t;
    }

    public override bool DeleteWithConfirmation(IDeleteable deletable)
    {
        if (deletable is DatabaseEntity d && !d.Exists())
            throw new Exception("Attempt made to delete an object which didn't exist");

        base.DeleteWithConfirmation(deletable);

        RefreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity)deletable));
        return true;
    }

    public override bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen) =>
        throw new NotImplementedException();

    public override bool SelectType(DialogArgs args, Type[] available, out Type chosen) =>
        throw new NotImplementedException();

    public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject) =>
        throw new NotImplementedException();

    public bool HasProblem(object model)
    {
        return _problemProviders.Any(p => p.HasProblem(model));
    }

    public string DescribeProblemIfAny(object model)
    {
        return _problemProviders.Select(p => p.DescribeProblem(model)).SingleOrDefault(prob => prob != null);
    }

    public string GetDocumentation(Type type) =>
        RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(type);

    public string CurrentDirectory => TestContext.CurrentContext.TestDirectory;

    public DialogResult ShowDialog(Form form)
    {
        Results.WindowsShown.Add(form);
        return DialogResult.OK;
    }


    public void KillForm(Form f, Exception reason)
    {
        Results.KilledForms.Add(f, reason);
    }

    public void KillForm(Form f, string reason)
    {
        Results.KilledForms.Add(f, new Exception(reason));
    }

    public void OnRuleRegistered(IBinderRule rule)
    {
        Results.RegisteredRules.Add(rule);
    }

    public bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity)
    {
        if (ShouldReloadFreshCopyDelegate == null)
        {
            Assert.Fail(
                $"Object {databaseEntity} was out of date with the database, normally user would be asked to load a new copy but since this is a test the test will be failed.  Solve this either by calling SaveToDatabase before launching your UI or by setting the ShouldReloadFreshCopyDelegate delegate (if the MessageBox showing is how the live system should respond)");
            return false;
        }

        return ShouldReloadFreshCopyDelegate();
    }

    public void ApplyTo(ToolStrip item)
    {
    }

    public bool ApplyThemeToMenus { get; set; }


    /// <summary>
    /// The answer to give when asked <see cref="YesNo"/>
    /// </summary>
    public bool? YesNoResponse { get; set; }

    public override bool YesNo(DialogArgs args, out bool chosen)
    {
        if (YesNoResponse.HasValue)
        {
            chosen = YesNoResponse.Value;

            //'user' consciously chose a value
            return true;
        }


        throw new Exception($"Did not expect to be asked a question but we were asked :{args}");
    }


    /// <summary>
    /// The answer to give when asked to <see cref="TypeText(string, string, int, string, out string, bool)"/>
    /// </summary>
    public string TypeTextResponse { get; set; }

    public override bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText)
    {
        text = TypeTextResponse;
        return !string.IsNullOrWhiteSpace(TypeTextResponse);
    }

    public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription) =>
        throw new NotImplementedException();

    public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription) =>
        throw new NotImplementedException();

    public override void ShowException(string errorText, Exception exception)
    {
        throw exception ?? new Exception(errorText);
    }

    public override void Wait(string title, Task task, CancellationTokenSource cts)
    {
        task.Wait(cts.Token);
    }

    public override List<CommandInvokerDelegate> GetDelegates()
    {
        return new List<CommandInvokerDelegate>
        {
            new(typeof(IActivateItems), true, p => this)
        };
    }

    public override IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects) =>
        throw new NotImplementedException();

    public override IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects) => throw new NotImplementedException();

    public override DirectoryInfo SelectDirectory(string prompt) => throw new NotImplementedException();

    public override FileInfo SelectFile(string prompt) => SelectFile(prompt, null, null);

    public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern) =>
        throw new NotImplementedException();

    public override FileInfo SelectFile(string prompt, string patternDescription, string pattern) =>
        throw new NotImplementedException();

    protected override bool
        SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen) =>
        throw new NotImplementedException();

    public void StartSession(string sessionName, IEnumerable<IMapsDirectlyToDatabaseTable> initialSelectionIfAny,
        string initialSearch)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<SessionCollectionUI> GetSessions() => throw new NotImplementedException();

    public override void ShowData(IViewSQLAndResultsCollection collection)
    {
        throw new NotImplementedException();
    }

    public override void ShowLogs(ILoggedActivityRootObject rootObject)
    {
        throw new NotImplementedException();
    }

    public override void ShowLogs(ExternalDatabaseServer loggingServer, LogViewerFilter filter)
    {
        throw new NotImplementedException();
    }

    public override void ShowGraph(AggregateConfiguration aggregate)
    {
        throw new NotImplementedException();
    }

    public override bool SelectObject<T>(DialogArgs args, T[] available, out T selected) =>
        throw new NotImplementedException();

    public override bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected) =>
        throw new NotImplementedException();

    public override void LaunchSubprocess(ProcessStartInfo startInfo)
    {
        throw new NotSupportedException();
    }

    public override void ShowData(System.Data.DataTable collection)
    {
        throw new NotImplementedException();
    }

    public override void ShowWarning(string message)
    {
        Show("Message", message);
    }
}

public class TestActivateItemsResults : ICheckNotifier
{
    public List<Control> WindowsShown = new();
    public Dictionary<Form, Exception> KilledForms = new();
    public List<IBinderRule> RegisteredRules = new();
    public List<CheckEventArgs> FatalCalls = new();

    public void Clear()
    {
        WindowsShown = new List<Control>();
        KilledForms = new Dictionary<Form, Exception>();
        RegisteredRules = new List<IBinderRule>();
        FatalCalls = new List<CheckEventArgs>();
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        if (args.Result >= CheckResult.Fail)
            FatalCalls.Add(args);

        return false;
    }
}