using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace DataExportLibrary.ExtractionTime.Commands
{
    public class ExtractCommandCollection
    {
        public IExtractDatasetCommand[] Datasets { get; set; }
        public IExtractCohortCustomTableCommand[] CustomTables { get; set; }
        

        public ExtractCommandCollection(IEnumerable<ExtractDatasetCommand> datasetBundles, IEnumerable<ExtractCohortCustomTableCommand> customTableCommands)
        {
            Datasets = datasetBundles.ToArray();
            CustomTables = customTableCommands.ToArray();
        }
    }
}
