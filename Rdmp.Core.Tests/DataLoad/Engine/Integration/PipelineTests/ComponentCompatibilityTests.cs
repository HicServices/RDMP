// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests;

public class ComponentCompatibilityTests :UnitTests
{
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupMEF();
    }

    [Test]
    public void GetComponentsCompatibleWithBulkInsertContext()
    {
        var array = MEF.GetTypes<IDataFlowComponent<DataTable>>().ToArray();

        Assert.Greater(array.Count(),0);
    }

    [Test]
    public void HowDoesMEFHandleTypeNames()
    {
        var expected = "Rdmp.Core.DataFlowPipeline.IDataFlowSource(System.Data.DataTable)";

        Assert.AreEqual(expected, MEF.GetMEFNameForType(typeof(IDataFlowSource<DataTable>)));
    }
}