// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Databases;
using ReusableLibraryCode.Checks;
using System;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Tests.Common;

namespace Rdmp.Core.Tests.Databases;

class MasterDatabaseScriptExecutorTests : DatabaseTests
{
    [Test]
    public void TestCreatingSchemaTwice()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var mds = new MasterDatabaseScriptExecutor(db);
        //setup as DQE
        mds.CreateAndPatchDatabase(new DataQualityEnginePatcher(), new AcceptAllCheckNotifier());

        //now try to setup same db as Logging
        var ex = Assert.Throws<Exception>(()=>mds.CreateAndPatchDatabase(new LoggingDatabasePatcher(), new AcceptAllCheckNotifier()));

        StringAssert.Contains("is already set up as a platform database for another schema (it has the 'ScriptsRun' table)", ex.InnerException.Message);

    }
}