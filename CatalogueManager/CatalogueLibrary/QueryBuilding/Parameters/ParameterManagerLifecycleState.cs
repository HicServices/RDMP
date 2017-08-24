using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.QueryBuilding.Parameters
{
    public enum ParameterManagerLifecycleState
    {
        AllowingGlobals,
        ParameterDiscovery,
        Finalized

    }
}
