// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;

namespace Diagnostics.TestData.Exercises
{
    public class DemographyExerciseTestData : ExerciseTestDataGenerator
    {
        Random r = new Random();

        public override object[] GenerateTestDataRow(TestPerson person)
        {
            //leave off data load run ID 
            var values = new object[39];
            
            values[0] = person.CHI;
            values[1] = TestPerson.GetRandomDateAfter(person.DateOfBirth,r);//all records must have been created after the person was born
            
            if(r.Next(0, 2) == 0)
                values[2] = true;
            else
                values[2] = false;

            values[3] = "Random record";
            
            if(r.Next(0,10 )== 0)//one in 10 records has one of these (an ALIAS chi)
                values[4] = TestPerson.GetRandomCHI(person.DateOfBirth,person.Gender,r);

            values[5] = GetRandomCHIStatus(r);
            values[6] = person.DateOfBirth.Year.ToString().Substring(0,2);
            values[7] = person.Surname;
            values[8] = person.Forename;
            values[9] = person.Gender;


            var randomAddress = new TestAddress(r);
            
            //if person is dead and dtCreated is after they died use the same address otehrwise use a random one (all records after a person dies have same address)
            values[10] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line1: randomAddress.Line1;
            values[11] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line2: randomAddress.Line2;
            values[12] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line3: randomAddress.Line3;
            values[13] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Line4: randomAddress.Line4;
            values[14] = person.DateOfDeath != null && (DateTime)values[1]>person.DateOfDeath ? person.Address.Postcode.Value: randomAddress.Postcode.Value;

            //if the person is dead and the dtCreated of the record is greater than the date of death populate it
            values[15] = person.GetDateOfDeathOrNullOn((DateTime)values[1]); //pass record creation date and get isdead date back
                
            //if we got a date put the source in as R
            if(values[15] != null)
                values[16] = 'R';
            

            if(!string.IsNullOrWhiteSpace(person.Address.Postcode.District))
                values[17] = person.Address.Postcode.District.Substring(0, 1);

            values[18] = GetRandomLetter(true,r);

            //healthboard 'A' use padding on the name field (to a length of 10!)
            if((char)values[18] == 'A')
                if (values[8] != null)
                    while (values[8].ToString().Length < 10)
                        values[8] = values[8] + " ";

            //in healthboard 'B' they give us both forename and suranme in the same field! - and surname is always blank
            if ((char)values[18] == 'B')
            {
                values[8] = values[8] + " " +values[7];
                values[7] = null;
            }

            values[19] = GetRandomGPCode(r);

            //birth surname and previous surname fields, sparsely populated
            if (r.Next(0, 10) == 0)
                values[20] = TestPerson.GetRandomSurname(r);
            if (r.Next(0, 10) == 0)
                values[21] = TestPerson.GetRandomSurname(r);
            
            if (r.Next(0, 3) == 0)
                values[22] = person.GetRandomForename(r); //random gender appropriate middle name for 1 person in 3
            
            if (r.Next(0, 5) == 0)
                values[23] = person.GetRandomForename(r); //alternate forename

            if(r.Next(0,3)==0)
                values[24] = GetRandomLetter(true,r);  //one in 3 has an initial

            //people only have previous addresses if they are alive
            if(r.Next(0, 2) == 0 && person.DateOfDeath != null)
            {
                var randomAddress2 = new TestAddress(r);

                values[25] = randomAddress2.Line1;
                values[26] = randomAddress2.Line2;
                values[27] = randomAddress2.Line3;
                values[28] = randomAddress2.Line4;
                values[29] = randomAddress2.Postcode.Value;

                //date of address change is unknown for 50% of records
                if (r.Next(0, 2) == 0)
                {
                    //get after birth but before dtCreated/date of death
                    values[30] = TestPerson.GetRandomDateBetween(person.DateOfBirth, GetMinimum(person.DateOfDeath,(DateTime)values[1]),r);
                }
            }

            //an always null field, why not?!
            values[31] = null;

            DateTime gp_accept_date = TestPerson.GetRandomDateAfter(person.DateOfBirth, r);
            
            //current_gp_accept_date
            values[32] = gp_accept_date;


            //before 1980 some records will be missing forename (deliberate errors!)
            if (gp_accept_date.Year < 1980)
                if (r.Next(gp_accept_date.Year - 1970) == 0)//the farther back you go the more likely they are to be missing a forename
                        values[8] = null;//some people are randomly missing a forename
            
            if(r.Next(0,3)==0)
            {
                values[33] = GetRandomGPCode(r);
                values[34] = TestPerson.GetRandomDateAfter((DateTime) values[32], r);
            }

            values[35] = TestPerson.GetRandomDateBetween(person.DateOfBirth, GetMinimum(person.DateOfDeath, (DateTime)values[1]), r);
            values[36] = person.DateOfBirth;
            values[37] = GetRandomDouble(r);

            //data load run id will be batches 1 (1900 is first year of possible dtCreated) to 12 (2015 - 1890 / 10 = 12)
            values[38] = (((DateTime) values[1]).Year - 1890)/10;

            return values;
        }

        private static DateTime GetMinimum(DateTime? date1, DateTime date2)
        {
            if (date1 == null)
                return date2;

            if (date2 > date1)
                return (DateTime)date1;

            return date2;
        }


        protected override void WriteHeaders(StreamWriter sw)
        {
            sw.Write("chi,");                                                   //0
            sw.Write("dtCreated,");                                             //1
            sw.Write("current_record,");                                        //2
            sw.Write("notes,");                                                 //3
            sw.Write("chi_num_of_curr_record,");                                //4
            sw.Write("chi_status,");                                            //5
            sw.Write("century,");                                               //6
            sw.Write("surname,");                                               //7
            sw.Write("forename,");                                              //8
            sw.Write("sex,");                                                   //9
            sw.Write("current_address_L1,");                                    //10
            sw.Write("current_address_L2,");                                    //11
            sw.Write("current_address_L3,");                                    //12
            sw.Write("current_address_L4,");                                    //13
            sw.Write("current_postcode,");                                      //14
            sw.Write("date_of_death,");                                         //15
            sw.Write("source_death,");                                          //16
            sw.Write("area_residence,");                                        //17
            sw.Write("hb_extract,");                                            //18
            sw.Write("current_gp,");                                            //19
            sw.Write("birth_surname,");                                         //20
            sw.Write("previous_surname,");                                      //21
            sw.Write("midname,");                                               //22
            sw.Write("alt_forename,");                                          //23
            sw.Write("other_initials,");                                        //24
            sw.Write("previous_address_L1,");                                   //25
            sw.Write("previous_address_L2,");                                   //26
            sw.Write("previous_address_L3,");                                   //27
            sw.Write("previous_address_L4,");                                   //28
            sw.Write("previous_postcode,");                                     //29
            sw.Write("date_address_changed,");                                  //30
            sw.Write("adr,");                                                   //31
            sw.Write("current_gp_accept_date,");                                //32
            sw.Write("previous_gp,");                                           //33
            sw.Write("previous_gp_accept_date,");                               //34
            sw.Write("date_into_practice,");                                    //35
            sw.Write("date_of_birth,");                                         //36
            sw.Write("patient_triage_score" + Environment.NewLine);             //37
        }
    }
}