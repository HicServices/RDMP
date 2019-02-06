// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement.Operations;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace AnonymisationTests
{
    /// <summary>
    /// For any test that requires both the ANOStore and the IdentifierDump and anything else we come up with in terms of anonymisation
    /// </summary>
    public class TestsRequiringFullAnonymisationSuite : TestsRequiringANOStore
    {
        protected string IdentifierDump_DatabaseName = TestDatabaseNames.GetConsistentName("IdentifierDump");
        protected ExternalDatabaseServer IdentifierDump_ExternalDatabaseServer { get; set; }
        public DiscoveredDatabase IdentifierDump_Database { get; set; }

        [OneTimeSetUp]
        public void Setup_IdentifierDump()
        {
            IdentifierDump_Database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(IdentifierDump_DatabaseName);

            if (IdentifierDump_Database.Exists())
                IdentifierDump_Database.Drop();

            var scriptCreate = new MasterDatabaseScriptExecutor(IdentifierDump_Database);
            scriptCreate.CreateAndPatchDatabase(typeof(IdentifierDump.Class1).Assembly, new ThrowImmediatelyCheckNotifier());

            //now create a new reference!
            IdentifierDump_ExternalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository,IdentifierDump_DatabaseName);
            IdentifierDump_ExternalDatabaseServer.SetProperties(IdentifierDump_Database);

            new ServerDefaults(CatalogueRepository).SetDefault(ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID, IdentifierDump_ExternalDatabaseServer);

        }

        [OneTimeTearDown]
        public override void FixtureTearDown()
        {
            if (IdentifierDump_Database.Exists())
                IdentifierDump_Database.Drop();

            base.FixtureTearDown();
        }
    }
}