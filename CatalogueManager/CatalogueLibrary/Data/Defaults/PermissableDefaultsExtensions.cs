// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.Defaults
{
    public static class PermissableDefaultsExtensions
    {
        /// <summary>
        /// Translates the given <see cref="PermissableDefaults"/> (a default that can be set) to a <see cref="Tier2DatabaseType"/> (identifies what type of database it is).
        /// </summary>
        /// <param name="permissableDefault"></param>
        /// <returns></returns>
        public static Tier2DatabaseType? ToTier2DatabaseType(this PermissableDefaults permissableDefault)
        {
            switch (permissableDefault)
            {
                case PermissableDefaults.LiveLoggingServer_ID:
                    return Tier2DatabaseType.Logging;
                case PermissableDefaults.IdentifierDumpServer_ID:
                    return Tier2DatabaseType.IdentifierDump;
                case PermissableDefaults.DQE:
                    return Tier2DatabaseType.DataQuality;
                case PermissableDefaults.WebServiceQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.RAWDataLoadServer:
                    return null;
                case PermissableDefaults.ANOStore:
                    return Tier2DatabaseType.ANOStore;
                default:
                    throw new ArgumentOutOfRangeException("permissableDefault");
            }
        }
    }
}