// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Naming;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Interface for an object that has a name that varies depending on which stage of a data load you are attempting to
///     reference.  For example TableInfo will have a
///     different name depending on whether you are addressing the live table or the table as it exists in during the
///     AdjustStaging during a data load.  Likewise an
///     anonymised ColumnInfo will have a different name in the live stage (e.g. ANOLabNumber) vs the raw stage (e.g.
///     LabNumber - column prior to anonymisation).
///     <para>See also IHasRuntimeName</para>
/// </summary>
public interface IHasStageSpecificRuntimeName
{
    /// <summary>
    ///     Returns the runtime name (unqualified name e.g. "MyColumn" ) for the column/table at the given stage of a data load
    ///     (RAW=>STAGING=>LIVE)
    ///     <seealso cref="IHasRuntimeName.GetRuntimeName" />
    /// </summary>
    /// <param name="stage"></param>
    /// <returns></returns>
    string GetRuntimeName(LoadStage stage);
}