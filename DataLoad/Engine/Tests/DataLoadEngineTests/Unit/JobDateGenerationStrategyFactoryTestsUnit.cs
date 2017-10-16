using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.Job.Scheduling.Exceptions;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace DataLoadEngineTests.Unit
{
    public class JobDateGenerationStrategyFactoryTestsUnit
    {
        [Test]
        public void NoDates()
        {
            var lp = MockRepository.GenerateMock<ILoadProgress>();
            
            var server = new DiscoveredServer(new SqlConnectionStringBuilder("server=localhost;initial catalog=fish"));

            var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp), new HICDatabaseConfiguration(server));

            var ex = Assert.Throws<LoadOrCacheProgressUnclearException>(() => factory.Create(lp,new ThrowImmediatelyDataLoadEventListener()));

            Assert.AreEqual("Don't know when to start the data load, both DataLoadProgress and OriginDate are null", ex.Message);
        }

        [Test]
        public void DateKnown_NoCache_SuggestSingleScheduleConsecutiveDateStrategy()
        {
            var lp = MockRepository.GenerateMock<ILoadProgress>();
            lp.Expect(p => p.DataLoadProgress).Return(new DateTime(2001, 01, 01));

            var server = new DiscoveredServer(new SqlConnectionStringBuilder("server=localhost;initial catalog=fish"));

            var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp), new HICDatabaseConfiguration(server));

            Assert.AreEqual(typeof(SingleScheduleConsecutiveDateStrategy), factory.Create(lp,new ThrowImmediatelyDataLoadEventListener()).GetType());
        }
    }
}