// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Diagnostics.TestData
{
    public class TestPerson
    {
        public const string ForenameDescription ="The (fictional) patients forename, randomly generated from a list of 100 common forenames that match the patients gender";
        public const string SurnameDescription = " The (fictional) patients surname, randomly generated from a list of 100 common surnames";

        public const string CHIDescription = "Community Health Index (CHI) number is a unique personal identifier allocated to each patient on first registration with a GP Practice. It follows the format DDMMYYRRGC where DDMMYY represents the persons date of birth, RR are random digits, G is another random digit but acts as a gender identifier, (where odd numbers indicate males and even numbers indicate females), and the final digit is an arithmetical check digit.";
        public const string ANOCHIDescription = "Annonymous identifier that has replaced the identifiable CHI.  The CHI number will be stored in an ANO database.";

        public const string DateOfBirthDescription =
            "The (fictional) date of birth of the patient, this should match the first 6 digits of the CHI (if the patient has a CHI and not an ANOCHI)";

        public const string GenderDescription = "M for Male patient, F for Female patient.  Always populated.  Will match the last digit of the CHI (if the patient has a CHI and not an ANOCHI) with odd numbers for females and even numbers for males.";


        public string Forename { get; set; }
        public string Surname { get; set; }
        public string CHI { get; set; }
        public string ANOCHI { get; set; }
        public DateTime DateOfBirth = new DateTime();
        public DateTime? DateOfDeath;
        public char Gender { get; set; }
        
        public TestAddress Address { get; set; }
        public TestAddress PreviousAddress { get; set; }

        static HashSet<string> AlreadyGeneratedCHIs = new HashSet<string>();
        static HashSet<string> AlreadyGeneratedANOCHIs = new HashSet<string>();

        private readonly List<TestAppointment> _appointments = new List<TestAppointment>();
        
        public TestPerson(Random r)
        {
            switch (r.Next(2))
            {
                case 0:
                    Gender = 'F';
                    break;
                case 1:
                    Gender = 'M';
                    break;
            }

            Forename = GetRandomForename(r);
            Surname = GetRandomSurname(r);

            DateOfBirth = GetRandomDate(r);
            
            //1 in 10 patients is dead
            if (r.Next(10) == 0)
                DateOfDeath = GetRandomDateAfter(DateOfBirth, r);
            else
                DateOfDeath = null;

            CHI = GetNovelCHI(r);

            ANOCHI = GetNovelANOCHI(r);

            Address = new TestAddress(r);

            //one in 10 people doesn't have a previous address
            if(r.Next(10) != 0)
                PreviousAddress = new TestAddress(r);

            //person has up to 30 random appointments (this generates a curve where less appointments are more likely)
            for(int i=0; i<r.Next(1,30);i++)
                _appointments.Add(new TestAppointment(this,r));

        }
        public string GetRandomForename(Random r)
        {
            if(Gender == 'F')
                return CommonGirlForenames[r.Next(100)];
            
            return CommonBoyForenames[r.Next(100)];
        }

        public DateTime GetRandomDateDuringLifetime(Random r)
        {
            if (DateOfDeath == null)
                return GetRandomDateAfter(DateOfBirth, r);

            return GetRandomDateBetween(DateOfBirth, (DateTime)DateOfDeath, r);
        }

        public static string GetRandomSurname(Random r)
        {
            return CommonSurnames[r.Next(100)];
        }
        
        public static DateTime GetRandomDateBetween(DateTime start, DateTime end, Random r)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException("end date was before start date");

            TimeSpan timeSpan = end - start;
            TimeSpan newSpan = new TimeSpan(0, r.Next(0, (int)timeSpan.TotalMinutes), 0);
            return start + newSpan;
        }

        public static DateTime GetRandomDate(Random r)
        {
            int year = r.Next(1970, 2014);
            int month = Math.Max(1, r.Next(13));
            int day = Math.Max(1, r.Next(28));

            try
            {
                return new DateTime(year, month, day);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid datetime " + year + "-" + month + "-" + day, e);
            }
        }


        /// <summary>
        /// If the person died before onDate it returns NULL (as of onDate we did not know when the person would die).  if onDate is > date of death it 
        /// returns the date of death (we knew when they died - you cannot predict the future but you can remember the past)
        /// </summary>
        /// <param name="onDate"></param>
        /// <returns></returns>
        public DateTime? GetDateOfDeathOrNullOn(DateTime onDate)
        {
            //patient is alive today
            if (DateOfDeath == null)
                return null;
            
            //retrospective
            if (onDate >= DateOfDeath)
                return DateOfDeath;

            //we cannot predict the future, they are dead today but you are pretending the date is onDate
            return null;
        }

        public static DateTime GetRandomDateAfter(DateTime afterDate,Random r)
        {
            var range = DateTime.Now - afterDate;

            var randTimeSpan = new TimeSpan((long) (r.NextDouble()*range.Ticks));

            return afterDate + randTimeSpan;
        }

        private string GetNovelANOCHI(Random r)
        {
            string anochi;
            do
            {
                anochi = GenerateANOCHI(r);
            } while (AlreadyGeneratedANOCHIs.Contains(anochi));
            AlreadyGeneratedANOCHIs.Add(anochi);
            
            return anochi;

        }

        private string GetNovelCHI(Random r)
        {
            string chi;
            do
            {
                chi = GenerateCHI(r);
            } while (AlreadyGeneratedCHIs.Contains(chi));
            AlreadyGeneratedCHIs.Add(chi);

            return chi;
        }

        private string GenerateANOCHI(Random r)
        {
            string toreturn = "";

            for (int i = 0; i < 10; i++)
                toreturn += r.Next(10);

            return toreturn + "_A";
        }

        private string GenerateCHI(Random r)
        {
           return GetRandomCHI(DateOfBirth,Gender,r);
        }


        public static string GetRandomCHI(DateTime dateOfBirth, char gender, Random r)
        {
            string toreturn = dateOfBirth.ToString("ddMMyy" + r.Next(100, 999));

            int chiLastDigit = r.Next(10);

            //odd last number for girls
            if (gender == 'F' && chiLastDigit % 2 == 0)
                chiLastDigit = 1;

            //even last number for guys
            if (gender == 'M' && chiLastDigit % 2 == 1)
                chiLastDigit = 2;

            return toreturn + chiLastDigit;
        }

        private static string[] CommonGirlForenames = new[]
        {
            "AMELIA",
            "OLIVIA",
            "EMILY",
            "AVA",
            "ISLA",
            "JESSICA",
            "POPPY",
            "ISABELLA",
            "SOPHIE",
            "MIA",
            "RUBY",
            "LILY",
            "GRACE",
            "EVIE",
            "SOPHIA",
            "ELLA",
            "SCARLETT",
            "CHLOE",
            "ISABELLE",
            "FREYA",
            "CHARLOTTE",
            "SIENNA",
            "DAISY",
            "PHOEBE",
            "MILLIE",
            "EVA",
            "ALICE",
            "LUCY",
            "FLORENCE",
            "SOFIA",
            "LAYLA",
            "LOLA",
            "HOLLY",
            "IMOGEN",
            "MOLLY",
            "MATILDA",
            "LILLY",
            "ROSIE",
            "ELIZABETH",
            "ERIN",
            "MAISIE",
            "LEXI",
            "ELLIE",
            "HANNAH",
            "EVELYN",
            "ABIGAIL",
            "ELSIE",
            "SUMMER",
            "MEGAN",
            "JASMINE",
            "MAYA",
            "AMELIE",
            "LACEY",
            "WILLOW",
            "EMMA",
            "BELLA",
            "ELEANOR",
            "ESME",
            "ELIZA",
            "GEORGIA",
            "HARRIET",
            "GRACIE",
            "ANNABELLE",
            "EMILIA",
            "AMBER",
            "IVY",
            "BROOKE",
            "ROSE",
            "ANNA",
            "ZARA",
            "LEAH",
            "MOLLIE",
            "MARTHA",
            "FAITH",
            "HOLLIE",
            "AMY",
            "BETHANY",
            "VIOLET",
            "KATIE",
            "MARYAM",
            "FRANCESCA",
            "JULIA",
            "MARIA",
            "DARCEY",
            "ISABEL",
            "TILLY",
            "MADDISON",
            "VICTORIA",
            "ISOBEL",
            "NIAMH",
            "SKYE",
            "MADISON",
            "DARCY",
            "AISHA",
            "BEATRICE",
            "SARAH",
            "ZOE",
            "PAIGE",
            "HEIDI",
            "LYDIA",
            "SARA"
        };

        private static string[] CommonBoyForenames = new[]
        {
            "OLIVER",
            "JACK",
            "HARRY",
            "JACOB",
            "CHARLIE",
            "THOMAS",
            "OSCAR",
            "WILLIAM",
            "JAMES",
            "GEORGE",
            "ALFIE",
            "JOSHUA",
            "NOAH",
            "ETHAN",
            "MUHAMMAD",
            "ARCHIE",
            "LEO",
            "HENRY",
            "JOSEPH",
            "SAMUEL",
            "RILEY",
            "DANIEL",
            "MOHAMMED",
            "ALEXANDER",
            "MAX",
            "LUCAS",
            "MASON",
            "LOGAN",
            "ISAAC",
            "BENJAMIN",
            "DYLAN",
            "JAKE",
            "EDWARD",
            "FINLEY",
            "FREDDIE",
            "HARRISON",
            "TYLER",
            "SEBASTIAN",
            "ZACHARY",
            "ADAM",
            "THEO",
            "JAYDEN",
            "ARTHUR",
            "TOBY",
            "LUKE",
            "LEWIS",
            "MATTHEW",
            "HARVEY",
            "HARLEY",
            "DAVID",
            "RYAN",
            "TOMMY",
            "MICHAEL",
            "REUBEN",
            "NATHAN",
            "BLAKE",
            "MOHAMMAD",
            "JENSON",
            "BOBBY",
            "LUCA",
            "CHARLES",
            "FRANKIE",
            "DEXTER",
            "KAI",
            "ALEX",
            "CONNOR",
            "LIAM",
            "JAMIE",
            "ELIJAH",
            "STANLEY",
            "LOUIE",
            "JUDE",
            "CALLUM",
            "HUGO",
            "LEON",
            "ELLIOT",
            "LOUIS",
            "THEODORE",
            "GABRIEL",
            "OLLIE",
            "AARON",
            "FREDERICK",
            "EVAN",
            "ELLIOTT",
            "OWEN",
            "TEDDY",
            "FINLAY",
            "CALEB",
            "IBRAHIM",
            "RONNIE",
            "FELIX",
            "AIDEN",
            "CAMERON",
            "AUSTIN",
            "KIAN",
            "RORY",
            "SETH",
            "ROBERT",
            "MAVERIC MCNULTY", //these two deliberately have spaces in them to break validation rules in the documentation
            "FRANKIE HOLLYWOOD"
        };


        private static string[] CommonSurnames = new[]
        {
            "Smith",
            "Jones",
            "Taylor",
            "Williams",
            "Brown",
            "Davies",
            "Evans",
            "Wilson",
            "Thomas",
            "Roberts",
            "Johnson",
            "Lewis",
            "Walker",
            "Robinson",
            "Wood",
            "Thompson",
            "White",
            "Watson",
            "Jackson",
            "Wright",
            "Green",
            "Harris",
            "Cooper",
            "King",
            "Lee",
            "Martin",
            "Clarke",
            "James",
            "Morgan",
            "Hughes",
            "Edwards",
            "Hill",
            "Moore",
            "Clark",
            "Harrison",
            "Scott",
            "Young",
            "Morris",
            "Hall",
            "Ward",
            "Turner",
            "Carter",
            "Phillips",
            "Mitchell",
            "Patel",
            "Adams",
            "Campbell",
            "Anderson",
            "Allen",
            "Cook",
            "Bailey",
            "Parker",
            "Miller",
            "Davis",
            "Murphy",
            "Price",
            "Bell",
            "Baker",
            "Griffith",
            "Kelly",
            "Simpson",
            "Marshall",
            "Collins",
            "Bennett",
            "Cox",
            "Richards",
            "Fox",
            "Gray",
            "Rose",
            "Chapman",
            "Hunt",
            "Robertson",
            "Shaw",
            "Reynolds",
            "Lloyd",
            "Ellis",
            "Richards",
            "Russell",
            "Wilkinson",
            "Khan",
            "Graham",
            "Stewart",
            "Reid",
            "Murray",
            "Powell",
            "Palmer",
            "Holmes",
            "Rogers",
            "Stevens",
            "Walsh",
            "Hunter",
            "Thomson",
            "Matthews",
            "Ross",
            "Owen",
            "Mason",
            "Knight",
            "Kennedy",
            "Butler",
            "Saunders"
        };

        /// <summary>
        /// Returns a guid representing one of the (random number) of appointments the person has made in thier lifetime.  This can be used to group
        /// events to a particular person e.g. hospitalisation record to a blood pressure measurement without crossing into other patients.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public TestAppointment GetRandomAppointment(Random r)
        {
            return _appointments[r.Next(0, _appointments.Count)];
        }
    }
}