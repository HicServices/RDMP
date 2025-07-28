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
    public class Accessibility
    {
        public Access access { get; set; }
        public Usage usage { get; set; }
        public FormatAndStandards formatAndStandards { get; set; }
    }
}
