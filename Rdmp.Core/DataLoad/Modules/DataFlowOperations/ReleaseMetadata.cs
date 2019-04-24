// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Sharing.CommandExecution;
using Sharing.Dependency.Gathering;

namespace LoadModules.Generic.DataFlowOperations
{
    /// <summary>
    /// Data release pipeline component which generates <see cref="CatalogueLibrary.Data.Serialization.ShareDefinition"/> files for all <see cref="Catalogue"/> being
    /// extracted.  These contain the dataset and column descriptions in a format that can be loaded by any remote RDMP instance via ExecuteCommandImportShareDefinitionList.
    /// 
    /// <para>This serialization includes the allocation of SharingUIDs to allow for later updates and to prevent duplicate loading in the destination.  In addition it handles
    /// system boundaries e.g. it doesn't serialize <see cref="CatalogueLibrary.Data.DataLoad.LoadMetadata"/> of the <see cref="Catalogue"/> or other deployment specific objects</para>
    /// </summary>
    public class ReleaseMetadata : IPluginDataFlowComponent<ReleaseAudit>, IPipelineRequirement<ReleaseData>
    {
        private ReleaseData _releaseData;

        public ReleaseAudit ProcessPipelineData(ReleaseAudit toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var allCatalogues = 
                _releaseData.SelectedDatasets.Values.SelectMany(sd => sd.ToList())
                .Select(sds => sds.ExtractableDataSet.Catalogue)
                .Distinct()
                .Cast<IMapsDirectlyToDatabaseTable>()
                .ToArray();
            
            if(!allCatalogues.Any())
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "No Catalogues are selected for release"));
                return toProcess;
            }

            var sourceFolder = _releaseData.ConfigurationsForRelease.First().Value.First().ExtractDirectory.Parent;
            if (sourceFolder == null)
                throw new Exception("Could not find Source Folder. DOes the project have an Extraction Directory defined?");

            var outputFolder = sourceFolder.CreateSubdirectory(ExtractionDirectory.METADATA_FOLDER_NAME);
            
            var cmd = new ExecuteCommandExportObjectsToFile(_releaseData.RepositoryLocator, allCatalogues, outputFolder);
            cmd.Execute();
            
            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            _releaseData = value;
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("No checking needed", CheckResult.Success));
        }
    }
}
