using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Checks;
using DataExportLibrary.DataRelease.Potential;
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
    /// The ultimate end point of the Data Export Manager is the provision of a packaged up Release of all the anonymised datasets for all the cohort(s) (e.g. 'Cases' and 'Controls') in
    /// a research project.  There is no going back once you have sent the package to the researcher, if you have accidentally included the wrong datasets or supplied identifiable data
    /// (e.g. in a free text field) then you are in big trouble.  For this reason the 'Release' process is a tightly controlled sequence which the RDMP undertakes to try to reduce error.
    /// 
    /// <para>In this control you will see all the currently selected datasets in a project's configuration(s) and the state of the dataset extraction (from the RDMP's perspective) as well 
    /// as the status of the 'Environment' (Ticketing System).  Right clicking on a dataset will give you options appropriate to it's state.</para>
    /// 
    /// <para>Extraction of large datasets can take days or weeks and a project extraction is an ongoing exercise.  It is possible that by the time you come to release a project some of the
    /// early datasets have been changed or the files deleted etc.  The status of each extracted dataset is shown in the list box.  You can only do an extraction once all the datasets in
    /// the configuration are releasable.</para>
    /// 
    /// <para>In addition to verifying the datasets you can tie the RDMP into your ticketing system.  For example if you have tickets for each project extraction with stages for validation
    /// (so that data analysts can log time against validation and sign off on it etc) then you can setup Data Export Manager when the 'Release' Ticket is at a certain state (e.g. validated).
    /// To configure a ticketing system see TicketingSystemConfigurationUI.</para>
    /// 
    /// <para>If you haven't configured a Ticketing System then you shouldn't have to worry about the Environment State.</para>
    /// 
    /// <para> Once you have selected all the configurations you want to release click Release.</para>
    /// </summary>
    public partial class DataReleaseUI : DataReleaseUI_Design
    {
        private Project _project;

        private bool _isFirstTime = true;

        private IPipelineSelectionUI _pipelineSelectionUI1;
        private IMapsDirectlyToDatabaseTable[] _globals;
        private DataExportChildProvider _childProvider;
        
        private ArbitraryFolderNode _globalsNode = new ArbitraryFolderNode(ExtractionDirectory.GLOBALS_DATA_NAME);


        private bool _isExecuting;
        private RDMPCollectionCommonFunctionality _commonFunctionality;
        private IEnumerable<ExtractionConfiguration> _configurations = new ExtractionConfiguration[0];
        private IEnumerable<ISelectedDataSets> _selectedDataSets = new ISelectedDataSets[0];

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

            checkAndExecuteUI1.AllowsYesNoToAll = false;

            _commonFunctionality = new RDMPCollectionCommonFunctionality();

            checkAndExecuteUI1.BackColor = Color.FromArgb(240, 240, 240);
            pictureBox1.BackColor = Color.FromArgb(240,240,240);
        }

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
            var configuration = rowObject as IExtractionConfiguration;
            var global = rowObject as IMapsDirectlyToDatabaseTable;

            if (releaseRunner == null)
                return null;

            ICheckable key = null;

            if (configuration != null)
            {
                var releasePotential = releaseRunner.ChecksDictionary.Keys.OfType<ReleaseEnvironmentPotential>().ToArray().SingleOrDefault(rp => rp.Configuration.Equals(configuration));
                if (releasePotential != null)
                    return releasePotential.Assesment;
                
                return null;
            }

            if (sds != null)
            {
                var releasePotential = releaseRunner.ChecksDictionary.Keys.OfType<ReleasePotential>().ToArray().SingleOrDefault(rp => rp.SelectedDataSet.ID == sds.ID);

                //not been released ever
                if (releasePotential is NoReleasePotential)
                    return Releaseability.NeverBeenSuccessfullyExecuted;

                //do we know the release state of the assesments
                if (releasePotential != null && releasePotential.Assessments != null && releasePotential.Assessments.Any())
                {
                    var releasability = releasePotential.Assessments.Values.Min();

                    if (releasability != Releaseability.Undefined)
                        return releasability;
                }

                //otherwise use the checks of it
                key = releasePotential;
                if (key != null)
                    return releaseRunner.ChecksDictionary[key].GetWorst();

                return null;
            }

            if (global != null && (global is SupportingDocument || global is SupportingSQLTable))
            {
                var releasePotential = releaseRunner.ChecksDictionary.Keys.OfType<GlobalReleasePotential>().ToList().SingleOrDefault(rp => rp.RelatedGlobal.Equals(global));
                if (releasePotential != null)
                    return releasePotential.Releasability;

                return null;
            }

            if (rowObject.Equals(_globalsNode))
            {
                var globalChecker = releaseRunner.ChecksDictionary.Keys.OfType<GlobalsReleaseChecker>().ToList().SingleOrDefault();
                if (globalChecker != null)
                    return releaseRunner.ChecksDictionary[globalChecker].GetWorst();
            }

            return null;
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            return new ReleaseOptions()
            {
                Pipeline = _pipelineSelectionUI1.Pipeline == null ? 0 : _pipelineSelectionUI1.Pipeline.ID,
                Configurations = _configurations.Where(c=>tlvReleasePotentials.IsChecked(c) || tlvReleasePotentials.IsCheckedIndeterminate(c)).Select(ec => ec.ID).ToArray(),
                SelectedDataSets = _selectedDataSets.All(tlvReleasePotentials.IsChecked)?new int[0]: tlvReleasePotentials.CheckedObjects.OfType<ISelectedDataSets>().Select(sds => sds.ID).ToArray(),
                Command = activityRequested,
                ReleaseGlobals = tlvReleasePotentials.IsChecked(_globalsNode),
            };
        }

        private IEnumerable ChildrenGetter(object model)
        {
            var p = model as Project;
            var ec = model as ExtractionConfiguration;

            if (p != null)
                return _configurations = _childProvider.GetActiveConfigurationsOnly(p);

            if (ec != null)
                return _selectedDataSets = _childProvider.GetChildren(ec).OfType<ISelectedDataSets>();

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
                
                _pipelineSelectionUI1.PipelineChanged += ResetChecksUI;
            }

            var checkedBefore = tlvReleasePotentials.CheckedObjects;

            tlvReleasePotentials.ClearObjects();
            tlvReleasePotentials.AddObject(_globalsNode);
            tlvReleasePotentials.AddObject(_project);
            tlvReleasePotentials.ExpandAll();
            
            if (_isFirstTime)
                tlvReleasePotentials.CheckAll();
            else if (checkedBefore.Count > 0)
                tlvReleasePotentials.CheckObjects(checkedBefore);

            _isFirstTime = false;

            tlvReleasePotentials.DisableObjects(_globals);
            //tlvReleasePotentials.DisableObject(_globalsNode);
        }
        
        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            base.ConsultAboutClosing(sender, e);
            checkAndExecuteUI1.ConsultAboutClosing(sender, e);
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

        private void ResetChecksUI(object sender, EventArgs e)
        {
            if (!checkAndExecuteUI1.IsExecuting)
                checkAndExecuteUI1.Reset();
        }
    }


    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataReleaseUI_Design, UserControl>))]
    public abstract class DataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>
    {

    }
}