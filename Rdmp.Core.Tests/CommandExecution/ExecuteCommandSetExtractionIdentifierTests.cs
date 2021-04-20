using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using System;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandSetExtractionIdentifierTests : CommandCliTests
    {
        [Test]
        public void TestSetExtractionIdentifier_Catalogue()
        {
            var ei1 = WhenIHaveA<ExtractionInformation>();
            
            ei1.Alias = "happyfun";
            ei1.IsExtractionIdentifier = false;
            ei1.SaveToDatabase();

            var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator().Object,ei1.CatalogueItem.Catalogue,null,"happyfun");
            cmd.Execute();

            Assert.IsTrue(ei1.IsExtractionIdentifier);
        }
        [Test]
        public void TestSetExtractionIdentifier_Catalogue_PickOther()
        {
            var ei1 = WhenIHaveA<ExtractionInformation>();

            var otherCol = new ColumnInfo(Repository, "Other", "varchar", ei1.ColumnInfo.TableInfo);
            var otherCatItem = new CatalogueItem(Repository, ei1.CatalogueItem.Catalogue,"Other");
            var otherEi = new ExtractionInformation(Repository, otherCatItem, otherCol, "FFF");

            ei1.Alias = "happyfun";
            ei1.IsExtractionIdentifier = true;
            ei1.SaveToDatabase();

            // before we run the command the primary ei1 is the identifier
            Assert.IsTrue(ei1.IsExtractionIdentifier);
            Assert.IsFalse(otherEi.IsExtractionIdentifier);

            // by picking the second (FFF) we should switch
            var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator().Object, ei1.CatalogueItem.Catalogue, null, "FFF");
            cmd.Execute();

            // original should no longer be the extraction identifer
            Assert.IsFalse(ei1.IsExtractionIdentifier);

            // and the one picked should now be the only one
            Assert.IsTrue(otherEi.IsExtractionIdentifier);
        }
        [Test]
        public void TestSetExtractionIdentifier_Catalogue_ButColumnDoesNotExist()
        {
            var ei1 = WhenIHaveA<ExtractionInformation>();

            ei1.Alias = "happyfun";
            ei1.IsExtractionIdentifier = false;
            ei1.SaveToDatabase();

            var ex = Assert.Throws<Exception>(()=>
                new ExecuteCommandSetExtractionIdentifier(GetMockActivator().Object, ei1.CatalogueItem.Catalogue, null, "trollolo")
                .Execute());
            Assert.AreEqual("Could not find column(s) trollolo amongst available columns (happyfun)", ex.Message);
        }


        [Test]
        public void TestSetExtractionIdentifier_Configuration()
        {
            var ec1 = WhenIHaveA<ExtractableColumn>();

            ec1.Alias = "happyfun";
            ec1.IsExtractionIdentifier = false;
            ec1.SaveToDatabase();

            var config = Repository.GetObjectByID<ExtractionConfiguration>(ec1.ExtractionConfiguration_ID);
               
            var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator().Object, 
                ec1.CatalogueExtractionInformation.CatalogueItem.Catalogue,
                config
                , "happyfun");
            cmd.Execute();

            // affects extraction specific version
            Assert.IsTrue(ec1.IsExtractionIdentifier);

            // but not master
            Assert.IsFalse(ec1.CatalogueExtractionInformation.IsExtractionIdentifier);
        }
    }
}
