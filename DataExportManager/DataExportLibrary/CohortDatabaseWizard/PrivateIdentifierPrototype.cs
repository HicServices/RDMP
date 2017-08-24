using System.Collections.Generic;
using CatalogueLibrary.Data;

namespace DataExportLibrary.CohortDatabaseWizard
{
    public class PrivateIdentifierPrototype
    {
        public PrivateIdentifierPrototype(ExtractionInformation extractionInformation)
        {
            RuntimeName = extractionInformation.GetRuntimeName();
            DataType = extractionInformation.ColumnInfo.Data_type;
            MatchingExtractionInformations = new List<ExtractionInformation>(new []{extractionInformation});
        }

        public string RuntimeName { get; internal set; }
        public string DataType { get; internal set; }
        public List<ExtractionInformation> MatchingExtractionInformations { get; internal set; }

        public bool IsCompatible(ExtractionInformation extractionInformation)
        {
            return extractionInformation.GetRuntimeName() == RuntimeName && extractionInformation.ColumnInfo.Data_type == DataType;
        }

        public int CountOfTimesSeen()
        {
            return  MatchingExtractionInformations.Count;
        }

        public string GetDeclarationSql()
        {
            return RuntimeName + " " + DataType;

        }
    }
}