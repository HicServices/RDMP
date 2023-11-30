// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport;

[Category("Database")]
public class TestExtractableTables : TestsRequiringACohort
{
    [Test]
    public void DodgyID_CreateCohortDatabaseTable_Fails()
    {
        var ex = Assert.Throws<Exception>(() =>
            new ExtractableCohort(DataExportRepository, _externalCohortTable, -899));
        Assert.That(ex.Message, Does.StartWith("ID -899 does not exist in Cohort Definitions"));
    }


    [Test]
    public void CreateExtractableDataSet()
    {
        ExtractableDataSet eds = null;
        var cata = new Catalogue(CatalogueRepository, "TestExtractableTables Cata");
        try
        {
            //creates with a Null Catalogue until it is associated with a catalogue and saved
            eds = new ExtractableDataSet(DataExportRepository, cata);
            Assert.That(eds.Catalogue_ID, Is.EqualTo(cata.ID));
        }
        finally
        {
            eds?.DeleteInDatabase();

            cata.DeleteInDatabase();
        }
    }


    [Test]
    public void UpdateProjectDatabaseTable()
    {
        var table = new Project(DataExportRepository, "unit_test_UpdateProjectDatabaseTable");

        try
        {
            Assert.That(table.Name, Is.EqualTo("unit_test_UpdateProjectDatabaseTable"));
            table.Name = "unit_test_UpdateProjectDatabaseTable2";
            table.SaveToDatabase();

            //get fresh copy from database and ensure that all fields are the same
            var tableAfter = DataExportRepository.GetObjectByID<Project>(table.ID);
            PropertyValuesAreEquals(table, tableAfter);
        }
        finally
        {
            table.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateExtractionConfiguration()
    {
        var parent = new Project(DataExportRepository, "unit_test_CreateExtractionConfiguration");

        var table = new ExtractionConfiguration(DataExportRepository, parent);

        try
        {
            Assert.That(Environment.UserName, Is.EqualTo(table.Username));
        }
        finally
        {
            table.DeleteInDatabase(); //must delete child before parent to preserve referential integrity
            parent.DeleteInDatabase();
        }
    }


    #region helper methods

    private static void PropertyValuesAreEquals(object actual, object expected)
    {
        var properties = expected.GetType().GetProperties()
            .Where(info => !Attribute.IsDefined(info, typeof(DoNotExtractProperty))).ToArray();

        foreach (var property in properties)
        {
            //count goes to the database and works out how many people are in the Cohort table underneath ! don't do that for fictional Test tables
            if (property.Name.StartsWith("Count"))
                continue;

            var expectedValue = property.GetValue(expected, null);
            var actualValue = property.GetValue(actual, null);

            if (expectedValue is SqlCommand &&
                actualValue is SqlCommand) //don't compare sql commands they will be subtly different or just refer to different objects with the exact same values
                continue;

            if (actualValue is IList list)
                AssertListsAreEquals(property, list, (IList)expectedValue);
            else if (!Equals(expectedValue, actualValue))
                Assert.Fail($"Property {property.DeclaringType?.Name}.{property.Name} does not match. Expected: {expectedValue} but was: {actualValue}");
        }
    }

    private static void AssertListsAreEquals(PropertyInfo property, IList actualList, IList expectedList)
    {
        if (actualList.Count != expectedList.Count)
            Assert.Fail($"Property {property.PropertyType.Name}.{property.Name} does not match. Expected IList containing {expectedList.Count} elements but was IList containing {actualList.Count} elements");

        for (var i = 0; i < actualList.Count; i++)
            if (!Equals(actualList[i], expectedList[i]))
                Assert.Fail($"Property {property.PropertyType.Name}.{property.Name} does not match. Expected IList with element {property.Name} equals to {expectedList[i]} but was IList with element {property.Name} equals to {actualList[i]}");
    }

    #endregion
}