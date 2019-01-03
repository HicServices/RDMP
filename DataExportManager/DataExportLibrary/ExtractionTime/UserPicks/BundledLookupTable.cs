using System;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    /// <summary>
    /// Identifies a TableInfo that acts as a Lookup for a given dataset which is being extracted.  Lookup tables can be extracted along with the extracted data
    /// set (See ExtractableDatasetBundle).
    /// </summary>
    public class BundledLookupTable : IBundledLookupTable
    {
        public ITableInfo TableInfo { get; set; }

        public BundledLookupTable(ITableInfo tableInfo)
        {
            if(!tableInfo.IsLookupTable())
                throw new Exception("TableInfo " + tableInfo + " is not a lookup table");

            TableInfo = tableInfo;
        }

        public override string ToString()
        {
            return TableInfo.ToString();
        }
    }
}