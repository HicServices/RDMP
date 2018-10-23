using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class GetDatabaseDiagramBinaryTest:DatabaseTests
    {
        [Test]
        public void GetBinaryText()
        {
            using (var con = CatalogueRepository.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand(
                    "SELECT definition  FROM sysdiagrams where name = 'Catalogue_Data_Diagram' ",
                    con.Connection, con.Transaction);

                var reader = cmd.ExecuteReader();
                
                //The system diagram exists
                Assert.IsTrue(reader.Read());

                var bytes = (byte[]) reader[0];
                var bytesAsString = ByteArrayToString(bytes);
                
                Console.WriteLine(bytesAsString);
                Assert.Greater(bytesAsString.Length,100000);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
    }
}

