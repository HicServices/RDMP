using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.Providers.Nodes.UsedByNodes;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using RDMPAutomationService.Runners;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace DataExportManager.ProjectUI
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
            
        public int TopX { get; set; }
        private ExtractionConfiguration _extractionConfiguration;
        
        private IMapsDirectlyToDatabaseTable[] _globals;
        private ISelectedDataSets[] _datasets;
        private HashSet<ObjectUsedByOtherObjectNode<ISelectedDataSets, IMapsDirectlyToDatabaseTable>> _bundledStuff;

        private RDMPCollectionCommonFunctionality _commonFunctionality = new RDMPCollectionCommonFunctionality();

        private const string CoreDatasets = "Core";
        private const string ProjectSpecificDatasets = "Project Specific";

        private ArbitraryFolderNode _coreDatasetsFolder = new ArbitraryFolderNode(CoreDatasets);
        private ArbitraryFolderNode _projectSpecificDatasetsFolder = new ArbitraryFolderNode(ProjectSpecificDatasets);
        private ArbitraryFolderNode _globalsFolder = new ArbitraryFolderNode(ExtractionDirectory.GLOBALS_DATA_NAME);

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
            
            checkAndExecuteUI1.BackColor = Color.FromArgb(240, 240, 240);
            pictureBox1.BackColor = Color.FromArgb(240, 240, 240);
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

            var eds = model as IExtractableDataSet;

            if (_bundledStuff != null && eds != null)
                return _bundledStuff.Where(s => s.User.Equals(eds));

            return null;
        }

        private object State_ImageGetter(object rowObject)
        {
            var state = GetState(rowObject);
            return state == null ? null : _activator.CoreIconProvider.GetImage(state);
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

        private object State_AspectGetter(object rowobject)
        {
            var state = GetState(rowobject);
            return state == null ? null : state.ToString();
        }
        
        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            return new ExtractionOptions() { 
                Command = activityRequested,
                ExtractGlobals = tlvDatasets.IsChecked(_globalsFolder),
                Datasets = _datasets.Where(tlvDatasets.IsChecked).Select(sds => sds.ExtractableDataSet.ID).ToArray(),
                ExtractionConfiguration = _extractionConfiguration.ID,
                Pipeline = _pipelineSelectionUI1.Pipeline == null? 0 :_pipelineSelectionUI1.Pipeline.ID
            };
        }

        
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            _extractionConfiguration = databaseObject;
            
            if(!_commonFunctionality.IsSetup)
                _commonFunctionality.SetUp(RDMPCollection.None, tlvDatasets,activator,olvName,null,new RDMPCollectionCommonFunctionalitySettings(){AddFavouriteColumn = false,AllowPinning=false,SuppressChildrenAdder=true});

            checkAndExecuteUI1.SetItemActivator(activator);

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
                var useCase = new ExtractionPipelineUseCase(_extractionConfiguration.Project);
                var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, useCase);

                _pipelineSelectionUI1 = factory.Create("Extraction Pipeline", DockStyle.Fill, panel1);
                _pipelineSelectionUI1.CollapseToSingleLineMode();

                //if the configuration has a default then use that pipeline
                if (_extractionConfiguration.DefaultPipeline_ID != null)
                    _pipelineSelectionUI1.Pipeline = _extractionConfiguration.DefaultPipeline;

                _pipelineSelectionUI1.PipelineChanged += ResetChecksUI;
            }

            TopX = -1;

            tlvDatasets.ExpandAll();
            tlvDatasets.CheckAll();
        }

        private void ResetChecksUI(object sender, EventArgs e)
        {
            if (!checkAndExecuteUI1.IsExecuting)
                checkAndExecuteUI1.Reset();
        }

        private void tbTopX_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTopX.Text))
            {
                TopX = -1;
                return;
            }
            try
            {
                TopX = int.Parse(tbTopX.Text);

                if (TopX < 0)
                {
                    TopX = -1;
                    throw new Exception("Cannot be negative");
                }

                tbTopX.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTopX.ForeColor = Color.Red;
            }
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
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExecuteExtractionUI_Design, UserControl>))]
    public abstract class ExecuteExtractionUI_Design:RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}
