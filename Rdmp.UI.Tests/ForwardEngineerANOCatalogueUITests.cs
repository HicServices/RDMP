// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ANOEngineeringUIs;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Tests;

internal class ForwardEngineerANOCatalogueUITests : UITests
{
    [Test,UITimeout(50000)]
    public void Test_ForwardEngineerANOCatalogueUI_NormalState()
    {
        SetupMEF();

        var eiChi = WhenIHaveA<ExtractionInformation>();
        var cata = eiChi.CatalogueItem.Catalogue;
            
        AndLaunch<ForwardEngineerANOCatalogueUI>(cata);

        AssertNoErrors(ExpectedErrorType.Any);
    }

    [Test, UITimeout(50000)]
    public void Test_ForwardEngineerANOCatalogueUI_NoColumns()
    {
        SetupMEF();

        var cata = WhenIHaveA<Catalogue>();

        //shouldn't be possible to launch the UI
        AssertCommandIsImpossible(new ExecuteCommandCreateANOVersion(ItemActivator, cata), "does not have any Extractable Columns");

        //and if we are depersisting it that should be angry
        AndLaunch<ForwardEngineerANOCatalogueUI>(cata);

        AssertErrorWasShown(ExpectedErrorType.Fatal, "Could not generate a valid query for the Catalogue");
    }

}