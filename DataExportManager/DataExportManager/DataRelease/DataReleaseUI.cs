using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Ticketing;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.Icons.IconProvision;
using DataExportManager.ProjectUI;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
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

        private IPipelineSelectionUI _pipelineUI;
        private IExtractionConfiguration[] _unreleasedConfigurations;

        
        public DataReleaseUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.DataExport;

            olvName.ImageGetter = Name_ImageGetter;

            tlvReleasePotentials.CanExpandGetter = CanExpandGetter;
            tlvReleasePotentials.ChildrenGetter = ChildrenGetter;
            checkAndExecuteUI1.CommandGetter = CommandGetter;
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            return new ReleaseOptions();
        }


        private IEnumerable ChildrenGetter(object model)
        {
            var p = model as Project;
            var ec = model as ExtractionConfiguration;

            if (p != null)
                return p.ExtractionConfigurations.Where(c => !c.IsReleased);

            if (ec != null)
                return ec.SelectedDataSets;

            return null;
        }
        private bool CanExpandGetter(object model)
        {
            var c = ChildrenGetter(model);

            return c != null && c.Cast<object>().Any();
        }
        private object Name_ImageGetter(object rowObject)
        {
            if (_activator == null)
                return null;

            return _activator.CoreIconProvider.GetImage(rowObject);
        }

        public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _project = databaseObject;

            if (_pipelineUI == null)
            {
                var context = new ReleaseUseCase(_project, new ReleaseData(){IsDesignTime = true});
                _pipelineUI = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, context).Create("Release", DockStyle.Fill, pnlPipeline);
                _pipelineUI.CollapseToSingleLineMode();
                _pipelineUI.Pipeline = null;
            }

            tlvReleasePotentials.ClearObjects();
            tlvReleasePotentials.AddObject(_project);
            tlvReleasePotentials.ExpandAll();
            tlvReleasePotentials.CheckAll();
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
    }

    
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataReleaseUI_Design, UserControl>))]
    public abstract class DataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>
    {

    }
}

