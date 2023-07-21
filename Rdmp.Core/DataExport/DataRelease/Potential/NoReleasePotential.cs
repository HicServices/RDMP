// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
///     Release Potential class to be used when nothing has ever been extracted
/// </summary>
public class NoReleasePotential : ReleasePotential
{
    public NoReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISelectedDataSets selectedDataSet) : base(repositoryLocator, selectedDataSet)
    {
    }

    protected override Releaseability GetSupplementalSpecificAssessment(
        IExtractionResults supplementalExtractionResults)
    {
        return Releaseability.NeverBeenSuccessfullyExecuted;
    }

    protected override Releaseability GetSpecificAssessment(IExtractionResults extractionResults)
    {
        return Releaseability.NeverBeenSuccessfullyExecuted;
    }

    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        notifier.OnCheckPerformed(new CheckEventArgs(
            $"{SelectedDataSet} is {Releaseability.NeverBeenSuccessfullyExecuted}", CheckResult.Fail));
    }
}