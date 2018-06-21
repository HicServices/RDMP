using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Registration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.MainFormUITabs.SubComponents;
using DataLoadEngine.DataFlowPipeline.Destinations;
using DataLoadEngine.Job;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.CrossPlatformParameterTests
{
    public class BasicParameterUseTests:DatabaseTests
    {
        [Test]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void Test_DatabaseTypeQueryWithParameter_IntParameter(DatabaseType dbType)
        {
            //Pick the destination server
            var tableName = TestDatabaseNames.GetConsistentName("tbl");
            
            //make sure theres a database ready to receive the data
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
                uploader.PreInitialize(db,new ThrowImmediatelyDataLoadJob());
                uploader.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
                uploader.Dispose(new ThrowImmediatelyDataLoadJob(),null );

                var tbl = db.ExpectTable(tableName);

                var importer = new TableInfoImporter(CatalogueRepository, tbl);
                TableInfo ti;
                ColumnInfo[] ci;
                importer.DoImport(out ti,out ci);
            
                var engineer = new ForwardEngineerCatalogue(ti, ci,true);
                Catalogue cata;
                CatalogueItem[] cis;
                ExtractionInformation[] ei;
                engineer.ExecuteForwardEngineering(out cata, out cis, out ei);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////

                /////////////////////////////////THE ACTUAL PROPER TEST////////////////////////////////////
                //create an extraction filter
                var extractionInformation = ei.Single();
                var filter = new ExtractionFilter(CatalogueRepository, "Filter by numbers", extractionInformation);
                filter.WhereSQL = extractionInformation.SelectSQL + " = @n";
                filter.SaveToDatabase();

                //create the parameters for filter (no globals, masters or scope adjacent parameters)
                new ParameterCreator(filter.GetFilterFactory(), null, null).CreateAll(filter,null);

                var p = filter.GetAllParameters().Single();
                Assert.AreEqual("@n",p.ParameterName);
                p.ParameterSQL = p.ParameterSQL.Replace("varchar(50)", "int"); //make it int
                p.Value = "20";
                p.SaveToDatabase();
                
                var qb = new QueryBuilder(null, null);
                qb.AddColumn(extractionInformation);
                qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(null,new []{filter},FilterContainerOperation.AND);
            
                using(var con = db.Server.GetConnection())
                {
                    con.Open();

                    string sql = qb.SQL;

                    var cmd = db.Server.GetCommand(sql, con);
                    var r = cmd.ExecuteReader();
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual(
                        20,
                        r[extractionInformation.GetRuntimeName()]);
                }
                ///////////////////////////////////////////////////////////////////////////////////////
            }
            finally
            {
                db.ForceDrop();
            }
        }
    }
}
