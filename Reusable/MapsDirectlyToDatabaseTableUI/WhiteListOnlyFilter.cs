using System.Collections.Generic;
using BrightIdeasSoftware;

namespace MapsDirectlyToDatabaseTableUI
{
    public class WhiteListOnlyFilter : IModelFilter
    {
        public HashSet<object> Whitelist { get; private set; }

        public WhiteListOnlyFilter(IEnumerable<object> whitelist)
        {
            Whitelist = new HashSet<object>(whitelist);
        }

        public bool Filter(object modelObject)
        {
            return Whitelist.Contains(modelObject);
        }
    }
}