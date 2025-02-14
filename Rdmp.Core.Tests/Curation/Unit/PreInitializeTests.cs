// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class PreInitializeTests
{
    private DataFlowPipelineContext<DataTable> context = new();
    private Fish fish = new();

    [Test]
    public void TestNormal()
    {
        var fishUser = new FishUser();

        Assert.That(fish, Is.Not.EqualTo(fishUser.IFish));
        context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser, fish);
        Assert.That(fish, Is.EqualTo(fishUser.IFish));
    }

    [Test]
    public void TestOneOFMany()
    {
        var fishUser = new FishUser();

        Assert.That(fish, Is.Not.EqualTo(fishUser.IFish));
        context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser, new object(), fish);
        Assert.That(fish, Is.EqualTo(fishUser.IFish));
    }

    [Test]
    public void TestCasting()
    {
        var fishUser = new FishUser();

        Assert.That(fish, Is.Not.EqualTo(fishUser.IFish));
        context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser, (IFish)fish);
        Assert.That(fish, Is.EqualTo(fishUser.IFish));
    }

    [Test]
    public void TestDownCasting()
    {
        var fishUser = new SpecificFishUser();

        IFish f = fish;
        Assert.That(fish, Is.Not.EqualTo(fishUser.IFish));
        context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser, f);
        Assert.That(fish, Is.EqualTo(fishUser.IFish));
    }

    [Test]
    public void TestNoObjects()
    {
        var fishUser = new SpecificFishUser();
        var ex = Assert.Throws<Exception>(() =>
            context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser));
        Assert.That(ex?.Message, Does.Contain("The following expected types were not passed to PreInitialize:Fish"));
    }

    [Test]
    public void TestWrongObjects()
    {
        var fishUser = new SpecificFishUser();
        var ex = Assert.Throws<Exception>(() =>
            context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, fishUser, new Penguin()));
        Assert.That(ex.Message, Does.Contain("The following expected types were not passed to PreInitialize:Fish"));
        Assert.That(ex.Message, Does.Contain("The object types passed were:"));
        Assert.That(ex.Message, Does.Contain("Penguin"));
    }


    private class FishUser : IPipelineRequirement<IFish>, IDataFlowComponent<DataTable>
    {
        public IFish IFish;

        public void PreInitialize(IFish value, IDataLoadEventListener listener)
        {
            IFish = value;
        }

        #region boiler plate

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    private class SpecificFishUser : IPipelineRequirement<Fish>, IDataFlowComponent<DataTable>
    {
        public IFish IFish;

        public void PreInitialize(Fish value, IDataLoadEventListener listener)
        {
            IFish = value;
        }

        #region boiler plate

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    private interface IFish
    {
        string GetFish();
    }

    private class Fish : IFish
    {
        public string GetFish() => "fish";
    }

    private class Penguin
    {
        public static string GetPenguin() => "Penguin";
    }
}