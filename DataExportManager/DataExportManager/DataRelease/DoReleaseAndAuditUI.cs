using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Repositories;
using DataExportManager.DataRelease.PipelineSource;
using DataExportManager.ItemActivation;
using MapsDirectlyToDatabaseTable.Revertable;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.Progress;

namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Shows all the currently selected configurations you are trying to release in a DataReleaseUI (See DataReleaseUI and ConfigurationReleasePotentialUI for fuller documentation about
    /// the releasable process).
    /// </summary>
    public partial class DoReleaseAndAuditUI : UserControl
    {
        public Project Project
        {
            get { return _project; }
            private set
            {
                _project = value; 

                ConfigurationsForRelease.Clear();
                ReloadTreeView();

                if (_project != null)
                {
                    _releaseEngine = new ReleaseEngine(value);
                }
            }
        }
     
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; private set;}

        private Project _project;
        ReleaseEngine _releaseEngine;
        
        public DoReleaseAndAuditUI()
        {
            InitializeComponent();

            ConfigurationsForRelease = new Dictionary<IExtractionConfiguration, List<ReleasePotential>>();
        }

        private ReleaseEnvironmentPotential _environmentPotential;

        private ReleaseState _releaseState = ReleaseState.Nothing;
        private IActivateDataExportItems _activator;

        public void AddToRelease(ReleasePotential[] datasetReleasePotentials, ReleaseEnvironmentPotential environmentPotential)
        {
            if (_releaseState == ReleaseState.DoingPatch)
            {
                MessageBox.Show("You are already trying to do a patch release, you cannot also do a proper release");
                return;
            }

            if (!datasetReleasePotentials.Any())
            {
                MessageBox.Show("You cannot release zero datasets!");
                return;
            }
            
            IExtractionConfiguration toAdd = datasetReleasePotentials.First().Configuration;
            
            if(datasetReleasePotentials.Any(p=>p.Configuration.ID != toAdd.ID))
                throw new Exception("ReleasePotential array contained datasets from multiple configurations");

            if(toAdd.Project_ID != Project.ID)
                throw new Exception("Mismatch between ProjectID of datasets selected for release and what this UI component recons the project is");

            if (ConfigurationsForRelease.Keys.Any(config=>config.ID == toAdd.ID))
            {
                MessageBox.Show("Configuration already added!");
                return;
            }

            CheckForCumulativeExtractionResults(datasetReleasePotentials);

            if (_environmentPotential != null)
                if (_environmentPotential.Assesment != _environmentPotential.Assesment)
                    throw new Exception("We have been given two ReleaseEnvironmentPotentials but they have different .Assesment properties");

            _environmentPotential = environmentPotential;

            ConfigurationsForRelease.Add((ExtractionConfiguration) toAdd, datasetReleasePotentials.ToList());
            _releaseState = ReleaseState.DoingProperRelease;

            ReloadTreeView();
        }

        private void CheckForCumulativeExtractionResults(ReleasePotential[] datasetReleasePotentials)
        {

            var staleDatasets = datasetReleasePotentials.Where(
                p => p.ExtractionResults.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();

            if (staleDatasets.Any())
                throw new Exception(
                    "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                    string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");
        }

        public void AddPatchRelease(ReleasePotential toPatchIn, ReleaseEnvironmentPotential environmentPotential)
        {
            if (_releaseState == ReleaseState.DoingProperRelease)
            {
                MessageBox.Show("You are already trying to do a Full release, you cannot also do a patch");
                return;
            }

            if (_environmentPotential != null)
                if (_environmentPotential.Assesment != _environmentPotential.Assesment)
                    throw new Exception("We have been given two ReleaseEnvironmentPotentials but they have different .Assesment properties");

            _environmentPotential = environmentPotential;

            if (toPatchIn.Configuration.Project_ID != Project.ID)
                throw new Exception("Mismatch between ProjectID of datasets selected for release and what this UI component recons the project is");

            if (ConfigurationsForRelease.Values.Any(array => array.Any(releasePotential => releasePotential.DataSet.ID == toPatchIn.DataSet.ID)))
            {
                MessageBox.Show("Dataset already included in the patch");
                return;
            }

            if (!ConfigurationsForRelease.ContainsKey(toPatchIn.Configuration))
                ConfigurationsForRelease.Add(toPatchIn.Configuration,new List<ReleasePotential>());

            ConfigurationsForRelease[toPatchIn.Configuration].Add(toPatchIn);
            _releaseState = ReleaseState.DoingPatch;
            ReloadTreeView();
        }

        private void ReloadTreeView()
        {
            treeView1.Nodes.Clear();

            foreach (var kvp in ConfigurationsForRelease)
            {
                TreeNode configurationNode = new TreeNode();
                configurationNode.Tag = kvp.Key;
                configurationNode.Text = kvp.Key.Name;
                
                treeView1.Nodes.Add(configurationNode);

                foreach (ReleasePotential potential in kvp.Value)
                {
                    TreeNode datasetReleaseNode = new TreeNode();
                    datasetReleaseNode.Tag = potential;
                    datasetReleaseNode.Text = potential.DataSet + " (" + potential.Assesment + ")";
                    configurationNode.Nodes.Add(datasetReleaseNode);
                }
            }

            if(treeView1.Nodes.Count == 0)
                _releaseState = ReleaseState.Nothing;
            
        }
        
        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (ConfigurationsForRelease.Count == 0)
            {
                MessageBox.Show("Nothing yet selected for release");
                return;
            }

            if (_pipelineUI.Pipeline == null)
                return;

            var factory = new DataFlowPipelineEngineFactory<ReleaseData>(_activator.RepositoryLocator.CatalogueRepository.MEF, ReleaseEngine.Context);

            FixedDataReleaseSource.CurrentRelease = new ReleaseData
            {
                ConfigurationsForRelease = ConfigurationsForRelease,
                EnvironmentPotential = _environmentPotential
            };

            factory.ExplicitSource = FixedDataReleaseSource;
            
            var pipelineEngine = factory.Create(_pipelineUI.Pipeline, new ThrowImmediatelyDataLoadEventListener());

            pipelineEngine.Initialize(_project, _activator);
            pipelineEngine.ExecutePipeline(new GracefulCancellationToken());
        }

        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                if(treeView1.SelectedNode != null)
                {
                    ExtractionConfiguration toDelete = treeView1.SelectedNode.Tag as ExtractionConfiguration;

                    if(toDelete != null)
                    {
                        ConfigurationsForRelease.Remove(toDelete);
                        ReloadTreeView();
                    }
                }
            }
        }

        public void SetProject(IActivateDataExportItems activator, Project project)
        {
            _activator = activator;
            Project = project;
            this.FixedDataReleaseSource = new FixedDataReleaseSource();

            SetupPipeline();
        }

        public FixedDataReleaseSource FixedDataReleaseSource { get; set; }

        private PipelineSelectionUI<ReleaseData> _pipelineUI;

        private void SetupPipeline()
        {
            if (_pipelineUI == null)
            {
                var cataRepository = _activator.RepositoryLocator.CatalogueRepository;
                _pipelineUI = new PipelineSelectionUI<ReleaseData>(FixedDataReleaseSource, null, cataRepository);
                _pipelineUI.Context = ReleaseEngine.Context;
                _pipelineUI.InitializationObjectsForPreviewPipeline.Add(_project);
                _pipelineUI.InitializationObjectsForPreviewPipeline.Add(_activator);

                _pipelineUI.CollapseToSingleLineMode();
                
                _pipelineUI.Dock = DockStyle.Fill;
                pnlPipeline.Controls.Add(_pipelineUI);
            }
        }

        private enum ReleaseState
        {
            Nothing,
            DoingPatch,
            DoingProperRelease
        }
    }

    
}
