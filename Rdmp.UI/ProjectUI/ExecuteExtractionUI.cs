// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.UsedByNodes;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.TransparentHelpSystem;
using Rdmp.UI.TransparentHelpSystem.ProgressTracking;
using ReusableLibraryCode.Checks;

namespace Rdmp.UI.ProjectUI
{
    /// <summary>
    /// Allows you to execute an extraction of a project configuration (Generate anonymous project data extractions for researchers).  You should make sure that you have already selected 
    /// the correct datasets, filters, transforms etc to meet the researchers project requirements (and governance approvals) - See ExtractionConfigurationUI and ConfigureDatasetUI.
    /// 
    /// <para>Start by selecting which datasets you want to execute (this can be an iterative process - you can extract half of them overnight and then come back and extract the other half the 
    /// next night).  See ChooseExtractablesUI for how to select datasets.</para>
    /// 
    /// <para>Next you should select/create a new extraction pipeline (See 'A Brief Overview Of What A Pipeline Is' in UserManual.docx).  This will determine the format of the extracted data
    /// (e.g. .CSV or .MDB database file or any other file for which you have a plugin implemented for).</para>
    /// </summary>
    public partial class ExecuteExtractionUI : ExecuteExtractionUI_Design
    {
        private IPipelineSelectionUI _pipelineSelectionUI1;
            
        private ExtractionConfiguration _extractionConfiguration;
        
        private IMapsDirectlyToDatabaseTable[] _globals;
        private ISelectedDataSets[] _datasets;
        private HashSet<ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>> _bundledStuff;

        private RDMPCollectionCommonFunctionality _commonFunctionality = new RDMPCollectionCommonFunctionality();

        private const string CoreDatasets = "Core";
        private const string ProjectSpecificDatasets = "Project Specific";

        private ArbitraryFolderNode _coreDatasetsFolder = new ArbitraryFolderNode(CoreDatasets,1);
        private ArbitraryFolderNode _projectSpecificDatasetsFolder = new ArbitraryFolderNode(ProjectSpecificDatasets,2);
        private ArbitraryFolderNode _globalsFolder = new ArbitraryFolderNode(ExtractionDirectory.GLOBALS_DATA_NAME,0);
        
        private ToolStripControlHost _pipelinePanel;

        private ToolStripLabel lblMaxConcurrent = new ToolStripLabel("Concurrent:");
        private ToolStripTextBox tbMaxConcurrent = new ToolStripTextBox(){Text="3"};

        public ExecuteExtractionUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataExport;

            checkAndExecuteUI1.CommandGetter = CommandGetter;
            checkAndExecuteUI1.StateChanged += CheckAndExecuteUI1OnStateChanged;
            checkAndExecuteUI1.GroupBySender();
            
            olvState.ImageGetter = State_ImageGetter;
            olvState.AspectGetter = State_AspectGetter;

            tlvDatasets.ChildrenGetter = ChildrenGetter;
            tlvDatasets.CanExpandGetter = CanExpandGetter;
            tlvDatasets.HierarchicalCheckboxes = true;
            tlvDatasets.ItemActivate += TlvDatasets_ItemActivate;
            
            checkAndExecuteUI1.BackColor = Color.FromArgb(240, 240, 240);

            tlvDatasets.CellClick += tlvDatasets_CellClick;

            _coreDatasetsFolder.CommandGetter = () =>
                new IAtomicCommand[]
                {
                    new ExecuteCommandAddDatasetsToConfiguration(Activator,_extractionConfiguration),
                    new ExecuteCommandAddPackageToConfiguration(Activator,_extractionConfiguration),
                };
        }

        private void TlvDatasets_ItemActivate(object sender, EventArgs e)
        {
            if (tlvDatasets.SelectedObject is SelectedDataSets sds) 
                Activator.Activate(sds);
        }

