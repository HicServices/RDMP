using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.DataRelease;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using DataExportManager.ProjectUI.Graphs;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying.Commands;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;
using ReusableUIComponents.SqlDialogs;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractionConfigurationMenu:RDMPContextMenuStrip
    {
        private readonly ExtractionConfiguration _extractionConfiguration;
        private readonly DataExportChildProvider _childProvider;
        private IExtractableDataSet[] _datasets;

        private IExtractableDataSet[] _importableDataSets;

        public ExtractionConfigurationMenu(RDMPContextMenuStripArgs args, ExtractionConfiguration extractionConfiguration)
            : base( args,extractionConfiguration)
        {
            _extractionConfiguration = extractionConfiguration;
            _childProvider = (DataExportChildProvider) _activator.CoreChildProvider;
            
            var extractionResults =  _extractionConfiguration.CumulativeExtractionResults.ToArray();

            _datasets = _childProvider.GetDatasets(extractionConfiguration).Select(n => n.ExtractableDataSet).ToArray();
            _importableDataSets = _childProvider.ExtractableDataSets.Except(_datasets).ToArray();
            
            ///////////////////Change Cohorts//////////////
            string message = extractionConfiguration.Cohort_ID == null ? "Choose Cohort" : "Change Cohort";


            var cohortMenuItem = new ToolStripMenuItem(message, _activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Link), (s, e) => LinkCohortToExtractionConfiguration());
            cohortMenuItem.Enabled = !extractionConfiguration.IsReleased;
            Items.Add(cohortMenuItem);

            /////////////////Add Datasets/////////////
            var addDataSets = new ToolStripMenuItem("Add DataSet(s)", _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link), (s, e) => AddDatasetsToConfiguration());
            addDataSets.Enabled = !extractionConfiguration.IsReleased && _importableDataSets.Any();//not frozen and must be at least 1 dataset that is not in the configuration!
            Items.Add(addDataSets);

            if (_childProvider.AllPackages.Any())
            {
                var addPackageMenuItem = new ToolStripMenuItem("Add DataSet Package", _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage));
                foreach (ExtractableDataSetPackage package in _childProvider.AllPackages)
                {
                    ExtractableDataSetPackage package1 = package;
                    addPackageMenuItem.DropDownItems.Add(package.Name, null, (s,e)=>AddPackageToConfiguration(package1));
                }
                addPackageMenuItem.Enabled = !extractionConfiguration.IsReleased && _importableDataSets.Any();//not frozen and must be at least 1 dataset that is not in the configuration!
                Items.Add(addPackageMenuItem);
            }


            /////////////////Other stuff///////////
            var generateDoc = new ToolStripMenuItem("Generate Release Document", FamFamFamIcons.page_white_word,(s, e) => GenerateReleaseDocument());
            generateDoc.Enabled = _datasets.Any() && extractionResults.Any();
            Items.Add(generateDoc);

            Add(new ExecuteCommandExecuteExtractionConfiguration(_activator).SetTarget(_extractionConfiguration));


            var freeze = new ToolStripMenuItem("Freeze Extraction", CatalogueIcons.FrozenExtractionConfiguration,(s, e) => Freeze());
            freeze.Enabled = !extractionConfiguration.IsReleased && _datasets.Any();
            Items.Add(freeze);

            if (extractionConfiguration.IsReleased)
                Add(new ExecuteCommandUnfreezeExtractionConfiguration(_activator, extractionConfiguration));
            
            var clone = new ToolStripMenuItem("Clone Extraction", CatalogueIcons.CloneExtractionConfiguration,(s, e) => Clone());
            clone.Enabled = _datasets.Any();
            Items.Add(clone);

            Add(new ExecuteCommandRefreshExtractionConfigurationsCohort(_activator, extractionConfiguration));

            AddCommonMenuItems();
        }


        private void Clone()
        {
            try
            {
                var clone = _extractionConfiguration.DeepCloneWithNewIDs();
                Publish(clone);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

        }

        private void Freeze()
        {
            _extractionConfiguration.IsReleased = true;
            _extractionConfiguration.SaveToDatabase();
            Publish(_extractionConfiguration);
        }

        private void GenerateReleaseDocument()
        {
            try
            {
                WordDataReleaseFileGenerator generator = new WordDataReleaseFileGenerator(_extractionConfiguration, RepositoryLocator.DataExportRepository);
                
                //null means leave word file on screen and dont save
                generator.GenerateWordFile(null);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }

      
        private void LinkCohortToExtractionConfiguration()
        {
            List<ExtractableCohort> compatibleCohorts = new List<ExtractableCohort>();

            var project = _extractionConfiguration.Project;
            int? projectNumber = project.ProjectNumber;

            if (!projectNumber.HasValue)
            {

                WideMessageBox.Show("Extraction Configuration '" + _extractionConfiguration + "' belongs to Project '" +
                                    project +
                                    "' which does not have a ProjectNumber, you must have a ProjectNumber to associate a cohort");
                return;         
            }

            //find cohorts that match the project number
            if(_childProvider.ProjectNumberToCohortsDictionary.ContainsKey(projectNumber.Value))
                compatibleCohorts.AddRange(_childProvider.ProjectNumberToCohortsDictionary[projectNumber.Value]);
            
            //if theres only one compatible cohort and that one is already selected
            if(compatibleCohorts.Count == 1 && compatibleCohorts.Single().ID == _extractionConfiguration.Cohort_ID)
            {
                WideMessageBox.Show("The only cohort available is the one that is already currently selected.  Cohorts must have ProjectNumber "+ projectNumber.Value + " to be elligible");
                return;
            }

            //there weren't any
            if (!compatibleCohorts.Any())
            {
                WideMessageBox.Show("There are no cohorts currently configured with ProjectNumber " + projectNumber.Value + " (Project '" + project + "')");
                return;
            }

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(compatibleCohorts.Where(c => c.ID != _extractionConfiguration.Cohort_ID), false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //clear current one
                _extractionConfiguration.Cohort_ID = ((ExtractableCohort)dialog.Selected).ID;
                _extractionConfiguration.SaveToDatabase();
                Publish(_extractionConfiguration);
            }
        }

        private void AddDatasetsToConfiguration()
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_importableDataSets, false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
                new ExecuteCommandAddDatasetsToConfiguration(_activator, new ExtractableDataSetCommand(dialog.MultiSelected.Cast<ExtractableDataSet>().ToArray()),_extractionConfiguration).Execute();
        }

        private void AddPackageToConfiguration(ExtractableDataSetPackage package)
        {
            new ExecuteCommandAddDatasetsToConfiguration(_activator,new ExtractableDataSetCommand(package),_extractionConfiguration).Execute();
        }
    }
}
