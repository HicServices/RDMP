using CommandLine;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.QueryBuilding;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rdmp.Core.Reports.ExtractionTime
{
    class DatasetVariableReportGenerator
    {
        private readonly ICatalogue _catalogue;
        private readonly IExecuteDatasetExtractionDestination _destination;
        private readonly List<ExtractableColumn> _columnsToExtract;
        private readonly List<ReleaseIdentifierSubstitution> _releaseSubs;


        public DatasetVariableReportGenerator(ExtractionPipelineUseCase executer) {
            _destination = executer.Destination;
            _catalogue = executer.Source.Request.Catalogue;
            _columnsToExtract = executer.Source.Request.ColumnsToExtract.Select(c => c.Cast<ExtractableColumn>()).ToList();
            _releaseSubs = executer.Source.Request.ReleaseIdentifierSubstitutions;
        }

        public void GenerateDatasetVariableReport()
        {
            var csv = new StringBuilder();
            WriteHeaders(csv);
            foreach(var column in _columnsToExtract)
            {
                WriteColumn(column, csv);
            }
            foreach (var sub in _releaseSubs)
            {
                WriteReleaseSubs(sub, csv);
            }
            File.WriteAllText(new FileInfo(Path.Join(_destination.DirectoryPopulated.FullName,
                $"{_destination.GetFilename()}Variables.csv")).ToString(), csv.ToString());
        }


        private string LookupKeyNameGenerator(ColumnInfo columnInfo)
        {
            var split = columnInfo.GetFullyQualifiedName().Split('.');
            return $"{split[^2]}.{split[^1]}";
        }
        private string LookupStringGenerator(Lookup lookup)
        {             
            return $"{LookupKeyNameGenerator(lookup.ForeignKey)} = {LookupKeyNameGenerator(lookup.PrimaryKey)}";
        }

        private void WriteColumn(ExtractableColumn column, StringBuilder sb)
        {
            if (_catalogue.CatalogueItems.Length == 0) return;
            var catalogueItem = _catalogue.CatalogueItems.FirstOrDefault(c => c.ColumnInfo_ID == column.ColumnInfo.ID);
            if (catalogueItem is null || catalogueItem.ExtractionInformation is null) return;
            bool isNull = !catalogueItem.ExtractionInformation.IsPrimaryKey;
            bool isIdentifier = catalogueItem.ExtractionInformation.IsExtractionIdentifier;
            var lookups = _catalogue.CatalogueRepository.GetAllObjectsWhere<Lookup>("ForeignKey_ID",column.ColumnInfo.ID);
            var lookupString = "";
            if (lookups.Length != 0) lookupString = string.Join(';', lookups.Select(l => LookupStringGenerator(l)));
            sb.AppendLine($"\"{column.GetRuntimeName()}\",\"{column.ColumnInfo.Data_type}\",{isNull},\"{catalogueItem.Description}\",{isIdentifier},{lookups.Length != 0},{lookupString}");
        }

        private void WriteReleaseSubs(ReleaseIdentifierSubstitution releaseIdentifierSubstitution,StringBuilder sb)
        {
            var column = releaseIdentifierSubstitution.ColumnInfo;
            var catalogueItem = _catalogue.CatalogueItems.Where(c => c.ColumnInfo_ID == column.ID).First();
            var lookups = _catalogue.CatalogueRepository.GetAllObjectsWhere<Lookup>("ForeignKey_ID", column.ID);
            var lookupString = "";
            if (lookups.Length != 0) lookupString = string.Join(';', lookups.Select(l => LookupStringGenerator(l)));
            sb.AppendLine($"\"{releaseIdentifierSubstitution.Alias}\",\"{column.Data_type}\",{false},\"{catalogueItem.Description}\",{releaseIdentifierSubstitution.IsExtractionIdentifier},{lookups.Any()},{lookupString}");

        }

        private static void WriteHeaders(StringBuilder sb)
        {
            sb.AppendLine("Variable Name, Type, Null possible(Y/N),Description,Identifier,HasLookups,Lookups");
        }
    }
}
