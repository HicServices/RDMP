// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport;

[Category("Database")]
public class ExtractionConfigurationTest : DatabaseTests
{
    [Test]
    public void ExtractableColumnTest()
    {
        ExtractableDataSet dataSet = null;
        ExtractionConfiguration configuration = null;
        Project project = null;

        Catalogue cata = null;
        CatalogueItem cataItem = null;
        ColumnInfo column = null;
        TableInfo table = null;

        ExtractionInformation extractionInformation = null;
        ExtractableColumn extractableColumn = null;

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

            extractableColumn = new ExtractableColumn(DataExportRepository, dataSet, configuration,
                extractionInformation, 0, "Hashme2(Name)");
            Assert.AreEqual(configuration.GetAllExtractableColumnsFor(dataSet).Length, 1);
        }
        finally
        {
            extractionInformation?.DeleteInDatabase();

            column?.DeleteInDatabase();

            table?.DeleteInDatabase();

            cataItem?.DeleteInDatabase();

            configuration?.DeleteInDatabase();

            project?.DeleteInDatabase();

            dataSet?.DeleteInDatabase();

            cata?.DeleteInDatabase();
        }
    }
}