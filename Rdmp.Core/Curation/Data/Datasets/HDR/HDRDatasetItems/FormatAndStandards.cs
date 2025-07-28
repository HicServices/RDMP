using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems
{
    /// <summary>
    /// Mapping from HDR API
    /// </summary>
    public class FormatAndStandards
    {
        public List<string> conformsTo { get; set; }
        public List<string> vocabularyEncodingScheme { get; set; }
        public List<string> language { get; set; }
        public List<string> format { get; set; }
    }
}
