using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets.HDRItems
{
    /// <summary>
    /// 
    /// </summary>
    public class Usage
    {
        public List<string> dataUseLimitation { get; set; }
        public object resourceCreator { get; set; }
        public List<string> dataUseRequirements { get; set; } = [];
    }
}
