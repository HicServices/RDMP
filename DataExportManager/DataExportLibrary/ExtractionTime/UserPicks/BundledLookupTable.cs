using System;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    public class BundledLookupTable : IBundledLookupTable
    {
        public TableInfo TableInfo { get; set; }

        public BundledLookupTable(TableInfo tableInfo)
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