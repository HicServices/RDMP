// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Implementations.MicrosoftSQL;
using NUnit.Framework;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class SqlSyntaxHelperTests
{
    [Test]
    public void GetNullSubstituteTests()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(3)", true), Is.EqualTo("-999"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(10)", true), Is.EqualTo("-9999999999"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", true), Is.EqualTo("-99.9"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", true), Is.EqualTo("-.9999"));


            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(3)", false), Is.EqualTo("999"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(10)", false), Is.EqualTo("9999999999"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", false), Is.EqualTo("99.9"));
            Assert.That(PrimaryKeyCollisionResolver.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", false), Is.EqualTo(".9999"));
        });
    }

    [Test]
    public void SplitMethod()
    {
        var syntaxHelper = MicrosoftQuerySyntaxHelper.Instance;

        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("count(*)", out var method, out var contents);

        Assert.Multiple(() =>
        {
            Assert.That(method, Is.EqualTo("count"));
            Assert.That(contents, Is.EqualTo("*"));
        });

        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("count()", out method, out contents);

        Assert.Multiple(() =>
        {
            Assert.That(method, Is.EqualTo("count"));
            Assert.That(contents, Is.EqualTo(""));
        });


        syntaxHelper.SplitLineIntoOuterMostMethodAndContents("LTRIM(RTRIM([Fish]))", out method, out contents);

        Assert.Multiple(() =>
        {
            Assert.That(method, Is.EqualTo("LTRIM"));
            Assert.That(contents, Is.EqualTo("RTRIM([Fish])"));
        });
    }
}