using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANOStore.ANOEngineering;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using Sharing.Sharing;
using Tests.Common;

namespace CatalogueLibraryTests.JsonSerializationTests
{
    public class JsonSerializationTests:DatabaseTests
    {
        [Test]
        public void TestSerialization_Catalogue()
        {
            Catalogue c = new Catalogue(RepositoryLocator.CatalogueRepository,"Fish");
            
            MySerializeableTestClass mySerializeable = new MySerializeableTestClass(new ShareManager(RepositoryLocator));
            mySerializeable.SelectedCatalogue = c;
            mySerializeable.Title = "War and Pieces";
            
            var dbConverter = new DatabaseEntityJsonConverter(RepositoryLocator);
            var lazyConverter = new PickAnyConstructorJsonConverter(RepositoryLocator);


            var asString = JsonConvert.SerializeObject(mySerializeable, dbConverter,lazyConverter);
            var mySerializeableAfter = (MySerializeableTestClass)JsonConvert.DeserializeObject(asString, typeof(MySerializeableTestClass), new JsonConverter[] { dbConverter, lazyConverter });

            Assert.AreNotEqual(mySerializeable, mySerializeableAfter);
            Assert.AreEqual(mySerializeable.SelectedCatalogue, mySerializeableAfter.SelectedCatalogue);
            Assert.AreEqual(mySerializeable.SelectedCatalogue.Name, mySerializeableAfter.SelectedCatalogue.Name);
            Assert.AreEqual("War and Pieces", mySerializeableAfter.Title);
            mySerializeableAfter.SelectedCatalogue.Name = "Cannon balls";
            mySerializeableAfter.SelectedCatalogue.SaveToDatabase();
            
            Assert.AreNotEqual(mySerializeable.SelectedCatalogue.Name, mySerializeableAfter.SelectedCatalogue.Name);
        }

        //todo null Catalogue test case
    }
    
    
    public class MySerializeableTestClass
    {
        public string Title { get; set; }

        public Catalogue SelectedCatalogue { get; set; }

        private readonly ShareManager _sm;

        public MySerializeableTestClass(IRDMPPlatformRepositoryServiceLocator locator)
        {
            _sm = new ShareManager(locator);
        }
        
        public MySerializeableTestClass(ShareManager sm)
        {
            _sm = sm;
        }
    }

}
