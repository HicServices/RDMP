using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandLine;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
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
        public void Test_RandomGarbage_Throws()
        {
            var ex = Assert.Throws<CommandLineObjectPickerParseException>(()=> new CommandLineObjectPicker(new []{$"Shiver me timbers"}, RepositoryLocator));

            Assert.AreEqual(0,ex.Index);
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
    }
}
