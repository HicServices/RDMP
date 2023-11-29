// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

internal class ColumnInfoTests : DatabaseTests
{
    [Test]
    public void CreateNewColumnInfoInDatabase_NewColumns_NewColumnsAreEqualAfterSave()
    {
        TableInfo parent = null;
        ColumnInfo child = null;

        try
        {
            parent = new TableInfo(CatalogueRepository, "CHI");
            child = new ColumnInfo(CatalogueRepository, "chi", "varchar(10)", parent)
            {
                Description = "The community health index, 10 digits of which the first 6 are date of birth",
                Status = ColumnInfo.ColumnStatus.Active,
                RegexPattern = "\\d*",
                ValidationRules = "Last digit must be odd for gents and even for ladies"
            };

            child.SaveToDatabase();

            var childAfter = CatalogueRepository.GetObjectByID<ColumnInfo>(child.ID);

            Assert.That(childAfter.Name, Is.EqualTo(child.Name));
            Assert.That(childAfter.Description, Is.EqualTo(child.Description));
            Assert.That(childAfter.Status, Is.EqualTo(child.Status));
            Assert.That(childAfter.RegexPattern, Is.EqualTo(child.RegexPattern));
            Assert.That(childAfter.ValidationRules, Is.EqualTo(child.ValidationRules));
        }
        finally
        {
            child.DeleteInDatabase();
            parent.DeleteInDatabase();
        }
    }

    [Test]
    public void GetAllColumnInfos_moreThan1_pass()
    {
        var parent = new TableInfo(CatalogueRepository, "Slalom");

        try
        {
            var ci = new ColumnInfo(CatalogueRepository, "MyAwesomeColumn", "varchar(1000)", parent);

            try
            {
                Assert.That(CatalogueRepository.GetAllObjectsWithParent<ColumnInfo>(parent).Length == 1);
            }
            finally
            {
                ci.DeleteInDatabase();
            }
        }
        finally
        {
            parent.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateNewColumnInfoInDatabase_valid_pass()
    {
        var parent = new TableInfo(CatalogueRepository, "Lazors");
        var columnInfo = new ColumnInfo(CatalogueRepository, "Lazor Reflection Vol", "varchar(1000)", parent);

        Assert.That(columnInfo, Is.Not.Null);

        columnInfo.DeleteInDatabase();

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            CatalogueRepository.GetObjectByID<ColumnInfo>(columnInfo.ID));
        Assert.That(ex.Message.StartsWith($"Could not find ColumnInfo with ID {columnInfo.ID}"), ex.Message);

        parent.DeleteInDatabase();
    }

    [Test]
    public void update_changeAllProperties_pass()
    {
        var parent = new TableInfo(CatalogueRepository, "Rokkits");
        var column = new ColumnInfo(CatalogueRepository, "ExplosiveVol", "varchar(1000)", parent)
        {
            Digitisation_specs = "Highly digitizable",
            Format = "Jpeg",
            Name = "mycol",
            Source = "Bazooka",
            Data_type = "Whatever"
        };

        column.SaveToDatabase();

        var columnAfter = CatalogueRepository.GetObjectByID<ColumnInfo>(column.ID);

        Assert.That(columnAfter.Digitisation_specs == "Highly digitizable");
        Assert.That(columnAfter.Format == "Jpeg");
        Assert.That(columnAfter.Name == "mycol");
        Assert.That(columnAfter.Source == "Bazooka");
        Assert.That(columnAfter.Data_type == "Whatever");

        columnAfter.DeleteInDatabase();
        parent.DeleteInDatabase();
    }

    [Test]
    public void Test_GetRAWStageTypeWhenPreLoadDiscardedDilution()
    {
        var parent = new TableInfo(CatalogueRepository, "Rokkits");
        var column = new ColumnInfo(CatalogueRepository, "MyCol", "varchar(4)", parent);

        var discard = new PreLoadDiscardedColumn(CatalogueRepository, parent, "MyCol")
        {
            SqlDataType = "varchar(10)",
            Destination = DiscardedColumnDestination.Dilute
        };
        discard.SaveToDatabase();

        Assert.That(column.GetRuntimeDataType(LoadStage.PostLoad), Is.EqualTo("varchar(4)"));
        Assert.That(column.GetRuntimeDataType(LoadStage.AdjustStaging), Is.EqualTo("varchar(4)"));
        Assert.That(column.GetRuntimeDataType(LoadStage.AdjustRaw), Is.EqualTo("varchar(10)"));

        discard.DeleteInDatabase();
        parent.DeleteInDatabase();
    }
}