﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.ArgumentTests;

public class ArgumentTypeTests : UnitTests
{
    private object[] _expectedAnswers =
    {
        5,
        new CultureInfo("en-us"),
        CultureInfo.CurrentCulture
    };

    [TestCase(typeof(int), "5", 0)]
    [TestCase(typeof(CultureInfo), "en-us", 1)]
    public void Test_Type_WithStringValue(Type t, string val, int expectedAnswerIdx)
    {
        var arg = WhenIHaveA<ProcessTaskArgument>();

        arg.SetType(t);
        arg.Value = val;

        Assert.That(arg.GetValueAsSystemType(), Is.EqualTo(_expectedAnswers[expectedAnswerIdx]));
    }

    [Test]
    public void TestClassDemandingDouble_CreateArgumentsForClassIfNotExists()
    {
        var args = WhenIHaveA<ProcessTask>().CreateArgumentsForClassIfNotExists<TestClassDemandingDouble>();

        Assert.Multiple(() =>
        {
            Assert.That(args.Single().GetValueAsSystemType(), Is.EqualTo(1.0));
            Assert.That(args.Single().Value, Is.EqualTo("1"));
        });
    }

    private class TestClassDemandingDouble
    {
        [DemandsInitialization("some field", defaultValue: 1)]
        public double MyVar { get; set; }
    }
}