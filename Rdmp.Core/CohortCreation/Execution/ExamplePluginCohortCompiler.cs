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
    /// <summary>
    /// Demonstration class for how to implement a plugin cohort e.g. to a REST API.
    /// This class generates a number of random chis when prompted to query the 'api'.
    /// 
    /// <para>If deployed as a patient index table, also returns random dates of birth and death</para>
    /// 
    /// </summary>
    public class ExamplePluginCohortCompiler : PluginCohortCompiler
    {
        public const string ExampleAPIName = ApiPrefix + "GenerateRandomChisExample";

        public override void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
        {
            if(ac.IsJoinablePatientIndexTable())
            {
                RunAsPatientIndexTable(ac, cache);
            }
            else
            {
                RunAsIdentifierList(ac, cache);
            }
        }

        private void RunAsPatientIndexTable(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("chi", typeof(string));
            dt.Columns.Add("dateOfBirth", typeof(DateTime));
            dt.Columns.Add("dateOfDeath", typeof(DateTime));

            // generate a list of random chis + date of birth/death
            var pc = new PersonCollection();
            pc.GeneratePeople(GetNumberToGenerate(ac), new Random());

            foreach (var p in pc.People)
            {
                dt.Rows.Add(p.CHI, p.DateOfBirth,p.DateOfDeath ?? (object)DBNull.Value);
            }

            SubmitPatientIndexTable(dt, ac, cache,true);
        }
        private void RunAsIdentifierList(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache)
        {
            var pc = new PersonCollection();
            pc.GeneratePeople(GetNumberToGenerate(ac), new Random());

            // generate a list of random chis
            SubmitIdentifierList("chi", pc.People.Select(p => p.CHI), ac, cache);
        }

        private int GetNumberToGenerate(AggregateConfiguration ac)
        {
            return int.TryParse(ac.Description, out int result) ? result: 5;
        }

        public override bool ShouldRun(ICatalogue cata)
        {
            return cata.Name.Equals(ExampleAPIName);
        }
        
    }
}
