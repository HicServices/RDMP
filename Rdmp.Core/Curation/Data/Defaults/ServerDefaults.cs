// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.Curation.Data.Defaults;

public static class ServerDefaults
{
    /// <summary>
    ///     The value that will actually be stored in the ServerDefaults table as a dictionary (see constructor for population
    /// </summary>
    public static readonly Dictionary<PermissableDefaults, string> StringExpansionDictionary = new();

    static ServerDefaults()
    {
        StringExpansionDictionary.Add(PermissableDefaults.LiveLoggingServer_ID, "Catalogue.LiveLoggingServer_ID");
        StringExpansionDictionary.Add(PermissableDefaults.IdentifierDumpServer_ID, "TableInfo.IdentifierDumpServer_ID");
        StringExpansionDictionary.Add(PermissableDefaults.CohortIdentificationQueryCachingServer_ID,
            "CIC.QueryCachingServer_ID");
        StringExpansionDictionary.Add(PermissableDefaults.ANOStore, "ANOTable.Server_ID");

        StringExpansionDictionary.Add(PermissableDefaults.WebServiceQueryCachingServer_ID, "WebServiceQueryCache");

        //this doesn't actually map to a field in the database, it is a bit of an abuse fo the defaults system
        StringExpansionDictionary.Add(PermissableDefaults.DQE, "DQE");
        StringExpansionDictionary.Add(PermissableDefaults.RAWDataLoadServer, "RAWDataLoadServer");
    }
}