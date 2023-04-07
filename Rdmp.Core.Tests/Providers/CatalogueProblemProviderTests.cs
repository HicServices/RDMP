// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Checks;
using Tests.Common;
using Rdmp.Core.Curation.Data.Aggregation;
using System.Globalization;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Tests.Providers;

class CatalogueProblemProviderTests : UnitTests
{
    #region ROOT CONTAINERS
    [Test]
    public void TestRootOrderCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();
            
        container.Operation = SetOperation.UNION;
        container.AddChild(childAggregateConfiguration, 1);
        container.AddChild(childAggregateConfiguration2, 1);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("Child order is ambiguous, show the Order column and reorder contents", problem);
    }

    [Test]
    public void TestEmptyRootUNIONCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();

        container.Operation = SetOperation.UNION;

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("You must have at least one element in the root container", problem);
    }

    [Test]
    public void TestEmptyRootEXCEPTCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();

        container.Operation = SetOperation.EXCEPT;

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestEmptyRootINTERSECTCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();

        container.Operation = SetOperation.INTERSECT;

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void Test1ChildRootUNIONCohortContainer_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.UNION;
        container.AddChild(childAggregateConfiguration, 0);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNull(problem);
    }

    [Test]
    public void Test2ChildRootUNIONCohortContainer_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.UNION;
        container.AddChild(childAggregateConfiguration, 1);
        container.AddChild(childAggregateConfiguration2, 2);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNull(problem);
    }

    [Test]
    public void Test1ChildRootEXCEPTCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.EXCEPT;
        container.AddChild(childAggregateConfiguration, 0);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void Test2ChildRootEXCEPTCohortContainer_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.EXCEPT;
        container.AddChild(childAggregateConfiguration, 1);
        container.AddChild(childAggregateConfiguration2, 2);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNull(problem);
    }

    [Test]
    public void Test1ChildRootINTERSECTCohortContainer_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.EXCEPT;
        container.AddChild(childAggregateConfiguration, 0);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNotNull(problem);
        Assert.AreEqual("EXCEPT/INTERSECT containers must have at least two elements within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void Test2ChildRootINTERSECTCohortContainer_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        container.Operation = SetOperation.INTERSECT;
        container.AddChild(childAggregateConfiguration, 1);
        container.AddChild(childAggregateConfiguration2, 2);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(container);

        Assert.IsNull(problem);
    }
    #endregion

    #region SET containers
    [Test]
    public void TestSetContainerUNION_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);

        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers cannot be empty. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer1ChildUNION_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers have no effect if there is only one child within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer2ChildUNION_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        childContainer.AddChild(childAggregateConfiguration2, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNull(problem);
    }

    [Test]
    public void TestSetContainerEXCEPT_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);

        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers cannot be empty. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer1ChildEXCEPT_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers have no effect if there is only one child within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer2ChildEXCEPT_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        childContainer.AddChild(childAggregateConfiguration2, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNull(problem);
    }

    [Test]
    public void TestSetContainerINTERSECT_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);

        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers cannot be empty. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer1ChildINTERSECT_IsProblem()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNotNull(problem);
        Assert.AreEqual("SET containers have no effect if there is only one child within. Either Add a Catalogue or Disable/Delete this container if not required", problem);
    }

    [Test]
    public void TestSetContainer2ChildINTERSECT_IsOk()
    {
        var container = WhenIHaveA<CohortAggregateContainer>();
        var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
        var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
        var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

        childContainer.AddChild(childAggregateConfiguration, 0);
        childContainer.AddChild(childAggregateConfiguration2, 0);
        container.AddChild(childContainer);

        var pp = new CatalogueProblemProvider();
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(childContainer);

        Assert.IsNull(problem);
    }


    #endregion


    [TestCase(null)]
    [TestCase("")]
    public void MixedCollationIsAProblemForJoinInfos_WhenNoExplicitCollation(string nullCollationExpression)
    {
        var ci1 = WhenIHaveA<ExtractionInformation>().CatalogueItem;
            
        var ci2 = WhenIHaveA<ExtractionInformation>().CatalogueItem;

        _=new JoinInfo((ICatalogueRepository)ci1.Repository,
            ci2.ColumnInfo,
            ci1.ColumnInfo,
            ExtractionJoinType.Right,
            nullCollationExpression);

        var pp = new CatalogueProblemProvider();
        var childProvider = GetActivator().CoreChildProvider;

        Assert.IsFalse(pp.HasProblem(ci1),"Should not be problem because no collations are declared");
        pp.RefreshProblems(childProvider);

        Assert.IsFalse(pp.HasProblem(ci1), "Should not be problem because no collations are declared");
        pp.RefreshProblems(childProvider);

        ci1.ColumnInfo.Collation = "fishy";
        ci2.ColumnInfo.Collation = "fishy";
        pp.RefreshProblems(childProvider);

        Assert.IsFalse(pp.HasProblem(ci1), "Should not be problem because collations are the same");

        ci1.ColumnInfo.Collation = "fishy";
        ci2.ColumnInfo.Collation = "splishy";
        pp.RefreshProblems(childProvider);

        Assert.IsTrue(pp.HasProblem(ci1));
        Assert.AreEqual("Columns in joins declared on this column have mismatched collations ( My_Col = My_Col)", pp.DescribeProblem(ci1));
    }

    [Test]
    public void MixedCollationIsAProblemForJoinInfos_ExplicitCollation()
    {
        var ci1 = WhenIHaveA<ExtractionInformation>().CatalogueItem;

        var ci2 = WhenIHaveA<ExtractionInformation>().CatalogueItem;

        _ = new JoinInfo((ICatalogueRepository)ci1.Repository,
            ci2.ColumnInfo,
            ci1.ColumnInfo,
            ExtractionJoinType.Right,
            // user knows they have different collations and has told
            // RDMP to collate with this
            "kaboom");

        var pp = new CatalogueProblemProvider();
        var childProvider = GetActivator().CoreChildProvider;


        Assert.IsFalse(pp.HasProblem(ci1), "Should not be problem because collations are the same");

        ci1.ColumnInfo.Collation = "fishy";
        ci2.ColumnInfo.Collation = "splishy";
        pp.RefreshProblems(childProvider);

        Assert.IsFalse(pp.HasProblem(ci1), "Should not be problem because JoinInfo explicitly states a resolution collation");

    }
    #region Parameters

    [TestCase("2001/01/01", true)]
    [TestCase("'2001/01/01", false)] // This is currently fine, we are only detecting bad dates.  SQL syntax will pick this up for them anyway
    [TestCase("'2001/01/01'", false)]
    [TestCase("\"2001/01/01\"", false)]
    public void TestParameterIsDate(string val, bool expectProblem)
    {
        var param = WhenIHaveA<AggregateFilterParameter>();
        param.Value = val;
        param.SaveToDatabase();

        var pp = new CatalogueProblemProvider { Culture = new CultureInfo("en-GB") };
        pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
        var problem = pp.DescribeProblem(param);

        if(expectProblem)
        {
            Assert.AreEqual("Parameter value looks like a date but is not surrounded by quotes", problem);
        }
        else
        {
            Assert.IsNull(problem);
        }

    }

    #endregion
}