// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     Conditions under which a PrematureLoadEnder should decide to end the ongoing load early
/// </summary>
public enum PrematureLoadEndCondition
{
    /// <summary>
    ///     As soon as the PrematureLoadEnder is hit the load should be stopped
    /// </summary>
    Always,

    /// <summary>
    ///     Stop the load if there are no records in any database tables in the current stage (e.g. if PrematureLoadEnder is at
    ///     AdjustRAW stage then the
    ///     load will end if there are no records in any tables in RAW).
    /// </summary>
    NoRecordsInAnyTablesInDatabase,

    /// <summary>
    ///     Stop the load if there are no files in the ForLoading directory of the current load when the PrematureLoadEnder
    ///     component is hit
    /// </summary>
    NoFilesInForLoading
}