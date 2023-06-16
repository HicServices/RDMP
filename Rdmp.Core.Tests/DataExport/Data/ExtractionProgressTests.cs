// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using System;
using System.IO;
using System.Linq;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Data;

internal class ExtractionProgressTests : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void ExtractionProgressConstructor_NoTimePeriodicity()
    {
        var cata = new Catalogue(CatalogueRepository, "MyCata");
        var eds = new ExtractableDataSet(DataExportRepository, cata);
        var project = new Project(DataExportRepository, "My Proj");
        var config = new ExtractionConfiguration(DataExportRepository, project);
        var sds = new SelectedDataSets(DataExportRepository, config, eds, null);

        var ex = Assert.Throws<ArgumentException>(() => new ExtractionProgress(DataExportRepository, sds));
        Assert.AreEqual(
            "Cannot create ExtractionProgress because Catalogue MyCata does not have a time coverage column",
            ex.Message);
    }

    [Test]
    public void ExtractionProgressConstructor_Normal()
    {
        ExtractionProgress progress = null;
        Assert.DoesNotThrow(()=> progress = CreateAnExtractionProgress());
        progress?.DeleteInDatabase();
    }

    [Test]
    public void ExtractionProgressConstructor_CannotHaveTwoAtOnce()
    {
        var progress = CreateAnExtractionProgress();

        var sds = progress.SelectedDataSets;

        // try to create a second progress for the same dataset being extracted
        var ex = Assert.Throws<Exception>(() => new ExtractionProgress(DataExportRepository, sds));

        Assert.AreEqual("There is already an ExtractionProgress associated with MyCata", ex.Message);

        // now delete the original and make sure we can recreate it ok
        progress.DeleteInDatabase();
        Assert.DoesNotThrow(() => progress = new ExtractionProgress(DataExportRepository, sds));

        // yeah we can great, let's cleanup the test now
        progress.DeleteInDatabase();
    }

    [Test]
    public void ExtractionProgressConstructor_DeleteSdsMustCascade()
    {
        var progress = CreateAnExtractionProgress();

        Assert.IsTrue(progress.Exists());
        progress.SelectedDataSets.DeleteInDatabase();
        Assert.IsFalse(progress.Exists());
    }

    [Test]
    public void ExtractionProgress_RetrySave()
    {
        var progress = CreateAnExtractionProgress();
        Assert.AreEqual(progress.Retry, RetryStrategy.NoRetry);

        progress.Retry = RetryStrategy.IterativeBackoff1Hour;
        progress.SaveToDatabase();

        progress.RevertToDatabaseState();
        Assert.AreEqual(progress.Retry, RetryStrategy.IterativeBackoff1Hour);

        var freshCopy = progress.Repository.GetObjectByID<ExtractionProgress>(progress.ID);
        Assert.AreEqual(freshCopy.Retry, RetryStrategy.IterativeBackoff1Hour);

        progress.DeleteInDatabase();
    }

    [Test]
    public void TestQueryGeneration_FirstBatch()
    {
        Reset();

        _catalogue.TimeCoverage_ExtractionInformation_ID =
            _extractionInformations.Single(e => e.GetRuntimeName().Equals("DateOfBirth")).ID;
        _catalogue.SaveToDatabase();

        var progress = new ExtractionProgress(DataExportRepository, _request.SelectedDataSets)
        {
            StartDate = new DateTime(2001, 01, 01),
            EndDate = new DateTime(2001, 01, 10),
            NumberOfDaysPerBatch = 10
        };
        progress.SaveToDatabase();

        _request.GenerateQueryBuilder();

        Execute(out _, out var result);

        Assert.IsTrue(result.GeneratesFiles);
        var fileContents = File.ReadAllText(result.OutputFile);

        // Headers should be in file because it is a first batch
        Assert.AreEqual(
            $"ReleaseID,Name,DateOfBirth{Environment.NewLine}Pub_54321,Dave,2001-01-01{Environment.NewLine}",
            fileContents);

        File.Delete(result.OutputFile);
        progress.DeleteInDatabase();
    }

    [Test]
    public void TestCloneResetsProgress()
    {
        CreateAnExtractionProgress(out var config);

        // get original objects
        var origSds = config.SelectedDataSets.Single();
        var origProgress = origSds.ExtractionProgressIfAny;
        origProgress.StartDate = new DateTime(2001, 01, 01);
        origProgress.ProgressDate = new DateTime(2005, 01, 01);
        origProgress.EndDate = new DateTime(2020, 01, 01);
        origProgress.SaveToDatabase();

        //clone
        var clone = config.DeepCloneWithNewIDs();

        // get new objects
        var cloneSds = clone.SelectedDataSets.Single();
        var cloneProgress = cloneSds.ExtractionProgressIfAny;


        // should be different instances
        Assert.AreNotSame(origProgress, cloneProgress);

        Assert.AreEqual(cloneProgress.StartDate, new DateTime(2001, 01, 01));
        Assert.IsNull(cloneProgress.ProgressDate, "Expected progress to be reset on clone");
        Assert.AreEqual(cloneProgress.EndDate, new DateTime(2020, 01, 01));
    }

    private ExtractionProgress CreateAnExtractionProgress() => CreateAnExtractionProgress(out _);

    private ExtractionProgress CreateAnExtractionProgress(out ExtractionConfiguration config)
    {
        var cata = new Catalogue(CatalogueRepository, "MyCata");
        var cataItem = new CatalogueItem(CatalogueRepository, cata, "MyCol");
        var table = new TableInfo(CatalogueRepository, "MyTable");
        var col = new ColumnInfo(CatalogueRepository, "mycol", "datetime", table);

        var ei = new ExtractionInformation(CatalogueRepository, cataItem, col, "mycol");
        cata.TimeCoverage_ExtractionInformation_ID = ei.ID;
        cata.SaveToDatabase();

        var eds = new ExtractableDataSet(DataExportRepository, cata);
        var project = new Project(DataExportRepository, "My Proj");
        config = new ExtractionConfiguration(DataExportRepository, project);
        var sds = new SelectedDataSets(DataExportRepository, config, eds, null);

        return new ExtractionProgress(DataExportRepository, sds);
    }
}