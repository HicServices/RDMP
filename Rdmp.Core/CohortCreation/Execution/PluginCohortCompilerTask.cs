// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution;

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