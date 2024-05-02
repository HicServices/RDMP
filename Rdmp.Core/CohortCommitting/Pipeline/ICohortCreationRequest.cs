// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Connections;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
///     See CohortCreationRequest
/// </summary>
public interface ICohortCreationRequest : ICheckable, IPipelineUseCase
{
    IProject Project { get; }
    ICohortDefinition NewCohortDefinition { get; set; }
    ExtractionInformation ExtractionIdentifierColumn { get; set; }
    CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }
    ExtractableCohort CohortCreatedIfAny { get; }
    FlatFileToLoad FileToLoad { get; set; }

    int ImportAsExtractableCohort(bool deprecateOldCohortOnSuccess, bool migrateUsages);
    void PushToServer(IManagedConnection transaction);
}