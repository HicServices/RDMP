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
using ReusableLibraryCode.Settings;
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


        /// <summary>
        /// Verifies that regardless of the user settings when the user types in the exact Type they want
        /// then they get it scored high
        /// </summary>
        /// <param name="userSetting"></param>
        [TestCase(true)]
        [TestCase(false)]
        public void Find_CohortAggregateContainer_ByTypeName(bool userSetting)
        {
            var container = WhenIHaveA<CohortAggregateContainer>();

            UserSettings.ScoreZeroForCohortAggregateContainers = userSetting;

            var scorer = new SearchablesMatchScorer();
            scorer.TypeNames.Add("CohortAggregateContainer");

            var childProvider = new DataExportChildProvider(RepositoryLocator, null, new ThrowImmediatelyCheckNotifier(), null);

            var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(),"", CancellationToken.None, new List<Type>() { typeof(CohortAggregateContainer)});

            var score = scores.Single(d => Equals(d.Key.Key, container));
            Assert.Greater(score.Value, 0);
        }

        /// <summary>
        /// Verifies that <see cref="UserSettings.ScoreZeroForCohortAggregateContainers"/> is respected when the user
        /// is typing for some text that appears in the name of the object
        /// </summary>
        /// <param name="userSetting"></param>
        [TestCase(true)]
        [TestCase(false)]
        public void Find_CohortAggregateContainer_ByFreeText(bool userSetting)
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            container.Name = "All the trolls in the troll kingdom";

            UserSettings.ScoreZeroForCohortAggregateContainers = userSetting;

            var scorer = new SearchablesMatchScorer();
            scorer.TypeNames.Add("CohortAggregateContainer");

            var childProvider = new DataExportChildProvider(RepositoryLocator, null, new ThrowImmediatelyCheckNotifier(), null);

            // user is searching for the text 'troll'
            var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "troll", CancellationToken.None, new List<Type>());

            var score = scores.Single(d => Equals(d.Key.Key, container));

            if(userSetting)
            {
                // although the text appears in the search they are not doing it by exact type name and there settings
                // mean they don't want to see these objects by default.
                Assert.AreEqual(0, score.Value);
            }
            else
            {
                Assert.Greater(score.Value, 0);
            }
            
        }
    }
}
