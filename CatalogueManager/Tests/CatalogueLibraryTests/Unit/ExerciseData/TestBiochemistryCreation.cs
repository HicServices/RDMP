using System;
using System.IO;
using System.Linq;
using Diagnostics.TestData;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace CatalogueLibraryTests.Unit.ExerciseData
{
    public class TestBiochemistryCreation
    {
        [Test]
        [TestCase(1000)]
        [TestCase(321)]
        [TestCase(100000)]
        public void CreateCSV(int numberOfRecords)
        {
            ExerciseTestIdentifiers people = new ExerciseTestIdentifiers();
            people.GeneratePeople(100);

            var f = new FileInfo("DeleteMeTestBiochemistry.csv");

            var messages = new ToMemoryDataLoadEventReceiver(true);

            BiochemistryExerciseTestData biochem = new BiochemistryExerciseTestData();
            biochem.GenerateTestDataFile(people, f, numberOfRecords, messages);

            //one progress task only, should have reported craeting 10,000 rows
            Assert.AreEqual(numberOfRecords
                , messages.LastProgressRecieivedByTaskName[messages.LastProgressRecieivedByTaskName.Keys.Single()].Progress.Value);

            Assert.GreaterOrEqual(File.ReadAllLines(f.FullName).Length, numberOfRecords);//can be newlines in middle of file

            Console.WriteLine("Created file: " + f.FullName);
            f.Delete();
        }

        [Test]
        public void SampleCodeStaticConstructor()
        {
            TestBiochemistrySample sample = new TestBiochemistrySample(new Random());

            Console.WriteLine("Test_code:" + sample.Test_code);
            Console.WriteLine("Result:" + sample.Result);
            Console.WriteLine("Units:" + sample.Units);
            Console.WriteLine("Sample_type:" + sample.Sample_type);
            Console.WriteLine("ReadCodeValue:" + sample.ReadCodeValue);
            Console.WriteLine("ReadCodeDescription:" + sample.ReadCodeDescription);
        }
    }
}