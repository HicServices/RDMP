// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Implementations.MicrosoftSQL;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class SqlSyntaxHelperTests
{
    [Test]
    public void GetNullSubstituteTests()
    {

        var ti = Mock.Of<ITableInfo>(t=>t.GetQuerySyntaxHelper() == MicrosoftQuerySyntaxHelper.Instance);

        var pk = new PrimaryKeyCollisionResolver(ti);

        Assert.AreEqual("-999",pk.GetNullSubstituteForComparisonsWithDataType("decimal(3)", true));
        Assert.AreEqual("-9999999999", pk.GetNullSubstituteForComparisonsWithDataType("decimal(10)", true));
        Assert.AreEqual("-99.9", pk.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", true));
        Assert.AreEqual("-.9999", pk.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", true));


        Assert.AreEqual("999", pk.GetNullSubstituteForComparisonsWithDataType("decimal(3)", false));
        Assert.AreEqual("9999999999", pk.GetNullSubstituteForComparisonsWithDataType("decimal(10)", false));
        Assert.AreEqual("99.9", pk.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", false));
        Assert.AreEqual(".9999", pk.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", false));

    }

    [Test]
    public void SplitMethod()
    {
        var syntaxHelper = MicrosoftQuerySyntaxHelper.Instance;

        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("count(*)",out var method, out var contents);
            
        Assert.AreEqual("count",method);
        Assert.AreEqual("*",contents);

        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("count()", out method, out contents);

        Assert.AreEqual("count", method);
        Assert.AreEqual("", contents);


        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("LTRIM(RTRIM([Fish]))", out method, out contents);

        Assert.AreEqual("LTRIM", method);
        Assert.AreEqual("RTRIM([Fish])", contents);
    }
}