using Rdmp.Core.Curation.Data.Dashboarding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.Providers.Nodes
{
    /// <summary>
    /// Collection container for all the <see cref="DashboardLayout"/> that have been defined.
    /// </summary>
    public class AllDashboardsNode : SingletonNode
    {
        public AllDashboardsNode() : base("Dashboards")
        {

        }
    }
}
