// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.DataAccess;

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

        var extractableColumn = new ExtractableColumn(DataExportRepository, ds, config, ExtractionInfo, 1, "fish");

        try
        {
            _=new SelectedDataSets(DataExportRepository,config, ds,null);

            var cols = config.GetAllExtractableColumnsFor(ds);

            Assert.AreEqual(1, cols.Length);
            Assert.AreEqual(extractableColumn, cols.Single());

            cols = config.GetAllExtractableColumnsFor(ds);

            Assert.AreEqual(1, cols.Length);
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