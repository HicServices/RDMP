using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Repositories;
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
                    
                    var intendedReleaseDirectory = _releaseEngine.GetIntendedReleaseDirectory();


                    if (intendedReleaseDirectory == null)
                        lblReleaseRootDirectory.Text = "Release Directory:NotSetYet";
                    else
                        lblReleaseRootDirectory.Text = "Release Directory:" + intendedReleaseDirectory.FullName;
                }
                else
                {
                    lblReleaseRootDirectory.Text = "Release Directory:";
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

        private void btnShowReleaseDirectory_Click(object sender, EventArgs e)
        {
            if (_releaseEngine != null)
            {
                DirectoryInfo d = _releaseEngine.GetIntendedReleaseDirectory();

                if (!d.Exists)
                    d.Create();

                Process.Start(d.FullName);
            }
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (ConfigurationsForRelease.Count == 0)
            {
                MessageBox.Show("Nothing yet selected for release");
                return;
            }

            CheckForCumulativeExtractionResults(ConfigurationsForRelease.SelectMany(c=>c.Value).ToArray());

            ReleaseEngine engine = new ReleaseEngine(Project);
            try
            {
                if(_releaseState== ReleaseState.Nothing)
                    throw new Exception("ReleaseState was Nothing... is this a patch or a proper release?");

                if(_releaseState== ReleaseState.DoingPatch)
                {

                    if(MessageBox.Show(
                        "CumulativeExtractionResults for datasets not included in the Patch will now be erased.","Erase redundant extraction results",MessageBoxButtons.OKCancel) 
                        == DialogResult.Cancel)
                        return;

                    int recordsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in ConfigurationsForRelease.Keys)
                    {
                        ExtractionConfiguration current = configuration;
                        var currentResults = configuration.CumulativeExtractionResults;

                        //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                        foreach (var redundantResult in currentResults.Where(r => ConfigurationsForRelease[current].All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                        {
                            redundantResult.DeleteInDatabase();
                            recordsDeleted++;
                        }
                    }

                    if(recordsDeleted != 0)
                        MessageBox.Show("Deleted " + recordsDeleted + " old CumulativeExtractionResults (That were not included in the final Patch you are preparing)");
                }

                engine.DoRelease(ConfigurationsForRelease, _environmentPotential, _releaseState == ReleaseState.DoingPatch);
                MessageBox.Show("Release Successful - Proceeding to Cleanup Dialog");

                CleanupConfirmationDialog cleanup = new CleanupConfirmationDialog(engine);
                cleanup.ShowDialog(this);

                ConfigurationsForRelease.Clear();
                ReloadTreeView();

                //fire release complete event
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(Project));
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);

                try
                {
                    int remnantsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in ConfigurationsForRelease.Keys)
                        foreach (ReleaseLogEntry remnant in configuration.ReleaseLogEntries)
                        {
                            remnant.DeleteInDatabase();
                            remnantsDeleted++;
                        }

                    if(remnantsDeleted > 0)
                        MessageBox.Show("Because release failed we are deleting ReleaseLogEntries, this resulted in " + remnantsDeleted + " deleted records, you will likely need to rextract these datasets or retrieve them from the Release directory");
                }
                catch (Exception e1)
                {
                    ExceptionViewer.Show("Error occurred when trying to clean up remnant ReleaseLogEntries",e1);
                }
            }
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
        }

        private enum ReleaseState
        {
            Nothing,
            DoingPatch,
            DoingProperRelease
        }

    }

    
}
