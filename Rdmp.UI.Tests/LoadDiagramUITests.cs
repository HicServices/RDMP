// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

namespace Rdmp.UI.Tests;

public class LoadDiagramUITests : UITests
{
    [Test, UITimeout(50000)]
    public void Test_LoadDiagramUITests_NormalState()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
            
        SetupMEF();

        _ = AndLaunch<LoadDiagramUI>(lmd);

        //it isn't impossible to show us
        AssertCommandIsPossible(new ExecuteCommandViewLoadDiagram(ItemActivator, lmd));

        AssertNoErrors(ExpectedErrorType.Any);

    }

    [Test, UITimeout(50000)]
    public void Test_LoadDiagramUITests_NoCatalogues()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
            
        //delete the Catalogue so the load is an orphan
        lmd.GetAllCatalogues().Single().DeleteInDatabase();
            
        SetupMEF();

        _ = AndLaunch<LoadDiagramUI>(lmd);

        //can't launch the command
        AssertCommandIsImpossible(new ExecuteCommandViewLoadDiagram(ItemActivator, lmd), "does not have any associated Catalogues");

        //and ui should be showing big problems
        AssertErrorWasShown(ExpectedErrorType.Fatal,"Could not fetch data");

    }
}