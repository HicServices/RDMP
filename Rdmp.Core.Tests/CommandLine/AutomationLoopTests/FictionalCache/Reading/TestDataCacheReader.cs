// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests.FictionalCache.Reading
{
    public class TestDataCacheReader : ICachedDataProvider
    {
        public ILoadProgress LoadProgress { get; set; }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
        {
            throw new NotImplementedException();
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public CacheArchiveType CacheArchiveType { get; set; }
        public string CacheDateFormat { get; set; }
        public Type CacheLayoutType { get; set; }
        public ILoadCachePathResolver CreateResolver(ILoadProgress loadProgress)
        {
            throw new NotImplementedException();
        }

    }
}
