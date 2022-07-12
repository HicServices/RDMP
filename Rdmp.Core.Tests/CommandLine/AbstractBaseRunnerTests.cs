using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System.Collections.Generic;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine
{
    public class AbstractBaseRunnerTests : UnitTests
    {
        [OneTimeSetUp]
        public void SetupMef()
        {
            SetupMEF();
        }
        
        [SetUp]
        public void CleanRemnants()
        {
            foreach (var o in Repository.GetAllObjectsInDatabase())
                o.DeleteInDatabase();
        }

        [Test]
        public void GetObjectFromCommandLineString_CatalogueByID()
        {
            var c = WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            var r = new TestRunner();
            Assert.AreEqual(c,r.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator,c.ID.ToString()));
        }

        [Test]
        public void GetObjectFromCommandLineString_CatalogueByPattern()
        {
            var c = WhenIHaveA<Catalogue>();
            c.Name = "gogogo";
            c.SaveToDatabase();

            WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            var r = new TestRunner();
            Assert.AreEqual(c, r.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator, "Catalogue:*go*"));
        }

        [Test]
        public void GetObjectFromCommandLineString_ProjectByID()
        {
            var c = WhenIHaveA<Project>();
            WhenIHaveA<Project>();
            WhenIHaveA<Project>();
            var r = new TestRunner();
            Assert.AreEqual(c, r.GetObjectFromCommandLineString<Project>(RepositoryLocator, c.ID.ToString()));
        }

        [Test]
        public void GetObjectFromCommandLineString_ProjectByPattern()
        {
            var c = WhenIHaveA<Project>();
            c.Name = "gogogo";
            c.SaveToDatabase();

            WhenIHaveA<Project>();
            WhenIHaveA<Project>();
            var r = new TestRunner();
            Assert.AreEqual(c, r.GetObjectFromCommandLineString<Project>(RepositoryLocator, "Project:*go*"));
        }

        /// <summary>
        /// Tests that things the user might enter for a parameter (or default parameter values specified in RDMP
        /// are going to be interpreted as null correctly
        /// </summary>
        /// <param name="expression"></param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("0")]
        [TestCase("null")]
        public void GetObjectFromCommandLineString_Null(string expression)
        {
            var c = WhenIHaveA<Catalogue>();
            c.Name = "gogogo";
            c.SaveToDatabase();

            WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            var r = new TestRunner();
            Assert.IsNull(r.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator, expression));
        }

        /// <summary>
        /// This test is for the IEnumerable version
        /// </summary>
        /// <param name="expression"></param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("0")]
        [TestCase("null")]
        public void GetObjectsFromCommandLineString_Null(string expression)
        {
            var c = WhenIHaveA<Catalogue>();
            c.Name = "gogogo";
            c.SaveToDatabase();

            WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            var r = new TestRunner();
            Assert.IsEmpty(r.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator, expression));
        }


        [Test]
        public void GetObjectsFromCommandLineString_CatalogueByID()
        {
            var c = WhenIHaveA<Catalogue>();
            var c2 = WhenIHaveA<Catalogue>();
            WhenIHaveA<Catalogue>();
            var r = new TestRunner();

            var results = r.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator,$"{c.ID},{c2.ID}").ToArray();

            Assert.AreEqual(2, results.Length);
            Assert.AreSame(c, results[0]);
            Assert.AreSame(c2, results[1]);
        }

        [Test]
        public void GetObjectsFromCommandLineString_CatalogueByPattern()
        {
            var c = WhenIHaveA<Catalogue>();
            c.Name = "go long";
            c.SaveToDatabase();

            var c2 = WhenIHaveA<Catalogue>();
            c2.Name = "go hard";
            c2.SaveToDatabase();
            
            WhenIHaveA<Catalogue>();

            var r = new TestRunner();
            var results = r.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator, "Catalogue:*go*").ToArray();

            Assert.AreEqual(2, results.Length);
            Assert.AreSame(c, results[0]);
            Assert.AreSame(c2, results[1]);
        }

        class TestRunner : Runner
        {
            new public T GetObjectFromCommandLineString<T>(IRDMPPlatformRepositoryServiceLocator locator, string arg) where T : IMapsDirectlyToDatabaseTable
            {
                return base.GetObjectFromCommandLineString<T>(locator, arg);
            }

            new public IEnumerable<T> GetObjectsFromCommandLineString<T>(IRDMPPlatformRepositoryServiceLocator locator, string arg) where T : IMapsDirectlyToDatabaseTable
            {
                return base.GetObjectsFromCommandLineString<T>(locator, arg);
            }

            public override int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
            {
                
                return 0;
            }
        }
    }
}
