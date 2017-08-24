using System;
using System.IO;
using System.Text;

namespace Diagnostics.TestData
{
    public class TestDemography
    {
        public const string DatasetDescription = "Test dataset of fictional people with randomly assigned forenames and surnames with random CHIs based off of random dates of birth.  If Anonymisation was used in the creation of this dataset then ANOCHI will be in the database otherwise it will be CHI (community health index - see column description in CatalogueItems)";

        public TestPerson[] People { get; private set; }

        Random _r;

        public TestDemography()
        {
            People = new TestPerson[500];

            _r = new Random();

            for(int i=0;i<500;i++)
                People[i] = new TestPerson(_r);
        }

        public string GetINSERTIntoDemographyTableSql(bool createANOVersion, string testTableName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (TestPerson person in People)
            {
                sb.Append("INSERT INTO " + testTableName + " VALUES(");
                sb.Append("'"+person.Forename + "',");
                sb.Append("'" + person.Surname + "',");
                sb.Append("'" + (createANOVersion?person.ANOCHI:person.CHI) + "',");
                sb.Append("'" + person.Gender + "',");
                sb.Append("'" + person.DateOfBirth.ToString("yyyy-MM-dd") + "'");
                sb.Append(")");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string GetCreateDemographyTableSql(bool createANOVersion, string testTableName)
        {
            return @"--Drop table if it already exists 
               IF OBJECT_ID('" + testTableName + @"') IS NOT NULL
 DROP TABLE " + testTableName + @"

CREATE TABLE " + testTableName + @"
(
Forename [varchar](20) NULL,
Surname [varchar](20) NULL,
"
               //if creating ANO version then use ANOCHI as the target type else use CHI
               +(createANOVersion?"ANOCHI varchar(12) not null PRIMARY KEY,":"CHI varchar(10) not null PRIMARY KEY,")+
@"
Gender char(1),
DateOfBirth datetime,
)

GO
              IF OBJECT_ID('" + testTableName + @"_Archive') IS NOT NULL
 DROP TABLE " + testTableName + @"_Archive
GO
";
        }


        public void CreateTestCSVFile(StreamWriter sw)
        {
            sw.WriteLine("Forename,Surname,CHI,Gender,DateOfBirth");

            
            //add 10 new people
            for(int i=0;i<10;i++)
            {
                TestPerson newPerson = new TestPerson(_r);

                sw.WriteLine(string.Format("{0},{1},{2},{3},{4}", 
                    newPerson.Forename,
                    newPerson.Surname,
                    newPerson.CHI,
                    newPerson.Gender,
                    newPerson.DateOfBirth.ToString("yyy-MM-dd")));

                //add a duplicate
                if (i == 9)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3},{4}",
                    newPerson.Forename,
                    newPerson.Surname,
                    newPerson.CHI,
                    newPerson.Gender,
                    "1900-01-01"));
                }
            }

            //now adjust data for 1 random person
            var randomPerson = People[_r.Next(People.Length)];

            if (randomPerson.Surname.Equals("Flibble"))
                throw new Exception("Random person had surname Flibble, how did that happen?");

            sw.WriteLine(string.Format("{0},{1},{2},{3},{4}",
                 randomPerson.Forename,
                 "Flibble",
                 randomPerson.CHI,
                 randomPerson.Gender,
                randomPerson.DateOfBirth.ToString("yyy-MM-dd")));

            
        }
    }
}
