// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine;

internal class CommandLineObjectPickerTests : UnitTests
{
    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        foreach (var c in Repository.GetAllObjects<Catalogue>()) c.DeleteInDatabase();
    }


    [Test]
    public void Test_RandomGarbage_GeneratesRawValueOnly()
    {
        const string str = "Shiver me timbers";
        var picker = new CommandLineObjectPicker(new[] { str }, GetActivator());

        Assert.Multiple(() =>
        {
            Assert.That(picker[0].RawValue, Is.EqualTo(str));
            Assert.That(picker[0].DatabaseEntities, Is.Null);
            Assert.That(picker[0].Database, Is.Null);
            Assert.That(picker[0].Table, Is.Null);
            Assert.That(picker[0].Type, Is.Null);
        });
    }

    [Test]
    public void Test_PickCatalogueByID_PickOne()
    {
        var cata = WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new[] { $"Catalogue:{cata.ID}" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities.Single(), Is.EqualTo(cata));


        //specifying the same ID twice shouldn't return duplicate objects
        picker = new CommandLineObjectPicker(new[] { $"Catalogue:{cata.ID},{cata.ID}" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities.Single(), Is.EqualTo(cata));
    }

    /// <summary>
    /// Tests behaviour of picker when user passes an explicit empty string e.g. ./rdmp.exe DoSomething " "
    /// </summary>
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\r\n")]
    public void Test_PickerForWhitespace(string val)
    {
        var picker = new CommandLineObjectPicker(new[] { val }, GetActivator());

        Assert.That(picker.Length, Is.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(picker[0].Database, Is.Null);
            Assert.That(picker[0].DatabaseEntities, Is.Null);
            Assert.That(picker[0].ExplicitNull, Is.False);
            Assert.That(picker[0].RawValue, Is.EqualTo(val));
            Assert.That(picker[0].Type, Is.Null);

            Assert.That(picker[0].GetValueForParameterOfType(typeof(string)), Is.EqualTo(val));
            Assert.That(picker.HasArgumentOfType(0, typeof(string)));
        });
    }

    [Test]
    public void Test_PickCatalogueByID_PickTwo()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new[] { $"Catalogue:{cata1.ID},{cata2.ID}" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata2));
    }

    [Test]
    public void Test_PickCatalogueByName_PickTwo()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();
        var cata3 = WhenIHaveA<Catalogue>();

        cata1.Name = "lolzy";
        cata2.Name = "lolxy";
        cata3.Name = "trollolxy"; //does not match pattern

        cata1.SaveToDatabase();
        cata2.SaveToDatabase();
        cata3.SaveToDatabase();

        var picker = new CommandLineObjectPicker(new[] { "Catalogue:lol*" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata2));
    }

    [Test]
    public void TestPicker_TypeYieldsEmptyArrayOfObjects()
    {
        foreach (var cat in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
            cat.DeleteInDatabase();

        Assert.That(RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), Is.Empty);

        //when interpreting the string "Catalogue" for a command
        var picker = new CommandLineObjectPicker(new[] { "Catalogue" }, GetActivator());

        Assert.Multiple(() =>
        {
            //we can pick it as either a Catalogue or a collection of all the Catalogues
            Assert.That(picker.Arguments.Single().Type, Is.EqualTo(typeof(Catalogue)));
            Assert.That(picker.Arguments.Single().DatabaseEntities, Is.Empty);

            //when interpretting as a Type we get Catalogue
            Assert.That(picker.Arguments.First().HasValueOfType(typeof(Type)));
            Assert.That(picker.Arguments.Single().GetValueForParameterOfType(typeof(Type)), Is.EqualTo(typeof(Catalogue)));

            //if it is looking for an ienumerable of objects
            Assert.That(picker.Arguments.First().HasValueOfType(typeof(IMapsDirectlyToDatabaseTable[])));
            Assert.That((IMapsDirectlyToDatabaseTable[])picker.Arguments.First()
                .GetValueForParameterOfType(typeof(IMapsDirectlyToDatabaseTable[])), Is.Empty);

            Assert.That(picker.Arguments.First().HasValueOfType(typeof(Catalogue[])));
            Assert.That(
                ((Catalogue[])picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue[]))).ToArray(), Is.Empty);
        });
    }

    [TestCase(typeof(PickDatabase))]
    [TestCase(typeof(PickTable))]
    [TestCase(typeof(PickObjectByID))]
    [TestCase(typeof(PickObjectByName))]
    public void Pickers_ShouldAllHaveValidExamples_MatchingRegex(Type pickerType)
    {
        var mem = new MemoryDataExportRepository();

        //create some objects that the examples can successfully reference
        new Catalogue(mem.CatalogueRepository, "mycata1"); //ID = 1
        new Catalogue(mem.CatalogueRepository, "mycata2"); //ID = 2
        new Catalogue(mem.CatalogueRepository, "mycata3"); //ID = 3

        var picker = (PickObjectBase)ObjectConstructor.Construct(pickerType, GetActivator(new RepositoryProvider(mem)));

        Assert.Multiple(() =>
        {
            Assert.That(picker.Help, Is.Not.Empty, $"No Help for picker {picker}");
            Assert.That(picker.Format, Is.Not.Empty, $"No Format for picker {picker}");
            Assert.That(picker.Examples, Is.Not.Null, $"No Examples for picker {picker}");
        });
        Assert.That(picker.Examples, Is.Not.Empty, $"No Examples for picker {picker}");

        foreach (var example in picker.Examples)
        {
            //examples should be matched by the picker!
            Assert.That(picker.IsMatch(example, 0), $"Example of picker '{picker}' did not match the regex,listed example is '{example}'");

            var result = picker.Parse(example, 0);

            Assert.That(result, Is.Not.Null);
        }
    }

    [Test]
    public void PickTypeName()
    {
        var picker = new CommandLineObjectPicker(new[] { "Name" }, GetActivator());

        Assert.Multiple(() =>
        {
            Assert.That(picker[0].Type, Is.Null);
            Assert.That(picker[0].RawValue, Is.EqualTo("Name"));
        });
    }

    [TestCase("null")]
    [TestCase("NULL")]
    public void PickNull(string nullString)
    {
        var picker = new CommandLineObjectPicker(new[] { nullString }, GetActivator());
        Assert.That(picker[0].ExplicitNull);
    }

    [Test]
    public void Test_PickCatalogueByName_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        cata1.Name = "Biochem";
        cata2.Name = "Haematology";

        var picker = new CommandLineObjectPicker(new[] { "c:*io*" }, GetActivator());

        Assert.Multiple(() =>
        {
            Assert.That(picker[0].DatabaseEntities[0], Is.EqualTo(cata1));
            Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Test_PickCatalogueByID_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new[] { $"c:{cata1.ID},{cata2.ID}" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata2));
    }

    [Test]
    public void Test_PickCatalogueByTypeOnly_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        // c is short for Catalogue
        // so this would be the use case 'rdmp cmd list Catalogue' where user can instead write 'rdmp cmd list c'
        var picker = new CommandLineObjectPicker(new[] { "c" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(cata2));
    }

    [Test]
    public void Test_PickWithPropertyQuery_CatalogueItemsByCatalogue()
    {
        // these two belong to the same catalogue
        var ci = WhenIHaveA<CatalogueItem>();
        var ci2 = new CatalogueItem(ci.CatalogueRepository, ci.Catalogue, "My item 2");

        // this one belongs to a different catalogue
        var ci3 = WhenIHaveA<CatalogueItem>();

        var cataId = ci.Catalogue.ID;
        var picker = new CommandLineObjectPicker(new[] { $"CatalogueItem?Catalogue_ID:{cataId}" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(ci));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(ci2));
        Assert.That(picker[0].DatabaseEntities, Does.Not.Contain(ci3));
    }

    [Test]
    public void Test_PickWithPropertyQuery_CatalogueByFolder()
    {
        // Catalogues
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();
        var c3 = WhenIHaveA<Catalogue>();

        c1.Folder = "\\datasets\\hi\\";
        c2.Folder = "\\datasets\\no\\";
        c3.Folder = "\\datasets\\hi\\";

        var picker = new CommandLineObjectPicker(new[] { "Catalogue?Folder:*hi*" }, GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(2));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(c1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(c3));
        Assert.That(picker[0].DatabaseEntities, Does.Not.Contain(c2));
    }

    [Test]
    public void Test_PickWithPropertyQuery_PeriodicityNull()
    {
        // Catalogues
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();

        c1.PivotCategory_ExtractionInformation_ID = 10;
        c2.PivotCategory_ExtractionInformation_ID = null;

        var picker = new CommandLineObjectPicker(new[] { "Catalogue?PivotCategory_ExtractionInformation_ID:null" },
            GetActivator());

        Assert.That(picker[0].DatabaseEntities, Has.Count.EqualTo(1));
        Assert.That(picker[0].DatabaseEntities, Does.Contain(c2));
    }

    [Test]
    public void Test_PickWithPropertyQuery_UnknownProperty()
    {
        var ex = Assert.Throws<Exception>(() =>
            new CommandLineObjectPicker(new[] { "Catalogue?Blarg:null" }, GetActivator()));
        Assert.That(ex?.Message, Is.EqualTo("Unknown property 'Blarg'.  Did not exist on Type 'Catalogue'"));
    }
}