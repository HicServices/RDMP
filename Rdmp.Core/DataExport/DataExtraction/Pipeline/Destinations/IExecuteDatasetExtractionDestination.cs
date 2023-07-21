// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;

/// <summary>
///     Destination for Extraction Pipelines.  Saves the extracted (anonymous) data contained in the DataTables received
///     (which arrive in batches) to some location
///     (depending on implementation).  Destinations must also support one off calls (per ExtractionConfiguration) to
///     ExtractGlobals
/// </summary>
public interface IExecuteDatasetExtractionDestination : IPluginDataFlowComponent<DataTable>,
    IDataFlowDestination<DataTable>, IPipelineRequirement<IExtractCommand>, IPipelineRequirement<DataLoadInfo>
{
    TableLoadInfo TableLoadInfo { get; }
    DirectoryInfo DirectoryPopulated { get; }
    bool GeneratesFiles { get; }
    string OutputFile { get; }
    int SeparatorsStrippedOut { get; }
    string DateFormat { get; }

    /// <summary>
    ///     Returns a string suitable for naming the extracted artifact e.g. "Biochemistry", or "BIO".  Should not contain a
    ///     file extension.
    /// </summary>
    /// <returns></returns>
    string GetFilename();


    /// <summary>
    ///     Provide a short description of where the <see cref="ExtractionDestination" /> puts rows e.g. a file path for a csv
    /// </summary>
    /// <returns></returns>
    string GetDestinationDescription();

    /// <summary>
    ///     Returns an assessment of how complete the <paramref name="selectedDataSet" /> extraction process is (e.g. does the
    ///     current configuration
    ///     match the live system, does the extracted file exist on disk / in a database)
    /// </summary>
    /// <param name="repositoryLocator"></param>
    /// <param name="selectedDataSet"></param>
    /// <returns></returns>
    ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSet);


    /// <summary>
    ///     Factory method, returns a source component (for a release pipeline) which is capable of detecting and packaging up
    ///     the artifacts created
    ///     by this <see cref="IExecuteDatasetExtractionDestination" /> (destination component for the extraction pipeline)
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <returns></returns>
    FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository);

    /// <summary>
    ///     Returns an assessment of how complete the <paramref name="globalResult" /> extraction process is (e.g. does the
    ///     extracted file exist on disk /
    ///     in a database)
    /// </summary>
    /// <param name="repositoryLocator"></param>
    /// <param name="globalResult"></param>
    /// <param name="globalToCheck"></param>
    /// <returns></returns>
    GlobalReleasePotential GetGlobalReleasabilityEvaluator(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck);
}