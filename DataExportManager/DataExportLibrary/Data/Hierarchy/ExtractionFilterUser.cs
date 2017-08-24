using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Data.Hierarchy
{
    public class ExtractionFilterUser
    {
        public FilterContainer Container { get; private set; }
        public int ExtractionConfiguration_ID;
        public int ExtractableDataset_ID;
        public int RootFilterContainer_ID;

        public ExtractionFilterUser(int extractionConfiguration_ID, int extractableDataset_ID, int rootFilterContainer_ID, FilterContainer container)
        {
            Container = container;
            ExtractionConfiguration_ID = extractionConfiguration_ID;
            ExtractableDataset_ID = extractableDataset_ID;
            RootFilterContainer_ID = rootFilterContainer_ID;

        }

    }
}
