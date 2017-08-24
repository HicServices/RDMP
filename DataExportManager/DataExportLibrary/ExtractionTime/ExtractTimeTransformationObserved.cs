using System;
using CatalogueLibrary.Data;

namespace DataExportLibrary.ExtractionTime
{
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