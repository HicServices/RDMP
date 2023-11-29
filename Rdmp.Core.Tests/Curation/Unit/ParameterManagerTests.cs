// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using FAnsi.Implementations.MicrosoftSQL;
using NUnit.Framework;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.Parameters;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class ParameterManagerTests
{
    [Test]
    public void Test_ParameterManager_SimpleRename()
    {
        var p1 = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);
        var p2 = new ConstantParameter("DECLARE @fish as int", "2", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);

        var pm1 = new ParameterManager();
        var pm2 = new ParameterManager();
        var pm3 = new ParameterManager();

        pm1.ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Add(p1);
        pm2.ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Add(p2);

        pm3.ImportAndElevateResolvedParametersFromSubquery(pm1, out var renames1);
        pm3.ImportAndElevateResolvedParametersFromSubquery(pm2, out var renames2);

        var final = pm3.GetFinalResolvedParametersList().ToArray();

        //the final composite parameters should have a rename in them
        Assert.That(final[0].ParameterName, Is.EqualTo("@fish"));
        Assert.That(final[1].ParameterName, Is.EqualTo("@fish_2"));

        Assert.That(renames1, Is.Empty);

        Assert.That(renames2.Single().Key, Is.EqualTo("@fish"));
        Assert.That(renames2.Single().Value, Is.EqualTo("@fish_2"));
    }

    [Test]
    [TestCase(ParameterLevel.TableInfo, ParameterLevel.Global)]
    [TestCase(ParameterLevel.QueryLevel, ParameterLevel.Global)]
    [TestCase(ParameterLevel.TableInfo, ParameterLevel.CompositeQueryLevel)]
    [TestCase(ParameterLevel.TableInfo, ParameterLevel.QueryLevel)]
    public void FindOverridenParameters_OneOnlyTest(ParameterLevel addAt, ParameterLevel overridingLevel)
    {
        var myParameter = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);
        var overridingParameter = new ConstantParameter("DECLARE @fish as int", "999", "overriding value",
            MicrosoftQuerySyntaxHelper.Instance);

        var pm = new ParameterManager();
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(myParameter);
        pm.ParametersFoundSoFarInQueryGeneration[overridingLevel].Add(overridingParameter);

        var overrides = pm.GetOverridenParameters().ToArray();

        Assert.That(pm.GetOverrideIfAnyFor(overridingParameter), Is.Null);
        Assert.That(overridingParameter, Is.EqualTo(pm.GetOverrideIfAnyFor(myParameter)));

        Assert.That(overrides, Has.Length.EqualTo(1));
        Assert.That(overrides[0], Is.EqualTo(myParameter));
        var final = pm.GetFinalResolvedParametersList().ToArray();

        Assert.That(final, Has.Length.EqualTo(1));
        Assert.That(final[0], Is.EqualTo(overridingParameter));
    }

    [Test]
    public void FindOverridenParameters_CaseSensitivityTest()
    {
        var baseParameter = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);
        var overridingParameter = new ConstantParameter("DECLARE @Fish as int", "3", "overriding value",
            MicrosoftQuerySyntaxHelper.Instance);

        var pm = new ParameterManager();
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(baseParameter);
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Add(overridingParameter);

        var parameters = pm.GetFinalResolvedParametersList().ToArray();

        Assert.That(parameters, Has.Length.EqualTo(1));

        var final = parameters.Single();
        Assert.That(final.ParameterName, Is.EqualTo("@Fish"));
        Assert.That(final.Value, Is.EqualTo("3"));
    }

    [Test]
    public void FindOverridenParameters_TwoTest()
    {
        var myParameter1 = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);
        var myParameter2 = new ConstantParameter("DECLARE @fish as int", "2", "fishes be here",
            MicrosoftQuerySyntaxHelper.Instance);

        var overridingParameter = new ConstantParameter("DECLARE @fish as int", "3", "overriding value",
            MicrosoftQuerySyntaxHelper.Instance);

        var pm = new ParameterManager();
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(myParameter1);
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.CompositeQueryLevel].Add(myParameter2);
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].Add(overridingParameter);

        var overrides = pm.GetOverridenParameters().ToArray();

        Assert.That(pm.GetOverrideIfAnyFor(overridingParameter), Is.Null);
        Assert.That(overridingParameter, Is.EqualTo(pm.GetOverrideIfAnyFor(myParameter1)));
        Assert.That(overridingParameter, Is.EqualTo(pm.GetOverrideIfAnyFor(myParameter2)));

        Assert.That(overrides, Has.Length.EqualTo(2));
        Assert.That(overrides[0], Is.EqualTo(myParameter1));
        Assert.That(overrides[1], Is.EqualTo(myParameter2));

        var final = pm.GetFinalResolvedParametersList().ToArray();
        Assert.That(final, Has.Length.EqualTo(1));
        Assert.That(final[0], Is.EqualTo(overridingParameter));
    }

    [Test]
    public void ParameterDeclarationAndDeconstruction()
    {
        var param = new ConstantParameter("DECLARE @Fish as int;", "3", "I've got a lovely bunch of coconuts",
            MicrosoftQuerySyntaxHelper.Instance);
        var sql = QueryBuilder.GetParameterDeclarationSQL(param);

        Assert.That(sql, Is.EqualTo(@"/*I've got a lovely bunch of coconuts*/
DECLARE @Fish as int;
SET @Fish=3;
"));

        var after = ConstantParameter.Parse(sql, MicrosoftQuerySyntaxHelper.Instance);

        Assert.That(after.ParameterSQL, Is.EqualTo(param.ParameterSQL));
        Assert.That(after.Value, Is.EqualTo(param.Value));
        Assert.That(after.Comment, Is.EqualTo(param.Comment));
    }
}