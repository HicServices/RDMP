// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
///     Determines the releasability of global objects (e.g. <see cref="SupportingDocument" />) that should have been
///     extracted as
///     part of a project extraction.  For <see cref="SupportingSQLTable" /> it will confirm that the table exists in the
///     database
/// </summary>
public class MsSqlGlobalsReleasePotential : GlobalReleasePotential
{
    public MsSqlGlobalsReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
        : base(repositoryLocator, globalResult, globalToCheck)
    {
    }

    protected override void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult)
    {
        var externalServerId = int.Parse(globalResult.DestinationDescription.Split('|')[0]);
        var externalServer =
            RepositoryLocator.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
        var dbName = globalResult.DestinationDescription.Split('|')[1];
        var tblName = globalResult.DestinationDescription.Split('|')[2];
        var server = DataAccessPortal.ExpectServer(externalServer, DataAccessContext.DataExport, false);
        var database = server.ExpectDatabase(dbName);
        if (!database.Exists())
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Database: {database} was not found", CheckResult.Fail));
            Releasability = Releaseability.ExtractFilesMissing;
        }

        var foundTable = database.ExpectTable(tblName);
        if (!foundTable.Exists())
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Table: {foundTable} was not found", CheckResult.Fail));
            Releasability = Releaseability.ExtractFilesMissing;
        }
    }
}