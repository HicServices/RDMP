using System;
using CatalogueLibrary.Data;

namespace DataExportLibrary.ExtractionTime
{
    /// <summary>
    /// Documents the extraction time data type of an extracted column.  This is done by inspecting the Type of the DataTable column fetched when executing the
    /// extraction SQL.  This can be different from the Database/Catalogue Type because there can be transformation SQL entered (e.g. LEFT etc).
    /// </summary>
    public class ExtractTimeTransformationObserved
    {
        public bool FoundAtExtractTime { get; set; }
        public CatalogueItem CatalogueItem { get; set; }
        public string DataTypeInCatalogue { get; set; }
        public Type DataTypeObservedInRuntimeBuffer { get; set; }
        public string RuntimeName { get; set; }

        public ExtractTimeTransformationObserved()
        {
            FoundAtExtractTime = false;
        }
    }
}