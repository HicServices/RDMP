using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Checks;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Providers;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using RDMPAutomationService.Runners;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Lists all the active (unreleased) configurations in a project extraction and allows you to start a data release with one or more of them.  Each configuration is hosted in a
    /// ConfigurationReleasePotentialUI (See ConfigurationReleasePotentialUI) which shows whether it is in a releasable state and allows you to add it to the Release.
    /// 
    /// <para> Once you have selected all the configurations you want to release click Release.</para>
    /// </summary>
    public partial class DataReleaseUI : DataReleaseUI_Design
    {
        private Project _project;

        private IPipelineSelectionUI _pipelineSelectionUI1;
        private IExtractionConfiguration[] _unreleasedConfigurations;
        private IMapsDirectlyToDatabaseTable[] _globals;
        private DataExportChildProvider _childProvider;
        

        private ArbitraryFolderNode _globalsNode = new ArbitraryFolderNode(ExtractionDirectory.GLOBALS_DATA_NAME);

        public DataReleaseUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.DataExport;

            tlvReleasePotentials.CanExpandGetter = CanExpandGetter;
            tlvReleasePotentials.ChildrenGetter = ChildrenGetter;
            checkAndExecuteUI1.CommandGetter = CommandGetter;

            olvReleaseability.AspectGetter = Releaseability_AspectGetter;
            olvReleaseability.ImageGetter = Releaseability_ImageGetter;
            checkAndExecuteUI1.StateChanged += CheckAndExecuteUI1OnStateChanged;

            _commonFunctionality = new RDMPCollectionCommonFunctionality();
        }
        
        private bool _isExecuting;
        private RDMPCollectionCommonFunctionality _commonFunctionality;

        private void CheckAndExecuteUI1OnStateChanged(object sender, EventArgs eventArgs)
        {
            tlvReleasePotentials.RefreshObjects(tlvReleasePotentials.Objects.Cast<object>().ToArray());

            if (_isExecuting && !checkAndExecuteUI1.IsExecuting)
            {
                //if it was executing before and now no longer executing the status of the ExtractionConfigurations / Projects might have changed
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_project));
            }

            _isExecuting = checkAndExecuteUI1.IsExecuting;
        }

        private object Releaseability_ImageGetter(object rowObject)
        {
            var state = GetState(rowObject);
            return state == null ? null : _activator.CoreIconProvider.GetImage(state);
        }

        private object Releaseability_AspectGetter(object rowObject)
        {
            var state = GetState(rowObject);
            return state == null ? null : state.ToString();
        }

        private object GetState(object rowObject)
        {
            var releaseRunner = checkAndExecuteUI1.CurrentRunner as ReleaseRunner;
            var sds = rowObject as ISelectedDataSets;

            if (releaseRunner == null)
                return null;

            ICheckable key = null;

            if (sds != null)
                key = releaseRunner.ChecksDictionary.Keys.OfType<ReleasePotential>().ToArray().SingleOrDefault(rp => rp.SelectedDataSet.ID == sds.ID);
            else
                if (Equals(rowObject, _globalsNode))
                    key = releaseRunner.ChecksDictionary.Keys.OfType<GlobalsReleaseChecker>().SingleOrDefault();

            if (key != null)
                return releaseRunner.ChecksDictionary[key].GetWorst();

            return null;
        }


        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            return new ReleaseOptions()
            {
                Pipeline = _pipelineSelectionUI1.Pipeline == null ? 0 : _pipelineSelectionUI1.Pipeline.ID,
                Configurations = tlvReleasePotentials.CheckedObjects.OfType<ExtractionConfiguration>().Select(ec => ec.ID).ToArray(),
                SelectedDataSets = tlvReleasePotentials.CheckedObjects.OfType<ISelectedDataSets>().Select(sds => sds.ID).ToArray(),
                Command = activityRequested,
                ReleaseGlobals = tlvReleasePotentials.IsChecked(_globalsNode),
            };
        }


        private IEnumerable ChildrenGetter(object model)
        {
            var p = model as Project;
            var ec = model as ExtractionConfiguration;

            if (p != null)
                return _childProvider.GetActiveConfigurationsOnly(p);

            if (ec != null)
                return _childProvider.GetChildren(ec).OfType<ISelectedDataSets>();

            if (Equals(model, _globalsNode))
                return _globals;

            return null;
        }
        private bool CanExpandGetter(object model)
        {
            var c = ChildrenGetter(model);

            return c != null && c.Cast<object>().Any();
        }
        public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            if(!_commonFunctionality.IsSetup)
                _commonFunctionality.SetUp(RDMPCollection.None, tlvReleasePotentials, _activator, olvName, null, new RDMPCollectionCommonFunctionalitySettings
                {
                    AddFavouriteColumn = false,
                    AllowPinning = false,
                    SuppressChildrenAdder = true
                });

            _childProvider = (DataExportChildProvider)_activator.CoreChildProvider;
            _project = databaseObject;

            //figure out the globals
            var ec = _project.ExtractionConfigurations.FirstOrDefault();
            _globals = ec != null ? ec.GetGlobals() : new IMapsDirectlyToDatabaseTable[0];


            checkAndExecuteUI1.SetItemActivator(activator);

            if (_pipelineSelectionUI1 == null)
            {
                var context = new ReleaseUseCase(_project, new ReleaseData(RepositoryLocator) { IsDesignTime = true });
                _pipelineSelectionUI1 = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, context).Create("Release", DockStyle.Fill, pnlPipeline);
                _pipelineSelectionUI1.CollapseToSingleLineMode();
                _pipelineSelectionUI1.Pipeline = null;
            }

            tlvReleasePotentials.ClearObjects();
            tlvReleasePotentials.AddObject(_globalsNode);
            tlvReleasePotentials.AddObject(_project);
            tlvReleasePotentials.ExpandAll();
            tlvReleasePotentials.CheckAll();

            tlvReleasePotentials.DisableObjects(_globals);
        }

        public override string GetTabName()
        {
            return "Release: " + _project;
        }
        /*
        private void ConfigurationReleasePotentialUIOnRequestRelease(object sender, ReleasePotential[] datasetReleasePotentials, ReleaseEnvironmentPotential environmentPotential)
        {
            if (!datasetReleasePotentials.All(p =>
            {
                var dsReleasability = p.Assessments[p.DatasetExtractionResult];
                return dsReleasability == Releaseability.Releaseable ||
                       dsReleasability == Releaseability.ColumnDifferencesVsCatalogue;
            }))
                throw new Exception("Attempt made to release one or more datasets that are not assessed as being Releaseable (or ColumnDifferencesVsCatalogue)");

            if (environmentPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable && environmentPotential.Assesment != TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
                throw new Exception("Ticketing system decided that Environment was not ready for release");
        }*/
        /*
        private ReleaseData GetReleaseData()
        {
            return new ReleaseData
            {
                ConfigurationsForRelease = flowLayoutPanel1
                                                    .Controls
                                                    .Cast<ConfigurationReleasePotentialUI>()
                                                    .ToDictionary(crp => (IExtractionConfiguration)crp.Configuration, crp => crp.ReleasePotentials),
                EnvironmentPotential = flowLayoutPanel1
                                                    .Controls
                                                    .Cast<ConfigurationReleasePotentialUI>()
                                                    .Select(crp => crp.EnvironmentalPotential).FirstOrDefault(),
                ReleaseState = ReleaseState.Nothing
            };
        }*/

        public void TickAllFor(ExtractionConfiguration configuration)
        {
            tlvReleasePotentials.UncheckAll();
            tlvReleasePotentials.CheckObject(configuration);
            tlvReleasePotentials.CheckObject(_globalsNode);
        }
    }


    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataReleaseUI_Design, UserControl>))]
    public abstract class DataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>
    {

    }
}