// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddDatasetsToConfiguration : BasicUICommandExecution
    {
        private readonly ExtractionConfiguration _targetExtractionConfiguration;

        private IExtractableDataSet[] _toadd;

        public ExecuteCommandAddDatasetsToConfiguration(IActivateItems activator,ExtractableDataSetCommand sourceExtractableDataSetCommand, ExtractionConfiguration targetExtractionConfiguration) 
            : this(activator,targetExtractionConfiguration)
        {
            SetExtractableDataSets(sourceExtractableDataSetCommand.ExtractableDataSets);
            
        }

        public ExecuteCommandAddDatasetsToConfiguration(IActivateItems itemActivator, ExtractableDataSet extractableDataSet, ExtractionConfiguration targetExtractionConfiguration)
            : this(itemActivator,targetExtractionConfiguration)
        {
            SetExtractableDataSets(extractableDataSet);
        }

        private ExecuteCommandAddDatasetsToConfiguration(IActivateItems itemActivator, ExtractionConfiguration targetExtractionConfiguration) : base(itemActivator)
        {
            _targetExtractionConfiguration = targetExtractionConfiguration;

            if (_targetExtractionConfiguration.IsReleased)
                SetImpossible("Extraction is Frozen because it has been released and is readonly, try cloning it instead");
        }

        private void SetExtractableDataSets(params IExtractableDataSet[] toAdd)
        {
            var alreadyInConfiguration = _targetExtractionConfiguration.GetAllExtractableDataSets().ToArray();
            _toadd = toAdd.Except(alreadyInConfiguration).ToArray();

            if(!_toadd.Any())
                SetImpossible("ExtractionConfiguration already contains this dataset(s)");
        }

        public override void Execute()
        {
            base.Execute();

            foreach (var ds in _toadd)
                _targetExtractionConfiguration.AddDatasetToConfiguration(ds);

            Publish(_targetExtractionConfiguration);
        }
    }
}