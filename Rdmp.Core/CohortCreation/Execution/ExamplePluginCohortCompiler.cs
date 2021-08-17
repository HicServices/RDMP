// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;
using System;
using System.Data;
using System.Linq;

namespace Rdmp.Core.CohortCreation.Execution
{
    public class ExamplePluginCohortCompiler : PluginCohortCompiler
    {
        public const string ExampleAPIName = ApiPrefix + "GenerateRandomChisExample";

        public override void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
        {
            int toGenerate = 5;
            if (int.TryParse(ac.Description, out int result))
            {
                toGenerate = result;
            }

            var pc = new PersonCollection();
            pc.GeneratePeople(toGenerate, new Random());

            SubmitIdentifierList("chi",pc.People.Select(p => p.CHI),ac,cache);
        }



        public override bool ShouldRun(ICatalogue cata)
        {
            return cata.Name.Equals(ExampleAPIName);
        }
        
    }
}
