// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
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

            Assert.AreEqual(child.Name, childAfter.Name);
            Assert.AreEqual(child.Description, childAfter.Description);
            Assert.AreEqual(child.Status, childAfter.Status);
            Assert.AreEqual(child.RegexPattern, childAfter.RegexPattern);
            Assert.AreEqual(child.ValidationRules, childAfter.ValidationRules);
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
                Assert.IsTrue(CatalogueRepository.GetAllObjectsWithParent<ColumnInfo>(parent).Length == 1);
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

        Assert.NotNull(columnInfo);

        columnInfo.DeleteInDatabase();

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            CatalogueRepository.GetObjectByID<ColumnInfo>(columnInfo.ID));
        Assert.IsTrue(ex.Message.StartsWith($"Could not find ColumnInfo with ID {columnInfo.ID}"), ex.Message);

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

        Assert.IsTrue(columnAfter.Digitisation_specs == "Highly digitizable");
        Assert.IsTrue(columnAfter.Format == "Jpeg");
        Assert.IsTrue(columnAfter.Name == "mycol");
        Assert.IsTrue(columnAfter.Source == "Bazooka");
        Assert.IsTrue(columnAfter.Data_type == "Whatever");

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

        Assert.AreEqual("varchar(4)", column.GetRuntimeDataType(LoadStage.PostLoad));
        Assert.AreEqual("varchar(4)", column.GetRuntimeDataType(LoadStage.AdjustStaging));
        Assert.AreEqual("varchar(10)", column.GetRuntimeDataType(LoadStage.AdjustRaw));

        discard.DeleteInDatabase();
        parent.DeleteInDatabase();
    }
}