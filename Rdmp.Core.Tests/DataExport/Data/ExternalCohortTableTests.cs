// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.Data;

internal class ExternalCohortTableTests : UnitTests
{
    /// <summary>
    /// Demonstrates the minimum properties required to create a <see cref="ExternalCohortTable"/>.  See <see cref="CreateNewCohortDatabaseWizard"/>
    /// for how to create one of these based on the datasets currently held in rdmp.
    /// </summary>
    [Test]
    public void Create_ExternalCohortTable_Manually()
    {
        var repository = new MemoryDataExportRepository();
        var table = new ExternalCohortTable(repository, "My Cohort Database", DatabaseType.MicrosoftSQLServer)
        {
            Database = "mydb",
            PrivateIdentifierField = "chi",
            ReleaseIdentifierField = "release",
            DefinitionTableForeignKeyField = "c_id",
            TableName = "Cohorts",
            DefinitionTableName = "InventoryTable",
            Server = "superfastdatabaseserver\\sqlexpress"
        };
        table.SaveToDatabase();

        var ex = Assert.Throws<Exception>(() => table.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Could not connect to Cohort database called 'My Cohort Database'"));
    }

    /// <summary>
    /// Demonstrates how to get a hydrated instance during unit tests.  This will not map to an actually existing database
    /// </summary>
    [Test]
    public void Create_ExternalCohortTable_InTests()
    {
        var tbl = WhenIHaveA<ExternalCohortTable>();

        Assert.That(tbl, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(tbl.PrivateIdentifierField, Is.Not.Null);
            Assert.That(tbl.ReleaseIdentifierField, Is.Not.Null);
        });
    }

    [Test]
    public void TestExternalCohortTableProperties_BasicValues()
    {
        var table = new ExternalCohortTable(RepositoryLocator.DataExportRepository, "ffff",
            DatabaseType.MicrosoftSQLServer);

        Assert.Multiple(() =>
        {
            Assert.That(table.Database, Is.Null);
            Assert.That(table.Server, Is.Null);
            Assert.That(table.TableName, Is.Null);
            Assert.That(table.PrivateIdentifierField, Is.Null);
            Assert.That(table.ReleaseIdentifierField, Is.Null);
            Assert.That(table.DefinitionTableForeignKeyField, Is.Null);
        });

        table.Database = "mydb";
        table.Server = "myserver";
        table.TableName = "mytbl";
        table.PrivateIdentifierField = "priv";
        table.ReleaseIdentifierField = "rel";
        table.DefinitionTableForeignKeyField = "fk";

        Assert.Multiple(() =>
        {
            Assert.That(table.Database, Is.EqualTo("mydb"));
            Assert.That(table.Server, Is.EqualTo("myserver"));
            Assert.That(table.TableName, Is.EqualTo("[mydb]..[mytbl]"));
            Assert.That(table.DefinitionTableName, Is.Null);

            Assert.That(table.PrivateIdentifierField, Is.EqualTo("[mydb]..[mytbl].[priv]"));
            Assert.That(table.ReleaseIdentifierField, Is.EqualTo("[mydb]..[mytbl].[rel]"));
            Assert.That(table.DefinitionTableForeignKeyField, Is.EqualTo("[mydb]..[mytbl].[fk]"));
        });
    }

    [Test]
    public void TestExternalCohortTableProperties_SetFullValues()
    {
        var table = new ExternalCohortTable(RepositoryLocator.DataExportRepository, "ffff",
            DatabaseType.MicrosoftSQLServer);

        Assert.Multiple(() =>
        {
            Assert.That(table.Database, Is.Null);
            Assert.That(table.Server, Is.Null);
            Assert.That(table.TableName, Is.Null);
            Assert.That(table.PrivateIdentifierField, Is.Null);
            Assert.That(table.ReleaseIdentifierField, Is.Null);
            Assert.That(table.DefinitionTableForeignKeyField, Is.Null);
        });

        table.PrivateIdentifierField = "[mydb]..[mytbl].[priv]";
        table.ReleaseIdentifierField = "[mydb]..[mytbl].[rel]";
        table.DefinitionTableForeignKeyField = "[mydb]..[mytbl].[fk]";
        table.Database = "mydb";
        table.Server = "myserver";
        table.TableName = "[mydb]..[mytbl]";

        Assert.Multiple(() =>
        {
            Assert.That(table.Database, Is.EqualTo("mydb"));
            Assert.That(table.Server, Is.EqualTo("myserver"));
            Assert.That(table.TableName, Is.EqualTo("[mydb]..[mytbl]"));
            Assert.That(table.DefinitionTableName, Is.Null);

            Assert.That(table.PrivateIdentifierField, Is.EqualTo("[mydb]..[mytbl].[priv]"));
            Assert.That(table.ReleaseIdentifierField, Is.EqualTo("[mydb]..[mytbl].[rel]"));
            Assert.That(table.DefinitionTableForeignKeyField, Is.EqualTo("[mydb]..[mytbl].[fk]"));
        });
    }
}