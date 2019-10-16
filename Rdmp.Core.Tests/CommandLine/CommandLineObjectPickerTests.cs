using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.DictionaryAdapter;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.CommandLine;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine
{
    class CommandLineObjectPickerTests : UnitTests
    {

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }


        [Test]
        public void Test_RandomGarbage_GeneratesRawValueOnly()
        {
            string str = $"Shiver me timbers";
             var picker = new CommandLineObjectPicker(new []{str}, RepositoryLocator);

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

           var picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata.ID}"}, RepositoryLocator);

           Assert.AreEqual(cata,picker[0].DatabaseEntities.Single());

           
           //specifying the same ID twice shouldn't return duplicate objects
           picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata.ID},{cata.ID}"}, RepositoryLocator);

           Assert.AreEqual(cata,picker[0].DatabaseEntities.Single());
        }

        
        [Test]
        public void Test_PickCatalogueByID_PickTwo()
        {
            var cata1 =  WhenIHaveA<Catalogue>();
            var cata2 =  WhenIHaveA<Catalogue>();

            var picker = new CommandLineObjectPicker(new []{$"Catalogue:{cata1.ID},{cata2.ID}"}, RepositoryLocator);

            Assert.AreEqual(cata1,picker[0].DatabaseEntities[0]);
            Assert.AreEqual(cata2,picker[0].DatabaseEntities[1]);
            Assert.AreEqual(2,picker[0].DatabaseEntities.Count);
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

           var picker = new CommandLineObjectPicker(new []{$"Catalogue:lol*"}, RepositoryLocator);

           Assert.AreEqual(cata1, picker[0].DatabaseEntities[0]);
           Assert.AreEqual(cata2, picker[0].DatabaseEntities[1]);
           Assert.AreEqual(2,picker[0].DatabaseEntities.Count);
        }

        [TestCase(typeof(PickDatabase))]
        [TestCase(typeof(PickTable))]
        [TestCase(typeof(PickObjectByID))]
        [TestCase(typeof(PickObjectByName))]
        public void Pickers_ShouldAllHaveValidExamples_MatchingRegex(Type pickerType)
        {
            var oc = new ObjectConstructor();

            var mem = new MemoryDataExportRepository();
            mem.CatalogueRepository.MEF = MEF;

            //create some objects that the examples can successfully reference
            new Catalogue(mem.CatalogueRepository, "mycata1"); //ID = 1
            new Catalogue(mem.CatalogueRepository, "mycata2"); //ID = 2
            new Catalogue(mem.CatalogueRepository, "mycata3"); //ID = 3


            PickObjectBase picker = (PickObjectBase) oc.Construct(pickerType, new RepositoryProvider(mem));

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
    }
}
