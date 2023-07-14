// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
/// An ongoing async execution of a cohort identification subquery in the CohortCompiler.  Includes the query used to fetch the cohort identifiers, the 
/// identifiers themselves (once complete), cancellation token etc.
/// </summary>
public class CohortIdentificationTaskExecution: IDisposable
{
    private readonly IDataAccessPoint _cacheServerIfAny;
    public int SubQueries { get; private set; }
    public int SubqueriesCached { get; private set; }

    public bool IsResultsForRootContainer { get; set; }

    public DataTable Identifiers { get; private set; }
    public DataTable CumulativeIdentifiers { get; private set; }

    public bool IsExecuting { get; private set; }

    public string CumulativeSQL { get; set; }

    /// <summary>
    /// Although this is called CountSQL it is actually a select distinct identifiers!
    /// </summary>
    public string CountSQL { get; set; }

    private CancellationTokenSource _cancellationTokenSource;
    private readonly DiscoveredServer _target;
    private DbCommand _cmdCount;
    private DbDataReader _rIds;
    private DbDataReader _rCumulative;
    private DbCommand _cmdCountCumulative;

    public CohortIdentificationTaskExecution(IDataAccessPoint cacheServerIfAny, string countSQL, string cumulativeSQL, CancellationTokenSource cancellationTokenSource, int subQueries, int subqueriesCached, bool isResultsForRootContainer,DiscoveredServer target)
    {
        _cacheServerIfAny = cacheServerIfAny;
        SubQueries = subQueries;
        SubqueriesCached = subqueriesCached;
        CountSQL = countSQL;
        CumulativeSQL = cumulativeSQL;
        _cancellationTokenSource = cancellationTokenSource;
        _target = target;
        IsResultsForRootContainer = isResultsForRootContainer;
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        if (_cmdCount != null && _cmdCount.Connection.State == ConnectionState.Open)
        {
            _cmdCount.Cancel();
        }

        if (_rIds != null && !_rIds.IsClosed)
        {
            try
            {
                _rIds.Close();
            }
            catch (InvalidOperationException)
            {
            }
        }

        if (_rCumulative != null && !_rCumulative.IsClosed)
        {
            try
            {
                _rCumulative.Close();
            }
            catch (InvalidOperationException)
            {
            }
        }
            
    }

        
    public void GetCohortAsync(int commandTimeout)
    {
        if(Identifiers != null)
            throw new Exception("GetCohortAsync has already been called for this object");

        Identifiers = new DataTable();
            
        IsExecuting = true;

        var server = _target;
            
        server.EnableAsync();

        using (var con = server.GetConnection())
        {
            con.Open();
            _cmdCount = server.GetCommand(CountSQL, con);
            _cmdCount.CommandTimeout = commandTimeout;

            var identifiersReader = _cmdCount.ExecuteReaderAsync(_cancellationTokenSource.Token);

            identifiersReader.Wait(_cancellationTokenSource.Token);
            _rIds = identifiersReader.Result;
            Identifiers.Load(_rIds);
            _rIds.Close();
            _rIds.Dispose();

            Task<DbDataReader> cumulativeIdentifiersRead = null;

            //if there is cumulative SQL happening
            if (!string.IsNullOrWhiteSpace(CumulativeSQL))
            {
                CumulativeIdentifiers = new DataTable();

                _cmdCountCumulative = server.GetCommand(CumulativeSQL, con);
                _cmdCountCumulative.CommandTimeout = commandTimeout;
                cumulativeIdentifiersRead = _cmdCountCumulative.ExecuteReaderAsync(_cancellationTokenSource.Token);
            }

            if (cumulativeIdentifiersRead != null)
            {
                cumulativeIdentifiersRead.Wait(_cancellationTokenSource.Token);
                _rCumulative = cumulativeIdentifiersRead.Result;
                CumulativeIdentifiers.Load(_rCumulative);
                _rCumulative.Close();
                _rCumulative.Dispose();
            }

            IsExecuting = false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Identifiers?.Dispose();
        CumulativeIdentifiers?.Dispose();
        _rIds?.Dispose();
        _rCumulative?.Dispose();
        _cmdCount?.Dispose();
        _cmdCountCumulative?.Dispose();
    }
}