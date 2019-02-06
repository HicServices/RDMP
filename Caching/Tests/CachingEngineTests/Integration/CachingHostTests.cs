// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace CachingEngineTests.Integration
{
    [Category("Integration")]
    public class CachingHostTests : DatabaseTests
    {

        private ICacheProgress CreateCacheProgressStubWithUnlockedLoadProgress()
        {
            var cacheProgress = MockRepository.GenerateStub<ICacheProgress>();

            var loadProgress = MockRepository.GenerateStub<ILoadProgress>();
            loadProgress.IsDisabled = false;

            cacheProgress.Stub(progress => progress.LoadProgress).Return(loadProgress);

            return cacheProgress;
        }


        /// <summary>
        /// Makes sure that a cache progress pipeline will not be run if we are outside the permission window
        /// </summary>
        [Test]
        public void CacheHostOutwithPermissionWindow()
        {
            var rootDir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var testDir = rootDir.CreateSubdirectory("C");

            if (testDir.Exists)
                Directory.Delete(testDir.FullName, true);

            var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(testDir, "Test");
            var loadMetadata = MockRepository.GenerateStub<ILoadMetadata>();
            loadMetadata.LocationOfFlatFiles = hicProjectDirectory.RootPath.FullName;

            // This feels a bit nasty, but quick and much better than having the test wait for an arbitrary time period.
            var listener = new ExpectedNotificationListener("Download not permitted at this time, sleeping for 60 seconds");

            var cacheProgress = CreateCacheProgressStubWithUnlockedLoadProgress();
            cacheProgress.CacheFillProgress = DateTime.Now.AddDays(-1);
            cacheProgress.PermissionWindow_ID = 1;
            cacheProgress.LoadProgress.Stub(schedule => schedule.LoadMetadata).Return(loadMetadata);

            var permissionWindow = MockRepository.GenerateStub<IPermissionWindow>();
            permissionWindow.RequiresSynchronousAccess = true;
            permissionWindow.ID = 1;
            permissionWindow.Name = "Test Permission Window";
            permissionWindow.Stub(window => window.WithinPermissionWindow()).Return(false);

            cacheProgress.Stub(progress => progress.PermissionWindow).Return(permissionWindow);

            var dataFlowPipelineEngine = MockRepository.GenerateMock<IDataFlowPipelineEngine>();

            // set up a factory stub to return our engine mock
            var cacheHost = new CachingHost(CatalogueRepository)
            {
                CacheProgressList = new List<ICacheProgress> { cacheProgress }
            };

            var stopTokenSource = new CancellationTokenSource();
            var abortTokenSource = new CancellationTokenSource();
            var cancellationToken = new GracefulCancellationToken(stopTokenSource.Token, abortTokenSource.Token);

            var task = Task.Run(() => cacheHost.Start(listener, cancellationToken), cancellationToken.CreateLinkedSource().Token);

            // Don't want to cancel before the DownloadUntilFinished loop starts and we receive the first "Download not permitted at this time, sleeping for 60 seconds" message
            listener.ReceivedMessage += abortTokenSource.Cancel;

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(1, e.InnerExceptions.Count);
                Assert.IsInstanceOf(typeof (TaskCanceledException), e.InnerExceptions[0], e.InnerExceptions[0].Message);

                dataFlowPipelineEngine.AssertWasCalled(engine => engine.ExecutePipeline(cancellationToken), options => options.Repeat.Times(0));
            }
            finally
            {
                testDir.Delete(true);
            }
        }


    }

    internal delegate void ReceivedMessageHandler();
    internal class ExpectedNotificationListener : IDataLoadEventListener
    {
        private readonly string _expectedNotificationString;
        public event ReceivedMessageHandler ReceivedMessage;

        protected virtual void OnReceivedMessage()
        {
            var handler = ReceivedMessage;
            if (handler != null) handler();
        }

        public ExpectedNotificationListener(string expectedNotificationString)
        {
            _expectedNotificationString = expectedNotificationString;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            Console.WriteLine(sender + " sent message: " + e.Message);

            if (e.Message.Equals(_expectedNotificationString))
                OnReceivedMessage();
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}