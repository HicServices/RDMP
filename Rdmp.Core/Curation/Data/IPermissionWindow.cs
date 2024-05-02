// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes a period of time in which a given act can take place (e.g. only cache data from the MRI imaging web
///     service during the hours of 11pm - 5am so as not to
///     disrupt routine hospital use).  Also serves as a Locking point for job control.  Once an IPermissionWindow is in
///     use by a process (e.g. Caching Pipeline) then it
///     is not available to other processes (e.g. loading or other caching pipelines that share the same
///     IPermissionWindow).
/// </summary>
public interface IPermissionWindow : INamed
{
    /// <summary>
    ///     Human readable description of the period of time described e.g. 'weekday nights only because we don't want to hit
    ///     local server backup periods'
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     Obsolete
    /// </summary>
    [Obsolete("NotUsed")]
    bool RequiresSynchronousAccess { get; set; }

    /// <summary>
    ///     All caching activities which are restricted to running in this time window
    /// </summary>
    IEnumerable<ICacheProgress> CacheProgresses { get; }

    /// <summary>
    ///     The time windows that the activity is allowed in
    /// </summary>
    List<PermissionWindowPeriod> PermissionWindowPeriods { get; }

    /// <summary>
    ///     Returns true if the current time is within one of the <see cref="PermissionWindowPeriods" />
    /// </summary>
    /// <returns></returns>
    bool WithinPermissionWindow();

    /// <summary>
    ///     Sets the time of day that activities are permitted in
    /// </summary>
    /// <param name="windowPeriods"></param>
    void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods);
}