using System;
using System.Collections.Generic;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Unit
{
    public class CacheFetchRequestProviderTests
    {
        /// <summary>
        /// Test that the fetch request created from the failure request by the provider is valid
        /// </summary>
        [Test]
        public void TestFailedFetchRequestProvider_CreationOfFetchRequest()
        {
            var failure = MockRepository.GenerateStub<ICacheFetchFailure>();
            failure.FetchRequestStart = new DateTime(2009, 8, 5, 8, 0, 0);
            failure.FetchRequestEnd = new DateTime(2009, 8, 5, 16, 0, 0);
            failure.LastAttempt = new DateTime(2016, 1, 1, 12, 0, 0);
            failure.ResolvedOn = null;

            var failures = new List<ICacheFetchFailure>
            {
                failure
            };

            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();
            cacheProgress.Stub(c => c.FetchPage(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(failures).Repeat.Once();
            var provider = new FailedCacheFetchRequestProvider(cacheProgress, 2);
            var fetchRequest = provider.GetNext(new ThrowImmediatelyDataLoadEventListener());
            Assert.IsNotNull(fetchRequest);
            Assert.AreEqual(fetchRequest.ChunkPeriod, new TimeSpan(8, 0, 0));
            Assert.AreEqual(fetchRequest.Start, failure.FetchRequestStart);
            Assert.IsTrue(fetchRequest.IsRetry);
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
                MockRepository.GenerateStub<ICacheFetchFailure>(),
                MockRepository.GenerateStub<ICacheFetchFailure>()
            };

            var failuresPage2 = new List<ICacheFetchFailure>
            {
                MockRepository.GenerateStub<ICacheFetchFailure>()
            };

            // Stub this so the 'repository' will return the first page, second page then empty page
            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();
            cacheProgress.Stub(c => c.FetchPage(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(failuresPage1).Repeat.Once();
            cacheProgress.Stub(c => c.FetchPage(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(failuresPage2).Repeat.Once();
            cacheProgress.Stub(c => c.FetchPage(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(new List<ICacheFetchFailure>()).Repeat.Once(); // last time returns empty page
            
            var provider = new FailedCacheFetchRequestProvider(cacheProgress, 2);

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
            var previousFailure = MockRepository.GenerateMock<ICacheFetchFailure>();
            previousFailure.FetchRequestEnd = DateTime.Now;
            previousFailure.FetchRequestStart = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));
            previousFailure.Expect(f => f.SaveToDatabase()).Repeat.Once();

            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();
            cacheProgress.Stub(c => c.GetPermissionWindow()).Return(MockRepository.GenerateStub<IPermissionWindow>());

            var request = new CacheFetchRequest(previousFailure, cacheProgress);
            request.RequestFailed(new Exception());

            previousFailure.VerifyAllExpectations();
        }

        /// <summary>
        /// If we construct the request with a previous failure, then Resolve should be called on it when successful
        /// </summary>
        [Test]
        public void FailedCacheFetchRequest_ResolveCalled()
        {
            var previousFailure = MockRepository.GenerateMock<ICacheFetchFailure>();
            previousFailure.FetchRequestEnd = DateTime.Now;
            previousFailure.FetchRequestStart = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));
            previousFailure.Expect(f => f.Resolve()).Repeat.Once();

            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();
            cacheProgress.Stub(c => c.GetPermissionWindow()).Return(MockRepository.GenerateStub<IPermissionWindow>());

            var request = new CacheFetchRequest(previousFailure, cacheProgress);
            request.RequestSucceeded();

            previousFailure.VerifyAllExpectations();
        }
    }
}