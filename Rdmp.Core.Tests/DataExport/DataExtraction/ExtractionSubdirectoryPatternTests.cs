﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

internal class ExtractionSubdirectoryPatternTests : UnitTests
{
    [Test]
    public void Test_NoRelativePaths()
    {
        var dest = new ExecuteDatasetExtractionFlatFileDestination
        {
            ExtractionSubdirectoryPattern = "../../troll"
        };

        var ex = Assert.Throws<Exception>(() => dest.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("ExtractionSubdirectoryPattern cannot contain dots"));
    }

    [TestCase("bad")]
    [TestCase("$n")]
    [TestCase("$d")]
    [TestCase("$a")]
    [TestCase("$n")]
    public void Test_NoConfigToken(string badString)
    {
        var dest = new ExecuteDatasetExtractionFlatFileDestination
        {
            ExtractionSubdirectoryPattern = badString
        };

        var ex = Assert.Throws<Exception>(() => dest.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("ExtractionSubdirectoryPattern must contain a Configuration element"));
    }

    [TestCase("$c/fff")]
    [TestCase("$i")]
    public void Test_NoDatasetToken(string badString)
    {
        var dest = new ExecuteDatasetExtractionFlatFileDestination
        {
            ExtractionSubdirectoryPattern = badString
        };

        var ex = Assert.Throws<Exception>(() => dest.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("ExtractionSubdirectoryPattern must contain a Dataset element"));
    }

    /*
     $c - Configuration Name (e.g. 'Cases')
     $i - Configuration ID (e.g. 459)
     $d - Dataset name (e.g. 'Prescribing')
     $a - Dataset acronym (e.g. 'Presc')
     $n - Dataset ID (e.g. 459)
     */


    [TestCase("$c/$a", "/AAA/C")]
    [TestCase("$c/$d", "/AAA/BBB")]
    [TestCase("$c/$n", "/AAA/\\d+")]
    [TestCase("$i/$a", "/\\d+/C")]
    [TestCase("$i/$d", "/\\d+/BBB")]
    [TestCase("$i/$n", "/\\d+/\\d+")]
    public void Test_ValidPaths(string goodString, string pattern)
    {
        var sds = WhenIHaveA<SelectedDataSets>();


        sds.ExtractionConfiguration.Project.ExtractionDirectory = TestContext.CurrentContext.WorkDirectory;
        sds.ExtractionConfiguration.Name = "AAA";
        sds.ExtractableDataSet.Catalogue.Name = "BBB";
        sds.ExtractableDataSet.Catalogue.Acronym = "C";


        var cmd = new ExtractDatasetCommand(sds.ExtractionConfiguration,
            new ExtractableDatasetBundle(sds.ExtractableDataSet));
        var dest = new ExecuteDatasetExtractionFlatFileDestination
        {
            ExtractionSubdirectoryPattern = goodString
        };

        Assert.DoesNotThrow(() => dest.Check(ThrowImmediatelyCheckNotifier.Quiet));

        var answer = dest.GetDirectoryFor(cmd);
        Assert.That(answer.FullName.Replace('\\', '/'), Does.Match(pattern));
    }
}