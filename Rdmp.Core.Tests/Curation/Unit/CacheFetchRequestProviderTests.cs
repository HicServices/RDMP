// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using ReusableLibraryCode.Progress;

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
        var failure = Mock.Of<ICacheFetchFailure>();
        failure.FetchRequestStart = new DateTime(2009, 8, 5, 8, 0, 0);
        failure.FetchRequestEnd = new DateTime(2009, 8, 5, 16, 0, 0);
        failure.LastAttempt = new DateTime(2016, 1, 1, 12, 0, 0);
        failure.ResolvedOn = null;

        var failures = new List<ICacheFetchFailure>
        {
            failure
        };

        var cacheProgress = new Mock<ICacheProgress>();
        cacheProgress.Setup(c => c.FetchPage(It.IsAny<int>(), It.IsAny<int>())).Returns(failures);

        var provider = new FailedCacheFetchRequestProvider(cacheProgress.Object, 2);
        var fetchRequest = provider.GetNext(new ThrowImmediatelyDataLoadEventListener());
        Assert.IsNotNull(fetchRequest);
        Assert.AreEqual(fetchRequest.ChunkPeriod, new TimeSpan(8, 0, 0));
        Assert.AreEqual(fetchRequest.Start, failure.FetchRequestStart);
        Assert.IsTrue(fetchRequest.IsRetry);
        cacheProgress.Verify();
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
            Mock.Of<ICacheFetchFailure>(),
            Mock.Of<ICacheFetchFailure>()
        };

        var failuresPage2 = new List<ICacheFetchFailure>
        {
            Mock.Of<ICacheFetchFailure>()
        };

        // Stub this so the 'repository' will return the first page, second page then empty page
        var cacheProgress = new Mock<ICacheProgress>();
        cacheProgress.SetupSequence<IEnumerable<ICacheFetchFailure>>(c => c.FetchPage(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(failuresPage1)
            .Returns(failuresPage2)
            .Returns(new List<ICacheFetchFailure>())
            .Throws<InvalidOperationException>();
                
            
        var provider = new FailedCacheFetchRequestProvider(cacheProgress.Object, 2);

        // We should get three ICacheFetchRequests in total, followed by a null to signify that there are no more ICacheFetchRequests
        Assert.IsNotNull(provider.GetNext(new ThrowImmediatelyDataLoadEventListener()));
        Assert.IsNotNull(provider.GetNext(new ThrowImmediatelyDataLoadEventListener()));
        Assert.IsNotNull(provider.GetNext(new ThrowImmediatelyDataLoadEventListener()));
        Assert.IsNull(provider.GetNext(new ThrowImmediatelyDataLoadEventListener()));
    }

    /// <summary>
    /// If we construct the request with a previous failure, then there should be a save operation when the updated failure is persisted to the database
    /// </summary>
    [Test]
    public void FailedCacheFetchRequest_SavesPreviousFailure()
    {
        var previousFailure = GetFailureMock();

        var cacheProgress = Mock.Of<ICacheProgress>(c => c.PermissionWindow==Mock.Of<IPermissionWindow>());

        var request = new CacheFetchRequest(previousFailure.Object, cacheProgress);
        request.RequestFailed(new Exception());

        previousFailure.Verify();
    }

    /// <summary>
    /// If we construct the request with a previous failure, then Resolve should be called on it when successful
    /// </summary>
    [Test]
    public void FailedCacheFetchRequest_ResolveCalled()
    {
        var previousFailure = GetFailureMock();

        var cacheProgress = Mock.Of<ICacheProgress>(c => c.PermissionWindow==Mock.Of<IPermissionWindow>());

        var request = new CacheFetchRequest(previousFailure.Object, cacheProgress);
        request.RequestSucceeded();

        previousFailure.Verify();
    }

    private Mock<ICacheFetchFailure> GetFailureMock()
    {
        var failure = Mock.Of<ICacheFetchFailure>(f=>
            f.FetchRequestEnd == DateTime.Now &&
            f.FetchRequestStart == DateTime.Now.Subtract(new TimeSpan(1, 0, 0)));
            
        return Mock.Get(failure);
    }
}