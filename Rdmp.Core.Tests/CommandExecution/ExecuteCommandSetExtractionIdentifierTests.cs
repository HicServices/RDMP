// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using System;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandSetExtractionIdentifierTests : CommandCliTests
{
        [Test]
        public void TestSetExtractionIdentifier_Catalogue()
        {
                var ei1 = WhenIHaveA<ExtractionInformation>();

                ei1.Alias = "happyfun";
                ei1.IsExtractionIdentifier = false;
                ei1.SaveToDatabase();

                var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator(), ei1.CatalogueItem.Catalogue,
                    null, "happyfun");
                cmd.Execute();

        Assert.That(ei1.IsExtractionIdentifier);
        }

        [Test]
        public void TestSetExtractionIdentifier_Catalogue_PickOther()
        {
                var ei1 = WhenIHaveA<ExtractionInformation>();

                var otherCol = new ColumnInfo(Repository, "Other", "varchar", ei1.ColumnInfo.TableInfo);
                var otherCatItem = new CatalogueItem(Repository, ei1.CatalogueItem.Catalogue, "Other");
                var otherEi = new ExtractionInformation(Repository, otherCatItem, otherCol, "FFF");

                ei1.Alias = "happyfun";
                ei1.IsExtractionIdentifier = true;
                ei1.SaveToDatabase();

        Assert.Multiple(() =>
        {
            // before we run the command the primary ei1 is the identifier
            Assert.That(ei1.IsExtractionIdentifier);
            Assert.That(otherEi.IsExtractionIdentifier, Is.False);
        });

        // by picking the second (FFF) we should switch
        var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator(), ei1.CatalogueItem.Catalogue,
                    null, "FFF");
                cmd.Execute();

        Assert.Multiple(() =>
        {
            // original should no longer be the extraction identifier
            Assert.That(ei1.IsExtractionIdentifier, Is.False);

            // and the one picked should now be the only one
            Assert.That(otherEi.IsExtractionIdentifier);
        });
    }

        [Test]
        public void TestSetExtractionIdentifier_Catalogue_ButColumnDoesNotExist()
        {
                var ei1 = WhenIHaveA<ExtractionInformation>();

                ei1.Alias = "happyfun";
                ei1.IsExtractionIdentifier = false;
                ei1.SaveToDatabase();

                var ex = Assert.Throws<Exception>(() =>
                    new ExecuteCommandSetExtractionIdentifier(GetMockActivator(), ei1.CatalogueItem.Catalogue, null,
                            "trollolo")
                        .Execute());
        Assert.That(ex.Message, Is.EqualTo("Could not find column(s) trollolo amongst available columns (happyfun)"));
        }


        [Test]
        public void TestSetExtractionIdentifier_Configuration()
        {
                var ec1 = WhenIHaveA<ExtractableColumn>();

                ec1.Alias = "happyfun";
                ec1.IsExtractionIdentifier = false;
                ec1.SaveToDatabase();

                var config = Repository.GetObjectByID<ExtractionConfiguration>(ec1.ExtractionConfiguration_ID);

                var cmd = new ExecuteCommandSetExtractionIdentifier(GetMockActivator(),
                    ec1.CatalogueExtractionInformation.CatalogueItem.Catalogue,
                    config
                    , "happyfun");
                cmd.Execute();

        Assert.Multiple(() =>
        {
            // affects extraction specific version
            Assert.That(ec1.IsExtractionIdentifier);

            // but not master
            Assert.That(ec1.CatalogueExtractionInformation.IsExtractionIdentifier, Is.False);
        });
    }
}