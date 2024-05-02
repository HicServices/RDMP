// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers;

internal class ExamplePluginCohortCompilerUI : PluginUserInterface
{
    private readonly ExamplePluginCohortCompiler compiler;

    public ExamplePluginCohortCompilerUI(IBasicActivateItems activator) : base(activator)
    {
        compiler = new ExamplePluginCohortCompiler();
    }

    public override bool CustomActivate(IMapsDirectlyToDatabaseTable o)
    {
        // we only care about responding to opening AggregateConfiguration objects
        // and then only if the aggregate being edited is one of the ones that our API handles
        if (o is not AggregateConfiguration ac || !compiler.ShouldRun(ac)) return false;

        // Look at the Description property for a number (your API could use a complex XML or YAML syntax if you want)
        // You could also store some info at the Catalogue level for reuse in other Aggregates

        if (!int.TryParse(ac.Description, out var number)) number = 5;

        // Launch a UI that prompts a new value to be entered
        if (BasicActivator.TypeText("Generate random CHIs", "Number of Chis:", 100, number.ToString(),
                out var result, false))
            if (int.TryParse(result, out var newCount))
            {
                ac.Description = newCount.ToString();
                ac.SaveToDatabase();
            }

        // we handled this, don't launch the default user interface
        return true;
    }
}