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
        protected override object[] GenerateTestDataRow(TestPerson person)
        {
            //leave off data load run ID 
            return BulkTestsData.GenerateTestDataRow(person).Take(38).ToArray();
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