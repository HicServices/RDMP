using Rdmp.Core.Curation.Data.Aggregation;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CohortCreation.Execution
{
    class PluginCohortCompilerTask : AggregationTask
    {
        public IPluginCohortCompiler PluginCompiler { get; }

        public PluginCohortCompilerTask(AggregateConfiguration ac,CohortCompiler mainCompiler, IPluginCohortCompiler pluginCompiler):base(ac,mainCompiler)
        {
            PluginCompiler = pluginCompiler;
        }

        public override IDataAccessPoint[] GetDataAccessPoints()
        {
            // for this task always go direct to the query cache
            return new[] { Aggregate.GetCohortIdentificationConfigurationIfAny().QueryCachingServer };
        }

    }
}
