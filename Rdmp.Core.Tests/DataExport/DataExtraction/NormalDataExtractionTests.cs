// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.QueryBuilding;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class NormalDataExtractionTests : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void ExtractNormally()
    {
        _catalogue.Name = "TestTable";
        _catalogue.SaveToDatabase();
        _request.DatasetBundle.DataSet.RevertToDatabaseState();

        Assert.That(_request.ColumnsToExtract.Count(c => c.IsExtractionIdentifier), Is.EqualTo(1));

        Execute(out _, out var result);

        var r = (ExecuteDatasetExtractionFlatFileDestination)result;

        Assert.Multiple(() =>
        {
            //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
            Assert.That(File.ReadAllText(r.OutputFile).Trim(), Is.EqualTo($@"ReleaseID,Name,DateOfBirth
{_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()]},Dave,2001-01-01"));

            Assert.That(_request.QueryBuilder.SelectColumns.Count(c => c.IColumn is ReleaseIdentifierSubstitution),
                Is.EqualTo(1));
        });
        File.Delete(r.OutputFile);
    }


    [Test]
    public void DodgyCharactersInCatalogueName()
    {
        var beforeName = _catalogue.Name;
        try
        {
            _catalogue.Name = "Fish;#:::FishFish";
            Assert.That(Catalogue.IsAcceptableName(_catalogue.Name), Is.False);
            _catalogue.SaveToDatabase();
            _extractableDataSet.RevertToDatabaseState();


            var extractionDirectory = new ExtractionDirectory(TestContext.CurrentContext.WorkDirectory, _configuration);


            var ex = Assert.Throws<NotSupportedException>(() =>
            {
                extractionDirectory.GetDirectoryForDataset(_extractableDataSet);
            });

            Assert.That(
                ex.Message,
                Is.EqualTo(
                    "Cannot extract dataset Fish;#:::FishFish because it points at Catalogue with an invalid name, name is invalid because:The following invalid characters were found:'#'"));
        }
        finally
        {
            _catalogue.Name = beforeName;
            _catalogue.SaveToDatabase();
            _extractableDataSet.RevertToDatabaseState();
        }
    }
}