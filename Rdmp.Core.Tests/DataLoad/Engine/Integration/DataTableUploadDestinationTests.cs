// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.TableCreation;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DataTableUploadDestinationTests:DatabaseTests
{
    [Test]
    public void DataTableChangesLengths_NoReAlter()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
            
        var dt1 = new DataTable();
        dt1.Columns.Add("name", typeof (string));
        dt1.Rows.Add(new []{"Fish"});
        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("name", typeof (string));
        dt2.Rows.Add(new []{"BigFish"});
        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData( dt1, toConsole,token);
        var ex = Assert.Throws<Exception>(()=>destination.ProcessPipelineData( dt2, toConsole,token));

        var expectedText =
            "BulkInsert failed on data row 1 the complaint was about source column <<name>> which had value <<BigFish>> destination data type was <<varchar(4)>>";
            
        Assert.IsNotNull(ex.InnerException);
        StringAssert.Contains(expectedText,ex.InnerException.Message);

        destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
    }

    //RDMPDEV-653
    [Test]
    [TestCase(true,10)]
    [TestCase(false,10)]
    public void DataTableChangesLengths_RandomColumnOrder(bool createIdentity,int numberOfRandomisations)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var tbl = db.ExpectTable("RandomOrderTable");
        var random = new Random();

        for (var i =0;i<numberOfRandomisations;i++)
        {
            if (tbl.Exists())
                tbl.Drop();

            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            var errorIsInColumnOrder = random.Next(3);
            var errorColumn = "";

            var sql = "CREATE TABLE RandomOrderTable (";

            var leftToCreate = new List<string>();

            leftToCreate.Add("name varchar(50),");
            leftToCreate.Add("color varchar(50),");
            leftToCreate.Add("age varchar(50),");

            if(createIdentity)
                leftToCreate.Add("id int IDENTITY(1,1),");

            var invalid = false;

            for (var j = 0; j < (createIdentity ? 4 : 3); j++)
            {
                var toAddNext = random.Next(leftToCreate.Count);

                var colSql = leftToCreate[toAddNext];

                leftToCreate.Remove(colSql);

                if (errorIsInColumnOrder == j)
                {
                    sql += colSql.Replace("(50)", "(1)");
                    errorColumn = colSql.Substring(0, colSql.IndexOf(" "));

                    if (errorColumn == "id")
                        invalid = true;
                }

                else
                    sql += colSql;
            }

            if(invalid)
                continue;

            sql = $"{sql.TrimEnd(',')})";

            Console.Write($"About to execute:{sql}");
                
            //problem is with the column name which appears at order 0 in the destination dataset (name with width 1)
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                db.Server.GetCommand(sql, con).ExecuteNonQuery();
            }


            //the bulk insert is
            var destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            //order is inverted where name comes out at the end column (index 2)
            var dt1 = new DataTable();
            dt1.Columns.Add("age", typeof (string));
            dt1.Columns.Add("color", typeof (string));
            dt1.Columns.Add("name", typeof (string));

            dt1.Rows.Add("30", "blue", "Fish");
            dt1.TableName = "RandomOrderTable";

            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

            var exceptionMessage = ex.InnerException.Message;
            var interestingBit = exceptionMessage.Substring(exceptionMessage.IndexOf(": <<") + ": ".Length);
                
            var expectedErrorMessage =
                $"<<{errorColumn}>> which had value <<{dt1.Rows[0][errorColumn]}>> destination data type was <<varchar(1)>>";
            StringAssert.Contains(expectedErrorMessage,interestingBit);

            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
            tbl.Drop();
        }
    }



    //RDMPDEV-653
    [Test]
    public void DataTableChangesLengths_DropColumns()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var tbl = db.ExpectTable("DroppedColumnsTable");
        if(tbl.Exists())
            tbl.Drop();

        var sql = @"CREATE TABLE DroppedColumnsTable (
name varchar(50),
color varchar(50),
age varchar(50)
)

