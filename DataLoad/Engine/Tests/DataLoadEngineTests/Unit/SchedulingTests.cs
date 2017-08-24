using System;
using CachingEngine.PipelineExecution.Destinations;
using DataLoadEngine.Job.Scheduling;
using NUnit.Framework;

namespace DataLoadEngineTests.Unit
{
    public class SchedulingTests
    {
        [Test]
        public void TestLoadDateCalculation_DayGranularity()
        {
            // We have cached a bit into 11/12/15
            var cacheFillProgress = new DateTime(2015, 12, 11, 15, 0, 0);

            // We should be loading up to and including 10/12/15, not touching 11/12/15
            var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Day, cacheFillProgress);

            Assert.AreEqual(new DateTime(2015, 12, 10, 0, 0, 0).Ticks, lastLoadDate.Ticks);
        }

        [Test]
        public void TestLoadDateCalculation_DayGranularityAtMonthBoundary()
        {
            // We have cached a bit into 1/12/15
            var cacheFillProgress = new DateTime(2015, 12, 1, 15, 0, 0);

            // We should be loading up to and including 30/11/15, not touching 1/12/15
            var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Day, cacheFillProgress);

            Assert.AreEqual(new DateTime(2015, 11, 30, 0, 0, 0).Ticks, lastLoadDate.Ticks);
        }

        [Test]
        public void TestLoadDateCalculation_HourGranularity()
        {
            // We have cached a bit into 11/12/15 15:00
            var cacheFillProgress = new DateTime(2015, 12, 11, 15, 30, 0);

            // We should be loading up to and including 11/12/15 14:00, not touching 11/12/15 15:00
            var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Hour, cacheFillProgress);

            Assert.AreEqual(new DateTime(2015, 12, 11, 14, 0, 0).Ticks, lastLoadDate.Ticks);
        }
    }
}
