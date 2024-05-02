// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.Mutilators;

/// <summary>
///     A user configurable component which will run during Data Load Engine execution and result in the modification of an
///     existing table in of the load
///     stages (RAW, STAGING or LIVE).  For example a PrimaryKeyCollisionResolverMutilation will delete records out of the
///     table such that the Primary Key
///     is unique (based on a column preference order).
/// </summary>
public interface IMutilateDataTables : ICheckable, IDisposeAfterDataLoad
{
    /// <summary>
    ///     Called after construction to tell you where you will be running.  Note that at Checks time this might not exist yet
    ///     (if you are in RAW/STAGING)
    /// </summary>
    /// <param name="dbInfo"></param>
    /// <param name="loadStage"></param>
    void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage);

    ExitCodeType Mutilate(IDataLoadJob job);
}