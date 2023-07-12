// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class ExtractionInformationTests : DatabaseTests
{
    ///////////////Create the things that we are going to create relationships between /////////////////

    private Catalogue cata;
    private CatalogueItem cataItem;
    private TableInfo ti;
    private ColumnInfo columnInfo;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        cata = new Catalogue(CatalogueRepository, "ExtractionInformationTestsCatalogue");
        cataItem = new CatalogueItem(CatalogueRepository, cata, "QuadlzorVelocity");
        ti = new TableInfo(CatalogueRepository, "HighEnergyShizzle");
        columnInfo = new ColumnInfo(CatalogueRepository, "VelocityOfMatter", "int", ti);

        ////////////Check the creation worked ok
        Assert.IsNotNull(cata); //catalogue
        Assert.IsNotNull(cataItem);

        Assert.IsNotNull(ti); //underlying table stuff
        Assert.IsNotNull(columnInfo);

        ////////////// Create links between stuff and check they were created successfully //////////////

        //create a link between catalogue item lazor and velocity column
        cataItem.SetColumnInfo(columnInfo);
                
    }

    [Test]
    public void BasicIDsAreCorrect()
    {
        var firstLinked = cataItem.ColumnInfo;
        Assert.IsTrue(firstLinked != null);
        Assert.IsTrue(firstLinked.ID == columnInfo.ID);
    }


    [Test]
    public void test_creating_ExtractionFilter()
    {

        ExtractionInformation extractInfo = null;
        ExtractionFilter filterFastThings = null;
        ExtractionFilterParameter parameter = null;

        try
        {
            //define extraction information
            extractInfo = new ExtractionInformation(CatalogueRepository, cataItem, columnInfo, "ROUND(VelocityOfMatter,2) VelocityOfMatterRounded");

            //define filter and parameter
            filterFastThings = new ExtractionFilter(CatalogueRepository, "FastThings", extractInfo)
            {
                WhereSQL = "VelocityOfMatter > @X",
                Description = "Query to identify things that travel faster than X miles per hour!"
            };
            filterFastThings.SaveToDatabase();
            Assert.AreEqual(filterFastThings.Name, "FastThings");

            parameter = new ExtractionFilterParameter(CatalogueRepository, "DECLARE @X INT", filterFastThings);

            Assert.IsNotNull(parameter);
            Assert.AreEqual(parameter.ParameterName ,"@X");

            parameter.Value = "500";
            parameter.SaveToDatabase();

            var afterSave = CatalogueRepository.GetObjectByID<ExtractionFilterParameter>(parameter.ID);
            Assert.AreEqual(afterSave.Value ,"500");


            var filterFastThings_NewCopyFromDB = CatalogueRepository.GetObjectByID<ExtractionFilter>(filterFastThings.ID);

            Assert.AreEqual(filterFastThings.ID, filterFastThings_NewCopyFromDB.ID);
            Assert.AreEqual(filterFastThings.Description, filterFastThings_NewCopyFromDB.Description);
            Assert.AreEqual(filterFastThings.Name, filterFastThings_NewCopyFromDB.Name);
            Assert.AreEqual(filterFastThings.WhereSQL, filterFastThings_NewCopyFromDB.WhereSQL);
        }
        finally
        {
            parameter?.DeleteInDatabase();

            //filters are children of extraction info with CASCADE DELETE so have to delete this one first if we want to test it programatically (although we could just skip deleting it since SQL will handle it anyway)
            filterFastThings?.DeleteInDatabase();

            extractInfo?.DeleteInDatabase();
        }
            


    }

    [Test]
    public void test_creating_ExtractionInformation()
    {
            
            
        ExtractionInformation extractInfo =null;

        try
        {

            //define extraction information
            //change some values and then save it
            extractInfo = new ExtractionInformation(CatalogueRepository, cataItem, columnInfo, "dave")
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Supplemental
            };
            extractInfo.SaveToDatabase();

            //confirm the insert worked
            Assert.AreEqual(extractInfo.SelectSQL,"dave");

            //fetch the extraction information via the linked CatalogueItem - ColumnInfo pair (i.e. we are testing the alternate route to fetch ExtractionInformation - by ID or by colum/item pair)
            var extractInfo2_CameFromLinker = cataItem.ExtractionInformation;
            Assert.AreEqual(extractInfo.ID, extractInfo2_CameFromLinker.ID);
            Assert.AreEqual(extractInfo.SelectSQL, extractInfo2_CameFromLinker.SelectSQL);

            //make sure it saves properly
            Assert.AreEqual(extractInfo2_CameFromLinker.Order,123 );
            Assert.AreEqual( extractInfo2_CameFromLinker.ExtractionCategory,ExtractionCategory.Supplemental);

        }
        finally
        {
            extractInfo?.DeleteInDatabase();

        }
    }
}