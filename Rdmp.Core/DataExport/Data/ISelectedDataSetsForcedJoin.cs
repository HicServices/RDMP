// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
/// Specifies that the given data export dataset in configuration x (SelectedDataSets) should include a mandatory join on the table TableInfo regardless of
/// what columns are selected in the extraction query.  The most common use case for this is to extract a dataset with WhereSQL that references a custom data
/// table e.g. 'Questionnaire answer x > 5'.  In that scenario the <see cref="TableInfo"/> would be the 'Project Specific Catalogue' dataset 'Questionnaire'
/// and the <see cref="ISelectedDataSets"/> would be the dataset you were extracting in your study e.g. 'biochemistry'.
/// 
/// <para>A <see cref="JoinInfo"/> must still exist to tell <see cref="QueryBuilding.QueryBuilder"/> how to write the Join section of the query.</para>
/// </summary>
public interface ISelectedDataSetsForcedJoin : IMapsDirectlyToDatabaseTable
{
    /// <summary>
    /// The dataset in an <see cref="IExtractionConfiguration"/> which should always be joined against the referred <see cref="TableInfo_ID"/>
    /// </summary>
    int SelectedDataSets_ID { get; }

    /// <summary>
    /// The <see cref="ITableInfo"/> which should always be joined with when extracting the given <see cref="SelectedDataSets_ID"/>
    /// </summary>
    int TableInfo_ID { get; }

    /// <inheritdoc cref="TableInfo_ID"/>
    TableInfo TableInfo { get; }
}