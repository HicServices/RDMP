using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace CatalogueLibraryTests.Unit.ExerciseData
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

            var f =new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"DeleteMeTestPeople.csv"));
            
            var messages = new ToMemoryDataLoadEventListener(true);

            DemographyExerciseTestData demog = new DemographyExerciseTestData();
            demog.GenerateTestDataFile(people, f, numberOfRecords, messages);

            //one progress task only, should have reported craeting 10,000 rows
            Assert.AreEqual(numberOfRecords
                ,messages.LastProgressRecieivedByTaskName[messages.LastProgressRecieivedByTaskName.Keys.Single()].Progress.Value);

            Assert.GreaterOrEqual(File.ReadAllLines(f.FullName).Length, numberOfRecords);//can be newlines in middle of file

            f.Delete();
        }

    }
}
