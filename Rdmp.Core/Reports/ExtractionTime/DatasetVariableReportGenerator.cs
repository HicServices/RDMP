using CommandLine;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.QueryBuilding;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Reports.ExtractionTime
{
    class DatasetVariableReportGenerator
    {
        private ICatalogue _catalogue;
        private ExtractionPipelineUseCase _executor;
        private IExecuteDatasetExtractionDestination _destination;
        private List<ExtractableColumn> _columnstoExtract;
        private List<ReleaseIdentifierSubstitution> _releaseSubs;


        public DatasetVariableReportGenerator(ExtractionPipelineUseCase executer) {
            _executor = executer;
            _destination = executer.Destination;
            _catalogue = executer.Source.Request.Catalogue;
            _columnstoExtract = executer.Source.Request.ColumnsToExtract.Select(c => c.Cast<ExtractableColumn>()).ToList();
            _releaseSubs = executer.Source.Request.ReleaseIdentifierSubstitutions;
        }

        public void GenerateDatasetVariableReport()
        {
            var csv = new StringBuilder();
            WriteHeaders(csv);
            foreach(var column in _columnstoExtract)
            {
                WriteColumn(column, csv);
            }
            foreach (var sub in _releaseSubs)
            {
                WriteReleaseSubs(sub, csv);
            }
            File.WriteAllText(new FileInfo(Path.Combine(_destination.DirectoryPopulated.FullName,
                $"{_destination.GetFilename()}Variables.csv")).ToString(), csv.ToString());
        }

        private void WriteColumn(ExtractableColumn column, StringBuilder sb)
        {
            var catalogueItem = _catalogue.CatalogueItems.Where(c => c.ColumnInfo_ID == column.ColumnInfo.ID).First();
            bool isNull = !catalogueItem.ExtractionInformation.IsPrimaryKey;
            bool isIdentifier = catalogueItem.ExtractionInformation.IsExtractionIdentifier;
            sb.AppendLine($"\"{column.GetRuntimeName()}\",\"{column.ColumnInfo.Data_type}\",{isNull},\"{catalogueItem.Description}\",{isIdentifier}");
        }

        private void WriteReleaseSubs(ReleaseIdentifierSubstitution releaseIdentifierSubstitution,StringBuilder sb)
        {
            var column = releaseIdentifierSubstitution.ColumnInfo;
            var catalogueItem = _catalogue.CatalogueItems.Where(c => c.ColumnInfo_ID == column.ID).First();
            sb.AppendLine($"\"{releaseIdentifierSubstitution.Alias}\",\"{column.Data_type}\",{false},\"{catalogueItem.Description}\",{releaseIdentifierSubstitution.IsExtractionIdentifier}");

        }

        private void WriteHeaders(StringBuilder sb)
        {
            sb.AppendLine("Variable Name, Type, Null possible(Y/N),Description,Identifier");
        }
    }
}
