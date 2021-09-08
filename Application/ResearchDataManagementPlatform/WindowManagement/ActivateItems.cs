// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.UI;
using Rdmp.UI.AggregationUIs;
using Rdmp.UI.CatalogueSummary.LoadEvents;
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
using Rdmp.UI.SimpleDialogs.NavigateTo;
using Rdmp.UI.SubComponents;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using Rdmp.UI.Versioning;
using Rdmp.UI.Wizard;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.WindowArranging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;


using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Central class for RDMP main application, this class provides acceess to all the main systems in RDMP user interface such as Emphasis, the RefreshBus, Child 
    /// provision etc.  See IActivateItems for full details
    /// </summary>
    public class ActivateItems : BasicActivateItems, IActivateItems, IRefreshBusSubscriber
    {
        private readonly DockPanel _mainDockPanel;
        private readonly WindowManager _windowManager;

        public WindowFactory WindowFactory { get; private set; }

        public ICoreIconProvider CoreIconProvider { get; private set; }

        public ITheme Theme { get; private set; }

        public RefreshBus RefreshBus { get; private set; }
        
        readonly UIObjectConstructor _constructor = new UIObjectConstructor();

        public IArrangeWindows WindowArranger { get; private set; }
        
        public override void Publish(IMapsDirectlyToDatabaseTable databaseEntity)
        {
            if(databaseEntity is DatabaseEntity de)
                RefreshBus.Publish(this,new RefreshObjectEventArgs(de));
        }

        public override void Show(string title, string message)
        {
            WideMessageBox.Show(title,message,Environment.StackTrace,true,null,WideMessageBoxTheme.Help);
        }

        public ICombineableFactory CommandFactory { get; private set; }
        public ICommandExecutionFactory CommandExecutionFactory { get; private set; }
        public CommentStore CommentStore { get { return RepositoryLocator.CatalogueRepository.CommentStore; } }
        public HistoryProvider HistoryProvider { get; private set; }

        public List<IProblemProvider> ProblemProviders { get; private set; }

        public ActivateItems(ITheme theme,RefreshBus refreshBus, DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowFactory windowFactory, WindowManager windowManager, ICheckNotifier globalErrorCheckNotifier):base(repositoryLocator,globalErrorCheckNotifier)
        {
            Theme = theme;
            InteractiveDeletes = true;
            WindowFactory = windowFactory;
            _mainDockPanel = mainDockPanel;
            _windowManager = windowManager;
            RefreshBus = refreshBus;

            CoreChildProvider = GetChildProvider();
            
            HistoryProvider = new HistoryProvider(repositoryLocator);

            //handle custom icons from plugin user interfaces in which
            CoreIconProvider = new DataExportIconProvider(repositoryLocator,PluginUserInterfaces.ToArray());
            
            SelectIMapsDirectlyToDatabaseTableDialog.ImageGetter = (model)=> CoreIconProvider.GetImage(model);

            WindowArranger = new WindowArranger(this,_windowManager,_mainDockPanel);
            
            CommandFactory = new RDMPCombineableFactory();
            CommandExecutionFactory = new RDMPCommandExecutionFactory(this);

            ProblemProviders = new List<IProblemProvider>();
            ProblemProviders.Add(new DataExportProblemProvider());
            ProblemProviders.Add(new CatalogueProblemProvider());
            RefreshProblemProviders();

            RefreshBus.Subscribe(this);
        }


        protected override ICoreChildProvider GetChildProvider()
        {
            //constructor call in base class
            if(PluginUserInterfaces == null)
                return null;

            //Dispose the old one
            ICoreChildProvider temp = null;

            //prefer a linked repository with both
            if(RepositoryLocator.DataExportRepository != null)
                try
                {
                    temp = new DataExportChildProvider(RepositoryLocator,PluginUserInterfaces.ToArray(),GlobalErrorCheckNotifier,CoreChildProvider as DataExportChildProvider);
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
            
            //there was an error generating a data export repository or there was no repository specified

            //so just create a catalogue one
            if (temp == null)
                temp = new CatalogueChildProvider(RepositoryLocator.CatalogueRepository, PluginUserInterfaces.ToArray(),GlobalErrorCheckNotifier, CoreChildProvider as CatalogueChildProvider);

            // first time
            if(CoreChildProvider == null)
                CoreChildProvider = temp;
            else
                CoreChildProvider.UpdateTo(temp);

            RefreshBus.ChildProvider = CoreChildProvider;

            return CoreChildProvider;
        }
        

        public Form ShowWindow(Control singleControlForm, bool asDocument = false)
        {
            int width = singleControlForm.Size.Width + SystemInformation.BorderSize.Width;
            int height = singleControlForm.Size.Height + SystemInformation.BorderSize.Height;

            //use the .Text or fallback on .Name 
            string name = string.IsNullOrWhiteSpace(singleControlForm.Text)
                ? singleControlForm.Name ?? singleControlForm.GetType().Name//or worst case scenario use the type name!
                : singleControlForm.Text;

            if(singleControlForm is Form && asDocument)
                throw new Exception("Control '" + singleControlForm + "' is a Form and asDocument was passed as true.  When asDocument is true you must be a Control not a Form e.g. inherit from RDMPUserControl instead of RDMPForm");

            var c = singleControlForm as RDMPUserControl;
            
            if(c != null)
                c.SetItemActivator(this);

            var content = WindowFactory.Create(this,singleControlForm,name , null);
            
            if (asDocument)
                content.Show(_mainDockPanel,DockState.Document);
            else
                content.Show(_mainDockPanel,new Rectangle(0,0,width,height));
            
            return content;
        }
                
        public override void RequestItemEmphasis(object sender, EmphasiseRequest request)
        {
            //ensure a relevant Toolbox is available
            var descendancy = CoreChildProvider.GetDescendancyListIfAnyFor(request.ObjectToEmphasise);
            object root = null;

            if (descendancy != null)
                root = descendancy.Parents.FirstOrDefault();
            else
                root = request.ObjectToEmphasise; //assume maybe o is a root object itself?

            if (root is CohortIdentificationConfiguration cic)
                Activate<CohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic);
            else
            if (root != null)
                _windowManager.ShowCollectionWhichSupportsRootObjectType(root);

            //really should be a listener now btw since we just launched the relevant Toolbox if it wasn't there before
            //Look at assignments to Sender, the invocation list can change the Sender!
            var args = new EmphasiseEventArgs(request);
            base.OnEmphasise(this,args);
            
            //might be different than sender that was passed in
            if(args.Sender is DockContent content)
                content.Activate();

            //user is being shown the given object so track it as a recent (e.g. GoTo etc)
            HistoryProvider.Add(args.Request.ObjectToEmphasise);
        }

        public override bool SelectEnum(string prompt, Type enumType, out Enum chosen)
        {
            var selector = new PickOneOrCancelDialog<Enum>(Enum.GetValues(enumType).Cast<Enum>().ToArray(), prompt,
                (o) => CoreIconProvider.GetImage(o),
                null);
            if (selector.ShowDialog() == DialogResult.OK)
            {
                chosen = selector.Picked;
                return true;
            }
            
            chosen = default;
            return false;
        }

        public override bool SelectType(string prompt, Type[] available,out Type chosen)
        {
            var dlg =  new PickOneOrCancelDialog<Type>(available, prompt);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                chosen = dlg.Picked;
                return true;
            }


            chosen = null;
            return false;
        }

        public override bool CanActivate(object target)
        {
            return CommandExecutionFactory.CanActivate(target);
        }
        protected override void ActivateImpl(object o)
        {
            if(CommandExecutionFactory.CanActivate(o))
                CommandExecutionFactory.Activate(o);
        }

        public bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject)
        {
            //if the collection an arbitrary one then it is definetly not the root collection for anyone
            if (collection == RDMPCollection.None)
                return false;

            return _windowManager.GetCollectionForRootObject(rootObject) == collection;
        }

        /// <summary>
        /// Consults all currently configured IProblemProviders and returns true if any report a problem with the object
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool HasProblem(object model)
        {
            return ProblemProviders.Any(p => p.HasProblem(model));
        }

        /// <summary>
        /// Consults all currently configured IProblemProviders and returns the first Problem reported by any about the object or null
        /// if there are no problems reported.
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

        public string CurrentDirectory { get { return Environment.CurrentDirectory; }}
        public DialogResult ShowDialog(Form form)
        {
            return form.ShowDialog();
        }

        public void KillForm(Form f, Exception reason)
        {
            f.Close();
            ExceptionViewer.Show("Window Closed",reason);
        }
        public void KillForm(Form f, string reason)
        {
            f.Close();
            ExceptionViewer.Show("Window Closed",reason);
        }
        public void OnRuleRegistered(IBinderRule rule)
        {
            //no special action required
        }

        /// <summary>
        /// Asks the user if they want to reload a fresh copy with a Yes/No message box.
        /// </summary>
        /// <param name="databaseEntity"></param>
        /// <returns></returns>
        public bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity)
        {
            return YesNo(databaseEntity + " is out of date with database, would you like to reload a fresh copy?",
                           "Object Changed");
        }

        public T Activate<T, T2>(T2 databaseObject)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            return Activate<T, T2>(databaseObject, (Bitmap)CoreIconProvider.GetImage(databaseObject));
        }
        
        public T Activate<T>(IPersistableObjectCollection collection)
            where T: Control,IObjectCollectionControl,new()

        {
            //if the window is already open
            if (PopExisting(typeof(T), collection, out var existingHostedControlInstance))
            {
                //just update it's state
                var existing = (T) existingHostedControlInstance;
                existing.SetCollection(this,collection);

                return existing;
            }
                

            var uiInstance = new T();
            Activate(uiInstance, collection);
            return uiInstance;
        }


        private T Activate<T, T2>(T2 databaseObject, Bitmap tabImage)
            where T : RDMPSingleDatabaseObjectControl<T2>, new()
            where T2 : DatabaseEntity
        {
            Control existingHostedControlInstance;
            if (PopExisting(typeof(T), databaseObject, out existingHostedControlInstance))
                return (T)existingHostedControlInstance;

            var uiInstance = new T();
            var floatable = WindowFactory.Create(this,RefreshBus, uiInstance, tabImage, databaseObject);
            floatable.Show(_mainDockPanel, DockState.Document);
            uiInstance.SetDatabaseObject(this, databaseObject);

            SetTabText(floatable,uiInstance);

            return uiInstance;
        }

        private bool PopExisting(Type windowType, IMapsDirectlyToDatabaseTable databaseObject, out Control existingHostedControlInstance)
        {
            var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, databaseObject);
            existingHostedControlInstance = null;

            if (existing != null)
            {
                existingHostedControlInstance = existing.GetControl();
                existing.Activate();

                // only refresh if there are changes to the underlying object
                if(databaseObject is IRevertable r && r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                    existing.HandleUserRequestingTabRefresh(this);
            }

            return existing != null;
        }

        private bool PopExisting(Type windowType, IPersistableObjectCollection collection, out Control existingHostedControlInstance)
        {
            var existing = _windowManager.GetActiveWindowIfAnyFor(windowType, collection);
            existingHostedControlInstance = null;

            if (existing != null)
            {
                existingHostedControlInstance = existing.GetControl();
                existing.Activate();
                
                // only refresh if there are changes to some of the underlying objects
                if(collection.DatabaseObjects.OfType<IRevertable>().Any(r=>r.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent))
                    existing.HandleUserRequestingTabRefresh(this);
            }

            return existing != null;
        }
        public DockContent Activate(DeserializeInstruction instruction)
        {
            if (instruction.DatabaseObject != null && instruction.ObjectCollection != null)
                throw new ArgumentException("DeserializeInstruction cannot have both a DatabaseObject and an ObjectCollection");

            var c = _constructor.Construct(instruction.UIControlType);

            var uiInstance = c as IRDMPSingleDatabaseObjectControl;
            var uiCollection = c as IObjectCollectionControl;

            //it has a database object so call SetDatabaseObject
            if (uiCollection != null)
                //if we get here then Instruction wasn't for a 
                return Activate(uiCollection, instruction.ObjectCollection);
            else
            if (uiInstance != null)
            {
                var databaseObject = instruction.DatabaseObject;

                //the database object is gone? deleted maybe
                if (databaseObject == null)
                    return null;

                DockContent floatable = WindowFactory.Create(this,RefreshBus, uiInstance,CoreIconProvider.GetImage(databaseObject), databaseObject);

                floatable.Show(_mainDockPanel, DockState.Document);
                try
                {
                    uiInstance.SetDatabaseObject(this,(DatabaseEntity) databaseObject);
                    SetTabText(floatable,uiInstance);
                }
                catch (Exception e)
                {
                    floatable.Close();
                    throw new Exception("SetDatabaseObject failed on Control of Type '"+instruction.UIControlType.Name+"', control closed, see inner Exception for details",e);
                }

                return floatable;
            }
            else
                throw new PersistenceException("DeserializeInstruction must have either a DatabaseObject or an ObjectCollection");
        }

        private void SetTabText(DockContent floatable, INamedTab tab)
        {
            string tabText = tab.GetTabName();

            floatable.TabText = tabText;

            if (floatable != null && floatable.ParentForm != null)
                floatable.ParentForm.Text = tabText + " - RDMP";
        }

        public PersistableObjectCollectionDockContent Activate(IObjectCollectionControl collectionControl, IPersistableObjectCollection objectCollection)
        {
            var floatable = WindowFactory.Create(this,collectionControl,objectCollection, null);
            floatable.Show(_mainDockPanel, DockState.Document);
            return floatable;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //update the child provider
            GetChildProvider();
            RefreshProblemProviders();
        }

        private void RefreshProblemProviders()
        {
            foreach (IProblemProvider p in ProblemProviders)
                p.RefreshProblems(CoreChildProvider);
        }

        /// <inheritdoc/>
        public override bool YesNo(string text,string caption,out bool chosen)
        {
            var dr = MessageBox.Show(text, caption, MessageBoxButtons.YesNo);

            if (dr == DialogResult.Yes)
            {
                chosen = true;
                return true;
            }

            if (dr == DialogResult.No)
            {
                chosen = false;
                return true;
            }

            chosen = false;
            return false;
        }

        public override bool TypeText(string header, string prompt, int maxLength, string initialText, out string text, bool requireSaneHeaderText)
        {
            var textTyper = new TypeTextOrCancelDialog(header, prompt, maxLength, initialText, allowBlankText: false, multiLine: maxLength > 1000)
            {
                RequireSaneHeaderText = requireSaneHeaderText
            };

            text = textTyper.ShowDialog() == DialogResult.OK ? textTyper.ResultText : null;
            return !string.IsNullOrWhiteSpace(text);
        }

        public override DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription)
        {
            var dialog = new ServerDatabaseTableSelectorDialog(taskDescription,false,true,this);
            dialog.ShowDialog();
            
            if (dialog.DialogResult != DialogResult.OK)
                return null;

            return dialog.SelectedDatabase;
        }

        public override DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription)
        {
            var dialog = new ServerDatabaseTableSelectorDialog(taskDescription,true,true,this);
            dialog.AllowTableValuedFunctionSelection = true;
            
            dialog.ShowDialog();

            if (dialog.DialogResult != DialogResult.OK)
                return null;

            return dialog.SelectedTable;
        }

        public override void ShowException(string errorText, Exception exception)
        {
            ExceptionViewer.Show(errorText, exception);
        }

        public override void Wait(string title, Task task, CancellationTokenSource cts)
        {
            var ui = new WaitUI(title,task,cts);
            ui.ShowDialog();
        }


        public override IEnumerable<Type> GetIgnoredCommands()
        {
            yield return typeof(ExecuteCommandPin);
            yield return typeof(ExecuteCommandUnpin);
            yield return typeof(ExecuteCommandRefreshObject);
            yield return typeof(ExecuteCommandChangeExtractability);
            yield return typeof (ExecuteCommandOpenInExplorer);
            yield return typeof (ExecuteCommandCreateNewFileBasedProcessTask);
        }

        
        public override IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
            string initialSearchText = null, bool allowAutoSelect = false)
        {
            if (!availableObjects.Any())
            {
                MessageBox.Show("There are no compatible objects in your RMDP for '"+ prompt +"''");
                return null;
            }

            //if there is only one object available to select
            if (availableObjects.Length == 1)
                if(allowAutoSelect || YesNo("You only have one compatible object, use '"+availableObjects[0]+"'","Select '" + availableObjects[0] + "'?"))
                {
                    return availableObjects[0];
                }
                else
                {
                    return null;
                }

            SelectIMapsDirectlyToDatabaseTableDialog selectDialog = new SelectIMapsDirectlyToDatabaseTableDialog(this, availableObjects, false, false);
            selectDialog.Text = prompt;
            selectDialog.SetInitialFilter(initialSearchText);


            if (selectDialog.ShowDialog() == DialogResult.OK)
                return selectDialog.Selected;

            return null; //user didn't select one of the IMapsDirectlyToDatabaseTable objects shown in the dialog
        }

        public override bool SelectObject<T>(string prompt, T[] available, out T selected, string initialSearchText = null, bool allowAutoSelect = false)
        {
            var pick = new PickOneOrCancelDialog<T>(available, prompt);

            if(pick.ShowDialog() == DialogResult.OK)
            {
                selected = pick.Picked;
                return true;
            }

            selected = default(T);
            return false;
        }

        public override DirectoryInfo SelectDirectory(string prompt)
        {
            using(var fb = new FolderBrowserDialog())
            {
                if (fb.ShowDialog() == DialogResult.OK)
                    return new DirectoryInfo(fb.SelectedPath);
            
                return null;
            }
        }

        public override FileInfo SelectFile(string prompt)
        {
            return SelectFile(prompt, null, null);
        }

        public override FileInfo SelectFile(string prompt, string patternDescription, string pattern)
        {
            using(var fb = new OpenFileDialog {CheckFileExists = false,Multiselect = false})
            {
                if (patternDescription != null && pattern != null)
                    fb.Filter = patternDescription + "|" + pattern;

                if (fb.ShowDialog() == DialogResult.OK)
                    return new FileInfo(fb.FileName);
            
                return null;
            }        
        }
        
        public override FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern)
        {
            using(var fb = new OpenFileDialog {CheckFileExists = false,Multiselect = true})
            {
                if (patternDescription != null && pattern != null)
                    fb.Filter = patternDescription + "|" + pattern;

                if (fb.ShowDialog() == DialogResult.OK)
                    return fb.FileNames.Select(f=>new FileInfo(f)).ToArray();
            
                return null;
            }
        }

        protected override bool SelectValueTypeImpl(string prompt, Type paramType, object initialValue, out object chosen)
        {
            //whatever else it is use string
            var typeTextDialog = new TypeTextOrCancelDialog("Enter Value", prompt + " (" + paramType.Name + ")",1000,
                initialValue?.ToString());

            if (typeTextDialog.ShowDialog() == DialogResult.OK)
            {
                chosen = UsefulStuff.ChangeType(typeTextDialog.ResultText, paramType);
                return true;
            }

            chosen = null;
            return false;
        }

        public override IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
            IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText)
        {
            if (!availableObjects.Any())
            {
                MessageBox.Show("There are no '" + arrayElementType.Name + "' objects in your RMDP");
                return null;
            }

            SelectIMapsDirectlyToDatabaseTableDialog selectDialog = new SelectIMapsDirectlyToDatabaseTableDialog(this, availableObjects, false, false);
            selectDialog.Text = prompt;
            selectDialog.SetInitialFilter(initialSearchText);
            selectDialog.AllowMultiSelect = true;
            
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                var ms = selectDialog.MultiSelected.ToList();
                var toReturn = Array.CreateInstance(arrayElementType, ms.Count);

                for(int i = 0;i<ms.Count;i++)
                    toReturn.SetValue(ms[i],i);
                
                return toReturn.Cast<IMapsDirectlyToDatabaseTable>().ToArray();
            }

            return null;
        }

        public override List<CommandInvokerDelegate> GetDelegates()
        {
            return new List<CommandInvokerDelegate>
            {
                new CommandInvokerDelegate(typeof(IActivateItems),true,(p)=>this)
            };
        }
        public void StartSession(string sessionName, IEnumerable<IMapsDirectlyToDatabaseTable> initialObjects)
        {
            var panel = WindowFactory.Create(this,new SessionCollectionUI(),new SessionCollection(sessionName)
            {
                DatabaseObjects = initialObjects?.ToList() ?? new List<IMapsDirectlyToDatabaseTable>()
            },CatalogueIcons.WindowLayout);
            panel.Show(_mainDockPanel,DockState.DockLeft);
        }

        
        /// <inheritdoc/>
        public IEnumerable<SessionCollectionUI> GetSessions()
        {
            return _windowManager.GetAllWindows<SessionCollectionUI>();
        }

        public override IPipelineRunner GetPipelineRunner(IPipelineUseCase useCase, IPipeline pipeline)
        {
             
            ConfigureAndExecutePipelineUI configureAndExecuteDialog = new ConfigureAndExecutePipelineUI(useCase,this);
            configureAndExecuteDialog.Dock = DockStyle.Fill;
            
            return configureAndExecuteDialog;
        }

        public override CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable, IProject project, string cohortInitialDescription)
        {
            var ui = new CohortCreationRequestUI(this,externalCohortTable,project);
                
            if(!string.IsNullOrWhiteSpace(cohortInitialDescription))
                ui.CohortDescription = cohortInitialDescription + " (" + Environment.UserName + " - " + DateTime.Now + ")";

            if (ui.ShowDialog() != DialogResult.OK)
                return null;

            return ui.Result;
        }

        public override ICatalogue CreateAndConfigureCatalogue(ITableInfo tableInfo, ColumnInfo[] extractionIdentifierColumns, string initialDescription, IProject projectSpecific, CatalogueFolder folder)
        {
            var ui = new ConfigureCatalogueExtractabilityUI(this, tableInfo, initialDescription, projectSpecific);
            ui.TargetFolder = folder;
            ui.ShowDialog();
            
            return ui.CatalogueCreatedIfAny;
        }
        public override ExternalDatabaseServer CreateNewPlatformDatabase(ICatalogueRepository catalogueRepository, PermissableDefaults defaultToSet, IPatcher patcher, DiscoveredDatabase db)
        {
            //launch the winforms UI for creating a database
            return CreatePlatformDatabase.CreateNewExternalServer(catalogueRepository,defaultToSet,patcher);
        }

        public override bool ShowCohortWizard(out CohortIdentificationConfiguration cic)
        {
            var wizard = new CreateNewCohortIdentificationConfigurationUI(this);

            if (wizard.ShowDialog() == DialogResult.OK)
            {
                cic = wizard.CohortIdentificationCriteriaCreatedIfAny;
            }
            else
            {
                cic = null;
            }

            // Wizard was shown so that's a thing
            return true;
        }

        public override void SelectAnythingThen(string prompt, Action<IMapsDirectlyToDatabaseTable> callback)
        {
            NavigateToObjectUI navigate = new NavigateToObjectUI(this) { Text = prompt };
            navigate.CompletionAction = callback;
            navigate.Show();
        }

        public override void ShowData(IViewSQLAndResultsCollection collection)
        {
            Activate<ViewSQLAndResultsWithDataGridUI>(collection);
        }

        public override void ShowLogs(ILoggedActivityRootObject rootObject)
        {
            Activate<LoadEventsTreeView>(new LoadEventsTreeViewObjectCollection(rootObject));
        }

        public override void ShowLogs(ExternalDatabaseServer loggingServer, LogViewerFilter filter)
        {
            LoggingTabUI loggingTabUI =  Activate<LoggingTabUI, ExternalDatabaseServer>(loggingServer);
            if(filter != null)
                loggingTabUI.SetFilter(filter);
        }

        public override void ShowGraph(AggregateConfiguration aggregate)
        {
            var graph = Activate<AggregateGraphUI, AggregateConfiguration>(aggregate);
            graph.LoadGraphAsync();
        }
    }
}
