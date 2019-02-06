// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See SelectedDataSets
    /// </summary>
    public interface ISelectedDataSets:IDeleteable,IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; set; }
        int? RootFilterContainer_ID { get; set; }
        IContainer RootFilterContainer { get; }

        IExtractionConfiguration ExtractionConfiguration { get;}
        IExtractableDataSet ExtractableDataSet { get; }
        ISelectedDataSetsForcedJoin[] SelectedDataSetsForcedJoins { get;}


        /// <summary>
        /// If this dataset has been extracted in the past this will return the last extract audit record.  Otherwise it will return null
        /// </summary>
        /// <returns></returns>
        ICumulativeExtractionResults GetCumulativeExtractionResultsIfAny();
    }
}