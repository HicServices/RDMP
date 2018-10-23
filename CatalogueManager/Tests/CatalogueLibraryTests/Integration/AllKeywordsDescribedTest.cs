using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using DataQualityEngine.Data;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    [TestFixture]
    public class AllKeywordsDescribedTest :DatabaseTests
    {

        [OneTimeSetUp]
        public void SetupCommentStore()
        {

            CatalogueRepository.SuppressHelpLoading = false;
            CatalogueRepository.LoadHelp();
            CatalogueRepository.SuppressHelpLoading = true;

        }

        [Test]
        public void AllTablesDescribed()
        {
            //ensures the DQERepository gets a chance to add it's help text
            new DQERepository(CatalogueRepository);

            List<string> problems = new List<string>();

            List<Exception> ex;
            var databaseTypes = CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex).Where(t => typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && !t.Name.StartsWith("Spontaneous") && !t.Name.Contains("Proxy")).ToArray();


            foreach (var type in databaseTypes)
            {
                var docs = CatalogueRepository.CommentStore[type.Name]??CatalogueRepository.CommentStore["I"+type.Name];
                
                if(string.IsNullOrWhiteSpace(docs))
                    problems.Add("Type " + type.Name + " does not have an entry in the help dictionary (maybe the class doesn't have documentation? - try adding /// <summary> style comments to the class)");
                
            }
            foreach (string problem in problems)
                Console.WriteLine("Fatal Problem:" + problem);

            Assert.AreEqual(0,problems.Count);
        }

        [Test]
        public void AllForeignKeysDescribed()
        {
            List<string> allKeys = new List<string>();

            //ensures the DQERepository gets a chance to add it's help text
            new DQERepository(CatalogueRepository);

            allKeys.AddRange(GetForeignKeys(CatalogueRepository.DiscoveredServer));
            allKeys.AddRange(GetForeignKeys(DataExportRepository.DiscoveredServer));
            allKeys.AddRange(GetForeignKeys(new DiscoveredServer(DataQualityEngineConnectionString)));

            List<string> problems = new List<string>();
            foreach (string fkName in allKeys)
            {
                if (!CatalogueRepository.CommentStore.ContainsKey(fkName))
                    problems.Add(fkName + " is a foreign Key (which does not CASCADE) but does not have any HelpText");
            }
            
            foreach (string problem in problems)
                Console.WriteLine("Fatal Problem:" + problem);

            Assert.AreEqual(0, problems.Count, @"Add a description for each of these to \CatalogueManager\CatalogueLibrary\KeywordHelp.txt");
        }

        [Test]
        public void AllUserIndexesDescribed()
        {
            List<string> allIndexes = new List<string>();

            //ensures the DQERepository gets a chance to add it's help text
            new DQERepository(CatalogueRepository);

            allIndexes.AddRange(GetIndexes(CatalogueRepository.DiscoveredServer));
            allIndexes.AddRange(GetIndexes(DataExportRepository.DiscoveredServer));
            allIndexes.AddRange(GetIndexes(new DiscoveredServer(DataQualityEngineConnectionString)));

            List<string> problems = new List<string>();
            foreach (string idx in allIndexes)
            {
                if (!CatalogueRepository.CommentStore.ContainsKey(idx))
                    problems.Add(idx + " is an index but does not have any HelpText");
            }
            
            foreach (string problem in problems)
                Console.WriteLine("Fatal Problem:" + problem);

            Assert.AreEqual(0,problems.Count,@"Add a description for each of these to \CatalogueManager\CatalogueLibrary\KeywordHelp.txt");
            
        }

        private IEnumerable<string> GetForeignKeys(DiscoveredServer server)
        {
            using (var con = server.GetConnection())
            {
                con.Open();
                var r = server.GetCommand(@"select name from sys.foreign_keys where delete_referential_action = 0", con).ExecuteReader();

                while (r.Read())
                    yield return (string)r["name"];
            }
        }

        private IEnumerable<string> GetIndexes(DiscoveredServer server)
        {
            using (var con = server.GetConnection())
            {
                con.Open();
                var r = server.GetCommand(@"select si.name from sys.indexes si 
  JOIN sys.objects so ON si.[object_id] = so.[object_id]
  WHERE
  so.type = 'U'  AND is_primary_key = 0
  and si.name is not null
and so.name <> 'sysdiagrams'", con).ExecuteReader();

                while (r.Read())
                    yield return (string)r["name"];
            }
        }
        [OneTimeTearDown]
        public void unsetHelpDispel()
        {
            CatalogueRepository.SuppressHelpLoading = true;
        }
    }
}
