// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Naming;

namespace Rdmp.Core.DataLoad.Triggers;

/// <summary>
///     Container class for constant variables for the names of special columns required by the backup trigger (_Archive
///     table and general DLE audit columns).
/// </summary>
public class SpecialFieldNames
{
    public const string ValidFrom = "hic_validFrom";
    public const string DataLoadRunID = "hic_dataLoadRunID";

    public static bool IsHicPrefixed(IHasRuntimeName col)
    {
        return IsHicPrefixed(col.GetRuntimeName());
    }

    public static bool IsHicPrefixed(string runtimeName)
    {
        return runtimeName.StartsWith("hic_", StringComparison.CurrentCultureIgnoreCase);
    }
}