ALTER TABLE DroppedColumnsTable Drop column color
ALTER TABLE DroppedColumnsTable add color varchar(1)
";

        Console.Write($"About to execute:{sql}");

        //problem is with the column name which appears at order 0 in the destination dataset (name with width 1)
        using (var con = db.Server.GetConnection())
        {
            con.Open();
            db.Server.GetCommand(sql, con).ExecuteNonQuery();
        }
            
        //the bulk insert is
        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, new ThrowImmediatelyDataLoadEventListener());

        //order is inverted where name comes out at the end column (index 2)
        var dt1 = new DataTable();
        dt1.Columns.Add("age", typeof(string));
        dt1.Columns.Add("color", typeof(string));
        dt1.Columns.Add("name", typeof(string));

        dt1.Rows.Add("30", "blue", "Fish");
        dt1.TableName = "DroppedColumnsTable";

        var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, new ThrowImmediatelyDataLoadEventListener(), token));

        var exceptionMessage = ex.InnerException.Message;
        var interestingBit = exceptionMessage.Substring(exceptionMessage.IndexOf(": <<") + ": ".Length);

        var expectedErrorMessage = "<<color>> which had value <<blue>> destination data type was <<varchar(1)>>";
        StringAssert.Contains(expectedErrorMessage, interestingBit);
            
        destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);

        if(tbl.Exists())
            tbl.Drop();
    }

    [Test]
    public void DataTableEmpty_ThrowHelpfulException()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);

        var dt1 = new DataTable();
        dt1.TableName = "MyEmptyTable";
        var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

        destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
            
        Assert.AreEqual("DataTable 'MyEmptyTable' had no Columns!", ex.Message);
    }
    [Test]
    public void DataTableNoRows_ThrowHelpfulException()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);

        var dt1 = new DataTable();
        dt1.Columns.Add("GoTeamGo");
        dt1.TableName = "MyEmptyTable";
        var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));
            
        destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);

        Assert.AreEqual("DataTable 'MyEmptyTable' had no Rows!", ex.Message);
    }
    [Test]
    public void DataTableChangesLengths_AllowAlter()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("name", typeof(string));
        dt1.Rows.Add(new[] { "Fish" });
        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("name", typeof(string));
        dt2.Rows.Add(new[] { "BigFish" });
        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData( dt1, toConsole, token);
        Assert.DoesNotThrow(() => destination.ProcessPipelineData( dt2, toMemory, token));

        Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column")));

        destination.Dispose(toConsole, null);
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(2,db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            
    }

    [Test]
    public void DoubleResizingBetweenIntAndDouble()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mynum", typeof(double));
        dt1.Rows.Add(new object[] {1});
        dt1.Rows.Add(new object[] { 5 });
        dt1.Rows.Add(new object[] { 15 });
        dt1.Rows.Add(new object[] { 2.5 });
        dt1.Rows.Add(new object[] { 5 });

        dt1.TableName = "DataTableUploadDestinationTests";
            
        destination.ProcessPipelineData(dt1, toConsole, token);
        destination.Dispose(toConsole, null);

        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(5, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
        Assert.AreEqual("decimal(3,1)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
    }


    [TestCase("varchar(3)", 1.5, "x")]//RDMPDEV-932
    [TestCase("varchar(27)", "2001-01-01", "x")] //see Guesser.MinimumLengthRequiredForDateStringRepresentation
    public void BatchResizing(string expectedDatatypeInDatabase,object batch1Value,object batch2Value)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mycol");
        dt1.Rows.Add(new[] {batch1Value});
            
        dt1.TableName = "DataTableUploadDestinationTests";
        try
        {
            destination.ProcessPipelineData(dt1, toConsole, token);
            
            var dt2 = new DataTable();
            dt2.Columns.Add("mycol");
            dt2.Rows.Add(new object[] { batch2Value });

            destination.ProcessPipelineData(dt2, toConsole, token);
            destination.Dispose(toConsole, null);
        }
        catch (Exception e)
        {
            destination.Dispose(toConsole, e);
            throw;
        }
            
        Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mycol").DataType.SQLType);
    }

    [TestCase("varchar(24)", "2", "987styb4ih0r9h4322938476", "tinyint")]
    public void BatchResizing_WithExplicitWriteTypes(string expectedDatatypeInDatabase, object batch1Value, object batch2Value, string batch1SqlType)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mycol");
        dt1.Rows.Add(new[] { batch1Value });

        dt1.TableName = "DataTableUploadDestinationTests";
        try
        {
            destination.AddExplicitWriteType("mycol", batch1SqlType);
            destination.ProcessPipelineData(dt1, toConsole, token);

            var dt2 = new DataTable();
            dt2.Columns.Add("mycol");
            dt2.Rows.Add(new object[] { batch2Value });

            destination.ProcessPipelineData(dt2, toConsole, token);
            destination.Dispose(toConsole, null);
        }
        catch (Exception e)
        {
            destination.Dispose(toConsole, e);
            throw;
        }

        Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mycol").DataType.SQLType);
    }

    [Test]
    public void VeryLongStringIsVarcharMax()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;


        var longBitOfText = "";

        for (var i = 0; i < 9000; i++)
            longBitOfText += 'A';

        var dt1 = new DataTable();
        dt1.Columns.Add("myText");
        dt1.Rows.Add(new object[] { longBitOfText });


        dt1.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);
        destination.Dispose(toConsole, null);

        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
        Assert.AreEqual("varchar(max)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("myText").DataType.SQLType);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DecimalResizing(bool negative)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mynum", typeof(string));
        dt1.Rows.Add(new[] { "1.51" });
        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("mynum", typeof(string));
        dt2.Rows.Add(new[] { negative ? "-999.99" : "999.99" });
        dt2.Rows.Add(new[] { "00000.00000" });
        dt2.Rows.Add(new[] { "0" });
        dt2.Rows.Add(new string[] { null });
        dt2.Rows.Add(new [] { "" }); 
        dt2.Rows.Add(new[] { DBNull.Value });
        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData( dt1, toConsole, token);
        destination.ProcessPipelineData( dt2, toMemory, token);

        Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column ")));

        destination.Dispose(toConsole, null);
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(7, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
        Assert.AreEqual("decimal(5,2)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
    }

    private static object[] _sourceLists = {
        new object[] {"decimal(3,3)",new object[]{"0.001"}, new object[]{0.001}},  //case 1
        new object[] {"decimal(6,3)",new object[]{"19","0.001","123.001",32.0f}, new object[]{19,0.001,123.001,32.0f}},  //case 2

        //Time tests
        new object[] {"time",new object[]{"12:01"}, new object[]{new TimeSpan(12,1,0)}},
        new object[] {"time",new object[]{"13:00:00"}, new object[]{new TimeSpan(13,0,0)}},

        //Send two dates expect datetime in database and resultant data to be legit dates
        new object[] {"datetime2",new object[]{"2001-01-01 12:01","2010-01-01"}, new object[]{new DateTime(2001,01,01,12,1,0),new DateTime(2010,01,01,0,0,0)}},

        //Mixed data types going from time to date results in us falling back to string
        new object[] {"varchar(10)",new object[]{"12:01","2001-01-01"}, new object[]{"12:01","2001-01-01"}}
    };

        
    [Test, TestCaseSource(nameof(_sourceLists))]
    public void DataTypeEstimation(string expectedDatatypeInDatabase, object[] rowValues, object[] expectedValuesReadFromDatabase)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
            
        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);

        var dt1 = new DataTable();
        dt1.Columns.Add("myCol", typeof(string));

        foreach (var rowValue in rowValues)
            dt1.Rows.Add(new[] {rowValue});
            
        dt1.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);

        destination.Dispose(toConsole, null);
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("myCol").DataType.SQLType);

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            using(var cmd = DatabaseCommandHelper.GetCommand("Select * from DataTableUploadDestinationTests", con))
            using(var r = cmd.ExecuteReader())
                foreach (var e in expectedValuesReadFromDatabase)
                {
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual(e, r["myCol"]);
                }
        }
    }



    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DecimalZeros(bool sendTheZero)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
            
        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mynum", typeof(string));
        dt1.Rows.Add(new[] { "0.000742548000424313" });

        if (sendTheZero)
            dt1.Rows.Add(new[] { "0" });

        dt1.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData( dt1, toConsole, token);
        destination.Dispose(toConsole, null);

        //table should exist
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());

        //should have 2 rows
        Assert.AreEqual(sendTheZero?2:1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            
        //should be decimal

        Assert.AreEqual(sendTheZero ?"decimal(19,18)":"decimal(18,18)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
    }

    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    public void TestResizing(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);
        var server = db.Server;

        var table = db.CreateTable("TestResizing",
            new DatabaseColumnRequest[] 
            {
                new DatabaseColumnRequest("MyInteger",new DatabaseTypeRequest(typeof(int))),
                new DatabaseColumnRequest("MyMaxString",new DatabaseTypeRequest(typeof(string),int.MaxValue)),
                new DatabaseColumnRequest("Description",new DatabaseTypeRequest(typeof(string),int.MaxValue)),
                new DatabaseColumnRequest("StringNotNull",new DatabaseTypeRequest(typeof(string),100),false),
                new DatabaseColumnRequest("StringAllowNull",new DatabaseTypeRequest(typeof(string),100),true),
                new DatabaseColumnRequest("StringPk",new DatabaseTypeRequest(typeof(string),50),true){IsPrimaryKey = true }
            });

        using (var con = server.GetConnection())
        {
            con.Open();
                
            //should not allow nulls before
            Assert.AreEqual(false, table.DiscoverColumn("StringNotNull").AllowNulls);
            //do resize
            table.DiscoverColumn("StringNotNull").DataType.Resize(500);

            //rediscover it to get the new state in database (it should now be 500 and still shouldn't allow nulls)
            AssertIsStringWithLength(table.DiscoverColumn("StringNotNull"), 500);

                
            Assert.AreEqual(false, table.DiscoverColumn("StringNotNull").AllowNulls);

            //do the same with the one that allows nulls
            Assert.AreEqual(true, table.DiscoverColumn("StringAllowNull").AllowNulls);
            table.DiscoverColumn("StringAllowNull").DataType.Resize(101);
            table.DiscoverColumn("StringAllowNull").DataType.Resize(103);
            table.DiscoverColumn("StringAllowNull").DataType.Resize(105);
                
            AssertIsStringWithLength(table.DiscoverColumn("StringAllowNull"), 105);
            Assert.AreEqual(true, table.DiscoverColumn("StringAllowNull").AllowNulls);

            //we should have correct understanding prior to resize
            AssertIsStringWithLength(table.DiscoverColumn("StringPk"),50);
            Assert.AreEqual(true, table.DiscoverColumn("StringPk").IsPrimaryKey);
            Assert.AreEqual(false, table.DiscoverColumn("StringPk").AllowNulls);

            //now we execute the resize
            table.DiscoverColumn("StringPk").DataType.Resize(500);

            AssertIsStringWithLength(table.DiscoverColumn("StringPk"), 500);

            Assert.AreEqual(true, table.DiscoverColumn("StringPk").IsPrimaryKey);
            Assert.AreEqual(false, table.DiscoverColumn("StringPk").AllowNulls);
                
            con.Close();
        }
    }

    private void AssertIsStringWithLength(DiscoveredColumn col, int expectedLength)
    {
        switch (col.Table.Database.Server.DatabaseType)
        {
            case DatabaseType.MicrosoftSQLServer:
            case DatabaseType.MySql:
                Assert.AreEqual($"varchar({expectedLength})",col.DataType.SQLType);
                break;
            case DatabaseType.Oracle:
                Assert.AreEqual($"varchar2({expectedLength})",col.DataType.SQLType);
                break;
            case DatabaseType.PostgreSql:
                Assert.AreEqual($"character varying({expectedLength})",col.DataType.SQLType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(col.Table.Database.Server.DatabaseType), col.Table.Database.Server.DatabaseType, null);
        }

    }

    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    public void TestResizing_WithDetection(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var table = db.CreateTable("TestResizing",
            new DatabaseColumnRequest[]
            {
                new DatabaseColumnRequest("MyInteger",new DatabaseTypeRequest(typeof(int))),
                new DatabaseColumnRequest("MyMaxString",new DatabaseTypeRequest(typeof(string),int.MaxValue)),
                new DatabaseColumnRequest("Description",new DatabaseTypeRequest(typeof(string),int.MaxValue)),
                new DatabaseColumnRequest("StringNotNull",new DatabaseTypeRequest(typeof(string),10),false),
                new DatabaseColumnRequest("StringAllowNull",new DatabaseTypeRequest(typeof(string),100),true),
                new DatabaseColumnRequest("StringPk",new DatabaseTypeRequest(typeof(string),50),true){IsPrimaryKey = true }
            });

        Assert.AreEqual(10, table.DiscoverColumn("StringNotNull").DataType.GetLengthIfString());

        var dt = new DataTable("TestResizing");
        dt.Columns.Add("MyInteger");
        dt.Columns.Add("MyMaxString");
        dt.Columns.Add("Description");
        dt.Columns.Add("StringNotNull");
        dt.Columns.Add("StringAllowNull");
        dt.Columns.Add("StringPk");

        dt.Rows.Add("1",    //MyInteger
            "fff",          //MyMaxString
            "fff2",         //Description
            "1234567891011",//StringNotNull - too long for the column, so it should resize
            DBNull.Value,   //StringAllowNull
            "f"             //StringPk
        );
            
        var dt2 = dt.Clone();
        dt2.Rows.Clear();
        dt2.Rows.Add("1",    //MyInteger
            "fff",          //MyMaxString
            "fff2",         //Description
            "12345678910112",//StringNotNull - too long for the column, so it should resize
            DBNull.Value,   //StringAllowNull
            "f2"             //StringPk
        );

        var dest = new DataTableUploadDestination();
        dest.AllowResizingColumnsAtUploadTime = true;
        dest.PreInitialize(db,new ThrowImmediatelyDataLoadEventListener());
            
        dest.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
        dest.ProcessPipelineData(dt2, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        dest.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);

        //it should have resized us.
        Assert.AreEqual(14, table.DiscoverColumn("StringNotNull").DataType.GetLengthIfString());

    }
    [TestCase(DatabaseType.MicrosoftSQLServer, "didn�t")]
    [TestCase(DatabaseType.MySql, "didn�t")]
    [TestCase(DatabaseType.Oracle, "didn�t")]
    [TestCase(DatabaseType.MicrosoftSQLServer, "didn't")]
    [TestCase(DatabaseType.MySql, "didn't")]
    [TestCase(DatabaseType.Oracle, "didn't")]
    public void Test_SingleQuote_InText(DatabaseType dbType,string testValue)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable("TestFreeText");
        dt.Columns.Add("MyFreeText");
        dt.Rows.Add(testValue);
            
        var dest = new DataTableUploadDestination();
        dest.AllowResizingColumnsAtUploadTime = true;
        dest.PreInitialize(db, new ThrowImmediatelyDataLoadEventListener());
        dest.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        dest.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

        var tbl = db.ExpectTable("TestFreeText");
        Assert.IsTrue(tbl.Exists());
        Assert.AreEqual(1,tbl.GetRowCount());
    }

    [Test]
    public void DodgyTypes()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("col1", typeof(double));
        dt1.Columns.Add("col2", typeof(double));
        dt1.Columns.Add("col3", typeof(bool));
        dt1.Columns.Add("col4", typeof(byte));
        dt1.Columns.Add("col5", typeof(byte[]));

        dt1.Rows.Add(new object[] { 0.425,0.451,true,(byte)2,new byte[]{0x5,0xA}});


        dt1.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);
        destination.Dispose(toConsole, null);

        //table should exist
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());

        //should have 2 rows
        Assert.AreEqual(1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

        //should be decimal
        Assert.AreEqual("decimal(3,3)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col1").DataType.SQLType);
        Assert.AreEqual("decimal(3,3)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col2").DataType.SQLType);
        Assert.AreEqual("bit", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col3").DataType.SQLType);
        Assert.AreEqual("tinyint", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col4").DataType.SQLType);
            
        Assert.AreEqual("varbinary(max)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col5").DataType.SQLType);
    }


    [Test]
    public void TypeAlteringlResizing()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mynum", typeof(string));
        dt1.Rows.Add(new[] { "true" });
        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("mynum", typeof(string));
        dt2.Rows.Add(new[] { "999" });
        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);
        destination.ProcessPipelineData(dt2, toMemory, token);

        Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column 'mynum' from 'bit' to 'int'")));

        destination.Dispose(toConsole, null);
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
        Assert.AreEqual("int", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
    }

    [Test]
    public void MySqlTest_Simple()
    {
        var token = new GracefulCancellationToken();
            
        var db = GetCleanedServer(DatabaseType.MySql);

        var toConsole = new ThrowImmediatelyDataLoadEventListener();

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;
            
        var dt = new DataTable();
        dt.Columns.Add("mystringcol", typeof(string));
        dt.Columns.Add("mynum", typeof(string));
        dt.Columns.Add("mydate", typeof (string));
        dt.Columns.Add("myLegitDateTime", typeof(DateTime));
        dt.Columns.Add("mynullcol", typeof(string));


        //drop the millisecond part
        var now = DateTime.Now;
        now = new DateTime(now.Year,now.Month,now.Day,now.Hour,now.Minute,now.Second);
            
        dt.Rows.Add(new object[] { "Anhoy there \"mates\"", "999", "2001-01-01", now,null});
        dt.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt, toConsole, token);

        destination.Dispose(toConsole, null);
        var tbl = db.ExpectTable("DataTableUploadDestinationTests");
        Assert.IsTrue(tbl.Exists());
        Assert.AreEqual(1, tbl.GetRowCount());
        Assert.AreEqual("int", tbl.DiscoverColumn("mynum").DataType.SQLType);

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            var r = db.Server.GetCommand(tbl.GetTopXSql(10), con).ExecuteReader();

            Assert.IsTrue(r.Read());
            Assert.AreEqual("Anhoy there \"mates\"", (string)r["mystringcol"]);
            Assert.AreEqual(999,(int)r["mynum"]);
            Assert.AreEqual(new DateTime(2001,1,1),(DateTime)r["mydate"]);
            Assert.AreEqual(now, (DateTime)r["myLegitDateTime"]);
            Assert.AreEqual(DBNull.Value, r["mynullcol"]);
        }

        db.Drop();
    }

    [Test]
    public void MySqlTest_Resize()
    {
        var token = new GracefulCancellationToken();

        var db = GetCleanedServer(DatabaseType.MySql);

        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("mynum", typeof(string));
        dt1.Rows.Add(new[] { "true" });
        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("mynum", typeof(string));
        dt2.Rows.Add(new[] { "999" });
        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);
        destination.ProcessPipelineData(dt2, toMemory, token);

        destination.Dispose(toConsole, null);
        var tbl = db.ExpectTable("DataTableUploadDestinationTests");
        Assert.IsTrue(tbl.Exists());
        Assert.AreEqual(2, tbl.GetRowCount());
        Assert.AreEqual("int", tbl.DiscoverColumn("mynum").DataType.SQLType);

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            var r = db.Server.GetCommand(tbl.GetTopXSql(10), con).ExecuteReader();

            //technically these can come out in a random order
            var numbersRead = new List<int>();
            Assert.IsTrue(r.Read());
            numbersRead.Add((int) r["mynum"]);
            Assert.IsTrue(r.Read());
            numbersRead.Add((int)r["mynum"]);

            Assert.IsFalse(r.Read());
            Assert.IsTrue(numbersRead.Contains(1));
            Assert.IsTrue(numbersRead.Contains(999));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDestinationAlreadyExistingIsOk(bool targetTableIsEmpty)
    {
        //create a table in the scratch database with a single column Name
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var tbl = db.CreateTable("TestDestinationAlreadyExistingIsOk",new[]{new DatabaseColumnRequest("Name","varchar(10)",false)});
        try
        {
            if(!targetTableIsEmpty)
            {
                //upload a single row
                var dtAlreadyThereData = new DataTable();
                dtAlreadyThereData.Columns.Add("Name");
                dtAlreadyThereData.Rows.Add(new[] {"Bob"});

                using(var bulk = tbl.BeginBulkInsert())
                    bulk.Upload(dtAlreadyThereData);
            }
            
            //create the destination component (what we want to test)
            var destinationComponent = new DataTableUploadDestination();
            destinationComponent.AllowResizingColumnsAtUploadTime = true;
            destinationComponent.AllowLoadingPopulatedTables = true;
            
            //create the simulated chunk that will be dispatched
            var dt = new DataTable("TestDestinationAlreadyExistingIsOk");
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] {"Bob"});
            dt.Rows.Add(new[] { "Frank" });
            dt.Rows.Add(new[] { "I've got a lovely bunch of coconuts" });

            var listener = new ThrowImmediatelyDataLoadEventListener();

            //pre initialzie with the database (which must be part of any pipeline use case involving a DataTableUploadDestination)
            destinationComponent.PreInitialize(db ,listener);

            //tell the destination component to process the data
            destinationComponent.ProcessPipelineData(dt, listener,new GracefulCancellationToken());
            
            destinationComponent.Dispose(listener,null);
            Assert.AreEqual(targetTableIsEmpty?3:4, tbl.GetRowCount());
        }
        finally
        {
            tbl.Drop();
        }
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void DataTableUploadDestinationTests_PrimaryKeyDataTableWithAlterSizeLater(DatabaseType dbtype)
    {
        var db = GetCleanedServer(dbtype);

        var destination = new DataTableUploadDestination();
            
        destination.AllowResizingColumnsAtUploadTime = true;
        destination.PreInitialize(db,new ThrowImmediatelyDataLoadEventListener());

        var dt1 = new DataTable();
        dt1.TableName = "MyTable";
        dt1.Columns.Add("Name");
        dt1.Rows.Add("Fish");

        dt1.PrimaryKey = dt1.Columns.Cast<DataColumn>().ToArray();

        destination.ProcessPipelineData(dt1, new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
            
        var dt2 = new DataTable();
        dt2.TableName = "MyTable";
        dt2.Columns.Add("Name");
        dt2.Rows.Add("Fish Monkey Fish Fish"); //notice that this is longer so the column must be resized

        dt2.PrimaryKey = dt2.Columns.Cast<DataColumn>().ToArray();

        destination.ProcessPipelineData(dt2, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());


        destination.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);

        var tbl = db.ExpectTable("MyTable");
            
        Assert.AreEqual(2,tbl.GetRowCount());
        Assert.IsTrue(tbl.DiscoverColumns().Single().IsPrimaryKey);


    }

    [Test]
    public void TestDestinationAlreadyExisting_ColumnSubset()
    {
        //create a table in the scratch database with a single column Name
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var tbl = db.CreateTable("TestDestinationAlreadyExisting_ColumnSubset", new[]
        {
            new DatabaseColumnRequest("Name", "varchar(10)", false),
            new DatabaseColumnRequest("Age","int"),
            new DatabaseColumnRequest("Address","varchar(1000)")

        });

        try
        {
                
            //upload a single row of already existing data
            var dtAlreadyThereData = new DataTable();
            dtAlreadyThereData.Columns.Add("Name");
            dtAlreadyThereData.Columns.Add("Age");
            dtAlreadyThereData.Rows.Add(new object[] { "Bob",5});

            using (var bulk = tbl.BeginBulkInsert())
                bulk.Upload(dtAlreadyThereData);
                
            //create the destination component (what we want to test)
            var destinationComponent = new DataTableUploadDestination();
            destinationComponent.AllowResizingColumnsAtUploadTime = true;
            destinationComponent.AllowLoadingPopulatedTables = true;

            //create the simulated chunk that will be dispatched
            var dt = new DataTable("TestDestinationAlreadyExisting_ColumnSubset");
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] { "Bob" });
            dt.Rows.Add(new[] { "Frank" });
            dt.Rows.Add(new[] { "I've got a lovely bunch of coconuts" });

            var listener = new ThrowImmediatelyDataLoadEventListener();

            //pre initialzie with the database (which must be part of any pipeline use case involving a DataTableUploadDestination)
            destinationComponent.PreInitialize(db, listener);

            //tell the destination component to process the data
            destinationComponent.ProcessPipelineData(dt, listener, new GracefulCancellationToken());

            destinationComponent.Dispose(listener, null);
            Assert.AreEqual(4, tbl.GetRowCount());
        }
        finally
        {
            tbl.Drop();
        }
    }

    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    public void Test_DataTableUploadDestination_ScientificNotation(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable("ff");
        dt.Columns.Add("mycol");
        dt.Rows.Add("-4.10235746055587E-05"); //this string is untyped

        var dest = new DataTableUploadDestination();
        dest.PreInitialize(db,new ThrowImmediatelyDataLoadEventListener());

        try
        {
            dest.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }
        catch(Exception ex)
        {
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(),ex);
            throw;
        }
                       
        //in the database it should be typed
        Assert.AreEqual(typeof(Decimal),db.ExpectTable("ff").DiscoverColumn("mycol").DataType.GetCSharpDataType());

        var dt2 = db.ExpectTable("ff").GetDataTable();
            
        Assert.IsTrue((decimal)dt2.Rows[0][0]  == (decimal)-0.0000410235746055587);

    }

    private class AdjustColumnDelegater : IDatabaseColumnRequestAdjuster
    {
        public static Action<List<DatabaseColumnRequest>> AdjusterDelegate;

        public void AdjustColumns(List<DatabaseColumnRequest> columns)
        {
            AdjusterDelegate(columns);
        }
    }

    /// <summary>
    /// T and F are NOT NORMALLY True and False, this test confirms that we can force T and F to go in
    /// as boolean instead
    /// </summary>
    /// <param name="dbType"></param>
    [TestCaseSource(typeof(All),nameof(All.DatabaseTypes))]
    public void Test_DataTableUploadDestination_ForceBool(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable("ForceStringTable");
        dt.Columns.Add("hb_extract");
        dt.Columns.Add("Name");

        dt.Rows.Add("T", "Abc");
        dt.Rows.Add("F", "Def");

        var dest = new DataTableUploadDestination();
        dest.PreInitialize(db, new ThrowImmediatelyDataLoadEventListener());
        dest.Adjuster = typeof(AdjustColumnDelegater);

        AdjustColumnDelegater.AdjusterDelegate = (s) =>
        {
            var col = s.Single(c => c.ColumnName.Equals("hb_extract"));

            //Guesser would normally make it a string
            Assert.AreEqual(typeof(string), col.TypeRequested.CSharpType);

            //we demand a boolean interpretation instead!
            col.TypeRequested.CSharpType = typeof(bool);
        };

        try
        {
            dest.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }
        catch (Exception ex)
        {
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
            throw;
        }

        var tbl = db.ExpectTable("ForceStringTable");

        if (dbType == DatabaseType.Oracle)
        {
            //in the database it should be typed as string
            Assert.AreEqual(typeof(string), tbl.DiscoverColumn("hb_extract").DataType.GetCSharpDataType());

            var dt2 = tbl.GetDataTable();
            Assert.Contains("T",dt2.Rows.Cast<DataRow>().Select(r=>r[0]).ToArray());
            Assert.Contains("F",dt2.Rows.Cast<DataRow>().Select(r =>r[0]).ToArray());
        }
        else
        {
            //in the database it should be typed as bool
            Assert.AreEqual(typeof(bool), tbl.DiscoverColumn("hb_extract").DataType.GetCSharpDataType());

            var dt2 = tbl.GetDataTable();
            Assert.Contains(true,dt2.Rows.Cast<DataRow>().Select(r=>r[0]).ToArray());
            Assert.Contains(false,dt2.Rows.Cast<DataRow>().Select(r =>r[0]).ToArray());
        }
    }

    #region Two Batch Tests
    [TestCase(DatabaseType.MySql, true)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    public void TwoBatch_BooleanResizingTest(DatabaseType dbType, bool giveNullValuesOnly)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(dbType);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("TestedCol", typeof(string));
        dt1.Columns.Add("OtherCol", typeof(string));
        dt1.Rows.Add(new[] { giveNullValuesOnly ? null : "true", "1.51" });

        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("TestedCol", typeof(string));
        dt2.Columns.Add("OtherCol", typeof(string));

        dt2.Rows.Add(new[] { "2001-01-01", "999.99" });

        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);

        Assert.AreEqual("bit", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("TestedCol").DataType.SQLType);

        destination.ProcessPipelineData(dt2, toMemory, token);

        Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column ")));

        destination.Dispose(toConsole, null);
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

        var tt = db.Server.GetQuerySyntaxHelper().TypeTranslater;

        Assert.AreEqual(

            //if all we got are nulls we should have a DateTime otherwise we had 1/true so the only usable data type is string 
            giveNullValuesOnly ? typeof(DateTime) : typeof(string),

            tt.GetCSharpTypeForSQLDBType(db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("TestedCol").DataType.SQLType));
    }

    /// <summary>
    /// Tests the systems ability to change live table datatypes during bulk insert to accomodate novel data
    /// 
    /// <para>This test set passes v1 in the first batch which determines the initial Type of the database table.  Then v2 is passed in the next batch
    /// which will (in most cases) require an ALTER of the live table to accomodate the wider datatype.</para>
    /// </summary>
    /// <param name="dbType">The DBMS to test</param>
    /// <param name="v1">The row value to send in batch 1</param>
    /// <param name="v2">The row value to send in batch 2 (after table creation)</param>
    /// <param name="expectedTypeForBatch1">The Type you expect to be used to store the v1</param>
    /// <param name="expectedTypeForBatch2">The Type you expect after ALTER to support all values seen SetUp till now (i.e. v1) AND v2</param>
    [TestCase(DatabaseType.MySql,null,"235", typeof(bool),typeof(int))]
    [TestCase(DatabaseType.MySql, "123", "2001-01-01 12:00:00" ,typeof(int), typeof(string))] //123 cannot be converted to date so it becomes string
    [TestCase(DatabaseType.MySql, "2001-01-01", "2001-01-01 12:00:00" ,  typeof(DateTime), typeof(DateTime) )]
    [TestCase(DatabaseType.MySql, "2001-01-01", "omg", typeof(DateTime), typeof(string))]

    [TestCase(DatabaseType.MicrosoftSQLServer, null, "235", typeof(bool), typeof(int))]
    [TestCase(DatabaseType.MicrosoftSQLServer, "123", "2001-01-01 12:00:00", typeof(int), typeof(string))] //123 cannot be converted to date so it becomes string
    [TestCase(DatabaseType.MicrosoftSQLServer, "2001-01-01", "2001-01-01 12:00:00", typeof(DateTime), typeof(DateTime))]
    [TestCase(DatabaseType.MicrosoftSQLServer, "2001-01-01", "omg", typeof(DateTime), typeof(string))]
    public void TwoBatch_MiscellaneousTest(DatabaseType dbType, string v1,string v2,Type expectedTypeForBatch1,Type expectedTypeForBatch2)
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(dbType);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;

        var dt1 = new DataTable();
        dt1.Columns.Add("TestedCol", typeof(string));
        dt1.Rows.Add(new[] { v1 });

        if (v1 != null && v2 != null)
            dt1.PrimaryKey = dt1.Columns.Cast<DataColumn>().ToArray();

        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("TestedCol", typeof(string));

        dt2.Rows.Add(new[] { v2 });
        dt2.TableName = "DataTableUploadDestinationTests";

        var tt = db.Server.GetQuerySyntaxHelper().TypeTranslater;
        var tbl = db.ExpectTable("DataTableUploadDestinationTests");

        try
        {
            destination.ProcessPipelineData(dt1, toConsole, token);
            Assert.AreEqual(expectedTypeForBatch1,
                tt.GetCSharpTypeForSQLDBType(tbl.DiscoverColumn("TestedCol").DataType.SQLType));

            destination.ProcessPipelineData(dt2, toMemory, token);
            destination.Dispose(toConsole, null);
        }
        catch(Exception ex)
        {
            destination.Dispose(toConsole, ex);
            throw;
        }
            
        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

        var colAfter = tbl.DiscoverColumn("TestedCol");

        Assert.AreEqual(v1 != null && v2 != null,colAfter.IsPrimaryKey);

        Assert.AreEqual(expectedTypeForBatch2, tt.GetCSharpTypeForSQLDBType(colAfter.DataType.SQLType));
    }

        
    [Test]
    public void TwoBatch_ExplicitRealDataType()
    {
        var token = new GracefulCancellationToken();
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var toConsole = new ThrowImmediatelyDataLoadEventListener();
        var toMemory = new ToMemoryDataLoadEventListener(true);

        var destination = new DataTableUploadDestination();
        destination.PreInitialize(db, toConsole);
        destination.AllowResizingColumnsAtUploadTime = true;
        destination.AddExplicitWriteType("FloatCol","real");

        var dt1 = new DataTable();
        dt1.Columns.Add("FloatCol", typeof(string));
        dt1.Rows.Add(new[] { "1.51" });

        dt1.TableName = "DataTableUploadDestinationTests";

        var dt2 = new DataTable();
        dt2.Columns.Add("FloatCol", typeof(string));
        dt2.Rows.Add(new[] { "99.9999" });

        dt2.TableName = "DataTableUploadDestinationTests";

        destination.ProcessPipelineData(dt1, toConsole, token);

        Assert.AreEqual("real", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("FloatCol").DataType.SQLType);

        destination.ProcessPipelineData(dt2, toMemory, token);

        destination.Dispose(toConsole, null);

        Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
        Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

        // should still be real
        Assert.AreEqual("real", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("FloatCol").DataType.SQLType);

    }
    #endregion
}