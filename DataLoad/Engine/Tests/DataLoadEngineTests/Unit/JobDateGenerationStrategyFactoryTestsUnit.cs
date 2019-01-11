using System;
using CatalogueLibrary.Data;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.Job.Scheduling.Exceptions;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using NUnit.Framework;
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
            
            var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp));

            var ex = Assert.Throws<LoadOrCacheProgressUnclearException>(() => factory.Create(lp,new ThrowImmediatelyDataLoadEventListener()));

            Assert.AreEqual("Don't know when to start the data load, both DataLoadProgress and OriginDate are null", ex.Message);
        }

        [Test]
        public void DateKnown_NoCache_SuggestSingleScheduleConsecutiveDateStrategy()
        {
            var lp = MockRepository.GenerateMock<ILoadProgress>();
            lp.Expect(p => p.DataLoadProgress).Return(new DateTime(2001, 01, 01));
            
            var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp));

            Assert.AreEqual(typeof(SingleScheduleConsecutiveDateStrategy), factory.Create(lp,new ThrowImmediatelyDataLoadEventListener()).GetType());
        }
    }
}