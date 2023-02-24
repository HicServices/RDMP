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
    [Test,UITimeout(50000)]
    public void Test_RemoveAllColumns_Only1Publish()
    {
        var sds = WhenIHaveA<SelectedDataSets>();
        var ui = AndLaunch<ConfigureDatasetUI>(sds);
            
        AssertNoErrors();

        var publishCount = 0;

        //should be at least 2 in the config for this test to be sensible
        var cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
        Assert.GreaterOrEqual(cols.Length,2);

        ItemActivator.RefreshBus.BeforePublish += (s,e)=>publishCount++;

        Assert.AreEqual(0, publishCount);

        ui.ExcludeAll();

        Assert.AreEqual(1,publishCount);

        AssertNoErrors();
            
        //should now be no columns in the extraction configuration
        Assert.IsEmpty(sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet));

        ui.IncludeAll();

        //should now be another publish event
        Assert.AreEqual(2, publishCount);

        //and the columns should be back in the configuration
        cols = sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet);
        Assert.GreaterOrEqual(cols.Length, 2);

        //multiple includes shouldnt change the number of columns
        ui.IncludeAll();
        ui.IncludeAll();
        ui.IncludeAll();

        Assert.AreEqual(cols.Length, sds.ExtractionConfiguration.GetAllExtractableColumnsFor(sds.ExtractableDataSet).Length);
    }
}