using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using NUnit.Framework;
using ReusableLibraryCode.Checks;

namespace CatalogueLibraryTests.Unit
{
    public class DocumentationReportMapsDirectlyToDatabaseTest
    {

        [Test]
        public void CheckProducesCorrectSummary()
        {
            var result =
            DocumentationReportMapsDirectlyToDatabase.GetSummaryFromContent(
                typeof(Catalogue),
                @"
        string GetServerName(bool allowCaching=true);
        string GetDatabaseName(bool allowCaching = true);

        /// <summary>
        /// In the context of the DLE, you should use current jobs tableinfo list where possible for performance gain etc
        /// </summary>
        /// <param name=includeLookupTables></param>
        /// <returns></returns>
        TableInfo[] GetTableInfoList(bool includeLookupTables);
        TableInfo[] GetLookupTableInfoList();

        Dictionary<string, string> GetListOfTableNameMappings(HICTableNamingConvention destinationNamingConvention, ITableNamingScheme tableNamingScheme);

        string GetRawDatabaseName();

        int? LiveLoggingServer_ID { get; set; }
        int? TestLoggingServer_ID { get; set; }

        string Name { get; }

        IDataAccessPoint GetLoggingServer(bool isTest);
    }

    /// <summary>
    /// The central class for the RDMP, a Catalogue is a virtual dataset e.g. 'Hospital Admissions'.  A Catalogue can be a merging of multiple underlying tables and exists 
    /// independent of where the data is actually stored (look at other classes like TableInfo to see the actual locations of data).
    /// 
    /// <para>As well as storing human readable names/descriptions of what is in the dataset it is the hanging off point for Attachments (SupportingDocument), validation logic, 
    /// extractable columns (CatalogueItem->ExtractionInformation->ColumnInfo) ways of filtering the data, aggregations to help understand the dataset etc.</para>
    /// 
    /// <para>Catalogues are always flat views although they can be built from multiple relational data tables underneath.</para>
    /// 
    /// <para>Whenever you see Catalogue, think Dataset (which is a reserved class in C#, hence the somewhat confusing name Catalogue)</para>
    /// </summary>
    public class Catalogue : IDeleteable, IComparable, IMapsDirectlyToDatabaseTable, ICatalogueMetadata, IHasDependencies, ICheckable,IRevertable
    {
        
",
            new ThrowImmediatelyCheckNotifier());


            Assert.AreEqual(@"The central class for the RDMP, a Catalogue is a virtual dataset e.g. 'Hospital Admissions'. A Catalogue can be a merging of multiple underlying tables and exists independent of where the data is actually stored (look at other classes like TableInfo to see the actual locations of data). 
As well as storing human readable names/descriptions of what is in the dataset it is the hanging off point for Attachments (SupportingDocument), validation logic, extractable columns (CatalogueItem->ExtractionInformation->ColumnInfo) ways of filtering the data, aggregations to help understand the dataset etc. 
Catalogues are always flat views although they can be built from multiple relational data tables underneath. 
Whenever you see Catalogue, think Dataset (which is a reserved class in C#, hence the somewhat confusing name Catalogue)",
                result);
        }
    }
}

