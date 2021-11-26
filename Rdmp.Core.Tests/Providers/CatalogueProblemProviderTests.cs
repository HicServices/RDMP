// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers
{
    class CatalogueProblemProviderTests : UnitTests
    {
        [Test]
        public void TestRootOrderCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();
            var childContainer2 = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();
            
            container.Operation = SetOperation.UNION;
            container.AddChild(childContainer, 1);
            container.AddChild(childContainer2, 1);

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
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
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
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        [Test]
        public void Test1ChildRootUNIONCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.UNION;
            container.AddChild(childContainer, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test2ChildRootUNIONCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();
            var childContainer2 = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.UNION;
            container.AddChild(childContainer, 1);
            container.AddChild(childContainer2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test1ChildRootEXCEPTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childContainer, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        public void Test2ChildRootEXCEPTCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();
            var childContainer2 = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childContainer, 1);
            container.AddChild(childContainer2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test1ChildRootINTERSECTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childContainer, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        public void Test2ChildRootINTERSECTCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();
            var childContainer2 = WhenIHaveA<Core.Curation.Data.Aggregation.AggregateConfiguration>();

            container.Operation = SetOperation.INTERSECT;
            container.AddChild(childContainer, 1);
            container.AddChild(childContainer2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }
    }
}
