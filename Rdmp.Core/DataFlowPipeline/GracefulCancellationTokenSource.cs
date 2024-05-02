// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;

namespace Rdmp.Core.DataFlowPipeline;

/// <summary>
///     Source for creating a GracefulCancellationToken.  See GracefulCancellationToken for description of this two level
///     Cancellation strategy.
/// </summary>
public class GracefulCancellationTokenSource
{
    private readonly CancellationTokenSource _stopTokenSource;
    private readonly CancellationTokenSource _abortTokenSource;

    /// <summary>
    ///     The object for checking whether stop / abort have been triggered
    /// </summary>
    public GracefulCancellationToken Token { get; private set; }

    /// <summary>
    ///     Creates a new source for issuing a <see cref="GracefulCancellationToken" /> and triggering stop/abort later on.
    /// </summary>
    public GracefulCancellationTokenSource()
    {
        _stopTokenSource = new CancellationTokenSource();
        _abortTokenSource = new CancellationTokenSource();
        Token = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);
    }

    /// <summary>
    ///     Triggers the stop flag of <see cref="Token" /> (<see cref="GracefulCancellationToken.StopToken" />
    /// </summary>
    public void Stop()
    {
        _stopTokenSource.Cancel();
    }

    /// <summary>
    ///     Triggers the abort flag of <see cref="Token" /> (<see cref="GracefulCancellationToken.AbortToken" />
    /// </summary>
    public void Abort()
    {
        _abortTokenSource.Cancel();
    }
}