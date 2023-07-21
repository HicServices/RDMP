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

namespace Rdmp.Core.Tests.DataExport;

internal class DataExportRepositoryTests:DatabaseTests
{
    [Test]
    public void TestNoIsExtractionIdentifierFinding()
    {
        //nothing in database means no dodgy datasets
        Assert.IsEmpty(DataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers());

        var cata = new Catalogue(CatalogueRepository, "ommn");
        var ds = new ExtractableDataSet(DataExportRepository, cata);
        var proj = new Project(DataExportRepository, "proj");
        var config = new ExtractionConfiguration(DataExportRepository, proj);
        var sds = new SelectedDataSets(DataExportRepository, config, ds, null);

        //only one selected dataset
        var dodgy = DataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers().ToArray();
        Assert.AreEqual(1, dodgy.Length);
        Assert.AreEqual(sds,dodgy[0]);

        //make an extarctable column on that dataset
        var col = new ColumnInfo(CatalogueRepository,"ff","varchar(1)",new TableInfo(CatalogueRepository, "fff"));
        var ci = new CatalogueItem(CatalogueRepository, cata, "A");
        var ei = new ExtractionInformation(CatalogueRepository, ci, col,col.Name);
        var ec = new ExtractableColumn(DataExportRepository, ds, config, ei, 0, col.Name);
            
        //still shouldn't be dodgy
        dodgy = DataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers().ToArray();
        Assert.AreEqual(1, dodgy.Length);
        Assert.AreEqual(sds, dodgy[0]);

        //now make it non dodgy by being IsExtractionIdentifier
        ec.IsExtractionIdentifier = true;
        ec.SaveToDatabase();

        //no longer dodgy because there is an extraction identifier
        dodgy = DataExportRepository.GetSelectedDatasetsWithNoExtractionIdentifiers().ToArray();
        Assert.AreEqual(0, dodgy.Length);

    }
}