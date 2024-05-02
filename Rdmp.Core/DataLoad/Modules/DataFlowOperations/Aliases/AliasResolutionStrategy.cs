// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;

/// <summary>
///     Determines how a system responds to M to 1 mapping (where a patient is known by multiple different identifiers over
///     time).
/// </summary>
public enum AliasResolutionStrategy
{
    /// <summary>
    ///     If a multi mapping / alias is found for an input row then throw an exception
    /// </summary>
    CrashIfAliasesFound,

    /// <summary>
    ///     If a multi mapping / alias is found for an input row generate multiple output rows (1 for each alias)
    /// </summary>
    MultiplyInputDataRowsByAliases
}