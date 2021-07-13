// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement;

namespace Rdmp.UI.Tests.DataLoadUIs.ANOUIs.ANOTableManagement
{
    public class ColumnInfoToANOTableConverterUITests :UITests
    {
        [Test,UITimeout(20000)]
        public void Test_ColumnInfoToANOTableConverterUI_Constructor()
        {
            var o = WhenIHaveA<ColumnInfo>();
            var ui = AndLaunch<ColumnInfoToANOTableConverterUI>(o);
            Assert.IsNotNull(ui);
            AssertErrorWasShown(ExpectedErrorType.KilledForm,"Could not get connection string because Server was null on dataAccessPoint 'My_Table'");
            //AssertNoErrors(ExpectedErrorType.Fatal);
            //AssertNoErrors(ExpectedErrorType.KilledForm);
        }
    }
}
