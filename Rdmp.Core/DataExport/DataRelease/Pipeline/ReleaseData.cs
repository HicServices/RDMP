// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     Collection passed down the Release Pipeline.  Release is the process of taking extracted files for a Project,
///     bundling them together into a release
///     structure and sending that artifact to a release directory.  The Releasability of each dataset in the extraction is
///     checked prior to release to confirm
///     that the extracted files match the current system configuration and that all expected files are there (See
///     ReleasePotential).  In addition the ticketing
///     system (if any) is consulted to confirm that it is happy for the collection to be released (See
///     EnvironmentPotential)
/// </summary>
public class ReleaseData
{
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
    public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; set; }
    public Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> EnvironmentPotentials { get; set; }
    public Dictionary<IExtractionConfiguration, IEnumerable<ISelectedDataSets>> SelectedDatasets { get; set; }
    public bool ReleaseGlobals { get; set; }

    public ReleaseState ReleaseState { get; set; }

    public ReleaseData(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        RepositoryLocator = repositoryLocator;
        ConfigurationsForRelease = new Dictionary<IExtractionConfiguration, List<ReleasePotential>>();
        EnvironmentPotentials = new Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential>();
        SelectedDatasets = new Dictionary<IExtractionConfiguration, IEnumerable<ISelectedDataSets>>();
    }
}