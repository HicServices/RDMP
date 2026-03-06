using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction
{

    internal class MSSqlMergeDestination_Test: MSSqlMergeDestination
    {
        public void Execute(DataTable dt) {
            Assert.DoesNotThrow(()=>WriteRows(dt, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken(),new System.Diagnostics.Stopwatch()));

        }
    }

    public class MSSQLMergeDestinationTests: TestsRequiringAnExtractionConfiguration
    {
        //create table first time
        [Test]
        public void MSSQLMerge_Creates_Table()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Creates_Table";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey= new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
        }
        //merge in new data
        [Test]
        public void MSSQLMerge_Merge_Data()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Data";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination.Execute(dt);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            tbl.Drop();
        }
        // merge in data with dupicates
        [Test]
        public void MSSQLMerge_Merge_Update()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Update";
            destination.DeleteMergeTempTable= true;
            destination.PreInitialize(new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination.Execute(dt);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            dt.Rows.Add("10", "thr");
            destination.Execute(dt);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            Assert.That(tbl.GetDataTable().Rows[1].ItemArray, Is.EqualTo(new List<object>() { 10, "thr" }));
            Assert.That(tbl.GetDataTable().Rows[0].ItemArray, Is.EqualTo(new List<object>() { 2, "two" }));
            tbl.Drop();
        }
        //megre in with perform delete
        [Test]
        public void MSSQLMerge_Merge_Delete()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Delete";
            destination.DeleteMergeTempTable = true;
            destination.AllowMergeToPerformDeletes = true;
            destination.PreInitialize(new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination.Execute(dt);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            Assert.That(tbl.GetDataTable().Rows[0].ItemArray, Is.EqualTo(new List<object>() {2, "two" }));
            tbl.Drop();
        }
    }
}
