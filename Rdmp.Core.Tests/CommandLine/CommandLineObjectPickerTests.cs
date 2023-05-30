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

class CommandLineObjectPickerTests : UnitTests
{

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupMEF();
    }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        foreach(var c in Repository.GetAllObjects<Catalogue>())
        {
            c.DeleteInDatabase();
        }
    }


    [Test]
    public void Test_RandomGarbage_GeneratesRawValueOnly()
    {
        var str = $"Shiver me timbers";
        var picker = new CommandLineObjectPicker(new []{str}, GetActivator());

        Assert.AreEqual(str,picker[0].RawValue); 
        Assert.IsNull(picker[0].DatabaseEntities);
        Assert.IsNull(picker[0].Database);
        Assert.IsNull(picker[0].Table);
        Assert.IsNull(picker[0].Type);
    }

    [Test]
    public void Test_PickCatalogueByID_PickOne()
    {
        var cata =  WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata.ID}"}, GetActivator());

        Assert.AreEqual(cata,picker[0].DatabaseEntities.Single());

           
        //specifying the same ID twice shouldn't return duplicate objects
        picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata.ID},{cata.ID}"}, GetActivator());

        Assert.AreEqual(cata,picker[0].DatabaseEntities.Single());
    }

    /// <summary>
    /// Tests behaviour of picker when user passes an explicit empty string e.g. ./rdmp.exe DoSomething " "
    /// </summary>
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\r\n")]
    public void Test_PickerForWhitespace(string val)
    {
        var picker = new CommandLineObjectPicker(new []{val }, GetActivator());

        Assert.AreEqual(1,picker.Length);
            
        Assert.IsNull(picker[0].Database);
        Assert.IsNull(picker[0].DatabaseEntities);
        Assert.IsFalse(picker[0].ExplicitNull);
        Assert.AreEqual(val,picker[0].RawValue);
        Assert.IsNull(picker[0].Type);
            
        Assert.AreEqual(val,picker[0].GetValueForParameterOfType(typeof(string)));
        Assert.IsTrue(picker.HasArgumentOfType(0, typeof(string)));
    }
        
    [Test]
    public void Test_PickCatalogueByID_PickTwo()
    {
        var cata1 =  WhenIHaveA<Catalogue>();
        var cata2 =  WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata1.ID},{cata2.ID}"}, GetActivator());

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(cata1,picker[0].DatabaseEntities);
        Assert.Contains(cata2,picker[0].DatabaseEntities);
    }
        
    [Test]
    public void Test_PickCatalogueByName_PickTwo()
    {
        var cata1 =  WhenIHaveA<Catalogue>();
        var cata2 =  WhenIHaveA<Catalogue>();
        var cata3 =  WhenIHaveA<Catalogue>();

        cata1.Name = "lolzy";
        cata2.Name = "lolxy";
        cata3.Name = "trollolxy"; //does not match pattern

        cata1.SaveToDatabase();
        cata2.SaveToDatabase();
        cata3.SaveToDatabase();

        var picker = new CommandLineObjectPicker(new []{$"Catalogue:lol*"}, GetActivator());

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(cata1, picker[0].DatabaseEntities);
        Assert.Contains(cata2, picker[0].DatabaseEntities);
    }

    [Test]
    public void TestPicker_TypeYieldsEmptyArrayOfObjects()
    {
        foreach(var cat in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
            cat.DeleteInDatabase();

        Assert.IsEmpty(RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());

        //when interpreting the string "Catalogue" for a command
        var picker = new CommandLineObjectPicker(new []{"Catalogue" }, GetActivator());

        //we can pick it as either a Catalogue or a collection of all the Catalogues
        Assert.AreEqual(typeof(Catalogue),picker.Arguments.Single().Type);
        Assert.IsEmpty(picker.Arguments.Single().DatabaseEntities);

        //when interpretting as a Type we get Catalogue
        Assert.IsTrue(picker.Arguments.First().HasValueOfType(typeof(Type)));
        Assert.AreEqual(typeof(Catalogue),picker.Arguments.Single().GetValueForParameterOfType(typeof(Type)));

        //if it is looking for an ienumerable of objects
        Assert.IsTrue(picker.Arguments.First().HasValueOfType(typeof(IMapsDirectlyToDatabaseTable[])));
        Assert.IsEmpty((IMapsDirectlyToDatabaseTable[])picker.Arguments.First().GetValueForParameterOfType(typeof(IMapsDirectlyToDatabaseTable[])));

        Assert.IsTrue(picker.Arguments.First().HasValueOfType(typeof(Catalogue[])));
        Assert.IsEmpty(((Catalogue[])picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue[]))).ToArray());

    }

    [TestCase(typeof(PickDatabase))]
    [TestCase(typeof(PickTable))]
    [TestCase(typeof(PickObjectByID))]
    [TestCase(typeof(PickObjectByName))]
    public void Pickers_ShouldAllHaveValidExamples_MatchingRegex(Type pickerType)
    {
        var oc = new ObjectConstructor();

        var mem = new MemoryDataExportRepository();
        mem.MEF = MEF;

        //create some objects that the examples can successfully reference
        new Catalogue(mem.CatalogueRepository, "mycata1"); //ID = 1
        new Catalogue(mem.CatalogueRepository, "mycata2"); //ID = 2
        new Catalogue(mem.CatalogueRepository, "mycata3"); //ID = 3

        var picker = (PickObjectBase) oc.Construct(pickerType, GetActivator(new RepositoryProvider(mem)));

        Assert.IsNotEmpty(picker.Help,"No Help for picker {0}",picker);
        Assert.IsNotEmpty(picker.Format,"No Format for picker {0}",picker);
        Assert.IsNotNull(picker.Examples,"No Examples for picker {0}",picker);
        Assert.IsNotEmpty(picker.Examples,"No Examples for picker {0}",picker);

        foreach (var example in picker.Examples)
        {
            //examples should be matched by the picker!
            Assert.IsTrue(picker.IsMatch(example,0),"Example of picker '{0}' did not match the regex,listed example is '{1}'",picker,example);

            var result = picker.Parse(example, 0);

            Assert.IsNotNull(result);
        }
    }
        
    [Test]
    public void PickTypeName()
    {
        var picker = new CommandLineObjectPicker(new []{"Name"}, GetActivator());
            
        Assert.IsNull(picker[0].Type);
        Assert.AreEqual("Name",picker[0].RawValue);
    }

    [TestCase("null")]
    [TestCase("NULL")]
    public void PickNull(string nullString)
    {
        var picker = new CommandLineObjectPicker(new []{nullString}, GetActivator());
        Assert.IsTrue(picker[0].ExplicitNull);
    }
    [Test]
    public void Test_PickCatalogueByName_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        cata1.Name = "Biochem";
        cata2.Name = "Haematology";

        var picker = new CommandLineObjectPicker(new[] { $"c:*io*" }, GetActivator());

        Assert.AreEqual(cata1, picker[0].DatabaseEntities[0]);
        Assert.AreEqual(1, picker[0].DatabaseEntities.Count);
    }

    [Test]
    public void Test_PickCatalogueByID_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new[] { $"c:{cata1.ID},{cata2.ID}" }, GetActivator());

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(cata1, picker[0].DatabaseEntities);
        Assert.Contains(cata2, picker[0].DatabaseEntities);
    }

    [Test]
    public void Test_PickCatalogueByTypeOnly_WithShortCode()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        // c is short for Catalogue 
        // so this would be the use case 'rdmp cmd list Catalogue' where user can instead write 'rdmp cmd list c'
        var picker = new CommandLineObjectPicker(new[] { $"c" }, GetActivator());

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(cata1, picker[0].DatabaseEntities);
        Assert.Contains(cata2, picker[0].DatabaseEntities);
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

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(ci, picker[0].DatabaseEntities);
        Assert.Contains(ci2, picker[0].DatabaseEntities);
        Assert.IsFalse(picker[0].DatabaseEntities.Contains(ci3));
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

        var picker = new CommandLineObjectPicker(new[] { $"Catalogue?Folder:*hi*" }, GetActivator());

        Assert.AreEqual(2, picker[0].DatabaseEntities.Count);
        Assert.Contains(c1, picker[0].DatabaseEntities);
        Assert.Contains(c3, picker[0].DatabaseEntities);
        Assert.IsFalse(picker[0].DatabaseEntities.Contains(c2));
    }
    [Test]
    public void Test_PickWithPropertyQuery_PeriodicityNull()
    {
        // Catalogues
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();

        c1.PivotCategory_ExtractionInformation_ID = 10;
        c2.PivotCategory_ExtractionInformation_ID = null;

        var picker = new CommandLineObjectPicker(new[] { $"Catalogue?PivotCategory_ExtractionInformation_ID:null" }, GetActivator());

        Assert.AreEqual(1, picker[0].DatabaseEntities.Count);
        Assert.Contains(c2, picker[0].DatabaseEntities);
    }
    [Test]
    public void Test_PickWithPropertyQuery_UnknownProperty()
    {
        var ex = Assert.Throws<Exception>(()=>new CommandLineObjectPicker(new[] { $"Catalogue?Blarg:null" }, GetActivator()));
        Assert.AreEqual("Unknown property 'Blarg'.  Did not exist on Type 'Catalogue'", ex.Message);
    }
}