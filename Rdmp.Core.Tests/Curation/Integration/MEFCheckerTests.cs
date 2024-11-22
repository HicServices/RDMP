// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Checks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public sealed class MEFCheckerTests : UnitTests
{
    [Test]
    public void FindClass_WrongCase_FoundAnyway()
    {
        Assert.That(Core.Repositories.MEF.GetType("catalogue"), Is.EqualTo(typeof(Catalogue)));
    }

    [Test]
    public void FindClass_EmptyString()
    {
        var m = new MEFChecker("", static _ => Assert.Fail());
        var ex = Assert.Throws<Exception>(() => m.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(
            ex?.Message, Is.EqualTo("MEFChecker was asked to check for the existence of an Export class but the _classToFind string was empty"));
    }

    [Test]
    public void FindClass_CorrectNamespace()
    {
        var m = new MEFChecker("Rdmp.Core.DataLoad.Modules.Attachers.AnySeparatorFileAttacher", static _ => Assert.Fail());
        m.Check(ThrowImmediatelyCheckNotifier.Quiet);
    }

    [Test]
    public void FindClass_WrongNamespace()
    {
        var m = new MEFChecker("CatalogueLibrary.AnySeparatorFileAttacher", static _ => Assert.Pass());
        m.Check(new AcceptAllCheckNotifier());

        Assert.Fail("Expected the class not to be found but to be identified under the correct namespace (above)");
    }

    [Test]
    public void FindClass_NonExistent()
    {
        var m = new MEFChecker("CatalogueLibrary.UncleSam", static _ => Assert.Fail());
        var ex = Assert.Throws<Exception>(() => m.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(
            ex?.Message, Does.Contain("Could not find MEF class called CatalogueLibrary.UncleSam in LoadModuleAssembly.GetAllTypes() and couldn't even find any with the same basic name"));
    }
}