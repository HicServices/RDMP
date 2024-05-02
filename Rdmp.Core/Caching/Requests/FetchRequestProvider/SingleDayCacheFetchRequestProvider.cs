// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Requests.FetchRequestProvider;

/// <summary>
///     Generates ICacheFetchRequests until the end of the day.  Day is based on the initial request.  This can be still be
///     multiple requests if the ICacheProgress
///     ChunkPeriod is, for example, 1 hour at a time.
/// </summary>
public class SingleDayCacheFetchRequestProvider : ICacheFetchRequestProvider
{
    private readonly ICacheFetchRequest _initialRequest;
    public ICacheFetchRequest Current { get; private set; }

    public SingleDayCacheFetchRequestProvider(ICacheFetchRequest initialRequest)
    {
        Current = null;
        _initialRequest = initialRequest;
    }

    public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
    {
        // If we haven't provided one, give out _initialRequest
        if (Current == null)
        {
            Current = _initialRequest;
        }
        else
        {
            Current = Current.GetNext();
            // We have provided requests for more than one day
            if (Current.Start >= _initialRequest.Start.AddDays(1))
                return null;
        }

        // Otherwise we have provided our request so signal there is none left
        return Current;
    }
}