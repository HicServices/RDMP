// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using FAnsi;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using Rdmp.Core.Databases;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.QueryCaching
{
    class QueryCachingCrossServerTests:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,typeof(QueryCachingPatcher))]
        //[TestCase(DatabaseType.MySql, typeof(QueryCachingPatcher))] (todo:not yet supported)
        //[TestCase(DatabaseType.Oracle, typeof(QueryCachingPatcher))] 

        [TestCase(DatabaseType.MicrosoftSQLServer, typeof(DataQualityEnginePatcher))]
        public void Create_QueryCache(DatabaseType dbType,Type patcherType)
        {
            var db = GetCleanedServer(dbType);

            var patcher = (Patcher)Activator.CreateInstance(patcherType);

            var mds = new MasterDatabaseScriptExecutor(db);
            mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        }
    }
}
