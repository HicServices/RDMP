// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Threading;
using FAnsi.Discovery;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
/// An ongoing async execution of a cohort identification subquery in the CohortCompiler.  Includes the query used to fetch the cohort identifiers, the
/// identifiers themselves (once complete), cancellation token etc.
/// </summary>
public sealed class CohortIdentificationTaskExecution : IDisposable
{
    internal readonly int SubQueries;
    public readonly int SubqueriesCached;
    internal readonly bool IsResultsForRootContainer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly DiscoveredServer _target;
    private readonly string _cumulativeSQL;

    public DataTable Identifiers { get; private set; }
    internal DataTable CumulativeIdentifiers { get; private set; }

    internal bool IsExecuting { get; private set; }

    /// <summary>
    /// Although this is called CountSQL it is actually a select distinct identifiers!
    /// </summary>
    public string CountSQL { get; set; }

    internal CohortIdentificationTaskExecution(string countSQL, string cumulativeSQL,
        CancellationTokenSource cancellationTokenSource, int subQueries, int subqueriesCached,
        bool isResultsForRootContainer, DiscoveredServer target)
    {
        SubQueries = subQueries;
        SubqueriesCached = subqueriesCached;
        CountSQL = countSQL;
        _cumulativeSQL = cumulativeSQL;
        _cancellationTokenSource = cancellationTokenSource;
        _target = target;
        IsResultsForRootContainer = isResultsForRootContainer;
    }

    internal void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }


    internal void GetCohortAsync(int commandTimeout)
    {
        if (Identifiers != null)
            throw new Exception("GetCohortAsync has already been called for this object");

        Identifiers = new DataTable();

        IsExecuting = true;

        _target.EnableAsync();

        using var con = _target.GetConnection();
        con.Open();
        using var cmdCount = _target.GetCommand(CountSQL, con);
        cmdCount.CommandTimeout = commandTimeout;

        var identifiersReader = cmdCount.ExecuteReaderAsync(_cancellationTokenSource.Token);

        identifiersReader.Wait(_cancellationTokenSource.Token);
        using var rIds = identifiersReader.Result;
        Identifiers.BeginLoadData();
        Identifiers.Load(rIds);
        Identifiers.EndLoadData();
        rIds.Close();

        //if there is cumulative SQL happening
        if (!string.IsNullOrWhiteSpace(_cumulativeSQL))
        {
            CumulativeIdentifiers = new DataTable();

            using var cmdCountCumulative = _target.GetCommand(_cumulativeSQL, con);
            cmdCountCumulative.CommandTimeout = commandTimeout;
            var cumulativeIdentifiersRead = cmdCountCumulative.ExecuteReaderAsync(_cancellationTokenSource.Token);
            cumulativeIdentifiersRead.Wait(_cancellationTokenSource.Token);
            using var rCumulative = cumulativeIdentifiersRead.Result;
            CumulativeIdentifiers.Load(rCumulative);
            rCumulative.Close();
        }

        IsExecuting = false;
    }

    public void Dispose()
    {
        Identifiers?.Dispose();
        CumulativeIdentifiers?.Dispose();
    }
}