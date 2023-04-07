// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Databases;

namespace Rdmp.Core.Curation.Data.Defaults;

public static class PermissableDefaultsExtensions
{
    /// <summary>
    /// Translates the given <see cref="PermissableDefaults"/> (a default that can be set) to the <see cref="IPatcher"/> which
    /// handles the creation/patching of database schema (identifies what type of database it is).
    /// </summary>
    /// <param name="permissableDefault"></param>
    /// <returns></returns>
    public static IPatcher ToTier2DatabaseType(this PermissableDefaults permissableDefault)
    {
        switch (permissableDefault)
        {
            case PermissableDefaults.LiveLoggingServer_ID:
                return new LoggingDatabasePatcher();
            case PermissableDefaults.IdentifierDumpServer_ID:
                return new IdentifierDumpDatabasePatcher();
            case PermissableDefaults.DQE:
                return new DataQualityEnginePatcher();
            case PermissableDefaults.WebServiceQueryCachingServer_ID:
                return new QueryCachingPatcher();
            case PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                return new QueryCachingPatcher();
            case PermissableDefaults.RAWDataLoadServer:
                return null;
            case PermissableDefaults.ANOStore:
                return new ANOStorePatcher();
            default:
                throw new ArgumentOutOfRangeException("permissableDefault");
        }
    }
}