using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DiscoveredParameter
    {
        public string ParameterName { get; set; }
        public DiscoveredParameter(string parameterName)
        {
            ParameterName = parameterName;
        }

        public DiscoveredDataType DataType { get; set; }
    }
}
