using FAnsi;
using Microsoft.Data.SqlClient;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class ExecuteCHIRedactionStageTests: DatabaseTests
{
    //[TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteCHIRedactionStage_Basic(DatabaseType dbType)
    {

        var db = GetCleanedServer(dbType);
        var dt = new DataTable("Test_Redaction");
        dt.Columns.Add("SomeValue");
        dt.Columns.Add("SomeOtherValue");
        dt.Rows.Add("1111111111","F1111111111");

        var tbl = db.CreateTable(dt.TableName, dt);
        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var tableInfo, out var colInfos);
        var cat = new Catalogue(CatalogueRepository,"Test_Redaction");
        cat.SaveToDatabase();
        var ci = new CatalogueItem(CatalogueRepository,cat, "SomeValue");
        ci.SaveToDatabase();
        var coi = new ColumnInfo(CatalogueRepository, "Test_Redaction.SomeValue", "varchar(10)",tableInfo);
        coi.IsPrimaryKey = true;
        coi.SaveToDatabase();
        ci.ColumnInfo_ID = coi.ID;
        ci.SaveToDatabase();

        var ci2 = new CatalogueItem(CatalogueRepository, cat, "SomeOtherValue");
        ci2.SaveToDatabase();
        var coi2 = new ColumnInfo(CatalogueRepository, "Test_Redaction.SomeOtherValue", "varchar(11)", tableInfo);
        coi2.IsPrimaryKey = false;
        coi2.SaveToDatabase();
        ci2.ColumnInfo_ID = coi.ID;
        ci2.SaveToDatabase();



        var job = Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo>() {tableInfo });
        job.RepositoryLocator.Returns(RepositoryLocator);
        job.Configuration.Returns(new HICDatabaseConfiguration(db.Server, null, null, null));
        //to the job is junk
        var task = new ExecuteCHIRedactionStage(job, db, LoadStage.AdjustRaw);
        task.Execute(true);
        using (var con = db.Server.GetConnection())
        {
            con.Open();

            var cmdCreateTable = db.Server.GetCommand(
                $"select SomeOtherValue from {db.GetRuntimeName()}.dbo.Test_Redaction", con);
            dt.Dispose();
            dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter((SqlCommand)cmdCreateTable);
            da.Fill(dt);
            da.Dispose();
            Assert.That(dt.Rows[0][0], Is.EqualTo("F#########1"));
        }
        dt.Dispose();
    }
}
