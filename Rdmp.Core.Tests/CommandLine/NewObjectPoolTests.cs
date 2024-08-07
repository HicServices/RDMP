﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine;

internal class NewObjectPoolTests : UnitTests
{
    [Test]
    public void TwoCataloguesWithSameName_NoSession()
    {
        var cata1 = new Catalogue(Repository, "Hey");

        // When there is only one object we can pick it by name
        var picker = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, GetActivator());
        Assert.Multiple(() =>
        {
            Assert.That(picker.HasArgumentOfType(0, typeof(Catalogue)));
            Assert.That(picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)), Is.EqualTo(cata1));
        });

        // But when there are 2 objects we don't know which to pick so cannot pick a Catalogue
        new Catalogue(Repository, "Hey");
        var picker2 = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, GetActivator());
        Assert.That(picker2.HasArgumentOfType(0, typeof(Catalogue)), Is.False);
    }

    [Test]
    public void TwoCataloguesWithSameName_WithSession()
    {
        using (NewObjectPool.StartSession())
        {
            var cata1 = new Catalogue(Repository, "Hey");

            // When there is only one object we can pick it by name
            var picker = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, GetActivator());
            Assert.Multiple(() =>
            {
                Assert.That(picker.HasArgumentOfType(0, typeof(Catalogue)));
                Assert.That(picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)), Is.EqualTo(cata1));
            });

            // There are now 2 objects with the same name but because we are in a session we can pick the latest
            var cata2 = new Catalogue(Repository, "Hey");
            var picker2 = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, GetActivator());

            Assert.Multiple(() =>
            {
                Assert.That(picker2.HasArgumentOfType(0, typeof(Catalogue)));
                Assert.That(picker2.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)), Is.EqualTo(cata2));
            });
        }
    }
}