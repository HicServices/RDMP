// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;

namespace Rdmp.Core.Curation.Data.Defaults;

public static class PermissableDefaultsExtensions
{
    /// <summary>
    ///     Translates the given <see cref="PermissableDefaults" /> (a default that can be set) to the <see cref="IPatcher" />
    ///     which
    ///     handles the creation/patching of database schema (identifies what type of database it is).
    /// </summary>
    /// <param name="permissibleDefault"></param>
    /// <returns></returns>
    public static IPatcher ToTier2DatabaseType(this PermissableDefaults permissibleDefault)
    {
        return permissibleDefault switch
        {
            PermissableDefaults.LiveLoggingServer_ID => new LoggingDatabasePatcher(),
            PermissableDefaults.IdentifierDumpServer_ID => new IdentifierDumpDatabasePatcher(),
            PermissableDefaults.DQE => new DataQualityEnginePatcher(),
            PermissableDefaults.WebServiceQueryCachingServer_ID => new QueryCachingPatcher(),
            PermissableDefaults.CohortIdentificationQueryCachingServer_ID => new QueryCachingPatcher(),
            PermissableDefaults.RAWDataLoadServer => null,
            PermissableDefaults.ANOStore => new ANOStorePatcher(),
            _ => throw new ArgumentOutOfRangeException(nameof(permissibleDefault))
        };
    }
}