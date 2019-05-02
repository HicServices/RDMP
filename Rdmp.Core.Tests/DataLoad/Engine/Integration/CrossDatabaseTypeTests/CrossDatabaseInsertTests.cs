// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.TypeTranslation;
using NUnit.Framework;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseInsertTests : DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,"Dave")]
        [TestCase(DatabaseType.MySql,"Dave")]
        [TestCase(DatabaseType.Oracle, "Dave")]
        
        [TestCase(DatabaseType.MicrosoftSQLServer, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.MySql, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.Oracle, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.MySql, "Dave")]
        [TestCase(DatabaseType.Oracle, "Dave")]
        [TestCase(DatabaseType.MicrosoftSQLServer, 1.5)]
        [TestCase(DatabaseType.MySql, 1.5)]
        [TestCase(DatabaseType.Oracle, 1.5)]
        public void CreateTableAndInsertAValue_ColumnOverload(DatabaseType type, object value)
        {
            var db = GetCleanedServer(type,true);
            var tbl = db.CreateTable("InsertTable",
                new []
                {
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(value.GetType(),100,new DecimalSize(5,5)))
                });
            
            var nameCol = tbl.DiscoverColumn("Name");

            tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,value}
            });

            var result = tbl.GetDataTable();
            Assert.AreEqual(1,result.Rows.Count);
            Assert.AreEqual(value,result.Rows[0][0]);

            tbl.Drop();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer, 1.5)]
        [TestCase(DatabaseType.MySql, 1.5)]
        [TestCase(DatabaseType.Oracle, 1.5)]
        public void CreateTableAndInsertAValue_StringOverload(DatabaseType type, object value)
        {
            var db = GetCleanedServer(type,true);
            var tbl = db.CreateTable("InsertTable",
                new[]
                {
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(value.GetType(),100,new DecimalSize(5,5)))
                });

            tbl.Insert(new Dictionary<string, object>()
            {
                {"Name",value}
            });

            var result = tbl.GetDataTable();
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(value, result.Rows[0][0]);

            tbl.Drop();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.Oracle)]
        public void CreateTableAndInsertAValue_ReturnsIdentity(DatabaseType type)
        {
            var db = GetCleanedServer(type,true);
            var tbl = db.CreateTable("InsertTable",
                new[]
                {
                    new DatabaseColumnRequest("myidentity",new DatabaseTypeRequest(typeof(int))){IsPrimaryKey = true,IsAutoIncrement = true}, 
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(typeof(string),100))
                });

            var nameCol = tbl.DiscoverColumn("Name");

            int result = tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,"fish"}
            });

            Assert.AreEqual(1,result);


            result = tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,"fish"}
            });

            Assert.AreEqual(2, result);
        }
    }
}
