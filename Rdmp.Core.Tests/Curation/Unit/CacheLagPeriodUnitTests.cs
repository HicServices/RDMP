// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cache;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class CacheLagPeriodUnitTests
{
    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", "NUnit2043:Use ComparisonConstraint for better assertion messages in case of failure", Justification = "<Pending>")]
    public void TestOperator()
    {
        Assert.Multiple(static () =>
        {
            Assert.That(new TimeSpan(32, 0, 0, 0) > new CacheLagPeriod("1m"));
            Assert.That(new TimeSpan(24, 0, 0, 0) < new CacheLagPeriod("1m"));

            Assert.That(new TimeSpan(3, 0, 0, 0) > new CacheLagPeriod("2d"));
            Assert.That(new TimeSpan(3, 0, 0, 0) > new CacheLagPeriod("3d"), Is.False);
            Assert.That(new TimeSpan(2, 0, 0, 0) < new CacheLagPeriod("2d"), Is.False);
            Assert.That(new TimeSpan(1, 0, 0, 0) < new CacheLagPeriod("2d"));

            Assert.That(new TimeSpan(2, 0, 0, 1) < new CacheLagPeriod("2d"), Is.False);
            Assert.That(new TimeSpan(2, 0, 0, 0) < new CacheLagPeriod("2d"), Is.False);
            Assert.That(new TimeSpan(2, 0, 0, 1) > new CacheLagPeriod("2d"));
        });
    }
}