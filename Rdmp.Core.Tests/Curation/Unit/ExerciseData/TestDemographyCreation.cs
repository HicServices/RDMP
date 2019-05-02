// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Curation.Unit.ExerciseData
{
    public class TestDemographyCreation
    {
        [Test]
        [TestCase(1000)]
        [TestCase(321)]
        [TestCase(100000)]
        public void CreateCSV(int numberOfRecords)
        {
            ExerciseTestIdentifiers people = new ExerciseTestIdentifiers();
            people.GeneratePeople(100);

            var f =new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"DeleteMeTestPeople.csv"));
            
            bool finished = false;
            int finishedWithRecords = -1;
            
            
            DemographyExerciseTestData demog = new DemographyExerciseTestData();
            demog.RowsGenerated += (s, e) =>
            {
                finished = e.IsFinished;
                finishedWithRecords = e.RowsWritten;
            };

            demog.GenerateTestDataFile(people, f, numberOfRecords);

            //one progress task only, should have reported craeting 10,000 rows
            //one progress task only, should have reported creating the correct number of rows
            Assert.IsTrue(finished);
            Assert.AreEqual(numberOfRecords, finishedWithRecords);

            Assert.GreaterOrEqual(File.ReadAllLines(f.FullName).Length, numberOfRecords);//can be newlines in middle of file

            f.Delete();
        }

    }
}
