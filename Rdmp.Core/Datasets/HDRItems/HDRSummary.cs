using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets.HDRItems
{
    public class HDRSummary
    {
        public HDRSummary(string title) {
            Title = title;
        }

        public string Title { get; set; }
    }
}
