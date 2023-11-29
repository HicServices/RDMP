// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class CacheFetchRequestProviderTests
{
    /// <summary>
    /// Test that the fetch request created from the failure request by the provider is valid
    /// </summary>
    [Test]
    public void TestFailedFetchRequestProvider_CreationOfFetchRequest()
    {
        var failure = Substitute.For<ICacheFetchFailure>();
        failure.FetchRequestStart = new DateTime(2009, 8, 5, 8, 0, 0);
        failure.FetchRequestEnd = new DateTime(2009, 8, 5, 16, 0, 0);
        failure.LastAttempt = new DateTime(2016, 1, 1, 12, 0, 0);
        failure.ResolvedOn = null;

        var failures = new List<ICacheFetchFailure>
        {
            failure
        };

        var cacheProgress = Substitute.For<ICacheProgress>();
        cacheProgress.FetchPage(Arg.Any<int>(), Arg.Any<int>()).Returns(failures);

        var provider = new FailedCacheFetchRequestProvider(cacheProgress, 2);
        var fetchRequest = provider.GetNext(ThrowImmediatelyDataLoadEventListener.Quiet);
        Assert.That(fetchRequest, Is.Not.Null);
        Assert.That(new TimeSpan(8, 0, 0), Is.EqualTo(fetchRequest.ChunkPeriod));
        Assert.That(failure.FetchRequestStart, Is.EqualTo(fetchRequest.Start));
        Assert.That(fetchRequest.IsRetry);
        cacheProgress.Received(1);
    }

    /// <summary>
    /// Test that the provider iterates through multiple batches of data retrieved from a repository correctly
    /// </summary>
    [Test]
    public void TestFailedFetchRequestProvider_MultiplePages()
    {
        // Our set of CacheFetchFailures
        var failuresPage1 = new List<ICacheFetchFailure>
        {
            Substitute.For<ICacheFetchFailure>(),
            Substitute.For<ICacheFetchFailure>()
        };

        var failuresPage2 = new List<ICacheFetchFailure>
        {
            Substitute.For<ICacheFetchFailure>()
        };

        // Stub this so the 'repository' will return the first page, second page then empty page
        var cacheProgress = Substitute.For<ICacheProgress>();
        // cacheProgress.SetupSequence<IEnumerable<ICacheFetchFailure>>();
        cacheProgress.FetchPage(Arg.Any<int>(), Arg.Any<int>())
            .Returns(failuresPage1,
            failuresPage2,
            new List<ICacheFetchFailure>());//, x => { throw new InvalidOperationException(); });


        var provider = new FailedCacheFetchRequestProvider(cacheProgress, 2);

        // We should get three ICacheFetchRequests in total, followed by a null to signify that there are no more ICacheFetchRequests
        Assert.That(provider.GetNext(ThrowImmediatelyDataLoadEventListener.Quiet), Is.Not.Null);
        Assert.That(provider.GetNext(ThrowImmediatelyDataLoadEventListener.Quiet), Is.Not.Null);
        Assert.That(provider.GetNext(ThrowImmediatelyDataLoadEventListener.Quiet), Is.Not.Null);
        Assert.That(provider.GetNext(ThrowImmediatelyDataLoadEventListener.Quiet), Is.Null);
    }

    /// <summary>
    /// If we construct the request with a previous failure, then there should be a save operation when the updated failure is persisted to the database
    /// </summary>
    [Test]
    public void FailedCacheFetchRequest_SavesPreviousFailure()
    {
        var previousFailure = GetFailureMock();

        var cacheProgress = Substitute.For<ICacheProgress>();
        cacheProgress.PermissionWindow.Returns(Substitute.For<IPermissionWindow>());

        var request = new CacheFetchRequest(previousFailure, cacheProgress);
        request.RequestFailed(new Exception());

        previousFailure.Received(1);
    }

    /// <summary>
    /// If we construct the request with a previous failure, then Resolve should be called on it when successful
    /// </summary>
    [Test]
    public void FailedCacheFetchRequest_ResolveCalled()
    {
        var previousFailure = GetFailureMock();

        var cacheProgress = Substitute.For<ICacheProgress>();
        cacheProgress.PermissionWindow.Returns(Substitute.For<IPermissionWindow>());

        var request = new CacheFetchRequest(previousFailure, cacheProgress);
        request.RequestSucceeded();

        previousFailure.Received(1);
    }

    private static ICacheFetchFailure GetFailureMock()
    {
        var failure = Substitute.For<ICacheFetchFailure>();
        failure.FetchRequestEnd.Returns(DateTime.Now);
        failure.FetchRequestStart.Returns(DateTime.Now.Subtract(new TimeSpan(1, 0, 0)));

        return failure;
    }
}