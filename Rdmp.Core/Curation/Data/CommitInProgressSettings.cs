// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes what to track and other configuration settings of <see cref="CommitInProgress" />
/// </summary>
public class CommitInProgressSettings
{
    /// <summary>
    ///     Set to true to detect ANY object creation or deletion that occurs on any of the
    ///     <see cref="IRepository" /> in the current application scope.  All new/deleted objects
    ///     during the lifteime of this object will then be included in any <see cref="Commit" />
    ///     created.  Do not use this for long lifetime <see cref="CommitInProgress" /> e.g. one
    ///     associated with an open tab.
    /// </summary>
    public bool TrackInsertsAndDeletes { get; }

    /// <summary>
    ///     Optional default description to use when describing the purpose of this commit.
    ///     If code is interactive then user may expand on the description or replace it entirely.
    ///     If null then a suitable description is generated based on objects/properties changed
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Set to true to use <see cref="IRepository.BeginNewTransaction" /> to allow cancellation unwinding.
    ///     DO NOT use this if you intend for <see cref="CommitInProgress" /> to lie around for a long time
    ///     e.g. in a user interface tab
    /// </summary>
    public bool UseTransactions { get; init; }

    /// <summary>
    ///     All initially known objects you want to detect changes in.  Note that depending on settings
    ///     this may be expanded to include new objects added/deleted during the lifetime of the
    ///     <see cref="CommitInProgress" />.
    /// </summary>
    public IMapsDirectlyToDatabaseTable[] ObjectsToTrack { get; }

    public CommitInProgressSettings(params IMapsDirectlyToDatabaseTable[] objectsToTrack)
    {
        ObjectsToTrack = objectsToTrack;
    }
}