// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Exceptions;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Operations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.DilutionTests;

[Category("Unit")]
public class DilutionCheckTests
{
    [Test]
    public void TestChecking_RoundDateToMiddleOfQuarter_NoColumnSet()
    {
        var dil = new RoundDateToMiddleOfQuarter();
        Assert.Throws<DilutionColumnNotSetException>(() => dil.Check(ThrowImmediatelyCheckNotifier.Quiet));
    }

    [TestCase("varchar(10)")]
    [TestCase("bit")]
    [TestCase("binary(50)")]
    public void TestChecking_RoundDateToMiddleOfQuarter_WrongDataType(string incompatibleType)
    {
        var col = Mock.Of<IPreLoadDiscardedColumn>(p => p.SqlDataType == incompatibleType);

        var dil = new RoundDateToMiddleOfQuarter
        {
            ColumnToDilute = col
        };

        Assert.Throws<Exception>(() => dil.Check(ThrowImmediatelyCheckNotifier.Quiet));
    }

    [TestCase("date")]
    [TestCase("datetime")]
    public void TestChecking_RoundDateToMiddleOfQuarter_CompatibleDataType(string incompatibleType)
    {
        var col = Mock.Of<IPreLoadDiscardedColumn>(p => p.SqlDataType == incompatibleType);

        var dil = new RoundDateToMiddleOfQuarter
        {
            ColumnToDilute = col
        };

        dil.Check(ThrowImmediatelyCheckNotifier.Quiet);
    }
}