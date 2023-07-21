// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ANOEngineeringUIs;

namespace Rdmp.UI.Tests;

internal class ANOTableUITests : UITests
{
    [Test]
    [UITimeout(50000)]
    public void Test_ANOTableUI_NormalState()
    {
        var anoTable = WhenIHaveA<ANOTable>();
        AndLaunch<ANOTableUI>(anoTable);

        //not much we can do about this without mocking the data access layer
        AssertErrorWasShown(ExpectedErrorType.Fatal, "Could not reach ANO Server");

        //but form was not killed and server is not in error
        AssertNoErrors(ExpectedErrorType.KilledForm);
        AssertNoErrors(ExpectedErrorType.ErrorProvider);

        anoTable.DeleteInDatabase();
    }

    [Test]
    [UITimeout(50000)]
    public void Test_ANOTableUI_ServerWrongType()
    {
        var anoTable = WhenIHaveA(Repository, out ExternalDatabaseServer srv);
        srv.CreatedByAssembly = null;
        srv.SaveToDatabase();

        AndLaunch<ANOTableUI>(anoTable);

        //should be an error on the server showing that it is misconfigured
        AssertErrorWasShown(ExpectedErrorType.ErrorProvider, "Server is not an ANO server");
        AssertErrorWasShown(ExpectedErrorType.Fatal, "Could not reach ANO Server");

        //but form was not killed
        AssertNoErrors(ExpectedErrorType.KilledForm);

        anoTable.DeleteInDatabase();
    }
}