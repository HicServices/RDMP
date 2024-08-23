// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class MetadataLoggingConfigurationChecksTests : UnitTests
{
    [Test]
    public void Test_NoLoggingTask()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
        var cata = WhenIHaveA<Catalogue>();
        lmd.LinkToCatalogue(cata);
        Assert.That(lmd.GetAllCatalogues().Count(), Is.EqualTo(2));

        var checks = new MetadataLoggingConfigurationChecks(lmd);
        var toMem = new ToMemoryCheckNotifier();
        checks.Check(toMem);

        AssertFailWithFix("Catalogues Mycata,Mycata do not have a logging task specified",
            "Create a new Logging Task called 'MyLoad'?", toMem);
    }

    [Test]
    public void Test_MismatchedLoggingTask()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
        var cata1 = lmd.GetAllCatalogues().Single();
        var cata2 = WhenIHaveA<Catalogue>();
        lmd.LinkToCatalogue(cata2);
        cata1.LoggingDataTask = "OMG YEAGH";

        Assert.That(lmd.GetAllCatalogues().Count(), Is.EqualTo(2));

        var checks = new MetadataLoggingConfigurationChecks(lmd);
        var toMem = new ToMemoryCheckNotifier();
        checks.Check(toMem);

        AssertFailWithFix("Some catalogues have NULL LoggingDataTasks", "Set task to OMG YEAGH", toMem);
    }

    [Test]
    public void Test_MissingLoggingServer()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
        var cata1 = lmd.GetAllCatalogues().Single();
        var cata2 = WhenIHaveA<Catalogue>();
        lmd.LinkToCatalogue(cata2);
        cata1.LoggingDataTask = "OMG YEAGH";
        cata1.LiveLoggingServer_ID = 2;
        cata2.LoggingDataTask = "OMG YEAGH";
        cata2.LiveLoggingServer_ID = null;

        Assert.That(lmd.GetAllCatalogues().Count(), Is.EqualTo(2));

        var checks = new MetadataLoggingConfigurationChecks(lmd);
        var toMem = new ToMemoryCheckNotifier();
        checks.Check(toMem);

        AssertFailWithFix("Some catalogues have NULL LiveLoggingServer_ID", "Set LiveLoggingServer_ID to 2", toMem);
    }

    [Test]
    public void Test_MissingLoggingServer_UseDefault()
    {
        var lmd = WhenIHaveA<LoadMetadata>();
        var cata1 = lmd.GetAllCatalogues().Single();
        var cata2 = WhenIHaveA<Catalogue>();

        var eds = WhenIHaveA<ExternalDatabaseServer>();
        eds.Name = "My Logging Server";
        eds.SaveToDatabase();

        lmd.LinkToCatalogue(cata2);
        cata1.LoggingDataTask = "OMG YEAGH";
        cata1.LiveLoggingServer_ID = null;
        cata2.LoggingDataTask = "OMG YEAGH";
        cata2.LiveLoggingServer_ID = null;

        var defaults = RepositoryLocator.CatalogueRepository;
        defaults.SetDefault(PermissableDefaults.LiveLoggingServer_ID, eds);

        Assert.That(lmd.GetAllCatalogues().Count(), Is.EqualTo(2));

        var checks = new MetadataLoggingConfigurationChecks(lmd);
        var toMem = new ToMemoryCheckNotifier();
        checks.Check(toMem);

        AssertFailWithFix("Some catalogues have NULL LiveLoggingServer_ID",
            "Set LiveLoggingServer_ID to 'My Logging Server' (the default)", toMem);
    }

    private static void AssertFailWithFix(string expectedMessage, string expectedFix, ToMemoryCheckNotifier toMem)
    {
        var msg = toMem.Messages.First(m => m.Result == CheckResult.Fail);

        Assert.Multiple(() =>
        {
            Assert.That(msg.Message, Is.EqualTo(expectedMessage), "Expected error message was wrong");
            Assert.That(msg.ProposedFix, Is.EqualTo(expectedFix), "Expected proposed fix was wrong");
        });
    }
}