// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers
{
    class SearchablesMatchScorerTests : UnitTests
    {
        [Test]
        public void Find_ExactMatch_ScoresHigher()
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "FF";
            var proj = WhenIHaveA<Project>();
            proj.Name = "FFFF";

            var scorer = new SearchablesMatchScorer();

            var childProvider = new DataExportChildProvider(RepositoryLocator, null, new ThrowImmediatelyCheckNotifier(), null);
            var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "FF", CancellationToken.None, new List<Type>());

            var cataScore = scores.Single(d => Equals(d.Key.Key, cata));
            var projScore = scores.Single(d => Equals(d.Key.Key, proj));

            // Both score because they have the text FF
            Assert.Greater(cataScore.Value, 0);
            Assert.Greater(projScore.Value, 0);

            // Catalogue scores higher because it is an exact match to the name
            Assert.Greater(cataScore.Value, projScore.Value);
        }


        [Test]
        public void Find_CohortAggregateContainer()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();

            var scorer = new SearchablesMatchScorer();
            scorer.TypeNames.Add("CohortAggregateContainer");

            var childProvider = new DataExportChildProvider(RepositoryLocator, null, new ThrowImmediatelyCheckNotifier(), null);

            var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(),"", CancellationToken.None, new List<Type>() { typeof(CohortAggregateContainer)});

            var score = scores.Single(d => Equals(d.Key.Key, container));
            Assert.Greater(score.Value, 0);
        }
    }
}
