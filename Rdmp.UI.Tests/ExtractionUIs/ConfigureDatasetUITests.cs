// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ProjectUI.Datasets;

namespace Rdmp.UI.Tests.ExtractionUIs;

internal class ConfigureDatasetUITests : UITests
{
    [Test]
    [UITimeout(50000)]
    public void Test_RemoveAllColumns_Only1Publish()
    {
        var sds = WhenIHaveA<SelectedDataSets>();
        var ui = AndLaunch<ConfigureDatasetUI>(sds);

        AssertNoErrors();

        var publishCount = 0;

        //should be at least 2 in the config for this test to be sensible
        var cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
        Assert.That(cols, Has.Length.GreaterThanOrEqualTo(2));

        ItemActivator.RefreshBus.BeforePublish += (s, e) => publishCount++;

        Assert.That(publishCount, Is.EqualTo(0));

        ui.ExcludeAll();

        Assert.That(publishCount, Is.EqualTo(1));

        AssertNoErrors();

        //should now be no columns in the extraction configuration
        Assert.That(sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet), Is.Empty);

        ui.IncludeAll();

        //should now be another publish event
        Assert.That(publishCount, Is.EqualTo(2));

        //and the columns should be back in the configuration
        cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
        Assert.That(cols, Has.Length.GreaterThanOrEqualTo(2));

        //multiple includes shouldn't change the number of columns
        ui.IncludeAll();
        ui.IncludeAll();
        ui.IncludeAll();

        Assert.That(sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet), Has.Length.EqualTo(cols.Length));
    }
}