using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.Dependencies
{
    public class DependencyTests : DatabaseTests
    {
        [Test]
        public void ExtractionInformationTriangle()
        {
            var t = new TableInfo(CatalogueRepository, "t");
            var col = new ColumnInfo(CatalogueRepository, "mycol", "varchar(10)", t);

            var cat = new Catalogue(CatalogueRepository, "MyCat");
            var ci = new CatalogueItem(CatalogueRepository, cat, "myci");


            try
            {
                //col depends on tr
                Assert.Contains(t,col.GetObjectsThisDependsOn());
                Assert.Contains(col,t.GetObjectsDependingOnThis());

                //catalogue depends on catalogue items existing (slightly counter intuitive but think of it as data flow out of technical low level data through transforms into datasets - and then into researchers and research projects)
                Assert.Contains(cat,ci.GetObjectsDependingOnThis());
                Assert.Contains(ci,cat.GetObjectsThisDependsOn());

                //catalogue item should not be relying on anything currently (no link to underlying technical data)
                Assert.IsNull(ci.GetObjectsThisDependsOn());

                //now they are associated so the ci should be dependent on the col
                ci.SetColumnInfo(col);
                Assert.Contains(col, ci.GetObjectsDependingOnThis());

            }
            finally
            {
                t.DeleteInDatabase();
                cat.DeleteInDatabase();
            }
        }
    }
}
