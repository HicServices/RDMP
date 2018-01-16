using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace DataExportLibrary.ExtractionTime.Commands
{
    /// <summary>
    /// Collection of all the datasets and custom tables that are available for extraction.  Since datasets can take a long time to extract sometimes a user
    /// will opt to only extract a subset of datasets (or he may have already succesfully extracted some datasets).  This collection should contain a full
    /// set of all the things that can be run for a given ExtractionConfiguration (including dataset specific lookups etc).
    /// 
    /// Use ExtractCommandCollectionFactory to create instances of this class.
    /// 
    /// </summary>
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