        void tlvDatasets_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Column == olvState)
            {
                var notifier = GetCheckNotifier(e.Model);

                if(notifier == null)
                    return;

                var popup = new PopupChecksUI(e.Model.ToString(), false);
                popup.Check(new ReplayCheckable(notifier));
            }
        }

        private HelpWorkflow BuildHelpFlow()
        {
            var helpWorkflow = new HelpWorkflow(this, new ExecuteCommandExecuteExtractionConfiguration(Activator), new NullHelpWorkflowProgressProvider());

            //////Normal work flow
            var root = new HelpStage(tlvDatasets, "Choose the datasets and Globals you want to extract here.\r\n" +
                                                 "\r\n" +
                                                 "Click on the red icon to disable this help.");
            var stage2 = new HelpStage(_pipelinePanel.Control, "Select the pipeline to run for extracting the data.\r\n");
            
            root.SetOption(">>", stage2);
            stage2.SetOption(">>", checkAndExecuteUI1.HelpStages.First());
            for (int i = 0; i < checkAndExecuteUI1.HelpStages.Count - 1; i++)
            {
                checkAndExecuteUI1.HelpStages[i].SetOption(">>", checkAndExecuteUI1.HelpStages[i+1]);
            }

            checkAndExecuteUI1.HelpStages.Last().SetOption("|<<", root);

            helpWorkflow.RootStage = root;
            return helpWorkflow;
        }

        private void CheckAndExecuteUI1OnStateChanged(object sender, EventArgs eventArgs)
        {
            tlvDatasets.RefreshObjects(tlvDatasets.Objects.Cast<object>().ToArray());
        }

        private bool CanExpandGetter(object model)
        {
            var e = ChildrenGetter(model);
            return  e != null && e.Cast<object>().Any();
        }

        private IEnumerable ChildrenGetter(object model)
        {
            if (model == _globalsFolder)
                return _globals;

            if (model == _coreDatasetsFolder)
                return _datasets.Where(sds => sds.ExtractableDataSet.Project_ID == null);

            if (model == _projectSpecificDatasetsFolder)
                return _datasets.Where(sds => sds.ExtractableDataSet.Project_ID != null);

            if (_bundledStuff != null && model is ISelectedDataSets sds2)
                return _bundledStuff.Where(s => s.User.Equals(sds2));

            return null;
        }

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            base.ConsultAboutClosing(sender, e);
            checkAndExecuteUI1.ConsultAboutClosing(sender, e);
        }

        private object State_ImageGetter(object rowObject)
        {
            var state = GetState(rowObject);
            return state == null ? null : Activator.CoreIconProvider.GetImage(state);
        }

        private object GetState(object rowObject)
        {
            var extractionRunner = checkAndExecuteUI1.CurrentRunner as ExtractionRunner;

            if (rowObject == _globalsFolder && extractionRunner != null)
                return extractionRunner.GetGlobalsState();

            var sds = rowObject as SelectedDataSets;

            if (extractionRunner != null && sds != null) 
                return extractionRunner.GetState(sds.ExtractableDataSet);
            
            return null;
        }

        private ToMemoryCheckNotifier GetCheckNotifier(object rowObject)
        {
            var extractionRunner = checkAndExecuteUI1.CurrentRunner as ExtractionRunner;

            if (rowObject == _globalsFolder && extractionRunner != null)
                return extractionRunner.GetGlobalCheckNotifier();

            var sds = rowObject as SelectedDataSets;

            if (extractionRunner != null && sds != null) 
                return extractionRunner.GetCheckNotifier(sds.ExtractableDataSet);
            
            return null;
        }
        private object State_AspectGetter(object rowobject)
        {
            var state = GetState(rowobject);
            return state == null ? null : state.ToString();
        }
        
        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            int max;

            //if user has defined an alternative maximum concurrent number of executing extraction threads
            max = int.TryParse(tbMaxConcurrent.Text, out max) ? max : 3;

            return new ExtractionOptions() { 
                Command = activityRequested,
                ExtractGlobals = tlvDatasets.IsChecked(_globalsFolder),
                MaxConcurrentExtractions = max,
                ExtractionConfiguration = _extractionConfiguration.ID,
                Pipeline = _pipelineSelectionUI1.Pipeline == null? 0 : _pipelineSelectionUI1.Pipeline.ID,
                Datasets = _datasets.All(tlvDatasets.IsChecked) ? new int[0] : _datasets.Where(tlvDatasets.IsChecked).Select(sds => sds.ExtractableDataSet.ID).ToArray()
            };
        }

        private bool _isFirstTime = true;
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            _extractionConfiguration = databaseObject;
            
            if(!_commonFunctionality.IsSetup)
                _commonFunctionality.SetUp(RDMPCollection.None, tlvDatasets,activator,olvName,null,new RDMPCollectionCommonFunctionalitySettings()
                {
                    AddFavouriteColumn = false,
                    AllowPinning=false,
                    SuppressChildrenAdder=true,
                    SuppressActivate = true,
                    AddCheckColumn = false
                });
            
            var checkedBefore = tlvDatasets.CheckedObjects;

            tlvDatasets.ClearObjects();

            _globals = _extractionConfiguration.GetGlobals();
            _datasets = databaseObject.SelectedDataSets.ToArray();

            GetBundledStuff();

            //add the folders
            tlvDatasets.AddObjects(new object[] { _globalsFolder, _coreDatasetsFolder, _projectSpecificDatasetsFolder });
            
            //enable all to start with 
            tlvDatasets.EnableObjects(tlvDatasets.Objects);

            tlvDatasets.DisableObjects(_globals);
            tlvDatasets.DisableObjects(_bundledStuff);

            //if there are no globals disable this option
            if(!_globals.Any())
                tlvDatasets.DisableObject(_globalsFolder);

            //if there are no project specific datasets
            if (_datasets.All(sds => sds.ExtractableDataSet.Project_ID == null))
                tlvDatasets.DisableObject(_projectSpecificDatasetsFolder); //disable this option

            //if all the datasets are project specific
            if (_datasets.All(sds => sds.ExtractableDataSet.Project_ID != null))
                tlvDatasets.DisableObject(_coreDatasetsFolder);
            
            //don't accept refresh while executing
            if (checkAndExecuteUI1.IsExecuting)
                return;
            
            if (_pipelineSelectionUI1 == null)
            {
                //create a new selection UI (pick an extraction pipeliene UI)
                var useCase = ExtractionPipelineUseCase.DesignTime();
                var factory = new PipelineSelectionUIFactory(Activator.RepositoryLocator.CatalogueRepository, null, useCase);

                _pipelineSelectionUI1 = factory.Create(activator,"Extraction Pipeline", DockStyle.Fill);
                _pipelineSelectionUI1.CollapseToSingleLineMode();

                //if the configuration has a default then use that pipeline
                if (_extractionConfiguration.DefaultPipeline_ID != null)
                    _pipelineSelectionUI1.Pipeline = _extractionConfiguration.DefaultPipeline;

                _pipelineSelectionUI1.PipelineChanged += ResetChecksUI;

                _pipelinePanel = new ToolStripControlHost((Control) _pipelineSelectionUI1);
                
                helpIcon1.SetHelpText("Extraction", "It is a wise idea to click here if you don't know what this screen can do for you...", BuildHelpFlow());
            }

            CommonFunctionality.Add(new ToolStripLabel("Extraction Pipeline:"));
            CommonFunctionality.Add(_pipelinePanel);
            CommonFunctionality.AddHelpStringToToolStrip("Extraction Pipeline","The sequence of components that will be executed in order to enable the datasets to be extracted. This will start with a source component that performs the linkage against the cohort followed by subsequent components (if any) and then a destination component that writes the final records (e.g. to database / csv file etc).");

            CommonFunctionality.AddToMenu(new ExecuteCommandRelease(activator).SetTarget(_extractionConfiguration));

            CommonFunctionality.Add(lblMaxConcurrent);
            CommonFunctionality.Add(tbMaxConcurrent);
            CommonFunctionality.AddHelpStringToToolStrip("Concurrent", "The maximum number of datasets to extract at once.  Once this number is reached the remainder will be queued and only started when one of the other extracting datasets completes.");

            checkAndExecuteUI1.SetItemActivator(activator);

            tlvDatasets.ExpandAll();

            if (_isFirstTime)
            {
                tlvDatasets.CheckAll();
                foreach (var disabledObject in tlvDatasets.DisabledObjects.OfType<ArbitraryFolderNode>())
                {
                    tlvDatasets.UncheckObject(disabledObject);
                }
            }
            else if (checkedBefore.Count > 0)
                tlvDatasets.CheckObjects(checkedBefore);

            _isFirstTime = false;
        }

        private void ResetChecksUI(object sender, EventArgs e)
        {
            if (!checkAndExecuteUI1.IsExecuting)
                checkAndExecuteUI1.Reset();
        }
        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            tlvDatasets.ModelFilter = new TextMatchFilter(tlvDatasets,tbFilter.Text);
            tlvDatasets.UseFiltering = true;
        }

        private void GetBundledStuff()
        {
            _bundledStuff = new HashSet<ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>>();

            foreach (ISelectedDataSets sds in _datasets)
            {
                var eds = sds.ExtractableDataSet;

                foreach (var document in eds.Catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableLocals))
                    _bundledStuff.Add( new ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>(sds,document));

                foreach (var supportingSQLTable in eds.Catalogue.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableLocals))
                    _bundledStuff.Add(new ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>(sds, supportingSQLTable));

                foreach (var lookup in eds.Catalogue.GetLookupTableInfoList())
                    _bundledStuff.Add(new ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>(sds, lookup));
            }
        }

        private void olvDatasets_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sds =  tlvDatasets.SelectedObject as SelectedDataSets;
            checkAndExecuteUI1.GroupBySender(sds != null ? sds.ToString() : null);
        }

        public void TickAllFor(SelectedDataSets selectedDataSet)
        {
            tlvDatasets.UncheckAll();
            tlvDatasets.CheckObject(_globalsFolder);
            tlvDatasets.CheckObject(selectedDataSet);
        }

        private void tlvDatasets_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            checkAndExecuteUI1.Enabled = tlvDatasets.CheckedObjects.Cast<object>().OfType<SelectedDataSets>().Any();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExecuteExtractionUI_Design, UserControl>))]
    public abstract class ExecuteExtractionUI_Design:RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}
