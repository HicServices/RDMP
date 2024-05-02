// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;

namespace Rdmp.Core.DataFlowPipeline;

/// <summary>
///     Wrapper for two System.Threading.CancellationTokens.  One for Stopping (please stop when you are finished with the
///     current job) and one for Aborting (please stop
///     as soon as possible).  If you like you can just specify the same token twice and nobody will be any the wiser.
///     Remember that System.Threading.CancellationToken is
///     for checking if cancellation is requested, in order to create and trigger cancellation you need a
///     System.Threading.CancellationTokenSource handily you can use
///     GracefulCancellationTokenSource to do that and then just reference .Token property.
/// </summary>
public class GracefulCancellationToken
{
    /// <summary>
    ///     CancellationToken for stopping where convenient (e.g. at the end of a data load)
    /// </summary>
    public CancellationToken StopToken { get; }

    /// <summary>
    ///     CancellationToken for stopping as soon as possible
    /// </summary>
    public CancellationToken AbortToken { get; }

    /// <summary>
    ///     Creates a new <see cref="GracefulCancellationToken" /> that will never be cancelled
    /// </summary>
    public GracefulCancellationToken() : this(default, default)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="GracefulCancellationToken" /> using the provided stop and abort tokens.  You can pass
    ///     the same CancellationToken for both parameters if you only want to support abort.
    /// </summary>
    /// <param name="stopToken"></param>
    /// <param name="abortToken"></param>
    public GracefulCancellationToken(CancellationToken stopToken, CancellationToken abortToken)
    {
        AbortToken = abortToken;
        StopToken = stopToken;
    }

    /// <summary>
    ///     True if <see cref="AbortToken" /> has been set
    /// </summary>
    public bool IsAbortRequested => AbortToken.IsCancellationRequested;

    /// <summary>
    ///     True if <see cref="StopToken" /> has been set
    /// </summary>
    public bool IsStopRequested => StopToken.IsCancellationRequested;

    /// <summary>
    ///     True if either <see cref="AbortToken" /> or <see cref="StopToken" /> has been set
    /// </summary>
    public bool IsCancellationRequested => IsAbortRequested || IsStopRequested;

    /// <summary>
    ///     Throws OperationCanceledException if either <see cref="AbortToken" /> or <see cref="StopToken" /> has been set
    /// </summary>
    public void ThrowIfCancellationRequested()
    {
        ThrowIfAbortRequested();
        ThrowIfStopRequested();
    }

    /// <summary>
    ///     Throws OperationCanceledException if <see cref="AbortToken" /> has been set
    /// </summary>
    public void ThrowIfAbortRequested()
    {
        AbortToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    ///     Throws OperationCanceledException if <see cref="StopToken" /> has been set
    /// </summary>
    public void ThrowIfStopRequested()
    {
        StopToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    ///     Creates a single CancellationTokenSource which will be triggered if either <see cref="StopToken" /> or
    ///     <see cref="AbortToken" /> is set
    /// </summary>
    /// <returns></returns>
    public CancellationTokenSource CreateLinkedSource()
    {
        return CancellationTokenSource.CreateLinkedTokenSource(StopToken, AbortToken);
    }
}