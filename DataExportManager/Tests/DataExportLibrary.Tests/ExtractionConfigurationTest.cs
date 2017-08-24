using System;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    [Category("Database")]
    public class ExtractionConfigurationTest : DatabaseTests
    {
        [Test]
        public void ExtractableColumnTest()
        {
            ExtractableDataSet dataSet =null;
            ExtractionConfiguration configuration = null;
            Project project = null;

            Catalogue cata = null;
            CatalogueItem cataItem = null;
            ColumnInfo column = null;
            TableInfo table = null;

            ExtractionInformation extractionInformation=null;
            ExtractableColumn extractableColumn=null;
            
            try
            {             
                //setup catalogue side of things
                cata = new Catalogue(CatalogueRepository, "unit_test_ExtractableColumnTest_Cata");
                cataItem = new CatalogueItem(CatalogueRepository, cata, "unit_test_ExtractableColumnTest_CataItem");
                table = new TableInfo(CatalogueRepository, "DaveTable");
                column = new ColumnInfo(CatalogueRepository, "Name", "string", table);
                cataItem.SetColumnInfo(column);

                extractionInformation = new ExtractionInformation(CatalogueRepository, cataItem, column, "Hashme(Name)");

                //setup extractor side of things
                dataSet = new ExtractableDataSet(DataExportRepository, cata);
                project = new Project(DataExportRepository, "unit_test_ExtractableColumnTest_Proj");

                configuration = new ExtractionConfiguration(DataExportRepository, project);

                extractableColumn = new ExtractableColumn(DataExportRepository, dataSet, configuration, extractionInformation, 0, "Hashme2(Name)");
                Assert.AreEqual(configuration.GetAllExtractableColumnsFor(dataSet).Length, 1);
            }
            finally 
            {
                if (extractionInformation != null)
                    extractionInformation.DeleteInDatabase();

                if (column != null)
                    column.DeleteInDatabase();

                if (table != null)
                    table.DeleteInDatabase();
                
                if (cataItem != null)
                    cataItem.DeleteInDatabase();

                if (configuration != null)
                    configuration.DeleteInDatabase();

                if (project != null)
                    project.DeleteInDatabase();

                if (dataSet != null)
                    dataSet.DeleteInDatabase();

                if (cata != null)
                    cata.DeleteInDatabase();


                
            }
        }
    }
}
