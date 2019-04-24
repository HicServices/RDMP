// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Diagnostics.TestData
{
    class TestAdmission
    {

        public const string AdmissionDateDescription = "The time and date that the (fictional) patient attended the hospital";
        public const string DischargeDateDescription = "The time and date that the (fictional) patient departed the hospital";
        public const string Condition1Description = "The primary condition that the (fictional) patient is suffering from, conditions 2-4 are optional but condition1 is always populated.  This is an ICD10 code which can be found in the ICD10 lookup";
        public const string Condition2To4Description = "See Condition1";
        

        public DateTime AdmissionDate{get; private set; }
        public DateTime DischargeDate { get; private set; }

        public string Condition1 { get; private set; }
        public string Condition2 { get; private set; }
        public string Condition3 { get; private set; }
        public string Condition4 { get; private set; }

        public TestPerson Person { get; set; }

        public TestAdmission(TestHospitalAdmissions parent, TestPerson person, DateTime afterDateX, Random r)
        {
            Person = person;
            if (person.DateOfBirth > afterDateX)
                afterDateX = person.DateOfBirth;

            AdmissionDate = GetRandomDate(afterDateX, DateTime.Now, r);
            DischargeDate = AdmissionDate.AddHours(r.Next(240));//discharged after random number of hours between 0 and 240 = 10 days

            //Condition 1 always populated
            Condition1 = parent.GetRandomICDCode(r);

            //50% chance of condition 2 as well as 1
            if(r.Next(2) == 0)
            {
                Condition2 = parent.GetRandomICDCode(r);

                //25% chance of condition 3 too
                if (r.Next(2) == 0)
                {
                    Condition3 = parent.GetRandomICDCode(r);
                    
                    //12.5% chance of all conditions
                    if (r.Next(2) == 0)
                        Condition4 = parent.GetRandomICDCode(r);

                    //1.25% chance of dirty data = the text 'Nul'
                    if(r.Next(10) ==0)
                        Condition4 = "Nul";
                }
            }
        }
        
        public static DateTime GetRandomDate(DateTime from, DateTime to, Random r)
        {
            var range = to - from;

            var randTimeSpan = new TimeSpan((long) (r.NextDouble()*range.Ticks));

            return from + randTimeSpan;
        }

    }
}
