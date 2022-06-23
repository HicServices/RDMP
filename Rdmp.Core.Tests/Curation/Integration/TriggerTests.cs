// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Exceptions;
using Tests.Common;
using System.Collections.Generic;
using TypeGuesser;
using System.Linq;
using System.Data;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class TriggerTests :DatabaseTests
    {
        private DiscoveredTable _table;
        private DiscoveredTable _archiveTable;
        private DiscoveredDatabase _database;
        

        public void CreateTable(DatabaseType dbType)
        {
            _database = GetCleanedServer(dbType);

            _table =_database.CreateTable("TriggerTests",new DatabaseColumnRequest[]{ 
                new DatabaseColumnRequest("name",new DatabaseTypeRequest(typeof(string),30)){AllowNulls = false },
                new DatabaseColumnRequest("bubbles",new DatabaseTypeRequest(typeof(int))),
                });

            _archiveTable = _database.ExpectTable("TriggerTests_Archive");
        }

        private ITriggerImplementer GetImplementer()
        {
            return new TriggerImplementerFactory(_database.Server.DatabaseType).Create(_table);
        }
        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void NoTriggerExists(DatabaseType dbType)
        {
            CreateTable(dbType);

            var implementer = GetImplementer();

            //most likely doesn't exist but may do
            implementer.DropTrigger(out string _, out string _);

            Assert.AreEqual(TriggerStatus.Missing, implementer.GetTriggerStatus());
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void CreateWithNoPks_Complain(DatabaseType dbType)
        {
            CreateTable(dbType);

            var ex = Assert.Throws<TriggerException>(() => GetImplementer().CreateTrigger(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("There must be at least 1 primary key", ex.Message);
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void CreateWithPks_Valid(DatabaseType dbType)
        {
            CreateTable(dbType);

            _table.CreatePrimaryKey(new []{_table.DiscoverColumn("name")});
            GetImplementer().CreateTrigger(new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(TriggerStatus.Enabled, GetImplementer().GetTriggerStatus());
            Assert.AreEqual(true, GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody());
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void Create_WithDodgyColumnNames(DatabaseType dbType)
        {
            _database = GetCleanedServer(dbType);

            _table =_database.CreateTable("Trol lol My Table Select * from Group by fish",new DatabaseColumnRequest[]{ 
                new DatabaseColumnRequest("My Lovely Column Select * From Lolz",new DatabaseTypeRequest(typeof(string),30)){AllowNulls = false,IsPrimaryKey = true},
                new DatabaseColumnRequest("ANormalColumnName",new DatabaseTypeRequest(typeof(int))),
                new DatabaseColumnRequest("Group By Meeee Colll trollolol",new DatabaseTypeRequest(typeof(int))),
            });

            GetImplementer().CreateTrigger(new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(TriggerStatus.Enabled, GetImplementer().GetTriggerStatus());
            Assert.AreEqual(true, GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody());
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void AlterTest_InvalidThenRecreateItAndItsValidAgain(DatabaseType dbType)
        {
            CreateWithPks_Valid(dbType);

            _table.AddColumn("fish",new DatabaseTypeRequest(typeof(int)),true,500);
            _archiveTable.AddColumn("fish",new DatabaseTypeRequest(typeof(int)),true,500);
            
            //still not valid because trigger SQL is missing it in the column list
            var ex = Assert.Throws<ExpectedIdenticalStringsException>(() => GetImplementer().CheckUpdateTriggerIsEnabledAndHasExpectedBody());
            Assert.IsNotNull(ex.Message);

            var implementer = GetImplementer();
            implementer.DropTrigger(out var problemsDroppingTrigger, out _);
            Assert.IsEmpty(problemsDroppingTrigger);

            implementer.CreateTrigger(new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(true, implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody());
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void NowTestDataInsertion(DatabaseType dbType)
        {
            AlterTest_InvalidThenRecreateItAndItsValidAgain(dbType);
            
            _table.Insert(new Dictionary<string, object>
            { 
                {"name","Franky" } ,
                {"bubbles",3 } ,
                {"hic_validFrom",new DateTime(2001,1,2)} ,
                {"hic_dataLoadRunID",7 } 
            });

            var liveOldRow = _table.GetDataTable().Rows.Cast<DataRow>().Single(r=>r["bubbles"] as int? ==3);
            Assert.AreEqual(new DateTime(2001,1,2),((DateTime)liveOldRow[SpecialFieldNames.ValidFrom]));

            RunSQL("UPDATE {0} set bubbles =99",_table.GetFullyQualifiedName());

            //new value is 99
            Assert.AreEqual(99,ExecuteScalar("Select bubbles FROM {0} where name = 'Franky'",_table.GetFullyQualifiedName()));
            //archived value is 3
            Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM {0} where name = 'Franky'", _archiveTable.GetFullyQualifiedName()));

            //Legacy table valued function only works for MicrosoftSQLServer
            if(dbType == DatabaseType.MicrosoftSQLServer)
            {
                //legacy in 2001-01-01 it didn't exist
                Assert.IsNull( ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-01') where name = 'Franky'"));
                //legacy in 2001-01-03 it did exist and was 3
                Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-03') where name = 'Franky'"));
                //legacy boundary case?
                Assert.AreEqual(3, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy('2001-01-02') where name = 'Franky'"));
            
                //legacy today it is 99
                Assert.AreEqual(99, ExecuteScalar("Select bubbles FROM TriggerTests_Legacy(GETDATE()) where name = 'Franky'"));
            }
            
            // Live row should now reflect that it is validFrom today
            var liveNewRow = _table.GetDataTable().Rows.Cast<DataRow>().Single(r=>r["bubbles"] as int? ==99);
            Assert.AreEqual(DateTime.Now.Date,((DateTime)liveNewRow[SpecialFieldNames.ValidFrom]).Date);

            // Archived row should not have had its validFrom field broken
            var archivedRow = _archiveTable.GetDataTable().Rows.Cast<DataRow>().Single(r=>r["bubbles"] as int? ==3);
            Assert.AreEqual(new DateTime(2001,1,2),((DateTime)archivedRow[SpecialFieldNames.ValidFrom]));
        }

        [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
        public void DiffDatabaseDataFetcherTest(DatabaseType dbType)
        {
            CreateTable(dbType);
            
            _table.CreatePrimaryKey(_table.DiscoverColumn("name"));
            
            GetImplementer().CreateTrigger(new ThrowImmediatelyCheckNotifier());
            
            _table.Insert(new Dictionary<string, object>
            { 
                {"name","Franky" } ,
                {"bubbles",3 } ,
                {"hic_validFrom",new DateTime(2001,1,2)} ,
                {"hic_dataLoadRunID",7 } 
            });
            
            Thread.Sleep(1000);
            RunSQL("UPDATE {0} SET bubbles=1",_table.GetFullyQualifiedName());
            
            Thread.Sleep(1000);
            RunSQL("UPDATE {0} SET bubbles=2",_table.GetFullyQualifiedName());
            
            Thread.Sleep(1000);
            RunSQL("UPDATE {0} SET bubbles=3",_table.GetFullyQualifiedName());
            
            Thread.Sleep(1000);
            RunSQL("UPDATE {0} SET bubbles=4",_table.GetFullyQualifiedName());

            Thread.Sleep(1000);

            Assert.AreEqual(1,_table.GetRowCount());
            Assert.AreEqual(4,_archiveTable.GetRowCount());

            Import(_table,out var ti,out var cols);
            DiffDatabaseDataFetcher fetcher = new DiffDatabaseDataFetcher(1,ti,7,100);
            
            fetcher.FetchData(new AcceptAllCheckNotifier());
            Assert.AreEqual(4,fetcher.Updates_New.Rows[0]["bubbles"]);
            Assert.AreEqual(3, fetcher.Updates_Replaced.Rows[0]["bubbles"]);

            Assert.AreEqual(1,fetcher.Updates_New.Rows.Count);
            Assert.AreEqual(1, fetcher.Updates_Replaced.Rows.Count);
        }


        [Test]
        public void IdentityTest()
        {
            CreateTable(DatabaseType.MicrosoftSQLServer);
            
            RunSQL("Alter TABLE TriggerTests ADD myident int identity(1,1) PRIMARY KEY");

            var implementer = new MicrosoftSQLTriggerImplementer(_table);

            implementer.CreateTrigger(new ThrowImmediatelyCheckNotifier());
            implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody();
        }

        private object ExecuteScalar(string sql, params string[] args)
        {
            if(args.Length != 0)
                sql = string.Format(sql,args);

            var svr = _database.Server;
            using (var con = svr.GetConnection())
            {
                con.Open();
                return svr.GetCommand(sql, con).ExecuteScalar();
            }
        }

        private void RunSQL(string sql, params string[] args)
        {
            if(args.Length != 0)
                sql = string.Format(sql,args);
            if (_database == null)
                throw new Exception("You must call CreateTable first");

            using (var con = _database.Server.GetConnection())
            {
                con.Open();
                _database.Server.GetCommand(sql, con).ExecuteNonQuery();
            }
        }
    }
}
