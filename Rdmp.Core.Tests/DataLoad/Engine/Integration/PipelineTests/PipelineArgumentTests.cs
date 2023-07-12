// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Pipelines;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests;

public class PipelineArgumentTests : DatabaseTests
{
    [Test]
    [TestCase(typeof(int?), null)]
    [TestCase(typeof(float?), null)]
    [TestCase(typeof(double?), null)]
    [TestCase(typeof(char?), null)]
    [TestCase(typeof(DateTime?), null)]


    [TestCase(typeof(int?), 3)]
    [TestCase(typeof(float?), 10.01f)]
    [TestCase(typeof(double?), 10.999)]
    [TestCase(typeof(char?), 'K')]
    [TestCase(typeof(DateTime?), "now")] //sadly can't pass DateTime.Now
    public void TestIArgumentsForNullableTypes(Type nullableType, object value)
    {
        if (string.Equals(value as string, "now")) //sadly can't pass DateTime.Now
            value = new DateTime(2001, 01, 01, 3, 20, 11); //hey btw when you put in milliseconds into DateTime IArgument it drops them... due to DateTime.Parse? or DateTime.ToString()?

        var p = new Pipeline(CatalogueRepository);
        var pc = new PipelineComponent(CatalogueRepository, p,
            GetType() //Normally this would be the PipelineComponent hosted class which would be a proper class declared as a MEF export with DemandsInitialization etc but we don't need all that
            , 0
            , "My imaginary Pipe Component");
        var arg = new PipelineComponentArgument(CatalogueRepository, pc);
        try
        {
            arg.SetType(nullableType);
            arg.SetValue(value);

            Assert.AreEqual(nullableType, arg.GetSystemType());
            Assert.AreEqual(value, arg.GetValueAsSystemType());
        }
        finally
        {
            p.DeleteInDatabase();
        }
    }
}