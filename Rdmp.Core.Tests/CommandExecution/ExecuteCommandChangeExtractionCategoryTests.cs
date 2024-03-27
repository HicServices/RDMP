// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Tests.Common;
namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandChangeExtractionCategoryTests : DatabaseTests
{
    private Catalogue _cata1;
    private TableInfo _t1;
    private ColumnInfo _c1;
    private CatalogueItem _ci1;

    private ExtractionInformation _extractionInfo1;

    [Test]
    public void TestProjectSpecificCatalogueChangeToSuplemental()
    {
        //change project specific to supplemental
        _cata1 =new Catalogue(CatalogueRepository, "Dataset1");
        _t1 = new TableInfo(CatalogueRepository, "T1");

        _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
        
        _ci1 = new CatalogueItem(CatalogueRepository, _cata1, "PrivateIdentifierA");

        _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
        {
            Order = 123,
            ExtractionCategory = ExtractionCategory.ProjectSpecific,
            IsExtractionIdentifier = true
        };
        _extractionInfo1.CatalogueItem.Catalogue.InjectKnown(new CatalogueExtractabilityStatus(true, true));

        ExtractionInformation[] eid = { _extractionInfo1 };
        var cmd = new ExecuteCommandChangeExtractionCategory(new ThrowImmediatelyActivator(RepositoryLocator), eid, ExtractionCategory.Supplemental);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(_extractionInfo1.ExtractionCategory, Is.EqualTo(ExtractionCategory.Supplemental));
    }

    [Test]
    public void TestExtractionCategoryCatalogueChangeFromSupplementalToCore()
    {
        //change a project specific column to core
        _cata1 = new Catalogue(CatalogueRepository, "Dataset1");
        _t1 = new TableInfo(CatalogueRepository, "T1");

        _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);

        _ci1 = new CatalogueItem(CatalogueRepository, _cata1, "PrivateIdentifierA");

        _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
        {
            Order = 123,
            ExtractionCategory = ExtractionCategory.Supplemental,
            IsExtractionIdentifier = true
        };
        _extractionInfo1.CatalogueItem.Catalogue.InjectKnown(new CatalogueExtractabilityStatus(true, true));

        ExtractionInformation[] eid = { _extractionInfo1 };
        var cmd = new ExecuteCommandChangeExtractionCategory(new ThrowImmediatelyActivator(RepositoryLocator), eid, ExtractionCategory.Core);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(_extractionInfo1.ExtractionCategory, Is.EqualTo(ExtractionCategory.ProjectSpecific));
    }
}
