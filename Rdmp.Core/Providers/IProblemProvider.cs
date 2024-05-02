// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Providers;

/// <summary>
///     Identifies problems with objects held in an ICoreChildProvider e.g. Projects missing cohorts, orphan
///     ExtractionInformations etc.  This class
///     differs from ICheckable etc because it is designed to identify and record a large number of problems very quickly
///     among a large number of
///     objects and then later report about the problems e.g. when rendering a UI.
/// </summary>
public interface IProblemProvider
{
    /// <summary>
    ///     Finds all the problems with all relevant objects known about by the child provider (Stored results are returned
    ///     through
    ///     HasProblem and DescribeProblem.
    /// </summary>
    /// <param name="childProvider"></param>
    void RefreshProblems(ICoreChildProvider childProvider);

    /// <summary>
    ///     True if the supplied object has problems with it
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    bool HasProblem(object o);

    /// <summary>
    ///     Returns the problem with object o or null if there are no problems
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    string DescribeProblem(object o);
}