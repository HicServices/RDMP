// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

internal class LinkerTests : DatabaseTests
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