// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
    public class PrescribingExerciseTestData : ExerciseTestDataGenerator
    {
        Random r = new Random();

        public override string GetName()
        {
            return "Prescribing";
        }

        protected override object[] GenerateTestDataRow(TestPerson p)
        {
            object[] values = new object[11];

            TestPrescription prescription = new TestPrescription(r);
            
            values[0] = p.CHI;
            values[1] = p.GetRandomDateDuringLifetime(r);
            values[2] = prescription.Quantity;
            values[3] = prescription.strength;
            values[4] = prescription.formulation_code;
            values[5] = prescription.measure_code;
            values[6] = prescription.name;
            values[7] = prescription.Approved_Name;
            values[8] = prescription.BNF_Code;
            values[9] = prescription.formatted_BNF_Code;
            values[10] = prescription.BNF_Description;

            return values;
        }

        protected override void WriteHeaders(StreamWriter sw)
        {
            string[] headers =
            {
                "chi",
                "prescribed_date",
                "quantity",
                "strength",
                "formulation_code",
                "measure_code",
                "name",
                "approved_name",
                "bnf",
                "formatted_bnf",
                "description"
            };

            sw.WriteLine(string.Join(",",headers));
        }
    }
}