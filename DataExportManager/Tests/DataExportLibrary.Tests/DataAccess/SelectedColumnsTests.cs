using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests.DataAccess
{
    public class SelectedColumnsTests:DatabaseTests
    {
		//Simple test SelectedColumns in which an extraction configuration is built for a test dataset with a single column configured for extraction
        [Test]
        public void CreateAndAssociateColumns()
        {
            var cata = new Catalogue(CatalogueRepository, "MyCat");
            var cataItem = new CatalogueItem(CatalogueRepository, cata,"MyCataItem");
            var TableInfo = new TableInfo(CatalogueRepository, "Cata");
            var ColumnInfo = new ColumnInfo(CatalogueRepository, "Col","varchar(10)",TableInfo);
            var ExtractionInfo = new ExtractionInformation(CatalogueRepository, cataItem, ColumnInfo, "fish");
            
            var ds = new ExtractableDataSet(DataExportRepository,cata);
            
            var proj = new Project(DataExportRepository, "MyProj");
            var config = new ExtractionConfiguration(DataExportRepository, proj);
            
            SelectedDataSets selectedDataSets;

            var extractableColumn = new ExtractableColumn(DataExportRepository, ds, config, ExtractionInfo, 1, "fish");

            try
            {
                
                selectedDataSets = new SelectedDataSets(DataExportRepository,config, ds,null);

                var cols = config.GetAllExtractableColumnsFor(ds);

                Assert.AreEqual(1,cols.Count());
                Assert.AreEqual(extractableColumn, cols.Single());

                cols = config.GetAllExtractableColumnsFor(ds);

                Assert.AreEqual(1, cols.Count());
                Assert.AreEqual(extractableColumn, cols.Single());
            }
            finally
            {
                extractableColumn.DeleteInDatabase();
                config.DeleteInDatabase();
                proj.DeleteInDatabase();

                ds.DeleteInDatabase();

                TableInfo.DeleteInDatabase();
                cata.DeleteInDatabase();

            }
        }

    }
}
