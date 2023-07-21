// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataQualityEngine.Data;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

/// <summary>
/// Interface for all UI charts that depict DQE results in <see cref="CatalogueDQEResultsUI"/>
/// </summary>
public interface IDataQualityReportingChart
{
    /// <summary>
    /// Clears the currently shown results in the UI
    /// </summary>
    void ClearGraph();

    /// <summary>
    /// Updates the currently shown results to depict those gathered during the <paramref name="evaluation"/>
    /// </summary>
    /// <param name="evaluation"></param>
    /// <param name="pivotCategoryValue">The pivot value within the results to show or "ALL" for all records gathered</param>
    void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue);
}