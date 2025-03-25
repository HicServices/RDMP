// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Reports;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit;

public class TestAcronymGeneration : DatabaseTests
{
    [Test]
    [TestCase("bob", "bob")]
    [TestCase("Demography", "Demog")]
    [TestCase("DMPCatalogue", "DMPC")]
    [TestCase("Datasheet1", "D1")]
    [TestCase("Frank Bettie Cardinality", "FBC")]
    [TestCase("Datashet DMP 32", "DDMP32")]
    public void Predict(string name, string predictedAcronym)
    {
        var suggestion = DitaCatalogueExtractor.GetAcronymSuggestionFromCatalogueName(name);

        Assert.That(suggestion, Is.EqualTo(predictedAcronym));
    }
}