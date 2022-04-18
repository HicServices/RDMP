// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Performance;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class ComprehensiveQueryPerformanceCounterTests : DatabaseTests
    {
        [Test]
        public void TestPerformance()
        {
            if (TestDatabaseSettings.UseFileSystemRepo)
                Assert.Inconclusive("No queries are run when using file back repository");

            var pCounter =  new ComprehensiveQueryPerformanceCounter();
            //enable performance counting
            DatabaseCommandHelper.PerformanceCounter = pCounter;
            try
            {

                //send some queries
                var cata =  new Catalogue(CatalogueRepository, "Fish");

                Assert.IsTrue(cata.Name.Equals("Fish"));

                var commands = pCounter.DictionaryOfQueries.Values.ToArray();
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
