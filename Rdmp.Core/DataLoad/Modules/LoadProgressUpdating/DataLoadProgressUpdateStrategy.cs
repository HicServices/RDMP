// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;

/// <summary>
///     Approaches user/system can use to determine what Date to update a LoadProgress to after a succesful data load (See
///     UpdateProgressIfLoadsuccessful for
///     description of why this is non trivial).
/// </summary>
public enum DataLoadProgressUpdateStrategy
{
    /// <summary>
    ///     Regardless of what data actually flowed through the data load, always use the maximum requested date
    /// </summary>
    UseMaxRequestedDay,

    /// <summary>
    ///     Run a piece of SQL in the RAW environment after AdjustRAW has completed to determine what the maximum date where
    ///     data was available.
    /// </summary>
    ExecuteScalarSQLInRAW,


    /// <summary>
    ///     Run a piece of SQL in the LIVE environment after the data load has completed to determine what the maximum date
    ///     where data was available.
    /// </summary>
    ExecuteScalarSQLInLIVE,

    /// <summary>
    ///     Do not update the DataLoadProgress at all after succesfully load.  This might result in repeatedly loading the same
    ///     batch of dates over and
    ///     over if you are running an IterativeScheduledDataLoadProcess
    /// </summary>
    DoNothing
}