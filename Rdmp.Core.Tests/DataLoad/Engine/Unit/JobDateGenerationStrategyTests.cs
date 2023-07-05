// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
public class JobDateGenerationStrategyTests
{
    [Test]
    public void TestSingleScheduleConsecutiveDateStrategy()
    {
        var schedule = Mock.Of<ILoadProgress>(loadProgress => loadProgress.DataLoadProgress == new DateTime(2015, 1, 1));

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
        //we have loaded SetUp to day before yesterday
        var schedule = Mock.Of<ILoadProgress>(loadProgress => loadProgress.DataLoadProgress == DateTime.Now.Date.AddDays(-2));

        var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

        var dates = strategy.GetDates(100, false);
        Assert.AreEqual(dates.Count, 1);
        Assert.AreEqual(dates[0], DateTime.Now.Date.AddDays(-1));//it should try to load yesterday
    }

    [Test]
    public void TestSingleScheduleConsecutiveDateStrategy_FutureDates_AreAllowed()
    {
        var schedule = Mock.Of<ILoadProgress>(loadProgress => loadProgress.DataLoadProgress == DateTime.Now.Date.AddDays(-2));//we have loaded SetUp to day before yesterday

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