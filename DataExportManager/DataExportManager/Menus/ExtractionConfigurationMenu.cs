// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using MapsDirectlyToDatabaseTableUI;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractionConfigurationMenu:RDMPContextMenuStrip
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
            
            _datasets = _childProvider.GetDatasets(extractionConfiguration).Select(n => n.ExtractableDataSet).ToArray();

            Items.Add("Edit", null, (s, e) => _activator.Activate<ExtractionConfigurationUI, ExtractionConfiguration>(extractionConfiguration));

            _importableDataSets = _childProvider.ExtractableDataSets.Except(_datasets).Where(ds=>ds.Project_ID == null || ds.Project_ID == extractionConfiguration.Project_ID).ToArray();
            
            ///////////////////Change Cohorts//////////////
            
            Add(new ExecuteCommandRelease(_activator).SetTarget(extractionConfiguration));

            Add(new ExecuteCommandChooseCohort(_activator, extractionConfiguration));
            
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

            Add(new ExecuteCommandGenerateReleaseDocument(_activator, extractionConfiguration));
            
            var freeze = new ToolStripMenuItem("Freeze Extraction", CatalogueIcons.FrozenExtractionConfiguration,(s, e) => Freeze());
            freeze.Enabled = !extractionConfiguration.IsReleased && _datasets.Any();
            Items.Add(freeze);

            if (extractionConfiguration.IsReleased)
                Add(new ExecuteCommandUnfreezeExtractionConfiguration(_activator, extractionConfiguration));

            Add(new ExecuteCommandCloneExtractionConfiguration(_activator, extractionConfiguration));

            Add(new ExecuteCommandRefreshExtractionConfigurationsCohort(_activator, extractionConfiguration));

            ReBrandActivateAs("Extract...", RDMPConcept.ExtractionConfiguration, OverlayKind.Execute);
        }


        private void Freeze()
        {
            _extractionConfiguration.IsReleased = true;
            _extractionConfiguration.SaveToDatabase();
            Publish(_extractionConfiguration);
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
