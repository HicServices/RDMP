// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Triggers;

/// <summary>
///     Handles the creation of _Archive tables and the trigger that populates them (on UPDATE) of the live data tables.
///     Also creates <see cref="SpecialFieldNames" /> fields
///     in the live table to allow versioning of records created by the DLE.
/// </summary>
public interface ITriggerImplementer
{
    /// <summary>
    ///     Deletes the backup trigger if any on the table the <see cref="ITriggerImplementer" /> is pointed at
    /// </summary>
    /// <param name="problemsDroppingTrigger"></param>
    /// <param name="thingsThatWorkedDroppingTrigger"></param>
    void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger);

    /// <summary>
    ///     Creates the backup trigger and associated _Archive table.  If there is already an _Archive table in place then only
    ///     the trigger
    ///     should be created
    /// </summary>
    /// <param name="notifier"></param>
    /// <returns></returns>
    string CreateTrigger(ICheckNotifier notifier);

    /// <summary>
    ///     Describes whether or not the table that the <see cref="ITriggerImplementer" /> is pointed at has an intact UPDATE
    ///     trigger that populates
    ///     a _Archive table.
    /// </summary>
    /// <returns></returns>
    TriggerStatus GetTriggerStatus();

    /// <summary>
    ///     Confirms firstly that there is a trigger on the table (See <see cref="GetTriggerStatus" />) Then looks at the
    ///     trigger method body to confirm
    ///     that the currently emplaced trigger matches the expected SQL if the trigger were to be created today.
    ///     <para>
    ///         This method helps reduce errors where the user updates the live table schema and does not also update the
    ///         _Archive table.
    ///     </para>
    /// </summary>
    /// <returns></returns>
    bool CheckUpdateTriggerIsEnabledAndHasExpectedBody();
}