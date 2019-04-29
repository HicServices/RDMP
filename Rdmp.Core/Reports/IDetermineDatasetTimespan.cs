// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CatalogueLibrary.Data;

namespace Rdmp.Core.Reports
{
    /// <summary>
    /// A class that can determine the actual dataset span of a given catalogue e.g. one strategy might be to have the user enter manually the start and end date.  A better
    /// strategy would be to consult the latest Data Quality Engine results to see what the realistic start/end dates are (e.g. discarding outliers / future dates etc)
    /// </summary>
    public interface IDetermineDatasetTimespan
    {
        /// <summary>
        /// Summarises the range of data in the tables that underly the <paramref name="catalogue"/> if known (e.g. based on the last recorded DQE results).
        /// </summary>
        /// <param name="catalogue"></param>
        /// <param name="discardOutliers">True to attempt to throw out outlier rows when determining the dataset timespan</param>
        /// <returns></returns>
        string GetHumanReadableTimepsanIfKnownOf(Catalogue catalogue, bool discardOutliers);
    }
}