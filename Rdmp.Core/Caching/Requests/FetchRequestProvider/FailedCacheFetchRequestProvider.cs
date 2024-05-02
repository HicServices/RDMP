// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Requests.FetchRequestProvider;

/// <summary>
///     Sometimes during caching you will identify a period of time that cannot be fetched because of problems outwith your
///     control (the remote server data is missing etc).
///     These periods are modeled by ICacheFetchFailure.  This Provider allows you to load a batch of failures and re try
///     them.
/// </summary>
public class FailedCacheFetchRequestProvider : ICacheFetchRequestProvider
{
    public ICacheFetchRequest Current { get; private set; }

    private readonly ICacheProgress _cacheProgress;
    private readonly int _batchSize;

    private int _start;
    private Queue<ICacheFetchFailure> _failuresToProvide = new();

    public FailedCacheFetchRequestProvider(ICacheProgress cacheProgress, int batchSize = 50)
    {
        _cacheProgress = cacheProgress;
        _batchSize = batchSize;

        Current = null;
    }

    /// <summary>
    /// </summary>
    /// <returns>Next CacheFetchRequest or null if there are no further request failures to process</returns>
    public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
    {
        if (!_failuresToProvide.Any())
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Getting next batch of request failures from database."));
            GetNextBatchFromDatabase();
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Fetched {_failuresToProvide.Count} failures from database."));

            // If there are still no failures in the queue then we're done
            if (!_failuresToProvide.Any())
                return null;
        }

        // Create a new CacheFetchRequest from the failure
        var cacheFetchFailure = _failuresToProvide.Dequeue();
        Current = new CacheFetchRequest(cacheFetchFailure, _cacheProgress);
        return Current;
    }

    private void GetNextBatchFromDatabase()
    {
        _failuresToProvide = new Queue<ICacheFetchFailure>(_cacheProgress.FetchPage(_start, _batchSize));
        _start += _batchSize;
    }
}