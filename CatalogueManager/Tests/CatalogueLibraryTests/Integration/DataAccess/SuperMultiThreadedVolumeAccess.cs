using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.DataAccess
{
    public class SuperMultiThreadedVolumeAccess:DatabaseTests
    {
        int _timeoutBefore;

        [SetUp]
        public void SetUpDeleteRemnants()
        {
            
            _timeoutBefore = DatabaseCommandHelper.GlobalTimeout;
            DatabaseCommandHelper.GlobalTimeout = 60;
            DeleteRemants();
        }

        private void DeleteRemants()
        {
            foreach (
                Catalogue catalogue in
                    CatalogueRepository.GetAllObjects<Catalogue>()
                        .Where(c => c.Name.StartsWith("SuperMultiThreadedTestCatalogue")))
                catalogue.DeleteInDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseCommandHelper.GlobalTimeout = _timeoutBefore;
            DeleteRemants();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SingleThreadedBulkCatalogueCreation(bool useTransactions)
        {
            if (useTransactions)
                CatalogueRepository.BeginNewTransactedConnection();
            
            //create lots of catalogues
            for(int i=0;i<30;i++)
            {
                var cata = new Catalogue(CatalogueRepository, "SuperMultiThreadedTestCatalogue" + Guid.NewGuid());
                var copy = CatalogueRepository.GetObjectByID<Catalogue>(cata.ID);

                copy.Description = "fish";
                Assert.IsTrue(copy.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent);
                
                copy.SaveToDatabase();
                Assert.IsTrue(copy.HasLocalChanges().Evaluation == ChangeDescription.NoChanges);
            }

            //now fetch them out of database lots of times
            for (int i = 0; i < 100; i++)
                CatalogueRepository.GetAllCatalogues(true);

            if (useTransactions)
                CatalogueRepository.EndTransactedConnection(false);

        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void MultiThreaded(bool useTransactions)
        {
            FireMultiThreaded(SingleThreadedBulkCatalogueCreation, 5, useTransactions);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SimpleCaseSingleThreaded(bool useTransaction)
        {
            IManagedConnection con;

            if (useTransaction)
                con = RepositoryLocator.CatalogueRepository.BeginNewTransactedConnection();
            else
                con = RepositoryLocator.CatalogueRepository.GetConnection();

            Assert.AreEqual(ConnectionState.Open,con.Connection.State);
            Thread.Sleep(1000);

            if (useTransaction)
                RepositoryLocator.CatalogueRepository.EndTransactedConnection(false);
            else
                con.Connection.Close();

            Assert.AreEqual(ConnectionState.Closed, con.Connection.State);

            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SimpleCaseMultiThreaded(bool useTransactions)
        {
            FireMultiThreaded(SimpleCaseSingleThreaded, 50, useTransactions);
        }

        private void FireMultiThreaded(Action<bool> method, int numberToFire, bool useTransactions)
        {
            
            List<Exception> exes = new List<Exception>();
            

            List<Thread> ts = new List<Thread>();

            for (int i = 0; i < numberToFire; i++)
            {
                int i1 = i;
                ts.Add(new Thread(() => {
                    try
                    {

                        method(useTransactions && i1 == 0);
                    }
                    catch (Exception ex)
                    {
                        exes.Add(ex);
                    }
                }));
            }

            foreach (Thread thread in ts)
                thread.Start();
            
            while(ts.Any(t=>t.IsAlive))
                Thread.Sleep(100);
            
            Assert.IsEmpty(exes);
        }
    }
}
