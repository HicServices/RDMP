// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Requests.FetchRequestProvider;

/// <summary>
///     Generates ICacheFetchRequest incrementally until the given end date.  You must provide an initial request.
/// </summary>
public class MultiDayCacheFetchRequestProvider : ICacheFetchRequestProvider
{
    private readonly ICacheFetchRequest _initialRequest;
    private readonly DateTime _endDateInclusive;
    public ICacheFetchRequest Current { get; private set; }

    public MultiDayCacheFetchRequestProvider(ICacheFetchRequest initialRequest, DateTime endDateInclusive)
    {
        Current = null;
        _initialRequest = initialRequest;
        _endDateInclusive = endDateInclusive;
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

            // We have provided requests for the whole time period
            if (Current.Start > _endDateInclusive)
                return null;
        }

        return Current;
    }
}