using FAnsi;
using Microsoft.Data.SqlClient;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using System.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class ExecuteCHIRedactionStageTests: DatabaseTests
{
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteCHIRedactionStage_Basic(DatabaseType dbType)
    {

        //var db = GetCleanedServer(dbType);
        //using (var con = db.Server.GetConnection())
        //{
        //    con.Open();

        //    var cmdCreateTable = db.Server.GetCommand(
        //        $"CREATE Table {db.GetRuntimeName()} (SomeValue varchar(10))", con);
        //    cmdCreateTable.ExecuteNonQuery();
        //    var data = db.Server.GetCommand(
        //        $"insert into {db.GetRuntimeName()} (SomeValue) values(1111111111) ", con);
        //    data.ExecuteNonQuery();
        //}

        //var job = Substitute.For<IDataLoadJob>();
        //var task = new ExecuteCHIRedactionStage(job, db, LoadStage.AdjustRaw);
        //task.Execute(true);
        //using (var con = db.Server.GetConnection())
        //{
        //    con.Open();

        //    var cmdCreateTable = db.Server.GetCommand(
        //        $"select SomeValue {db.GetRuntimeName()}", con);
        //   var dt = new DataTable();
        //    SqlDataAdapter da = new SqlDataAdapter((SqlCommand)cmdCreateTable);
        //    da.Fill(dt);
        //    Assert.That(dt.Rows[0][0], Is.EqualTo("##########"));
        //}
    }
}
