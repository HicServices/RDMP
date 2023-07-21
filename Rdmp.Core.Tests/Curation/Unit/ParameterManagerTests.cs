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
        Assert.AreEqual("@fish", final[0].ParameterName);
        Assert.AreEqual("@fish_2", final[1].ParameterName);

        Assert.IsEmpty(renames1);

        Assert.AreEqual("@fish", renames2.Single().Key);
        Assert.AreEqual("@fish_2", renames2.Single().Value);
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

        Assert.IsNull(pm.GetOverrideIfAnyFor(overridingParameter));
        Assert.AreEqual(pm.GetOverrideIfAnyFor(myParameter), overridingParameter);

        Assert.AreEqual(1, overrides.Length);
        Assert.AreEqual(myParameter, overrides[0]);
        var final = pm.GetFinalResolvedParametersList().ToArray();

        Assert.AreEqual(1, final.Length);
        Assert.AreEqual(overridingParameter, final[0]);
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

        Assert.AreEqual(1, parameters.Length);

        var final = parameters.Single();
        Assert.AreEqual("@Fish", final.ParameterName);
        Assert.AreEqual("3", final.Value);
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

        Assert.IsNull(pm.GetOverrideIfAnyFor(overridingParameter));
        Assert.AreEqual(pm.GetOverrideIfAnyFor(myParameter1), overridingParameter);
        Assert.AreEqual(pm.GetOverrideIfAnyFor(myParameter2), overridingParameter);

        Assert.AreEqual(2, overrides.Length);
        Assert.AreEqual(myParameter1, overrides[0]);
        Assert.AreEqual(myParameter2, overrides[1]);

        var final = pm.GetFinalResolvedParametersList().ToArray();
        Assert.AreEqual(1, final.Length);
        Assert.AreEqual(overridingParameter, final[0]);
    }

    [Test]
    public void ParameterDeclarationAndDeconstruction()
    {
        var param = new ConstantParameter("DECLARE @Fish as int;", "3", "I've got a lovely bunch of coconuts",
            MicrosoftQuerySyntaxHelper.Instance);
        var sql = QueryBuilder.GetParameterDeclarationSQL(param);

        Assert.AreEqual(@"/*I've got a lovely bunch of coconuts*/
DECLARE @Fish as int;
SET @Fish=3;
", sql);

        var after = ConstantParameter.Parse(sql, MicrosoftQuerySyntaxHelper.Instance);

        Assert.AreEqual(param.ParameterSQL, after.ParameterSQL);
        Assert.AreEqual(param.Value, after.Value);
        Assert.AreEqual(param.Comment, after.Comment);
    }
}