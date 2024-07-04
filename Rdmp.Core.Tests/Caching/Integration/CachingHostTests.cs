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
using NUnit.Framework;
using Rdmp.Core.Caching;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.Caching.Integration;

public class CachingHostTests : UnitTests
{
    /// <summary>
    /// Makes sure that a cache progress pipeline will not be run if we are outside the permission window
    /// </summary>
    [Test]
    public void CacheHostOutwithPermissionWindow()
    {
        var rootDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var testDir = rootDir.CreateSubdirectory("C");

        if (testDir.Exists)
            Directory.Delete(testDir.FullName, true);

        var cp = WhenIHaveA<CacheProgress>();
        var loadMetadata = cp.LoadProgress.LoadMetadata;

        var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "Test", false, loadMetadata);

        // This feels a bit nasty, but quick and much better than having the test wait for an arbitrary time period.
        var listener = new ExpectedNotificationListener("Download not permitted at this time, sleeping for 60 seconds");

        cp.CacheFillProgress = DateTime.Now.AddDays(-1);
        cp.PermissionWindow_ID = 1;

        var permissionWindow = new PermissionWindow(Repository)
        {
            RequiresSynchronousAccess = true,
            ID = 1,
            Name = "Test Permission Window"
        };


        //Create a time period that we are outwith (1 hour ago to 30 minutes ago).
        var start = DateTime.Now.TimeOfDay.Subtract(new TimeSpan(0, 1, 0, 0));
        var stop = DateTime.Now.TimeOfDay.Subtract(new TimeSpan(0, 0, 30, 0));
        permissionWindow.SetPermissionWindowPeriods(new List<PermissionWindowPeriod>(new[]
        {
            new PermissionWindowPeriod(
                (int)DateTime.Now.DayOfWeek,
                start,
                stop)
        }));
        permissionWindow.SaveToDatabase();

        cp.PermissionWindow_ID = permissionWindow.ID;
        cp.SaveToDatabase();

        // set SetUp a factory stub to return our engine mock
        var cacheHost = new CachingHost(Repository)
        {
            CacheProgress = cp
        };

        var stopTokenSource = new CancellationTokenSource();
        var abortTokenSource = new CancellationTokenSource();
        var cancellationToken = new GracefulCancellationToken(stopTokenSource.Token, abortTokenSource.Token);

        var task = Task.Run(() => cacheHost.Start(listener, cancellationToken),
            cancellationToken.CreateLinkedSource().Token);

        // Don't want to cancel before the DownloadUntilFinished loop starts and we receive the first "Download not permitted at this time, sleeping for 60 seconds" message
        listener.ReceivedMessage += abortTokenSource.Cancel;

        try
        {
            task.Wait();
        }
        catch (AggregateException e)
        {
            Assert.That(e.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(e.InnerExceptions[0], Is.InstanceOf(typeof(TaskCanceledException)), e.InnerExceptions[0].Message);
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
        handler?.Invoke();
    }

    public ExpectedNotificationListener(string expectedNotificationString)
    {
        _expectedNotificationString = expectedNotificationString;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        Console.WriteLine($"{sender} sent message: {e.Message}");

        if (e.Message.Equals(_expectedNotificationString))
            OnReceivedMessage();
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        throw new NotImplementedException();
    }
}