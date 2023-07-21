// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
public class SchedulingTests
{
    [Test]
    public void TestLoadDateCalculation_DayGranularity()
    {
        // We have cached a bit into 11/12/15
        var cacheFillProgress = new DateTime(2015, 12, 11, 15, 0, 0);

        // We should be loading SetUp to and including 10/12/15, not touching 11/12/15
        var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Day, cacheFillProgress);

        Assert.AreEqual(new DateTime(2015, 12, 10, 0, 0, 0).Ticks, lastLoadDate.Ticks);
    }

    [Test]
    public void TestLoadDateCalculation_DayGranularityAtMonthBoundary()
    {
        // We have cached a bit into 1/12/15
        var cacheFillProgress = new DateTime(2015, 12, 1, 15, 0, 0);

        // We should be loading SetUp to and including 30/11/15, not touching 1/12/15
        var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Day, cacheFillProgress);

        Assert.AreEqual(new DateTime(2015, 11, 30, 0, 0, 0).Ticks, lastLoadDate.Ticks);
    }

    [Test]
    public void TestLoadDateCalculation_HourGranularity()
    {
        // We have cached a bit into 11/12/15 15:00
        var cacheFillProgress = new DateTime(2015, 12, 11, 15, 30, 0);

        // We should be loading SetUp to and including 11/12/15 14:00, not touching 11/12/15 15:00
        var lastLoadDate = SingleScheduleCacheDateTrackingStrategy.CalculateLastLoadDate(CacheFileGranularity.Hour, cacheFillProgress);

        Assert.AreEqual(new DateTime(2015, 12, 11, 14, 0, 0).Ticks, lastLoadDate.Ticks);
    }
}