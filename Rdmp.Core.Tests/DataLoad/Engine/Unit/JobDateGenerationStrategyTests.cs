// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NSubstitute;
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
        var schedule =
            Substitute.For<ILoadProgress>();
        schedule.DataLoadProgress.Returns(new DateTime(2015, 1, 1));

        var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

        var dates = strategy.GetDates(2, false);
        Assert.That(dates, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(dates[0], Is.EqualTo(new DateTime(2015, 1, 2)));
            Assert.That(dates[1], Is.EqualTo(new DateTime(2015, 1, 3)));
        });

        dates = strategy.GetDates(2, false);
        Assert.That(dates, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(dates[0], Is.EqualTo(new DateTime(2015, 1, 4)));
            Assert.That(dates[1], Is.EqualTo(new DateTime(2015, 1, 5)));
        });
    }

    [Test]
    public void TestSingleScheduleConsecutiveDateStrategy_FutureDates_AreForbidden()
    {
        //we have loaded SetUp to day before yesterday
        var schedule =
            Substitute.For<ILoadProgress>();
        schedule.DataLoadProgress.Returns(DateTime.Now.Date.AddDays(-2));

        var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

        var dates = strategy.GetDates(100, false);
        Assert.Multiple(() =>
        {
            Assert.That(dates, Has.Count.EqualTo(1));
            Assert.That(DateTime.Now.Date.AddDays(-1), Is.EqualTo(dates[0])); //it should try to load yesterday
        });
    }

    [Test]
    public void TestSingleScheduleConsecutiveDateStrategy_FutureDates_AreAllowed()
    {
        var schedule =
            Substitute.For<ILoadProgress>();
        schedule.DataLoadProgress.Returns(
        DateTime.Now.Date.AddDays(-2)); //we have loaded SetUp to day before yesterday

        var strategy = new SingleScheduleConsecutiveDateStrategy(schedule);

        var dates = strategy.GetDates(100, true);
        Assert.Multiple(() =>
        {
            Assert.That(dates, Has.Count.EqualTo(100));
            Assert.That(DateTime.Now.Date.AddDays(-1), Is.EqualTo(dates[0])); //it should try to load yesterday
            Assert.That(DateTime.Now.Date.AddDays(98), Is.EqualTo(dates[99])); //it should try to load yesterday
        });
    }

    [Test]
    public void TestListOfScheduleDatesFromCacheDirectory()
    {
        // todo: rewrite!
    }
}