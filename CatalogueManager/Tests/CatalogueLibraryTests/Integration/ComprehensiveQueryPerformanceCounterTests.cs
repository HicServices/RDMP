using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Performance;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class ComprehensiveQueryPerformanceCounterTests : DatabaseTests
    {
        [Test]
        public void TestPerformance()
        {
            var pCounter =  new ComprehensiveQueryPerformanceCounter();
            //enable performance counting
            DatabaseCommandHelper.PerformanceCounter = pCounter;
            try
            {

                //send some queries
                var cata =  new Catalogue(CatalogueRepository, "Fish");

                Assert.IsTrue(cata.Name.Equals("Fish"));

                var commands = pCounter.DictionaryOfQueries.Seconds.ToArray();
                Assert.IsTrue(commands.Any(c => c.QueryText.Contains("SELECT * FROM Catalogue WHERE ID=")));
                
                cata.DeleteInDatabase();
            }
            finally
            {
                DatabaseCommandHelper.PerformanceCounter = null;
            }
        }
    }
}
