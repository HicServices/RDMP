using System.Collections.Generic;
using BrightIdeasSoftware;

namespace MapsDirectlyToDatabaseTableUI
{
    public class WhiteListOnlyFilter : IModelFilter
    {
        private HashSet<object> _whitelist;

        public WhiteListOnlyFilter(IEnumerable<object> whitelist)
        {
            _whitelist = new HashSet<object>(whitelist);
        }

        public bool Filter(object modelObject)
        {
            return _whitelist.Contains(modelObject);
        }
    }
}