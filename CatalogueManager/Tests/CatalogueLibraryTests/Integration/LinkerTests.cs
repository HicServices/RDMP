using System;
using System.Linq;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    class LinkerTests : DatabaseTests
    {
        [Test]
        public void AddSameLinkTwice()
        {
            Catalogue predator = null;
            CatalogueItem lazor = null;
            TableInfo highEnergyTable = null;
            ColumnInfo velocityColumn = null;
            try
            {
                ///////////////Create the things that we are going to create relationships between /////////////////
                predator = new Catalogue(CatalogueRepository, "Predator");
                lazor = new CatalogueItem(CatalogueRepository, predator, "QuadlzorVelocity");
                highEnergyTable = new TableInfo(CatalogueRepository, "HighEnergyShizzle");
                velocityColumn = new ColumnInfo(CatalogueRepository, "Velocity Of Matter", "int", highEnergyTable);

                //now you can add as many links as you want, it just skips them
                lazor.SetColumnInfo(velocityColumn);
                Assert.AreEqual(lazor.ColumnInfo,velocityColumn);
                
            }
            finally
            {
                lazor.DeleteInDatabase(); //delete child
                predator.DeleteInDatabase(); //delete parent
                
                velocityColumn.DeleteInDatabase();//delete child
                highEnergyTable.DeleteInDatabase();//delete parent
            }
          
         
        }

        [Test]
        public void AddLinkBetween_createNewLink_pass()
        {

            ///////////////Create the things that we are going to create relationships between /////////////////
            var predator = new Catalogue(CatalogueRepository, "Predator");
            var lazor = new CatalogueItem(CatalogueRepository, predator, "QuadlzorVelocity");
            var highEnergyTable = new TableInfo(CatalogueRepository, "HighEnergyShizzle");
            var velocityColumn = new ColumnInfo(CatalogueRepository, "Velocity Of Matter", "int", highEnergyTable);

            ////////////Check the creation worked ok
            Assert.IsNotNull(predator); //catalogue
            Assert.IsNotNull(lazor);

            Assert.IsNotNull(highEnergyTable); //underlying table stuff
            Assert.IsNotNull(velocityColumn);

            ////////////// Create links between stuff and check they were created successfully //////////////

            //create a link between catalogue item lazor and velocity column
            lazor.SetColumnInfo(velocityColumn);
            Assert.IsTrue(lazor.ColumnInfo.ID == velocityColumn.ID);
            
            ////////////////cleanup ---- Delete everything that we created -------- ////////////// 
            velocityColumn.DeleteInDatabase(); //delete causes CASCADE: CatalogueItem no longer associated with ColumnInfo because ColumnInfo died
            
            lazor.RevertToDatabaseState();

            Assert.IsNull(lazor.ColumnInfo);//involves a database query so won't actually invalidate the below

            predator.DeleteInDatabase();

            highEnergyTable.DeleteInDatabase();
        }
    }
}
