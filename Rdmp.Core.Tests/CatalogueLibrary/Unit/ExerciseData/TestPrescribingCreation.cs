// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Diagnostics.TestData;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.CatalogueLibrary.Unit.ExerciseData
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

            var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"DeleteMeTestPrescribing.csv"));

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