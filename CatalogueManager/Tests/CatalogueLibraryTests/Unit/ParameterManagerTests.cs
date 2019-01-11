using System.Linq;
using System.Reflection.Emit;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.QueryBuilding.Parameters;
using Fansi.Implementations.MicrosoftSQL;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    public class ParameterManagerTests
    {
        [Test]
        [TestCase(ParameterLevel.TableInfo,ParameterLevel.Global)]
        [TestCase(ParameterLevel.QueryLevel, ParameterLevel.Global)]
        [TestCase(ParameterLevel.TableInfo,ParameterLevel.CompositeQueryLevel)]
        [TestCase(ParameterLevel.TableInfo,ParameterLevel.QueryLevel)]
        public void FindOverridenParameters_OneOnlyTest(ParameterLevel addAt, ParameterLevel overridingLevel)
        {
            var myParameter = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",new MicrosoftQuerySyntaxHelper());
            var overridingParameter = new ConstantParameter("DECLARE @fish as int", "999", "overriding value",new MicrosoftQuerySyntaxHelper());

            var pm = new ParameterManager();
            pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(myParameter);
            pm.ParametersFoundSoFarInQueryGeneration[overridingLevel].Add(overridingParameter);

            var overrides = pm.GetOverridenParameters().ToArray();

            Assert.IsNull(pm.GetOverrideIfAnyFor(overridingParameter));
            Assert.AreEqual(pm.GetOverrideIfAnyFor(myParameter), overridingParameter);

            Assert.AreEqual(1,overrides.Length);
            Assert.AreEqual(myParameter, overrides[0]);
            var final = pm.GetFinalResolvedParametersList().ToArray();

            Assert.AreEqual(1, final.Length);
            Assert.AreEqual(overridingParameter, final[0]);
        }
        
        [Test]
        public void FindOverridenParameters_CaseSensitivityTest()
        {
            var baseParameter = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here", new MicrosoftQuerySyntaxHelper());
            var overridingParameter = new ConstantParameter("DECLARE @Fish as int", "3", "overriding value", new MicrosoftQuerySyntaxHelper());

            var pm = new ParameterManager();
            pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].Add(baseParameter);
            pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.QueryLevel].Add(overridingParameter);

            var parameters = pm.GetFinalResolvedParametersList().ToArray();

            Assert.AreEqual(1,parameters.Count());
            
            var final = parameters.Single();
            Assert.AreEqual("@Fish",final.ParameterName);
            Assert.AreEqual("3", final.Value);
        }

        [Test]
        public void FindOverridenParameters_TwoTest()
        {
            var myParameter1 = new ConstantParameter("DECLARE @fish as int", "1", "fishes be here",new MicrosoftQuerySyntaxHelper());
            var myParameter2 = new ConstantParameter("DECLARE @fish as int", "2", "fishes be here",new MicrosoftQuerySyntaxHelper());

            var overridingParameter = new ConstantParameter("DECLARE @fish as int", "3", "overriding value",new MicrosoftQuerySyntaxHelper());

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
            Assert.AreEqual(1,final.Length);
            Assert.AreEqual(overridingParameter, final[0]);
        }

        [Test]
        public void ParameterDeclarationAndDeconstruction()
        {
            var param = new ConstantParameter("DECLARE @Fish as int;","3","I've got a lovely bunch of coconuts",new MicrosoftQuerySyntaxHelper());
            var sql = QueryBuilder.GetParameterDeclarationSQL(param);

            Assert.AreEqual(@"/*I've got a lovely bunch of coconuts*/
DECLARE @Fish as int;
SET @Fish=3;
", sql);

            var after = ConstantParameter.Parse(sql, new MicrosoftQuerySyntaxHelper());

            Assert.AreEqual(param.ParameterSQL,after.ParameterSQL);
            Assert.AreEqual(param.Value, after.Value);
            Assert.AreEqual(param.Comment, after.Comment);
        }

    }
}
