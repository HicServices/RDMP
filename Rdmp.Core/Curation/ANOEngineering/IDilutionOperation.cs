// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.Checks;
using TypeGuesser;

namespace Rdmp.Core.Curation.ANOEngineering;

/// <summary>
///     Describes a way of anonymising a field (ColumnToDilute) by dilution (making data less granular) e.g. rounding dates
///     to the nearest quarter.  Implementation
///     must be based on running an SQL query in AdjustStaging.  See Dilution for more information.
/// </summary>
public interface IDilutionOperation : ICheckable
{
    IPreLoadDiscardedColumn ColumnToDilute { set; }
    string GetMutilationSql(INameDatabasesAndTablesDuringLoads namer);
    DatabaseTypeRequest ExpectedDestinationType { get; }
}