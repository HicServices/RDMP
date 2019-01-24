using System.Data;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using FAnsi;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class PrematureLoadEnderTests:DatabaseTests
    {
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void TestEndLoadBecause_NoTables(DatabaseType type)
        {
            var database = GetCleanedServer(type,true);
            
            Assert.AreEqual(0,database.DiscoverTables(false).Length);

            var ender = new PrematureLoadEnder();
            ender.ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase;
            ender.ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired;
            
            ender.Initialize(database,LoadStage.AdjustRaw);

            Assert.AreEqual(ExitCodeType.OperationNotRequired ,ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
        }

        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void TestEndLoadBecause_NoRows(DatabaseType type)
        {
            var database = GetCleanedServer(type, true);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fish");

            database.CreateTable("MyTable", dt);
            var ender = new PrematureLoadEnder();
            ender.ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase;
            ender.ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired;

            ender.Initialize(database, LoadStage.AdjustRaw);

            Assert.AreEqual(ExitCodeType.OperationNotRequired, ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
        }

        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void TestNoEnd_BecauseRows(DatabaseType type)
        {
            var database = GetCleanedServer(type, true);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fish");
            dt.Rows.Add("myval");

            database.CreateTable("MyTable", dt);
            var ender = new PrematureLoadEnder();
            ender.ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase;
            ender.ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired;

            ender.Initialize(database, LoadStage.AdjustRaw);

            Assert.AreEqual(ExitCodeType.Success, ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
        }
    }
}
