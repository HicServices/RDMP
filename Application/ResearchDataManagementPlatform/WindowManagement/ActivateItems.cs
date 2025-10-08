// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.AggregationUIs;
using Rdmp.UI.CatalogueSummary.LoadEvents;
using Rdmp.UI.CohortUI.CreateHoldoutLookup;
using Rdmp.UI.CohortUI.ImportCustomData;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying;
using Rdmp.UI.DataViewing;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.Logging;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SimpleDialogs.ForwardEngineering;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.SubComponents;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using Rdmp.UI.Versioning;
using Rdmp.UI.Wizard;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.WindowArranging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WeifenLuo.WinFormsUI.Docking;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = System.Drawing.Rectangle;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
///     Central class for RDMP main application, this class provides access to all the main systems in RDMP user interface
///     such as Emphasis, the RefreshBus, Child
///     provision etc.  See IActivateItems for full details
/// </summary>
public class ActivateItems : BasicActivateItems, IActivateItems, IRefreshBusSubscriber
{
    private readonly DockPanel _mainDockPanel;
    private readonly WindowManager _windowManager;

    private WindowFactory WindowFactory { get; }


    public ITheme Theme { get; }

    public RefreshBus RefreshBus { get; }

    public IArrangeWindows WindowArranger { get; }

    public override void Publish(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        if (databaseEntity is DatabaseEntity de)
            RefreshBus.Publish(this, new RefreshObjectEventArgs(de));
    }

