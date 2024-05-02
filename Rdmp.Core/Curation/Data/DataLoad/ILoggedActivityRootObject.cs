// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using FAnsi.Discovery;
using Rdmp.Core.Logging.PastEvents;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Interface for classes who can identify a specific subsection of logs (see <see cref="Logging.LogManager" />) which
///     relates specifically to them
/// </summary>
public interface ILoggedActivityRootObject
{
    /// <summary>
    ///     Returns the server on which logging takes place into
    /// </summary>
    /// <returns></returns>
    DiscoveredServer GetDistinctLoggingDatabase();

    /// <summary>
    ///     Returns the server on which the logging takes place into and the rdmp reference to a specific database on it with
    ///     the logs in
    /// </summary>
    /// <param name="serverChosen"></param>
    /// <returns></returns>
    DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen);

    /// <summary>
    ///     Returns the unique task under which logs are captured for this object.  Note that this object may claim only a
    ///     subset of the runs logged under this task (see
    ///     <see cref="FilterRuns(System.Collections.Generic.IEnumerable{Rdmp.Core.Logging.PastEvents.ArchivalDataLoadInfo})" />
    ///     )
    /// </summary>
    /// <returns></returns>
    string GetDistinctLoggingTask();

    /// <summary>
    ///     Returns the runs under the <see cref="GetDistinctLoggingTask" /> that relate specifically to this object
    /// </summary>
    /// <param name="runs"></param>
    /// <returns></returns>
    IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs);
}