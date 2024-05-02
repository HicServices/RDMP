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
using FAnsi.Discovery;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using TypeGuesser;

namespace Rdmp.Core.CohortCreation.Execution;

public abstract class PluginCohortCompiler : IPluginCohortCompiler
{
    /// <summary>
    ///     The prefix that should be on <see cref="Catalogue" /> names if they reflect API calls.
    ///     Each <see cref="IPluginCohortCompiler" /> should expand upon this to identify its specific
    ///     responsibilities (e.g. if you have 2+ Types of API available)
    /// </summary>
    public const string ApiPrefix = "API_";

    /// <summary>
    ///     The string to put into the database when no <see cref="AggregateConfiguration.Description" /> exists
    /// </summary>
    protected const string None = "None";

    /// <summary>
    ///     Override to fetch the results from your API.
    /// </summary>
    /// <param name="ac">Stores any configuration information about what query to to execute on your API</param>
    /// <param name="cache">
    ///     Where to store results.  Note you can use helper method <see cref="SubmitIdentifierList{T}" /> instead
    ///     of using this directly
    /// </param>
    /// <param name="token">Check this token for cancellation regularly if your API call takes a while to complete</param>
    public abstract void Run(AggregateConfiguration ac, CachedAggregateConfigurationResultsManager cache,
        CancellationToken token);

    public virtual bool ShouldRun(AggregateConfiguration ac)
    {
        return ShouldRun(ac.Catalogue);
    }

    public abstract bool ShouldRun(ICatalogue catalogue);


    /// <summary>
    ///     Submits the resulting <paramref name="enumerable" /> list to the query <paramref name="cache" /> as the result
    ///     of executing the API call of the <paramref name="aggregate" />
    /// </summary>
    /// <typeparam name="T">Type of the identifiers, must be a basic value type supported by DBMS e.g. string, int etc</typeparam>
    /// <param name="identifierName"></param>
    /// <param name="enumerable"></param>
    /// <param name="aggregate"></param>
    /// <param name="cache"></param>
    protected void SubmitIdentifierList<T>(string identifierName, IEnumerable<T> enumerable,
        AggregateConfiguration aggregate, CachedAggregateConfigurationResultsManager cache)
    {
        var g = new Guesser(new DatabaseTypeRequest(typeof(T)));

        // generate random chi numbers
        using var dt = new DataTable();
        dt.Columns.Add(identifierName, typeof(T));
        foreach (var p in enumerable)
        {
            dt.Rows.Add(p);
            g.AdjustToCompensateForValue(p);
        }

        // this is how you commit the results to the cache
        var args = new CacheCommitIdentifierList(aggregate, GetDescription(aggregate), dt,
            new DatabaseColumnRequest(identifierName, g.Guess, false), 5000);

        cache.CommitResults(args);
    }

    /// <summary>
    ///     Submits the <paramref name="results" /> of calling your API to the cache ready for joining
    ///     against other datasets as a patient index table.  Only use this method if you must return
    ///     multiple columns.
    /// </summary>
    /// <param name="results"></param>
    /// <param name="aggregate"></param>
    /// <param name="cache"></param>
    /// <param name="knownTypes">
    ///     If your DataTable is properly Typed (i.e. columns in <paramref name="results" /> have assigned Types)
    ///     then pass true.  If everything is a string and you want types to be assigned for these for querying later pass
    ///     false.
    /// </param>
    protected void SubmitPatientIndexTable(DataTable results, AggregateConfiguration aggregate,
        CachedAggregateConfigurationResultsManager cache, bool knownTypes)
    {
        // The data table has to go into the database so we need to know max length of strings, decimal precision etc
        var guessers = new Dictionary<string, Guesser>();

        foreach (DataColumn col in results.Columns)
        {
            // if the user told us the datatypes were right then assume they are honest otherwise make it up as you go along
            var g = knownTypes ? new Guesser(new DatabaseTypeRequest(col.DataType)) : new Guesser();

            // measure data being submitted
            g.AdjustToCompensateForValues(col);

            guessers.Add(col.ColumnName, g);
        }

        // this is how you commit the results to the cache
        var args = new CacheCommitJoinableInceptionQuery(aggregate, GetDescription(aggregate), results,
            guessers.Select(k => new DatabaseColumnRequest(k.Key, k.Value.Guess)).ToArray()
            , 5000);
        cache.CommitResults(args);
    }

    /// <summary>
    ///     Returns a description of the <paramref name="aggregate" />.  This will be persisted along
    ///     with the results in the cache to detect when changes are made to the config (and therefore
    ///     the cached result list should be discarded).
    /// </summary>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    protected virtual string GetDescription(AggregateConfiguration aggregate)
    {
        return aggregate.Description ?? "none";
    }

    public virtual bool IsStale(AggregateConfiguration aggregate, string oldDescription)
    {
        return !string.Equals(GetDescription(aggregate), oldDescription, StringComparison.CurrentCultureIgnoreCase);
    }

    public virtual IHasRuntimeName GetJoinColumnForPatientIndexTable(AggregateConfiguration joinedTo)
    {
        var colName = GetJoinColumnNameFor(joinedTo);
        return new SpontaneouslyInventedColumn(new MemoryRepository(), colName, colName);
    }

    /// <summary>
    ///     If your API supports returning multiple columns (it can be a patient index table)
    ///     then you must return (for <paramref name="joinedTo" />) the name of the column
    ///     that will be joined to other cohorts in the cohort builder.
    /// </summary>
    /// <param name="joinedTo"></param>
    /// <returns></returns>
    protected abstract string GetJoinColumnNameFor(AggregateConfiguration joinedTo);
}