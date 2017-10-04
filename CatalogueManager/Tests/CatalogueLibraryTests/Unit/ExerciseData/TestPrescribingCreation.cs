using System;
using System.IO;
using System.Linq;
using Diagnostics.TestData;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace CatalogueLibraryTests.Unit.ExerciseData
{
    public class TestPrescribingCreation
    {
        [Test]
        [TestCase(1000)]
        [TestCase(321)]
        [TestCase(100000)]
        public void CreateCSV(int numberOfRecords)
        {
            ExerciseTestIdentifiers people = new ExerciseTestIdentifiers();
            people.GeneratePeople(100);

            var f = new FileInfo("DeleteMeTestPrescribing.csv");

            var messages = new ToMemoryDataLoadEventListener(true);

            PrescribingExerciseTestData prescribing = new PrescribingExerciseTestData();
            prescribing.GenerateTestDataFile(people, f, numberOfRecords, messages);

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
            TestPrescription prescription = new TestPrescription(new Random());

            Console.WriteLine("res_seqno:" + prescription.res_seqno);
            Console.WriteLine("name:" + prescription.name);
            Console.WriteLine("Approved_Name:" + prescription.Approved_Name);
            Console.WriteLine("BNF_Code:" + prescription.BNF_Code);
            Console.WriteLine("formatted_BNF_Code:" + prescription.formatted_BNF_Code);
            Console.WriteLine("BNF_Description:" + prescription.BNF_Description);
            Console.WriteLine("Quantity:" + prescription.Quantity);
            Console.WriteLine("strength:" + prescription.strength);
            Console.WriteLine("measure_code:" + prescription.measure_code);
            Console.WriteLine("formulation_code:" + prescription.formulation_code);
           
        }
    }
}