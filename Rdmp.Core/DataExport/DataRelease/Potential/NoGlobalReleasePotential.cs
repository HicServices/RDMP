// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
///     Release potential for global objects (e.g. <see cref="SupportingDocument" />) that have never been recorded as
///     extracted for a given extraction project
///     (i.e. no <see cref="ISupplementalExtractionResults" /> exists for them).
/// </summary>
public class NoGlobalReleasePotential : GlobalReleasePotential
{
    public NoGlobalReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
        : base(repositoryLocator, globalResult, globalToCheck)
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(new CheckEventArgs(
            $"{RelatedGlobal} is {Releaseability.NeverBeenSuccessfullyExecuted}", CheckResult.Fail));
        Releasability = Releaseability.NeverBeenSuccessfullyExecuted;
    }

    protected override void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult)
    {
        Releasability = Releaseability.NeverBeenSuccessfullyExecuted;
    }
}