    public override void ShowWarning(string message)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowWarning(message));
            return;
        }

        WideMessageBox.Show("Message", message, Environment.StackTrace, true, null, WideMessageBoxTheme.Warning);
    }

    public override void Show(string title, string message)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => Show(title, message));
            return;
        }

        WideMessageBox.Show(title, message, Environment.StackTrace, true, null, WideMessageBoxTheme.Help);
    }

    public ICombineableFactory CommandFactory { get; }
    public ICommandExecutionFactory CommandExecutionFactory { get; }
    public HistoryProvider HistoryProvider { get; }

    private List<IProblemProvider> ProblemProviders { get; }

    public ActivateItems(ITheme theme, RefreshBus refreshBus, DockPanel mainDockPanel,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowFactory windowFactory,
        WindowManager windowManager, ICheckNotifier globalErrorCheckNotifier) : base(repositoryLocator,
        globalErrorCheckNotifier)
    {
        Theme = theme;
        IsWinForms = true;
        InteractiveDeletes = true;
        WindowFactory = windowFactory;
        _mainDockPanel = mainDockPanel;
        _windowManager = windowManager;
        RefreshBus = refreshBus;

        RefreshBus.ChildProvider = CoreChildProvider;

        HistoryProvider = new HistoryProvider(repositoryLocator);

        WindowArranger = new WindowArranger(this, _windowManager, _mainDockPanel);

        CommandFactory = new RDMPCombineableFactory();
        CommandExecutionFactory = new RDMPCommandExecutionFactory(this);

        ProblemProviders = new List<IProblemProvider>
        {
            new DataExportProblemProvider(),
            new CatalogueProblemProvider()
        };
        RefreshProblemProviders();

        RefreshBus.Subscribe(this);

        // We can run subprocesses
        IsAbleToLaunchSubprocesses = true;
    }

    protected override ICoreChildProvider GetChildProvider()
    {
        var provider = base.GetChildProvider();

        if (RefreshBus != null) RefreshBus.ChildProvider = provider;

        return provider;
    }


    public Form ShowWindow(Control singleControlForm, bool asDocument = false)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => ShowWindow(singleControlForm, asDocument));

        var width = singleControlForm.Size.Width + SystemInformation.BorderSize.Width;
        var height = singleControlForm.Size.Height + SystemInformation.BorderSize.Height;

        //use the .Text or fallback on .Name
        var name = string.IsNullOrWhiteSpace(singleControlForm.Text)
            ? singleControlForm.Name ?? singleControlForm.GetType().Name //or worst case scenario use the type name!
            : singleControlForm.Text;

        switch (singleControlForm)
        {
            case Form when asDocument:
                throw new Exception(
                    $"Control '{singleControlForm}' is a Form and asDocument was passed as true.  When asDocument is true you must be a Control not a Form e.g. inherit from RDMPUserControl instead of RDMPForm");
            case RDMPUserControl c:
                c.SetItemActivator(this);
                break;
        }

        var content = WindowFactory.Create(this, singleControlForm, name, null);

        if (asDocument)
            content.Show(_mainDockPanel, DockState.Document);
        else
            content.Show(_mainDockPanel, new Rectangle(0, 0, width, height));

        return content;
    }

    public override void RequestItemEmphasis(object sender, EmphasiseRequest request)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => RequestItemEmphasis(sender, request));
            return;
        }

        AdjustEmphasiseRequest(request);

        //ensure a relevant Toolbox is available
        var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(request.ObjectToEmphasise);

        var root = descendancy != null
            ? descendancy.Parents.FirstOrDefault()
            : request.ObjectToEmphasise; //assume maybe o is a root object itself?

        if (root is CohortIdentificationConfiguration cic)
            Activate<CohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic);
        else if (root != null)
            _windowManager.ShowCollectionWhichSupportsRootObjectType(root);

        //really should be a listener now btw since we just launched the relevant Toolbox if it wasn't there before
        //Look at assignments to Sender, the invocation list can change the Sender!
        var args = new EmphasiseEventArgs(request);
        OnEmphasise(this, args);

        //might be different than sender that was passed in
        if (args.Sender is DockContent content)
            content.Activate();

        //user is being shown the given object so track it as a recent (e.g. GoTo etc)
        if (args.Request.ObjectToEmphasise is IMapsDirectlyToDatabaseTable m)
            HistoryProvider.Add(m);
    }

    public override bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            Enum result = default;
            var rtn = _mainDockPanel.Invoke(() => SelectEnum(args, enumType, out result));
            chosen = result;
            return rtn;
        }

        return SelectObject(args, Enum.GetValues(enumType).Cast<Enum>().ToArray(), out chosen);
    }

    public override bool SelectType(DialogArgs args, Type[] available, out Type chosen)
    {
        return SelectObject(args, available, out chosen);
    }

    public override bool CanActivate(object target)
    {
        return CommandExecutionFactory.CanActivate(target);
    }

    protected override void ActivateImpl(object o)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ActivateImpl(o));
            return;
        }

        if (CommandExecutionFactory.CanActivate(o))
            CommandExecutionFactory.Activate(o);
    }

    public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject)
    {
        //if the collection an arbitrary one then it is definitely not the root collection for anyone
        return collection != RDMPCollection.None && _windowManager.GetCollectionForRootObject(rootObject) == collection;
    }

    /// <summary>
    ///     Consults all currently configured IProblemProviders and returns true if any report a problem with the object
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool HasProblem(object model)
    {
        return ProblemProviders.Any(p => p.HasProblem(model));
    }

    /// <summary>
    ///     Consults all currently configured IProblemProviders and returns the first Problem reported by any about the object
    ///     or null
    ///     if there are no problems reported.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public string DescribeProblemIfAny(object model)
    {
        return ProblemProviders.Select(p => p.DescribeProblem(model)).FirstOrDefault(desc => desc != null);
    }

    public string GetDocumentation(Type type)
    {
        return RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(type);
    }

    public string CurrentDirectory => Environment.CurrentDirectory;

    public DialogResult ShowDialog(Form form)
    {
        // if on wrong Thread
        return _mainDockPanel?.InvokeRequired ?? false
            ? _mainDockPanel.Invoke(() => ShowDialog(form))
            : form.ShowDialog();
    }

    public void KillForm(Form f, Exception reason)
    {
        // if on wrong Thread
        if (f.InvokeRequired)
        {
            f.Invoke(() => KillForm(f, reason));
            return;
        }

        f.Close();
        ExceptionViewer.Show("Window Closed", reason);
    }

    public void KillForm(Form f, string reason)
    {
        // if on wrong Thread
        if (f.InvokeRequired)
        {
            f.Invoke(() => KillForm(f, reason));
            return;
        }

        f.Close();
        WideMessageBox.Show("Window Closed", reason);
    }

    public void OnRuleRegistered(IBinderRule rule)
    {
        //no special action required
    }

    /// <summary>
    ///     Asks the user if they want to reload a fresh copy with a Yes/No message box.
    /// </summary>
    /// <param name="databaseEntity"></param>
    /// <returns></returns>
    public bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity)
    {
        return YesNo($"{databaseEntity} is out of date with database, would you like to reload a fresh copy?",
            "Object Changed");
    }

    public T Activate<T, T2>(T2 databaseObject)
        where T : RDMPSingleDatabaseObjectControl<T2>, new()
        where T2 : DatabaseEntity
    {
        return Activate<T, T2>(databaseObject, CoreIconProvider.GetImage(databaseObject));
    }

    public T Activate<T>(IPersistableObjectCollection collection)
        where T : Control, IObjectCollectionControl, new()

    {
        //if the window is already open
        if (PopExisting(typeof(T), collection, out var existingHostedControlInstance))
        {
            //just update its state
            var existing = (T)existingHostedControlInstance;
            existing.SetCollection(this, collection);

            return existing;
        }


        var uiInstance = new T();
        Activate(uiInstance, collection);
        return uiInstance;
    }


    private T Activate<T, T2>(T2 databaseObject, Image<Rgba32> tabImage)
        where T : RDMPSingleDatabaseObjectControl<T2>, new()
        where T2 : DatabaseEntity
    {
        if (PopExisting(typeof(T), databaseObject, out var existingHostedControlInstance))
            return (T)existingHostedControlInstance;

        var uiInstance = new T();
        var floatable = WindowFactory.Create(this, RefreshBus, uiInstance, tabImage, databaseObject);

        int? insertIndex = null;
        var panel = _mainDockPanel.Panes.Where(p => p.IsActiveDocumentPane).SingleOrDefault();
        if (panel is not null)
        {
            var contents = panel.Contents;
            insertIndex = contents.IndexOf(panel.ActiveContent) + 1;
        }

        floatable.Show(_mainDockPanel, DockState.Document);

        uiInstance.SetDatabaseObject(this, databaseObject);

        if (insertIndex is not null && panel is not null)
        {
            panel.SetContentIndex(floatable, (int)insertIndex);
        }

        SetTabText(floatable, uiInstance);

        return uiInstance;
    }

    private bool PopExisting(Type windowType, IMapsDirectlyToDatabaseTable databaseObject,
        out Control existingHostedControlInstance)
    {
        var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, databaseObject);
        existingHostedControlInstance = null;

        if (existing != null)
        {
            existingHostedControlInstance = existing.Control;
            existing.Activate();

            // only refresh if there are changes to the underlying object
            if (databaseObject is IRevertable r &&
                r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                existing.HandleUserRequestingTabRefresh(this);
        }

        return existing != null;
    }

    private bool PopExisting(Type windowType, IPersistableObjectCollection collection,
        out Control existingHostedControlInstance)
    {
        var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, collection);
        existingHostedControlInstance = null;

        if (existing != null)
        {
            existingHostedControlInstance = existing.Control;
            existing.Activate();

            // only refresh if there are changes to some of the underlying objects
            if (collection.DatabaseObjects.OfType<IRevertable>().Any(r =>
                    r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent))
                existing.HandleUserRequestingTabRefresh(this);
        }

        return existing != null;
    }

    public DockContent Activate(DeserializeInstruction instruction, IActivateItems activator)
    {
        if (instruction.DatabaseObject != null && instruction.ObjectCollection != null)
            throw new ArgumentException(
                "DeserializeInstruction cannot have both a DatabaseObject and an ObjectCollection");

        var c = (Control)UIObjectConstructor.Construct(instruction.UIControlType, activator);

        switch (c)
        {
            //it has a database object so call SetDatabaseObject
            //if we get here then Instruction wasn't for a
            case IObjectCollectionControl uiCollection:
                return Activate(uiCollection, instruction.ObjectCollection);
            case IRDMPSingleDatabaseObjectControl uiInstance:
            {
                var databaseObject = instruction.DatabaseObject;

                //the database object is gone? deleted maybe
                if (databaseObject == null)
                    return null;

                DockContent floatable = WindowFactory.Create(this, RefreshBus, uiInstance,
                    CoreIconProvider.GetImage(databaseObject), databaseObject);

                floatable.Show(_mainDockPanel, DockState.Document);
                try
                {
                    uiInstance.SetDatabaseObject(this, (DatabaseEntity)databaseObject);
                    SetTabText(floatable, uiInstance);
                }
                catch (Exception e)
                {
                    floatable.Close();
                    throw new Exception(
                        $"SetDatabaseObject failed on Control of Type '{instruction.UIControlType.Name}', control closed, see inner Exception for details",
                        e);
                }

                return floatable;
            }
            default:
                return (DockContent)activator.ShowWindow(c, true);
        }
    }

    private static void SetTabText(DockContent floatable, INamedTab tab)
    {
        var tabText = tab.GetTabName();
        var tabToolTipText = tab.GetTabToolTip();

        floatable.TabText = tabText;

        // set tool tip to the full tab name or custom representation
        floatable.ToolTipText = string.IsNullOrEmpty(tabToolTipText) ? tabText : tabToolTipText;

        if (floatable.ParentForm != null)
            floatable.ParentForm.Text = $"{tabText} - RDMP";
    }

    public PersistableObjectCollectionDockContent Activate(IObjectCollectionControl collectionControl,
        IPersistableObjectCollection objectCollection)
    {
        var floatable = WindowFactory.Create(this, collectionControl, objectCollection, null);
        floatable.Show(_mainDockPanel, DockState.Document);
        return floatable;
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        // if we don't want to do selective refresh or can't (because partial refreshes are not supported on the type)
        if (HardRefresh || !UserSettings.SelectiveRefresh || !CoreChildProvider.SelectiveRefresh(e.Object))
        {
            //update the child provider with a full refresh
            GetChildProvider();
            HardRefresh = false;
        }

        RefreshProblemProviders();
    }

    private void RefreshProblemProviders()
    {
        foreach (var p in ProblemProviders)
            p.RefreshProblems(CoreChildProvider);
    }

    /// <inheritdoc />
    public override bool YesNo(DialogArgs args, out bool chosen)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            bool result = default;
            var rtn = _mainDockPanel.Invoke(() => YesNo(args, out result));
            chosen = result;
            return rtn;
        }

        var dr = MessageBox.Show(args.TaskDescription ?? args.EntryLabel, args.WindowTitle, MessageBoxButtons.YesNo);
        chosen = dr == DialogResult.Yes;
        return dr switch
        {
            DialogResult.Yes => true,
            DialogResult.No => true,
            _ => false
        };
    }

    public override bool TypeText(DialogArgs args, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            string result = default;
            var rtn = _mainDockPanel.Invoke(() =>
                TypeText(args, maxLength, initialText, out result, requireSaneHeaderText));
            text = result;
            return rtn;
        }

        var textTyper =
            new TypeTextOrCancelDialog(args, maxLength, initialText, false, maxLength > MultiLineLengthThreshold)
            {
                RequireSaneHeaderText = requireSaneHeaderText
            };

        text = textTyper.ShowDialog() == DialogResult.OK ? textTyper.ResultText : null;
        return !string.IsNullOrWhiteSpace(text);
    }

    public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectDatabase(allowDatabaseCreation, taskDescription));

        using var dialog = new ServerDatabaseTableSelectorDialog(taskDescription, false, true, this);
        dialog.ShowDialog();
        return dialog.DialogResult == DialogResult.OK ? dialog.SelectedDatabase : null;
    }

    public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectTable(allowDatabaseCreation, taskDescription));

        using var dialog = new ServerDatabaseTableSelectorDialog(taskDescription, true, true, this)
        {
            AllowTableValuedFunctionSelection = true
        };

        dialog.ShowDialog();

        return dialog.DialogResult == DialogResult.OK ? dialog.SelectedTable : null;
    }

    public override void ShowException(string errorText, Exception exception)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowException(errorText, exception));
            return;
        }

        ExceptionViewer.Show(errorText, exception);
    }

    public override void Wait(string title, Task task, CancellationTokenSource cts)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => Wait(title, task, cts));
            return;
        }

        var ui = new WaitUI(title, task, cts);
        ui.ShowDialog();
    }


    public override IEnumerable<Type> GetIgnoredCommands()
    {
        yield return typeof(ExecuteCommandRefreshObject);
        yield return typeof(ExecuteCommandOpenInExplorer);
        yield return typeof(ExecuteCommandDeletePlugin);
        yield return typeof(ExecuteCommandCreateNewFileBasedProcessTask);
    }


    public override IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectOne(args, availableObjects));

        if (!availableObjects.Any())
        {
            MessageBox.Show($"There are no compatible objects in your RMDP for:{Environment.NewLine}{args}");
            return null;
        }

        //if there is only one object available to select
        if (availableObjects.Length == 1)
            if (args.AllowAutoSelect)
                return availableObjects[0];

        if (SelectObject(args, availableObjects, out var selected)) return selected;

        return null; //user didn't select one of the IMapsDirectlyToDatabaseTable objects shown in the dialog
    }

    public override bool SelectObject<T>(DialogArgs args, T[] available, out T selected)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            T result = default;
            var rtn = _mainDockPanel.Invoke(() => SelectObject(args, available, out result));
            selected = result;
            return rtn;
        }

        var pick = new SelectDialog<T>(args, this, available, false);

        if (pick.ShowDialog() == DialogResult.OK)
        {
            selected = pick.Selected;
            return true;
        }

        selected = default;
        return false;
    }

    public override bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            T[] result = default;
            var rtn = _mainDockPanel.Invoke(() => SelectObjects(args, available, out result));
            selected = result;
            return rtn;
        }

        var pick = new SelectDialog<T>(args, this, available, false)
        {
            AllowMultiSelect = true
        };

        if (pick.ShowDialog() == DialogResult.OK)
        {
            selected = pick.MultiSelected.ToArray();
            return true;
        }

        selected = default;
        return false;
    }

    public override DirectoryInfo SelectDirectory(string prompt)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false) return _mainDockPanel.Invoke(() => SelectDirectory(prompt));

        using var fb = new FolderBrowserDialog();
        return fb.ShowDialog() == DialogResult.OK ? new DirectoryInfo(fb.SelectedPath) : null;
    }

    public override FileInfo SelectFile(string prompt)
    {
        // if on wrong Thread
        return _mainDockPanel?.InvokeRequired ?? false
            ? _mainDockPanel.Invoke(() => SelectFile(prompt))
            : SelectFile(prompt, null, null);
    }

    public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectFile(prompt, patternDescription, pattern));

        using var fb = new OpenFileDialog { CheckFileExists = false, Multiselect = false };
        if (patternDescription != null && pattern != null)
            fb.Filter = $"{patternDescription}|{pattern}";

        if (fb.ShowDialog() == DialogResult.OK)
            // entering "null" in a winforms file dialog will return something like "D:\Blah\null"
            return string.Equals(Path.GetFileName(fb.FileName), "null", StringComparison.CurrentCultureIgnoreCase)
                ? null
                : new FileInfo(fb.FileName);

        return null;
    }

    public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectFiles(prompt, patternDescription, pattern));

        using var fb = new OpenFileDialog { CheckFileExists = false, Multiselect = true };
        if (patternDescription != null && pattern != null)
            fb.Filter = $"{patternDescription}|{pattern}";

        return fb.ShowDialog() == DialogResult.OK ? fb.FileNames.Select(f => new FileInfo(f)).ToArray() : null;
    }

    protected override bool SelectValueTypeImpl(DialogArgs args, Type paramType, object initialValue, out object chosen)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            object result = default;
            var rtn = _mainDockPanel.Invoke(() => SelectValueTypeImpl(args, paramType, initialValue, out result));
            chosen = result;
            return rtn;
        }

        //whatever else it is use string
        var typeTextDialog = new TypeTextOrCancelDialog(args, 1000, initialValue?.ToString());

        if (typeTextDialog.ShowDialog() == DialogResult.OK)
        {
            chosen = UsefulStuff.ChangeType(typeTextDialog.ResultText, paramType);
            return true;
        }

        chosen = null;
        return false;
    }

    public override IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => SelectMany(args, arrayElementType, availableObjects));

        if (!availableObjects.Any())
        {
            MessageBox.Show($"There are no '{arrayElementType.Name}' objects in your RMDP");
            return null;
        }

        using var selectDialog = new SelectDialog<IMapsDirectlyToDatabaseTable>(args, this, availableObjects, false)
        {
            AllowMultiSelect = true
        };

        if (!new List<DialogResult> { DialogResult.OK, DialogResult.Cancel }.Contains(selectDialog.ShowDialog()))
            return null;
        var ms = selectDialog.MultiSelected.ToList();
        var toReturn = Array.CreateInstance(arrayElementType, ms.Count);

        for (var i = 0; i < ms.Count; i++)
            toReturn.SetValue(ms[i], i);

        return toReturn.Cast<IMapsDirectlyToDatabaseTable>().ToArray();
    }

    public override List<CommandInvokerDelegate> GetDelegates()
    {
        return new List<CommandInvokerDelegate>
        {
            new(typeof(IActivateItems), true, p => this)
        };
    }

    public void StartSession(string sessionName, IEnumerable<IMapsDirectlyToDatabaseTable> initialObjects,
        string initialSearch)
    {
        if (initialObjects == null)
        {
            initialObjects = SelectMany(new DialogArgs
            {
                WindowTitle = sessionName.StartsWith(ExecuteCommandStartSession.FindResultsTitle)
                    ? "Find Multiple"
                    : "Session Objects",
                TaskDescription =
                    "Pick which objects you want added to the session window.  You can always add more later",
                InitialSearchText = initialSearch,

                IsFind = sessionName.StartsWith(ExecuteCommandStartSession.FindResultsTitle)
            }, typeof(IMapsDirectlyToDatabaseTable), CoreChildProvider.GetAllSearchables().Keys.ToArray())?.ToList();

            if (initialObjects?.Any() != true)
                // user cancelled picking objects
                return;
        }

        var panel = WindowFactory.Create(this, new SessionCollectionUI(), new SessionCollection(sessionName)
        {
            DatabaseObjects = initialObjects.ToList()
        }, Image.Load<Rgba32>(CatalogueIcons.WindowLayout));
        panel.Show(_mainDockPanel, DockState.DockLeft);
    }


    /// <inheritdoc />
    public IEnumerable<SessionCollectionUI> GetSessions()
    {
        return _windowManager.GetAllWindows<SessionCollectionUI>();
    }

    public override IPipelineRunner GetPipelineRunner(DialogArgs args, IPipelineUseCase useCase, IPipeline pipeline)
    {
        if (useCase is not null && pipeline is not null) return new PipelineRunner(useCase, pipeline);
        var configureAndExecuteDialog = new ConfigureAndExecutePipelineUI(args, useCase, this)
        {
            Dock = DockStyle.Fill
        };

        return configureAndExecuteDialog;
    }

    public override CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable,
        IProject project, string cohortInitialDescription)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() =>
                GetCohortCreationRequest(externalCohortTable, project, cohortInitialDescription));

        var ui = new CohortCreationRequestUI(this, externalCohortTable, project);

        if (!string.IsNullOrWhiteSpace(cohortInitialDescription))
            ui.CohortDescription = $"{cohortInitialDescription} ({Environment.UserName} - {DateTime.Now})";

        return ui.ShowDialog() == DialogResult.OK ? ui.Result : null;
    }

    public override CohortHoldoutLookupRequest GetCohortHoldoutLookupRequest(ExternalCohortTable externalCohortTable,
        IProject project, CohortIdentificationConfiguration cic)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() =>
                GetCohortHoldoutLookupRequest(externalCohortTable, project, cic));

        var ui = new CreateHoldoutLookupUI(this, externalCohortTable, cic);

        if (!string.IsNullOrWhiteSpace(cic.Description))
            ui.CohortDescription = $"{cic.Description} ({Environment.UserName} - {DateTime.Now})";
        return ui.ShowDialog() == DialogResult.OK ? ui.Result : null;
    }

    public override ICatalogue CreateAndConfigureCatalogue(ITableInfo tableInfo,
        ColumnInfo[] extractionIdentifierColumns, string initialDescription, IProject projectSpecific, string folder)
    {
        if (extractionIdentifierColumns is not null)
            return base.CreateAndConfigureCatalogue(tableInfo, extractionIdentifierColumns, initialDescription,
                projectSpecific, folder);
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(() => CreateAndConfigureCatalogue(tableInfo, extractionIdentifierColumns,
                initialDescription, projectSpecific, folder));

        var ui = new ConfigureCatalogueExtractabilityUI(this, tableInfo, initialDescription, projectSpecific)
        {
            TargetFolder = folder
        };
        ui.ShowDialog();

        return ui.CatalogueCreatedIfAny;
    }

    public override ExternalDatabaseServer CreateNewPlatformDatabase(ICatalogueRepository catalogueRepository,
        PermissableDefaults defaultToSet, IPatcher patcher, DiscoveredDatabase db)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
            return _mainDockPanel.Invoke(
                () => CreateNewPlatformDatabase(catalogueRepository, defaultToSet, patcher, db));

        //launch the winforms UI for creating a database
        return CreatePlatformDatabase.CreateNewExternalServer(catalogueRepository, defaultToSet, patcher);
    }

    public override bool ShowCohortWizard(out CohortIdentificationConfiguration cic)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            CohortIdentificationConfiguration result = default;
            var rtn = _mainDockPanel.Invoke(() => ShowCohortWizard(out result));
            cic = result;
            return rtn;
        }

        var wizard = new CreateNewCohortIdentificationConfigurationUI(this);

        cic = wizard.ShowDialog() == DialogResult.OK ? wizard.CohortIdentificationCriteriaCreatedIfAny : null;

        // Wizard was shown so that's a thing
        return true;
    }

    public override void SelectAnythingThen(DialogArgs args, Action<IMapsDirectlyToDatabaseTable> callback)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => SelectAnythingThen(args, callback));
            return;
        }

        var select = new SelectDialog<IMapsDirectlyToDatabaseTable>(
            args, this, CoreChildProvider.GetAllSearchables().Select(k => k.Key), false,
            _windowManager.GetFocusedCollection());

        if (select.ShowDialog() == DialogResult.OK && select.Selected != null) callback(select.Selected);
    }

    public override void ShowData(IViewSQLAndResultsCollection collection)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowData(collection));
            return;
        }

        Activate<ViewSQLAndResultsWithDataGridUI>(collection);
    }

    public override void ShowLogs(ILoggedActivityRootObject rootObject)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowLogs(rootObject));
            return;
        }

        Activate<LoadEventsTreeView>(new LoadEventsTreeViewObjectCollection(rootObject));
    }

    public override void ShowLogs(ExternalDatabaseServer loggingServer, LogViewerFilter filter)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowLogs(loggingServer, filter));
            return;
        }


        var loggingTabUI = Activate<LoggingTabUI, ExternalDatabaseServer>(loggingServer);
        if (filter != null)
            loggingTabUI.SetFilter(filter);
    }

    public override void ShowGraph(AggregateConfiguration aggregate)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowGraph(aggregate));
            return;
        }

        var graph = Activate<AggregateGraphUI, AggregateConfiguration>(aggregate);
        graph.LoadGraphAsync();
    }

    public override void LaunchSubprocess(ProcessStartInfo startInfo)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => LaunchSubprocess(startInfo));
            return;
        }

        var ctrl = new ConsoleControl.ConsoleControl();
        ShowWindow(ctrl, true);
        ctrl.StartProcess(startInfo);
    }

    public override void ShowData(DataTable table)
    {
        // if on wrong Thread
        if (_mainDockPanel?.InvokeRequired ?? false)
        {
            _mainDockPanel.Invoke(() => ShowData(table));
            return;
        }

        var ui = new DataTableViewerUI(table, "Table");
        ShowDialog(new SingleControlForm(ui, true));
    }
}