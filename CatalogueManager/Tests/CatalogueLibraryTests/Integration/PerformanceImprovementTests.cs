using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class PerformanceImprovementTests :DatabaseTests
    {
        [Test]
        public void TestClassification()
        {
            Catalogue c = new Catalogue(CatalogueRepository,"dentistry");

            CatalogueItem ciWithC = new CatalogueItem(CatalogueRepository,c, "drill");
            TableInfo t = new TableInfo(CatalogueRepository, "Dental");
            ColumnInfo col = new ColumnInfo(CatalogueRepository, "[Dental]..[drill]","varchar(10)",t);
            ciWithC.SetColumnInfo(col);

            CatalogueItem ciWithCAndE = new CatalogueItem(CatalogueRepository, c, "drill standardised transform");
            ColumnInfo colPk = new ColumnInfo(CatalogueRepository, "[Dental]..[drillPk]", "varchar(10)", t);
            colPk.IsPrimaryKey = true;
            colPk.SaveToDatabase();
            ciWithCAndE.SetColumnInfo(colPk);
            ExtractionInformation ei = new ExtractionInformation(CatalogueRepository, ciWithCAndE, colPk, "UPPER(drill) as drillStandardised");
            ei.ExtractionCategory = ExtractionCategory.SpecialApprovalRequired;
            ei.SaveToDatabase();

            ExtractionFilter ef = new ExtractionFilter(CatalogueRepository,"FlimsyDrills",ei);

            CatalogueItem ciOrphan = new CatalogueItem(CatalogueRepository, c,"orphan");

            TableInfo zDrillCost = new TableInfo(CatalogueRepository,"zDrillCost");
            ColumnInfo drillCode = new ColumnInfo(CatalogueRepository, "[zDrillCost]..[drillCode]", "int", zDrillCost);
            ColumnInfo drillDesc = new ColumnInfo(CatalogueRepository, "[zDrillCost]..[drillDesc]", "varchar(10)", zDrillCost);

            Catalogue drillCostCata = new Catalogue(CatalogueRepository,"DrillCostCata");
            CatalogueItem ciDrillCode = new CatalogueItem(CatalogueRepository,drillCostCata,"drillCode");
            CatalogueItem ciDrillDesc = new CatalogueItem(CatalogueRepository, drillCostCata,"drillDesc");
            ExtractionInformation eDrillCode = new ExtractionInformation(CatalogueRepository,ciDrillCode,drillCode, "[zDrillCost]..[drillCode]");
            ExtractionInformation eDrillDesc = new ExtractionInformation(CatalogueRepository,ciDrillDesc,drillDesc, "[zDrillCost]..[drillDesc]");

            var lookup = new Lookup(CatalogueRepository, drillDesc, colPk,drillCode, ExtractionJoinType.Inner,"collate Fishy");
            
            var classifications = CatalogueRepository.ClassifyAllCatalogueItems();

            Assert.IsTrue(classifications.ContainsKey(ciOrphan.ID));
            Assert.IsTrue(classifications.ContainsKey(ciWithC.ID));
            Assert.IsTrue(classifications.ContainsKey(ciWithCAndE.ID));

            var orphan = classifications[ciOrphan.ID];
            Assert.IsTrue(orphan.ColumnInfo_ID == null);
            Assert.IsTrue(orphan.ExtractionCategory == null);
            Assert.IsTrue(orphan.ExtractionFilterCount == 0);
            Assert.IsFalse(orphan.IsPrimaryKey);
            Assert.IsFalse(orphan.IsExtractionInformationOrphan());

            Assert.IsFalse(orphan.IsLookupForeignKey);
            Assert.IsFalse(orphan.IsLookupPrimaryKey);
            Assert.IsFalse(orphan.IsLookupDescription);
            
            var withC = classifications[ciWithC.ID];
            Assert.AreEqual(col.ID , withC.ColumnInfo_ID);
            Assert.IsNull(withC.ExtractionCategory);
            Assert.AreEqual(0,withC.ExtractionFilterCount);
            Assert.IsFalse(withC.IsPrimaryKey);
            Assert.IsFalse(withC.IsExtractionInformationOrphan());
            Assert.IsNull(withC.ExtractionInformation_ID);

            Assert.IsFalse(withC.IsLookupForeignKey);
            Assert.IsFalse(withC.IsLookupPrimaryKey);
            Assert.IsFalse(withC.IsLookupDescription);
            
            var withCAndE = classifications[ciWithCAndE.ID];
            Assert.AreEqual(colPk.ID, withCAndE.ColumnInfo_ID);
            Assert.IsTrue(withCAndE.IsPrimaryKey);
            Assert.AreEqual(ExtractionCategory.SpecialApprovalRequired, withCAndE.ExtractionCategory);
            Assert.AreEqual(1, withCAndE.ExtractionFilterCount);
            Assert.IsFalse(withCAndE.IsExtractionInformationOrphan());
            Assert.AreEqual(ei.ID,withCAndE.ExtractionInformation_ID);
            
            Assert.IsTrue(withCAndE.IsLookupForeignKey);
            Assert.IsFalse(withCAndE.IsLookupPrimaryKey);
            Assert.IsFalse(withCAndE.IsLookupDescription);


            var drillCodeClassification = classifications[ciDrillCode.ID];
            var drillDescClassification = classifications[ciDrillDesc.ID];
            
            Assert.IsTrue(drillCodeClassification.IsLookupPrimaryKey);
            Assert.IsFalse(drillCodeClassification.IsLookupForeignKey);
            Assert.IsFalse(drillCodeClassification.IsLookupDescription);

            Assert.IsFalse(drillDescClassification.IsLookupPrimaryKey);
            Assert.IsFalse(drillDescClassification.IsLookupForeignKey);
            Assert.IsTrue(drillDescClassification.IsLookupDescription);

            lookup.DeleteInDatabase();

            zDrillCost.DeleteInDatabase();
            t.DeleteInDatabase();

            classifications = CatalogueRepository.ClassifyAllCatalogueItems();
            Assert.IsFalse(classifications[ciOrphan.ID].IsExtractionInformationOrphan());
            Assert.IsFalse(classifications[ciWithC.ID].IsExtractionInformationOrphan());
            Assert.IsTrue(classifications[ciWithCAndE.ID].IsExtractionInformationOrphan());

            Assert.IsTrue(classifications[ciDrillCode.ID].IsExtractionInformationOrphan());
            Assert.IsTrue(classifications[ciDrillDesc.ID].IsExtractionInformationOrphan());

            drillCostCata.DeleteInDatabase();

            c.DeleteInDatabase();
        }
    }
}
