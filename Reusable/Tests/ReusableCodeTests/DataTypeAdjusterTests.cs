using System.Collections.Generic;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TableCreation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace ReusableCodeTests
{
    class DataTypeAdjusterTests:DatabaseTests
    {
        [Test]
        public void CreateTable_WithAdjuster()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("MyTable", new[]
            {
                new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof (string), 10))
            }, null, new DataTypeAdjusterTestsPadder());

            Assert.AreEqual(12,tbl.DiscoverColumn("Name").DataType.GetLengthIfString());
            tbl.Drop();
        }

        internal class DataTypeAdjusterTestsPadder : IDatabaseColumnRequestAdjuster
        {
            public void AdjustColumns(List<DatabaseColumnRequest> columns)
            {
                columns[0].TypeRequested.MaxWidthForStrings = 12;
            }
        }
    }

}
