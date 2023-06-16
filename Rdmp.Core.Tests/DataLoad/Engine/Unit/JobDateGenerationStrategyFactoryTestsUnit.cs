// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling.Exceptions;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
public class JobDateGenerationStrategyFactoryTestsUnit : UnitTests
{
    [Test]
    public void NoDates()
    {
        var lp = WhenIHaveA<LoadProgress>();
        var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp));
        var ex = Assert.Throws<LoadOrCacheProgressUnclearException>(() =>
                   factory.Create(lp, new ThrowImmediatelyDataLoadEventListener()));

        var ex = Assert.Throws<LoadOrCacheProgressUnclearException>(() => factory.Create(lp,ThrowImmediatelyDataLoadEventListener.Quiet));

        Assert.AreEqual("Don't know when to start the data load, both DataLoadProgress and OriginDate are null", ex.Message);
    }

    [Test]
    public void DateKnown_NoCache_SuggestSingleScheduleConsecutiveDateStrategy()
    {
        var lp = WhenIHaveA<LoadProgress>();
        lp.DataLoadProgress = new DateTime(2001, 01, 01);

        var factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(lp));

        Assert.AreEqual(typeof(SingleScheduleConsecutiveDateStrategy), factory.Create(lp,ThrowImmediatelyDataLoadEventListener.Quiet).GetType());
    }
}