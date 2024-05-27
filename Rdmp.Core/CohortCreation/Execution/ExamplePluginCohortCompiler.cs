// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using SynthEHR;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     Demonstration class for how to implement a plugin cohort e.g. to a REST API.
///     This class generates a number of random chis when prompted to query the 'api'.
///     <para>If deployed as a patient index table, also returns random dates of birth and death</para>
/// </summary>
public class ExamplePluginCohortCompiler : PluginCohortCompiler
{
    public const string ExampleAPIName = $"{ApiPrefix}GenerateRandomChisExample";

    public override void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // The user of RDMP will have configured ac as either patient index table or normal cohort aggregate
        if (ac.IsJoinablePatientIndexTable())
            // user expects multiple columns from the API
            RunAsPatientIndexTable(ac, cache, token);
        else
            // user expects only a single linkage identifier to be returned by the API
            RunAsIdentifierList(ac, cache, token);
    }

    private void RunAsPatientIndexTable(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache,
        CancellationToken _)
    {
        using var dt = new DataTable();
        dt.Columns.Add("chi", typeof(string));
        dt.Columns.Add("dateOfBirth", typeof(DateTime));
        dt.Columns.Add("dateOfDeath", typeof(DateTime));

        // generate a list of random chis + date of birth/death
        var pc = new PersonCollection();
        pc.GeneratePeople(GetNumberToGenerate(ac), new Random());

        foreach (var p in pc.People) dt.Rows.Add(p.CHI, p.DateOfBirth, p.DateOfDeath ?? (object)DBNull.Value);

        SubmitPatientIndexTable(dt, ac, cache, true);
    }

    private void RunAsIdentifierList(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache,
        CancellationToken _)
    {
        var pc = new PersonCollection();
        var requiredNumber = GetNumberToGenerate(ac);
        var rand = new Random();
        pc.GeneratePeople(requiredNumber, rand);

        var set = new HashSet<string>(pc.People.Select(p => p.CHI));

        // there may be duplicates, if so we need to bump up the number to match the required count
        while (set.Count < requiredNumber)
        {
            pc.GeneratePeople(1, rand);
            set.Add(pc.People[0].CHI);
        }

        // generate a list of random chis
        SubmitIdentifierList("chi", set, ac, cache);
    }

    private static int GetNumberToGenerate(AggregateConfiguration ac)
    {
        // You can persist configuration info about how to query the API any way
        // you want.  Here we just use the Description field
        return int.TryParse(ac.Description, out var result) ? result : 5;
    }

    public override bool ShouldRun(ICatalogue cata)
    {
        // we will handle any dataset where the associated Catalogue has this name
        // you can customise how to spot your API calls however you want
        return cata.Name.Equals(ExampleAPIName);
    }

    protected override string GetJoinColumnNameFor(AggregateConfiguration joinedTo)
    {
        // when RunAsPatientIndexTable is being used the column that can be linked
        // to other datasets is called "chi"
        return "chi";
    }
}