// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.QueryBuilding;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class BundledLookupTableTests : UnitTests
{
    [Test]
    public void TestLookupGetDataTableFetchSql()
    {
        var l = WhenIHaveA<Lookup>();
        var t = l.PrimaryKey.TableInfo;

        var bundle = new BundledLookupTable(t);
        Assert.That(bundle.GetDataTableFetchSql(), Is.EqualTo("select * from [MyDb]..[ChildTable]"));
    }

    [Test]
    public void TestLookupGetDataTableFetchSql_WithCatalogue()
    {
        var l = WhenIHaveA<Lookup>();
        var t = l.PrimaryKey.TableInfo;

        var engineer = new ForwardEngineerCatalogue(t, t.ColumnInfos);
        engineer.ExecuteForwardEngineering(out var cata, out _, out var eis);

        var bundle = new BundledLookupTable(t);
        Assert.That(bundle.GetDataTableFetchSql(), Is.EqualTo(@"
SELECT 

ChildCol,
Desc
FROM 
ChildTable"));


        // ei 1 is supplemental now
        eis[1].ExtractionCategory = ExtractionCategory.Supplemental;
        eis[1].SaveToDatabase();

        Assert.That(bundle.GetDataTableFetchSql(), Is.EqualTo(@"
SELECT 

ChildCol
FROM 
ChildTable"));


        // ei 0 is marked IsExtractionIdentifier - so is also not a valid
        // lookup extractable column (Lookups shouldn't have patient linkage
        // identifiers in them so)
        eis[0].IsExtractionIdentifier = true;
        eis[0].SaveToDatabase();

        // so now there are no columns at all that are extractable
        var ex = Assert.Throws<QueryBuildingException>(() => bundle.GetDataTableFetchSql());
        Assert.That(
            ex.Message, Is.EqualTo("Lookup table 'ChildTable' has a Catalogue defined 'ChildTable' but it has no Core extractable columns"));
    }
}