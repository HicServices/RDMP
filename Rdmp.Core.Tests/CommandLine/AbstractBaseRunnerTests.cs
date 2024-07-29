// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine;

public class AbstractBaseRunnerTests : UnitTests
{
    [SetUp]
    public void CleanRemnants()
    {
        foreach (var o in Repository.GetAllObjectsInDatabase())
            o.DeleteInDatabase();
    }

    [Test]
    public void GetObjectFromCommandLineString_CatalogueByID()
    {
        var c = WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator, c.ID.ToString()), Is.EqualTo(c));
    }

    [Test]
    public void GetObjectFromCommandLineString_CatalogueByPattern()
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "gogogo";
        c.SaveToDatabase();

        WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator, "Catalogue:*go*"), Is.EqualTo(c));
    }

    [Test]
    public void GetObjectFromCommandLineString_ProjectByID()
    {
        var c = WhenIHaveA<Project>();
        WhenIHaveA<Project>();
        WhenIHaveA<Project>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectFromCommandLineString<Project>(RepositoryLocator, c.ID.ToString()), Is.EqualTo(c));
    }

    [Test]
    public void GetObjectFromCommandLineString_ProjectByPattern()
    {
        var c = WhenIHaveA<Project>();
        c.Name = "gogogo";
        c.SaveToDatabase();

        WhenIHaveA<Project>();
        WhenIHaveA<Project>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectFromCommandLineString<Project>(RepositoryLocator, "Project:*go*"), Is.EqualTo(c));
    }

    /// <summary>
    /// Tests that things the user might enter for a parameter (or default parameter values specified in RDMP
    /// are going to be interpreted as null correctly
    /// </summary>
    /// <param name="expression"></param>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("0")]
    [TestCase("null")]
    public void GetObjectFromCommandLineString_Null(string expression)
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "gogogo";
        c.SaveToDatabase();

        WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectFromCommandLineString<Catalogue>(RepositoryLocator, expression), Is.Null);
    }

    /// <summary>
    /// This test is for the IEnumerable version
    /// </summary>
    /// <param name="expression"></param>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("0")]
    [TestCase("null")]
    public void GetObjectsFromCommandLineString_Null(string expression)
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "gogogo";
        c.SaveToDatabase();

        WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        var r = new TestRunner();
        Assert.That(TestRunner.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator, expression), Is.Empty);
    }


    [Test]
    public void GetObjectsFromCommandLineString_CatalogueByID()
    {
        var c = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();
        WhenIHaveA<Catalogue>();
        var r = new TestRunner();

        var results = TestRunner.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator, $"{c.ID},{c2.ID}")
            .ToArray();

        Assert.That(results, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(results[0], Is.SameAs(c));
            Assert.That(results[1], Is.SameAs(c2));
        });
    }

    [Test]
    public void GetObjectsFromCommandLineString_CatalogueByPattern()
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "go long";
        c.SaveToDatabase();

        var c2 = WhenIHaveA<Catalogue>();
        c2.Name = "go hard";
        c2.SaveToDatabase();

        WhenIHaveA<Catalogue>();

        var r = new TestRunner();
        var results = TestRunner.GetObjectsFromCommandLineString<Catalogue>(RepositoryLocator, "Catalogue:*go*")
            .ToArray();

        Assert.That(results, Has.Length.EqualTo(2));
        Assert.That(results, Does.Contain(c));
        Assert.That(results, Does.Contain(c2));
    }

    private class TestRunner : Runner
    {
        public new static T GetObjectFromCommandLineString<T>(IRDMPPlatformRepositoryServiceLocator locator, string arg)
            where T : IMapsDirectlyToDatabaseTable => Runner.GetObjectFromCommandLineString<T>(locator, arg);

        public new static IEnumerable<T>
            GetObjectsFromCommandLineString<T>(IRDMPPlatformRepositoryServiceLocator locator, string arg)
            where T : IMapsDirectlyToDatabaseTable => Runner.GetObjectsFromCommandLineString<T>(locator, arg);

        public override int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
            IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null) => 0;
    }
}