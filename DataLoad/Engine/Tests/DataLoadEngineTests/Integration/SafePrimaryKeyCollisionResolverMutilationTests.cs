using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class SafePrimaryKeyCollisionResolverMutilationTests:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,true)]
        [TestCase(DatabaseType.MYSQLServer,true)]
        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        public void SafePrimaryKeyCollisionResolverMutilationTests_NoDifference_NoRecordsDeleted(DatabaseType dbType,bool bothNull)
        {
            var db = GetCleanedServer(dbType);

            DataTable dt = new DataTable();
            dt.Columns.Add("PK");
            dt.Columns.Add("ResolveOn");
            dt.Columns.Add("AnotherCol");
            
            dt.Rows.Add(1, bothNull?null:"fish", "cat");
            dt.Rows.Add(1, bothNull ? null : "fish", "flop");
            dt.Rows.Add(2, "fish", "flop");
            dt.Rows.Add(3, "dave", "franl");

            var tbl = db.CreateTable("MyTable", dt);

            TableInfo ti;
            ColumnInfo[] cis;
            Import(tbl,out ti,out cis);

            var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();
            
            var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

            var mutilation = new SafePrimaryKeyCollisionResolverMutilation();
            mutilation.ColumnToResolveOn = resolveOn;
            
            mutilation.PreferLargerValues = true;
            mutilation.PreferNulls = false;
            
            mutilation.Initialize(db, LoadStage.AdjustRaw);
            mutilation.Mutilate(new ThrowImmediatelyDataLoadEventListener());

            Assert.AreEqual(4,tbl.GetRowCount());
        }
        [TestCase(DatabaseType.MicrosoftSQLServer,false)]
        [TestCase(DatabaseType.MYSQLServer,false)]
        [TestCase(DatabaseType.MicrosoftSQLServer, true)]
        [TestCase(DatabaseType.MYSQLServer, true)]
        public void SafePrimaryKeyCollisionResolverMutilationTests_PreferNull_RecordsDeleted(DatabaseType dbType,bool preferNulls)
        {
            var db = GetCleanedServer(dbType);

            DataTable dt = new DataTable();
            dt.Columns.Add("PK");
            dt.Columns.Add("ResolveOn");
            dt.Columns.Add("AnotherCol");

            dt.Rows.Add(1, null, "cat");
            dt.Rows.Add(1, "fish", "flop");
            dt.Rows.Add(2, "fish", "flop");
            dt.Rows.Add(3, "dave", "franl");

            var tbl = db.CreateTable("MyTable", dt);

            TableInfo ti;
            ColumnInfo[] cis;
            Import(tbl, out ti, out cis);

            var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

            var mutilation = new SafePrimaryKeyCollisionResolverMutilation();
            mutilation.ColumnToResolveOn = resolveOn;

            mutilation.PreferLargerValues = true;
            mutilation.PreferNulls = preferNulls;

            mutilation.Initialize(db, LoadStage.AdjustRaw);
            mutilation.Mutilate(new ThrowImmediatelyDataLoadEventListener());

            Assert.AreEqual(3, tbl.GetRowCount());
            var result = tbl.GetDataTable();
            
            //if you prefer nulls you shouldn't want this one
            Assert.AreEqual(preferNulls? 0:1 ,result.Rows.Cast<DataRow>().Count(r=>(int)r["PK"] == 1 && r["ResolveOn"] as string == "fish" && r["AnotherCol"] as string == "flop" ));

            //if you prefer nulls you should have this one
            Assert.AreEqual(preferNulls ? 1 : 0, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"));
        }


        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer, true)]
        [TestCase(DatabaseType.MYSQLServer, true)]
        public void SafePrimaryKeyCollisionResolverMutilationTests_PreferLarger_RecordsDeleted(DatabaseType dbType, bool preferLarger)
        {
            var db = GetCleanedServer(dbType);

            DataTable dt = new DataTable();
            dt.Columns.Add("PK");
            dt.Columns.Add("ResolveOn");
            dt.Columns.Add("AnotherCol");

            dt.Rows.Add(1, null, "cat");
            dt.Rows.Add(1, "a", "flop");
            dt.Rows.Add(1, "b", "flop");
            dt.Rows.Add(2, "fish", "flop");
            dt.Rows.Add(3, "dave", "franl");

            var tbl = db.CreateTable("MyTable", dt);

            TableInfo ti;
            ColumnInfo[] cis;
            Import(tbl, out ti, out cis);

            var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

            var mutilation = new SafePrimaryKeyCollisionResolverMutilation();
            mutilation.ColumnToResolveOn = resolveOn;

            mutilation.PreferLargerValues = preferLarger;
            mutilation.PreferNulls = false; 

            mutilation.Initialize(db, LoadStage.AdjustRaw);
            mutilation.Mutilate(new ThrowImmediatelyDataLoadEventListener());

            Assert.AreEqual(3, tbl.GetRowCount());
            var result = tbl.GetDataTable();

            //if you like larger values (alphabetically) then you want the 'b'
            Assert.AreEqual(preferLarger ? 1 : 0, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && r["ResolveOn"] as string == "b" && r["AnotherCol"] as string == "flop"));
            Assert.AreEqual(preferLarger ? 0 : 1, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && r["ResolveOn"] as string == "a" && r["AnotherCol"] as string == "flop"));

            //either way you shouldn't have the null one
            Assert.AreEqual(0, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"));
        }



        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer, true)]
        [TestCase(DatabaseType.MYSQLServer, true)]
        public void SafePrimaryKeyCollisionResolverMutilationTests_PreferLarger_Dates_RecordsDeleted(DatabaseType dbType, bool preferLarger)
        {
            var db = GetCleanedServer(dbType);

            DataTable dt = new DataTable();
            dt.Columns.Add("PK");
            dt.Columns.Add("ResolveOn");
            dt.Columns.Add("AnotherCol");

            dt.Rows.Add(1, null, "cat");
            dt.Rows.Add(1, new DateTime(2001,01,01), "flop");
            dt.Rows.Add(1, new DateTime(2002, 01, 01), "flop");
            dt.Rows.Add(2, null, "flop");
            dt.Rows.Add(3, null, "franl");

            var tbl = db.CreateTable("MyTable", dt);

            TableInfo ti;
            ColumnInfo[] cis;
            Import(tbl, out ti, out cis);

            var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

            var mutilation = new SafePrimaryKeyCollisionResolverMutilation();
            mutilation.ColumnToResolveOn = resolveOn;

            mutilation.PreferLargerValues = preferLarger;
            mutilation.PreferNulls = false;

            mutilation.Initialize(db, LoadStage.AdjustRaw);
            mutilation.Mutilate(new ThrowImmediatelyDataLoadEventListener());

            Assert.AreEqual(3, tbl.GetRowCount());
            var result = tbl.GetDataTable();

            //if you like larger values then you want 2002 thats larger than 2001
            Assert.AreEqual(preferLarger ? 1 : 0, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && Equals(r["ResolveOn"], new DateTime(2002,01,01)) && r["AnotherCol"] as string == "flop"));
            Assert.AreEqual(preferLarger ? 0 : 1, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && Equals(r["ResolveOn"], new DateTime(2001,01,01))  && r["AnotherCol"] as string == "flop"));

            //either way you shouldn't have the null one
            Assert.AreEqual(0, result.Rows.Cast<DataRow>().Count(r => (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"));
        }
    }
}