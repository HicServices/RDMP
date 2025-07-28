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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers;

internal class SearchablesMatchScorerTests : UnitTests
{
    [Test]
    public void Find_ExactMatch_ScoresHigher()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "FF";
        var proj = WhenIHaveA<Project>();
        proj.Name = "FFFF";

        var scorer = new SearchablesMatchScorer();

        var childProvider =
            new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);
        var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "FF", new List<Type>(),
            CancellationToken.None);

        var cataScore = scores.Single(d => Equals(d.Key.Key, cata));
        var projScore = scores.Single(d => Equals(d.Key.Key, proj));

        Assert.Multiple(() =>
        {
            // Both score because they have the text FF
            Assert.That(cataScore.Value, Is.GreaterThan(0));
            Assert.That(projScore.Value, Is.GreaterThan(0));
        });

        // Catalogue scores higher because it is an exact match to the name
        Assert.That(cataScore.Value, Is.GreaterThan(projScore.Value));
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

        var childProvider =
            new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "",
            new List<Type> { typeof(CohortAggregateContainer) }, CancellationToken.None);

        var score = scores.Single(d => Equals(d.Key.Key, container));
        Assert.That(score.Value, Is.GreaterThan(0));
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

        var childProvider =
            new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        // user is searching for the text 'troll'
        var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "troll", new List<Type>(),
            CancellationToken.None);

        var score = scores.Single(d => Equals(d.Key.Key, container));

        if (userSetting)
            // although the text appears in the search they are not doing it by exact type name and their settings
            // mean they don't want to see these objects by default.
            Assert.That(score.Value, Is.EqualTo(0));
        else
            Assert.That(score.Value, Is.GreaterThan(0));
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    public void TestScoringCatalogueFlag_IsDeprecated(bool hasFlag, bool shouldShow, bool expectedResult)
    {
        TestScoringFlag((c, eds) =>
        {
            c.IsDeprecated = hasFlag;
            UserSettings.ShowDeprecatedCatalogues = shouldShow;
        }, expectedResult);
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    public void TestScoringCatalogueFlag_IsColdStorage(bool hasFlag, bool shouldShow, bool expectedResult)
    {
        TestScoringFlag((c, eds) =>
        {
            c.IsColdStorageDataset = hasFlag;
            UserSettings.ShowColdStorageCatalogues = shouldShow;
        }, expectedResult);
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    public void TestScoringCatalogueFlag_IsInternalDataset(bool hasFlag, bool shouldShow, bool expectedResult)
    {
        TestScoringFlag((c, eds) =>
        {
            c.IsInternalDataset = hasFlag;
            UserSettings.ShowInternalCatalogues = shouldShow;
        }, expectedResult);
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    public void TestScoringCatalogueFlag_IsExtractable(bool notExtractable, bool shouldShow, bool expectedResult)
    {
        TestScoringFlag((c, eds) =>
        {
            if (notExtractable) eds.DeleteInDatabase();

            UserSettings.ShowNonExtractableCatalogues = shouldShow;
        }, expectedResult);
    }

    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    public void TestScoringCatalogueFlag_IsProjectSpecific(bool projectSpecific, bool shouldShow, bool expectedResult)
    {
        TestScoringFlag((c, eds) =>
        {
            if (projectSpecific)
            {
                // this makes it project specific
                var p = new Project(Repository, "Test Proj");
                p.SaveToDatabase();
                var edsp = new ExtractableDataSetProject(Repository,eds,p);
                edsp.SaveToDatabase();
                eds.SaveToDatabase();
            }

            UserSettings.ShowProjectSpecificCatalogues = shouldShow;
        }, expectedResult);
    }

    private void TestScoringFlag(Action<Catalogue, ExtractableDataSet> setter, bool expectedResult)
    {
        // Filter is hungry and eager to please.  If you want to see ProjectSpecific Catalogues then
        // that it will show you them regardless of other settings.  Likewise clicking Deprecated shows
        // all deprecated catalogues regardless of other settings.
        //
        // So set all to false to except the condition we are testing
        UserSettings.ShowDeprecatedCatalogues = false;
        UserSettings.ShowNonExtractableCatalogues = false;
        UserSettings.ShowProjectSpecificCatalogues = false;
        UserSettings.ShowInternalCatalogues = false;
        UserSettings.ShowColdStorageCatalogues = false;

        var c = WhenIHaveA<Catalogue>();
        c.Name = "Bunny";
        c.SaveToDatabase();

        // this makes c extractable (the usual case for Catalogues)
        var eds = new ExtractableDataSet(Repository, c);
        eds.SaveToDatabase();

        setter(c, eds);
        c.SaveToDatabase();


        var scorer = new SearchablesMatchScorer
        {
            RespectUserSettings = true
        };

        var childProvider =
            new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        // user is searching for the text 'troll'
        var scores = scorer.ScoreMatches(childProvider.GetAllSearchables(), "Bunny", new List<Type>(),
            CancellationToken.None);

        var score = scores.Single(d => Equals(d.Key.Key, c));

        if (expectedResult)
            Assert.That(score.Value, Is.GreaterThan(0));
        else
            // score 0 and don't be included in results
            Assert.That(score.Value, Is.EqualTo(0));

        // Cleanup test
        foreach (var d in Repository.GetAllObjects<ExtractableDataSet>()) d.DeleteInDatabase();
        foreach (var cat in Repository.GetAllObjects<Catalogue>()) cat.DeleteInDatabase();
    }
}