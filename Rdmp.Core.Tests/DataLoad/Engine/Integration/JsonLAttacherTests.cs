using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    class JsonLAttacherTests : DatabaseTests
    {
        private LoadDirectory LoadDirectory;
        DirectoryInfo parentDir;
        private DiscoveredDatabase _database;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            parentDir = workingDir.CreateSubdirectory("FlatFileAttacherTests");

            DirectoryInfo toCleanup = parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("Test_CSV_Attachment"));
            if (toCleanup != null)
                toCleanup.Delete(true);

            LoadDirectory = LoadDirectory.CreateDirectoryStructure(parentDir, "JsonLAttacherTests",true);

            // create a separate builder for setting an initial catalog on (need to figure out how best to stop child classes changing ServerICan... as this then causes TearDown to fail)
            _database = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            using (var con = _database.Server.GetConnection())
            {
                con.Open();

                var cmdCreateTable = _database.Server.GetCommand("CREATE Table " + _database.GetRuntimeName() + "..Bob([name] [varchar](500),[name2] [varchar](500))", con);
                cmdCreateTable.ExecuteNonQuery();
            }
        }

        [Test]
        public void SimpleJsonl_OneTable()
        {
            string json = @"
        {""name"": ""Gilbert"", ""wins"": [[""straight"", ""7♣""], [""one pair"", ""10♥""]]}
{""name"": ""Alexa"", ""wins"": [[""two pair"", ""4♠""], [""two pair"", ""9♠""]]}
{ ""name"": ""May"", ""wins"": []}
{ ""name"": ""Deloise"", ""wins"": [[""three of a kind"", ""5♣""]]}
";

            string filename = Path.Combine(LoadDirectory.ForLoading.FullName, "some.jsonl");
            File.WriteAllText(filename, json);

            var attacher = new JsonLAttacher();
            attacher.Initialize(LoadDirectory, _database);
            attacher.FilePattern = "some.jsonl";

            var player = _database.CreateTable("Player", new[] { 
                new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof(string), 10)){IsPrimaryKey = true},
                new DatabaseColumnRequest("wins", new DatabaseTypeRequest(typeof(string), int.MaxValue))
            });

            Import(player, out ITableInfo playerTi,out _);

            attacher.RootTable = playerTi;
                        
            //other cases (i.e. correct separator)
            attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

            Assert.AreEqual(4, player.GetRowCount());

            using (var con = _database.Server.GetConnection())
            {
                con.Open();
                using (var r = _database.Server.GetCommand("Select * from Player order by name", con).ExecuteReader())
                {
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual("Alexa", r["name"]);
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual("Deloise", r["name"]);
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual("Gilbert", r["name"]);
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual("May", r["name"]);
                }
            }

            attacher.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadEventListener());

            File.Delete(filename);
        }

    }
}
