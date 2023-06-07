// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataExport.Cohort;

class ProjectConsistentGuidReleaseIdentifierAllocatorTests:DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestPreserveHistoricalReleaseIdentifiers(DatabaseType databaseType)
    {
        var db = GetCleanedServer(databaseType);
            
        var privateIdentifierDataType = db.Server.GetQuerySyntaxHelper().TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof(string),10));

        var wizard = new CreateNewCohortDatabaseWizard(db,CatalogueRepository,DataExportRepository,false);
        var ect = wizard.CreateDatabase(new PrivateIdentifierPrototype("chi", privateIdentifierDataType),new AcceptAllCheckNotifier());
            
        var defTable = ect.DiscoverDefinitionTable();
        var cohortTable = ect.DiscoverCohortTable();

        var p = new Project(DataExportRepository,"MyProject");
        p.ProjectNumber = 10;
        p.SaveToDatabase();

        var req = new CohortCreationRequest(p,new CohortDefinition(null,"TestCohort1",1,p.ProjectNumber.Value,ect),DataExportRepository,"Ignoreme");

        var allocator = new ProjectConsistentGuidReleaseIdentifierAllocator();
        allocator.Initialize(req);

        //allocator is being asked to allocate when there are no cohorts at all defined
        Assert.AreEqual(0, defTable.GetRowCount());
        Assert.IsNotNull(allocator.AllocateReleaseIdentifier("0101010101"));
            
        //Now let's define a cohort identifier for someone (0202020202) who is not in our project
        defTable.Insert(new Dictionary<string, object>
        {
            {"projectNumber", 11}, //project is not our project
            {"version", 1},
            {"description","flibble"}
        });

        Assert.AreEqual(1,defTable.GetRowCount());

        cohortTable.Insert(new Dictionary<string, object>
        {
            {ect.GetQuerySyntaxHelper().GetRuntimeName(ect.DefinitionTableForeignKeyField), 1},
            {"chi", "0202020202"},
            {"ReleaseId", "0x0123"}
        });
            
        //recreate allocator to clear map
        allocator = new ProjectConsistentGuidReleaseIdentifierAllocator();
        allocator.Initialize(req);

        //allocator is being asked to allocate when there are cohorts defined including one with our person 02020202 but that person was in a different project
        Assert.AreEqual(1, defTable.GetRowCount());
        Assert.AreEqual(1,cohortTable.GetRowCount());
        Assert.IsNotNull(allocator.AllocateReleaseIdentifier("0202020202"));
        Assert.AreNotEqual("0x0123",allocator.AllocateReleaseIdentifier("0202020202"));


        //Now let's define a cohort identifier for someone (0202020202) who IS in our project
        defTable.Insert(new Dictionary<string, object>
        {
            {"projectNumber", 10}, //this is our project number!
            {"version", 1},
            {"description","flibble"}
        });

        Assert.AreEqual(2, defTable.GetRowCount());

        cohortTable.Insert(new Dictionary<string, object>
        {
            {ect.GetQuerySyntaxHelper().GetRuntimeName(ect.DefinitionTableForeignKeyField), 2},
            {"chi", "0202020202"},
            {"ReleaseId", "0x0127"}
        });

        //recreate allocator to clear map
        allocator = new ProjectConsistentGuidReleaseIdentifierAllocator();
        allocator.Initialize(req);

        //allocator is being asked to allocate when the person 0202020202 has previously appeared under our project (10)
        Assert.AreEqual(2, defTable.GetRowCount());
        Assert.AreEqual(2, cohortTable.GetRowCount());
        Assert.IsNotNull(allocator.AllocateReleaseIdentifier("0202020202"));
        Assert.AreEqual("0x0127", allocator.AllocateReleaseIdentifier("0202020202"));


        //finally let's break it by giving it conflicting historical records
        //let's pretend that previously we had already got 2 historical batches for the project, batch 1 released 0202020202 as 0x0127 (see above) and batch 2 released 0202020202 as 0x0128
        defTable.Insert(new Dictionary<string, object>
        {
            {"projectNumber", 10}, //this is our project number!
            {"version", 2},
            {"description","flibble"}
        });

        Assert.AreEqual(3, defTable.GetRowCount());

        cohortTable.Insert(new Dictionary<string, object>
        {
            {ect.GetQuerySyntaxHelper().GetRuntimeName(ect.DefinitionTableForeignKeyField), 3},
            {"chi", "0202020202"},
            {"ReleaseId", "0x0128"}
        });

        //recreate allocator to clear map
        allocator = new ProjectConsistentGuidReleaseIdentifierAllocator();
        allocator.Initialize(req);

        //allocator is being asked to allocate when the person 0202020202 has previously appeared under our project (10) as release identifiers 0x0127 and 0x0128
        Assert.AreEqual(3, defTable.GetRowCount());
        Assert.AreEqual(3, cohortTable.GetRowCount());

        var ex = Assert.Throws<Exception>(() => allocator.AllocateReleaseIdentifier("0202020202"));

        //should be complaining about both of these conflicting release identifiers existing
        StringAssert.Contains("0x0127",ex.Message);
        StringAssert.Contains("0x0128", ex.Message);

        //fix the problem
        using (var con = db.Server.GetConnection())
        {
            con.Open();
            db.Server.GetCommand($"UPDATE {cohortTable} SET ReleaseId='0x0127' WHERE ReleaseId='0x0128'", con).ExecuteScalar();
        }

        //should be happy now again
        Assert.AreEqual(3, defTable.GetRowCount());
        Assert.AreEqual(3, cohortTable.GetRowCount());
        Assert.IsNotNull(allocator.AllocateReleaseIdentifier("0202020202"));
        Assert.AreEqual("0x0127", allocator.AllocateReleaseIdentifier("0202020202"));

    }

}