// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit;

public class PermissionWindowTests : DatabaseTests
{
    [Test]
    public void TestSerialisation()
    {
        var period1 = new PermissionWindowPeriod((int)DayOfWeek.Monday, new TimeSpan(0, 0, 0), new TimeSpan(6, 0, 0));
        var period2 =
            new PermissionWindowPeriod((int)DayOfWeek.Monday, new TimeSpan(17, 0, 0), new TimeSpan(23, 59, 59));

        var permissionWindow = new PermissionWindow(CatalogueRepository);
        permissionWindow.SetPermissionWindowPeriods(new List<PermissionWindowPeriod>
        {
            period1,
            period2
        });

        var permissionPeriodConfig = permissionWindow.PermissionPeriodConfig;

        var newPermissionWindow = new PermissionWindow(CatalogueRepository)
        {
            PermissionPeriodConfig = permissionPeriodConfig
        };

        var periods = newPermissionWindow.PermissionWindowPeriods;
        Assert.That(periods, Has.Count.EqualTo(2));

        var newPeriod1 = periods[0];
        Assert.Multiple(() =>
        {
            Assert.That(newPeriod1.DayOfWeek, Is.EqualTo((int)DayOfWeek.Monday));

            Assert.That(newPeriod1.End.Hours, Is.EqualTo(6));
        });

        var newPeriod2 = periods[1];
        Assert.That(newPeriod2.Start.Hours, Is.EqualTo(17));
    }

    [Test]
    public void TestCurrentlyWithinPermissionPeriod()
    {
        var dtNow = DateTime.UtcNow;

        if (dtNow is { Hour: 23, Minute: >= 40 } or { Hour: 0, Minute: <= 5 })
            Assert.Inconclusive("This test cannot run at midnight since it is afraid of the dark");

        var fiveMinutes = new TimeSpan(0, 5, 0);

        var utcTime = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
        var period1 = new PermissionWindowPeriod((int)DateTime.UtcNow.DayOfWeek, utcTime.Subtract(fiveMinutes),
            utcTime.Add(fiveMinutes));

        var permissionWindow = new PermissionWindow(CatalogueRepository);
        permissionWindow.SetPermissionWindowPeriods(new List<PermissionWindowPeriod> { period1 });
        Assert.That(permissionWindow.WithinPermissionWindow());
    }

    [Test]
    public void TestCurrentlyOutsidePermissionPeriod()
    {
        var dtNow = DateTime.UtcNow;

        if (dtNow is { Hour: 23, Minute: >= 50 } or { Hour: 0, Minute: <= 3 })
            Assert.Inconclusive("This test cannot run at midnight since it is afraid of the dark");

        var oneMinute = new TimeSpan(0, 1, 0);
        var utcTime = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
        var period1 =
            new PermissionWindowPeriod((int)DateTime.Now.DayOfWeek, utcTime.Add(oneMinute), utcTime.Add(oneMinute));

        var permissionWindow = new PermissionWindow(CatalogueRepository);
        permissionWindow.SetPermissionWindowPeriods(new List<PermissionWindowPeriod> { period1 });
        Assert.That(permissionWindow.WithinPermissionWindow(), Is.False);
    }
}