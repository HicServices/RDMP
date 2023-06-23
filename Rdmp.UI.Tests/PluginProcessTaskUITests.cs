// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;

namespace Rdmp.UI.Tests;

internal class PluginProcessTaskUITests : UITests
{
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupMEF();
    }

    [Test]
    [UITimeout(20000)]
    public void PluginProcessTaskUI_NoClass()
    {
        AndLaunch<PluginProcessTaskUI>(WhenIHaveA<ProcessTask>());
        AssertErrorWasShown(ExpectedErrorType.KilledForm, "No class has been specified");
    }

    [Test]
    [UITimeout(20000)]
    public void PluginProcessTaskUI_ClassNotFound()
    {
        var pt = WhenIHaveA<ProcessTask>();
        pt.Path = "ArmageddonAttacher";
        pt.SaveToDatabase();

        AndLaunch<PluginProcessTaskUI>(pt);
        AssertErrorWasShown(ExpectedErrorType.KilledForm, "Could not find Type 'ArmageddonAttacher' for ProcessTask ");
    }

    [Test]
    [UITimeout(20000)]
    public void PluginProcessTaskUI_ClassIsLegit()
    {
        var pt = WhenIHaveA<ProcessTask>();
        pt.Path = typeof(DelimitedFlatFileAttacher).FullName;
        pt.SaveToDatabase();

        AndLaunch<PluginProcessTaskUI>(pt);
        AssertNoErrors(ExpectedErrorType.Any);
    }

    [Test]
    [UITimeout(20000)]
    public void PluginProcessTaskUI_InvalidParameter_Date()
    {
        MEF.AddTypeToCatalogForTesting(typeof(OmgDates));
        var pt = WhenIHaveA<ProcessTask>();
        pt.Path = typeof(OmgDates).FullName;
        var arg = pt.CreateArgumentsForClassIfNotExists<OmgDates>().Single();

        //set the argument value to 2001
        arg.SetValue(new DateTime(2001, 01, 01));
        pt.SaveToDatabase();

        AndLaunch<PluginProcessTaskUI>(pt);
        AssertNoErrors(ExpectedErrorType.Any);

        //there should be a text box with our argument value in it
        var tb = GetControl<TextBox>().Single(t => t.Text.Contains("2001"));

        //set the text to something nasty that won't compile
        tb.Text = "hahahah fff";

        Publish(pt);
        AssertNoErrors(ExpectedErrorType.Any);

        AndLaunch<PluginProcessTaskUI>(pt);

        AssertNoErrors(ExpectedErrorType.Any);
    }

    private class OmgDates
    {
        [DemandsInitialization("A Date")] public DateTime MyDate { get; set; }
    }
}