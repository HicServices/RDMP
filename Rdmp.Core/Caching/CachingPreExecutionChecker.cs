// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Checks;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching;

/// <summary>
///     Determines whether a given CacheProgress can be run.  This includes checking if there is a data time period to
///     process, whether it is Locked, whether the classes required in the Pipeline
///     can be constructed etc.
/// </summary>
public class CachingPreExecutionChecker : ICheckable
{
    private readonly ICacheProgress _cacheProgress;

    public CachingPreExecutionChecker(ICacheProgress cacheProgress)
    {
        _cacheProgress = cacheProgress;
    }

    public void Check(ICheckNotifier notifier)
    {
        try
        {
            if (_cacheProgress.Pipeline_ID == null)
                throw new Exception($"CacheProgress {_cacheProgress.ID} doesn't have a caching pipeline!");

            IPipeline pipeline = null;
            try
            {
                pipeline = _cacheProgress.Pipeline;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Error when trying to load Pipeline ID = {_cacheProgress.Pipeline_ID.Value}", CheckResult.Fail,
                    e));
            }

            if (pipeline == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not run Pipeline checks due to previous errors",
                    CheckResult.Fail));
            }
            else
            {
                var checker = new PipelineChecker(pipeline);
                checker.Check(notifier);
            }

            if (_cacheProgress.CacheFillProgress == null && _cacheProgress.LoadProgress.OriginDate == null)
                //if we don't know what dates to request
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Both the CacheFillProgress and the LoadProgress.OriginDate are null, this means we don't know where the cache has filled up to and we don't know when the dataset is supposed to start.  This means it is impossible to know what dates to fetch",
                        CheckResult.Fail));

            if (_cacheProgress.PermissionWindow_ID != null && !_cacheProgress.PermissionWindow.WithinPermissionWindow())
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Current time is {DateTime.UtcNow} which is not a permitted time according to the configured PermissionWindow {_cacheProgress.PermissionWindow.Description} of the CacheProgress {_cacheProgress}",
                    CheckResult.Warning));

            var shortfall = _cacheProgress.GetShortfall();

            if (shortfall <= TimeSpan.Zero)
                if (_cacheProgress.CacheLagPeriod == null)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"CacheProgress reports that it has loaded up till {_cacheProgress.CacheFillProgress} which is in the future.  So we don't need to load this cache.",
                            CheckResult.Warning));
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"CacheProgress reports that it has loaded up till {_cacheProgress.CacheFillProgress} but there is a lag period of {_cacheProgress.CacheLagPeriod} which means we are not due to load any cached data yet.",
                            CheckResult.Warning));

            var factory = new CachingPipelineUseCase(_cacheProgress);
            IDataFlowPipelineEngine engine = null;
            try
            {
                engine = factory.GetEngine(new FromCheckNotifierToDataLoadEventListener(notifier));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not create IDataFlowPipelineEngine",
                    CheckResult.Fail, e));
            }

            engine?.Check(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Entire checking process for cache progress {_cacheProgress} crashed, see Exception for details",
                    CheckResult.Fail, e));
        }
    }
}