// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Security.Cryptography;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Logging;
using Tests.Common;

namespace Rdmp.Core.Tests.Logging;

internal class FatalErrorLoggingTest : DatabaseTests
{
    [TestCase]
    public void CreateNewDataLoadTask()
    {
        var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        lm.CreateNewLoggingTaskIfNotExists("Fish");

        Assert.Multiple(() =>
        {
            Assert.That(lm.ListDataTasks(), Does.Contain("Fish"));
            Assert.That(lm.ListDataSets(), Does.Contain("Fish"));
        });

        lm.CreateNewLoggingTaskIfNotExists("Fish");
        lm.CreateNewLoggingTaskIfNotExists("Fish");
        lm.CreateNewLoggingTaskIfNotExists("Fish");
    }

    [TestCase]
    public void FataErrorLoggingTest()
    {
        var d = new DataLoadInfo("Internal", "HICSSISLibraryTests.FataErrorLoggingTest",
            "Test case for fatal error generation",
            "No rollback is possible/required as no database rows are actually inserted",
            true, new DiscoveredServer(UnitTestLoggingConnectionString));

        var ds = new DataSource[] { new("nothing", DateTime.Now) };


        var t = new TableLoadInfo(d, "Unit test only", "Unit test only", ds, 5);
        t.Inserts += 3; //simulate that it crashed after 3

        d.LogFatalError("HICSSISLibraryTests.FataErrorLoggingTest", "Some terrible event happened");

        Assert.That(d.IsClosed);
    }

    [Test]
    public void MD5Test()
    {
        const string fileContents = "TestStringThatCouldBeSomethingInAFile";
        byte[] hashAsBytes;

        using var memory = new MemoryStream();
        using var writeToMemory = new StreamWriter(memory);
        writeToMemory.Write(fileContents);
        memory.Flush();
        memory.Position = 0;

        using (var md5 = MD5.Create())
        {
            hashAsBytes = md5.ComputeHash(memory);
        }

        var ds = new DataSource[] { new("nothing", DateTime.Now) };

        ds[0].MD5 = hashAsBytes; //MD5 is a property so confirm write and read are the same - and don't bomb

        Assert.That(hashAsBytes, Is.EqualTo(ds[0].MD5));

        var d = new DataLoadInfo("Internal", "HICSSISLibraryTests.FatalErrorLoggingTest",
            "Test case for fatal error generation",
            "No rollback is possible/required as no database rows are actually inserted",
            true,
            new DiscoveredServer(UnitTestLoggingConnectionString));

        var t = new TableLoadInfo(d, "Unit test only", "Unit test only", ds, 5);
        t.Inserts += 5; //simulate that it crashed after 3
        t.CloseAndArchive();

        d.CloseAndMarkComplete();
    }
}