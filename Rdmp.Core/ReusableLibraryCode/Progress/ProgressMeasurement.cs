// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     Part of Event args for IDataLoadEventListener.OnProgress events.  Records how far through the operation the
///     ProgressEventArgs is (how many records have
///     been processed / how many kilobytes have been written etc).  You can include a knownTargetValue if you know how
///     many records etc you need to process in
///     total or you can just keep incrementing the count without knowing the goal number.
/// </summary>
public class ProgressMeasurement
{
    public int Value { get; set; }
    public ProgressType UnitOfMeasurement { get; set; }
    public int KnownTargetValue { get; set; }

    public ProgressMeasurement(int value, ProgressType unit)
    {
        Value = value;
        UnitOfMeasurement = unit;
    }

    public ProgressMeasurement(int value, ProgressType unit, int knownTargetValue)
    {
        Value = value;
        UnitOfMeasurement = unit;
        KnownTargetValue = knownTargetValue;
    }
}