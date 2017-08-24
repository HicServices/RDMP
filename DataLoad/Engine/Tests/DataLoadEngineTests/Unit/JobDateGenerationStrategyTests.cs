using System;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;

namespace DataLoadEngineTests.Unit
{
    public class JobDateGenerationStrategyTests
    {
        [Test]
        public void TestSingleScheduleConsecutiveDateStrategy()
        {
            var schedule = MockRepository.GenerateMock<ILoadProgress>();
            schedule.Stub(loadProgress => loadProgress.DataLoadProgress).Return(new DateTime(2015, 1, 1));

            var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

            var dates = strategy.GetDates(2,false);
            Assert.AreEqual(2, dates.Count);
            Assert.AreEqual(new DateTime(2015, 1, 2), dates[0]);
            Assert.AreEqual(new DateTime(2015, 1, 3), dates[1]);

            dates = strategy.GetDates(2,false);
            Assert.AreEqual(2, dates.Count);
            Assert.AreEqual(new DateTime(2015, 1, 4), dates[0]);
            Assert.AreEqual(new DateTime(2015, 1, 5), dates[1]);
        }

        [Test]
        public void TestSingleScheduleConsecutiveDateStrategy_FutureDates_AreForbidden()
        {
            var schedule = MockRepository.GenerateMock<ILoadProgress>();
            schedule.Stub(loadProgress => loadProgress.DataLoadProgress).Return(DateTime.Now.Date.AddDays(-2));//we have loaded up to day before yesterday

            
            var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

            var dates = strategy.GetDates(100, false);
            Assert.AreEqual(dates.Count, 1);
            Assert.AreEqual(dates[0], DateTime.Now.Date.AddDays(-1));//it should try to load yesterday
        }

        [Test]
        public void TestSingleScheduleConsecutiveDateStrategy_FutureDates_AreAllowed()
        {
            var schedule = MockRepository.GenerateMock<ILoadProgress>();
            schedule.Stub(loadProgress => loadProgress.DataLoadProgress).Return(DateTime.Now.Date.AddDays(-2));//we have loaded up to day before yesterday


            var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);
            
            var dates = strategy.GetDates(100, true);
            Assert.AreEqual(dates.Count, 100);
            Assert.AreEqual(dates[0], DateTime.Now.Date.AddDays(-1));//it should try to load yesterday
            Assert.AreEqual(dates[99], DateTime.Now.Date.AddDays(98));//it should try to load yesterday
        }
        [Test]
        public void TestListOfScheduleDatesFromCacheDirectory()
        {
            // todo: rewrite!
        }
    }
}
