// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using SynthEHR;
using SynthEHR.Datasets;
using NUnit.Framework;

namespace Rdmp.Core.Tests.Curation.Unit.ExerciseData;

[Category("Unit")]
public class TestBiochemistryCreation
{
    [Test]
    [TestCase(1000)]
    [TestCase(321)]
    [TestCase(100000)]
    public void CreateCSV(int numberOfRecords)
    {
        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(100, r);

        var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "DeleteMeTestBiochemistry.csv"));

        var finished = false;
        var finishedWithRecords = -1;

        var biochem = new Biochemistry(new Random(500));
        biochem.RowsGenerated += (s, e) =>
        {
            finished = e.IsFinished;
            finishedWithRecords = e.RowsWritten;
        };

        biochem.GenerateTestDataFile(people, f, numberOfRecords);

        Assert.Multiple(() =>
        {
            //one progress task only, should have reported creating the correct number of rows
            Assert.That(finished);
            Assert.That(finishedWithRecords, Is.EqualTo(numberOfRecords));

            Assert.That(File.ReadAllLines(f.FullName), Has.Length.GreaterThanOrEqualTo(numberOfRecords)); //can be newlines in middle of file
        });

        Console.WriteLine($"Created file: {f.FullName}");
        f.Delete();
    }
}