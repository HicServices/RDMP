// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

public abstract class GlobalReleasePotential : ICheckable
{
    protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
    protected readonly ISupplementalExtractionResults GlobalResult;

    public IMapsDirectlyToDatabaseTable RelatedGlobal { get; protected set; }
    public Releaseability Releasability { get; protected set; }

    protected GlobalReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
    {
        RepositoryLocator = repositoryLocator;
        GlobalResult = globalResult;
        RelatedGlobal = globalToCheck;
    }

    public virtual void Check(ICheckNotifier notifier)
    {
        if (GlobalResult == null) // this should be already covered by the NoGlobalPotential
            return;
            
        if (GlobalResult.SQLExecuted != null)
        {
            var table = RelatedGlobal as SupportingSQLTable;
            if (table == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"The executed Global {GlobalResult.ExtractedName} has a SQL script but the extracted type ({GlobalResult.ReferencedObjectType}) is not a SupportingSQLTable", CheckResult.Fail));
                Releasability = Releaseability.ExtractionSQLDesynchronisation;
            }
            if (table.SQL != GlobalResult.SQLExecuted)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"The executed Global {GlobalResult.ExtractedName} has a SQL script which is different from the current one!", CheckResult.Fail));
                Releasability = Releaseability.ExtractionSQLDesynchronisation;
            }
        }

        if (GlobalResult.IsReferenceTo(typeof (SupportingDocument)))
            CheckFileExists(notifier, GlobalResult.DestinationDescription);
        else
            CheckDestination(notifier, GlobalResult);

        if (Releasability == Releaseability.Undefined)
            Releasability = Releaseability.Releaseable;
    }

    protected void CheckFileExists(ICheckNotifier notifier, string filePath)
    {
        var file = new FileInfo(filePath);
        if (!file.Exists)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"File: {file.FullName} was not found", CheckResult.Fail));
            Releasability = Releaseability.ExtractFilesMissing;
        }
    }

    protected abstract void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult);
}