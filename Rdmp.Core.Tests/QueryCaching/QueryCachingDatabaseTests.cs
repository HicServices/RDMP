// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.QueryCaching
{
    public class QueryCachingDatabaseTests:DatabaseTests
    {
        protected string QueryCachingDatabaseName = global::Tests.Common.TestDatabaseNames.GetConsistentName("QueryCaching");
        public DiscoveredDatabase DiscoveredQueryCachingDatabase { get; set; }
        public ExternalDatabaseServer QueryCachingDatabaseServer;

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            DiscoveredQueryCachingDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(QueryCachingDatabaseName);

            if(DiscoveredQueryCachingDatabase.Exists())
                DiscoveredQueryCachingDatabase.Drop();

            MasterDatabaseScriptExecutor scripter = new MasterDatabaseScriptExecutor(DiscoveredQueryCachingDatabase);
            var p = new QueryCachingPatcher();
            scripter.CreateAndPatchDatabase(p, new ThrowImmediatelyCheckNotifier());

            QueryCachingDatabaseServer = new ExternalDatabaseServer(CatalogueRepository,QueryCachingDatabaseName,p);
            QueryCachingDatabaseServer.SetProperties(DiscoveredQueryCachingDatabase);
        }

    }
}
