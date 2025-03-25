// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.CrossPlatformParameterTests;

public class BasicParameterUseTests : DatabaseTests
{
    [Test]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void Test_DatabaseTypeQueryWithParameter_IntParameter(DatabaseType dbType)
    {
        //Pick the destination server
        var tableName = TestDatabaseNames.GetConsistentName("tbl");

        //make sure there's a database ready to receive the data
        var db = GetCleanedServer(dbType);
        db.Create(true);


        //this is the table we are uploading
        var dt = new DataTable();
        dt.Columns.Add("numbercol");
        dt.Rows.Add(10);
        dt.Rows.Add(15);
        dt.Rows.Add(20);
        dt.Rows.Add(25);
        dt.TableName = tableName;
        try
        {
            ///////////////////////UPLOAD THE DataTable TO THE DESTINATION////////////////////////////////////////////
            var uploader = new DataTableUploadDestination();
            uploader.PreInitialize(db, new ThrowImmediatelyDataLoadJob());
            uploader.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
            uploader.Dispose(new ThrowImmediatelyDataLoadJob(), null);

            var tbl = db.ExpectTable(tableName);

            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            importer.DoImport(out var ti, out var ci);

            var engineer = new ForwardEngineerCatalogue(ti, ci);
            engineer.ExecuteForwardEngineering(out _, out _, out var ei);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            /////////////////////////////////THE ACTUAL PROPER TEST////////////////////////////////////
            //create an extraction filter
            var extractionInformation = ei.Single();
            var filter = new ExtractionFilter(CatalogueRepository, "Filter by numbers", extractionInformation)
            {
                WhereSQL = $"{extractionInformation.SelectSQL} = @n"
            };
            filter.SaveToDatabase();

            //create the parameters for filter (no globals, masters or scope adjacent parameters)
            new ParameterCreator(filter.GetFilterFactory(), null, null).CreateAll(filter, null);

            var p = filter.GetAllParameters().Single();
            Assert.That(p.ParameterName, Is.EqualTo("@n"));
            p.ParameterSQL = p.ParameterSQL.Replace("varchar(50)", "int"); //make it int
            p.Value = "20";
            p.SaveToDatabase();

            var qb = new QueryBuilder(null, null);
            qb.AddColumn(extractionInformation);
            qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(new MemoryCatalogueRepository(), null,
                new[] { filter }, FilterContainerOperation.AND);

            using var con = db.Server.GetConnection();
            con.Open();

            var sql = qb.SQL;

            var cmd = db.Server.GetCommand(sql, con);
            var r = cmd.ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(
                    r[extractionInformation.GetRuntimeName()], Is.EqualTo(20));
            });
            ///////////////////////////////////////////////////////////////////////////////////////
        }
        finally
        {
            db.Drop();
        }
    }
}