// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CohortUI.CohortSourceManagement;

namespace Rdmp.UI.Tests.CohortUI.CohortSourceManagement;

public class ExternalCohortTableUITests : UITests
{
    [Test]
    [UITimeout(20000)]
    public void Test_ExternalCohortTableUI_Constructor()
    {
        var o = WhenIHaveA<ExternalCohortTable>();
        var ui = AndLaunch<ExternalCohortTableUI>(o);
        Assert.IsNotNull(ui);

        //because cohort table doesnt actually go to a legit database the source should have been forbidlisted during the child provider stage (not really related to our UI).
        AssertErrorWasShown(ExpectedErrorType.GlobalErrorCheckNotifier,
            "Could not reach cohort 'My cohorts' (it may be slow responding or inaccessible");
        AssertNoErrors(ExpectedErrorType.Fatal);
        AssertNoErrors(ExpectedErrorType.KilledForm);
        AssertNoErrors(ExpectedErrorType.ErrorProvider);
        AssertNoErrors(ExpectedErrorType.FailedCheck); //checks are not run until user manually runs them in this UI
    }
}