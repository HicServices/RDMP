using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Diagnostics.TestData.Relational
{
    public class CIATestInformant
    {
        public int ID;
        public string Name;
        public DateTime DateOfBirth;

        public CIATestInformant(Random random, List<CIATestInformant> informants)
        {
            int id = 1;

            //avoid colliding with existing informants
            while (informants.Any(report => report.ID == id))
            {
                id = random.Next(100000);
            }
            ID = id; 

            Name = TestPerson.GetRandomSurname(random);

            DateOfBirth = DateTime.Now.AddYears(-random.Next(0, 100)).Date;

        }

        public void CommitToDatabase(SqlConnection con)
        {
            new SqlCommand(string.Format("INSERT INTO CIATestInformant VALUES ({0},'{1}','{2}')",ID,Name,DateOfBirth.ToString("yyyy-MM-dd")), con).ExecuteNonQuery();
        }
    }
}