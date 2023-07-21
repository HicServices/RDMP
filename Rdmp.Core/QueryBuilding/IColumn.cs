// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Naming;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Interface for defining classes which store a single line of SELECT Sql for use in query building (See
///     ISqlQueryBuilder).  This includes basic stuff like SelectSQL
///     and Alias but also logical things like Order (which column order it should appear in the select statement being
///     built).
///     <para>Note that many properties can be null including ColumnInfo and Alias etc.</para>
/// </summary>
public interface IColumn : IHasRuntimeName, ICheckable, IOrderable, IMapsDirectlyToDatabaseTable
{
    /// <summary>
    ///     Gets the underlying <see cref="ColumnInfo" /> behind this line of SELECT SQL.
    /// </summary>
    ColumnInfo ColumnInfo { get; }

    /// <summary>
    ///     The single line of SQL that should be executed in a SELECT statement built by an <see cref="ISqlQueryBuilder" />
    ///     <para>This may just be the fully qualified column name verbatim or it could be a transform</para>
    ///     <para>This does not include the <see cref="Alias" /> section of the SELECT line e.g. " AS MyTransform"</para>
    /// </summary>
    [Sql]
    string SelectSQL { get; set; }

    /// <summary>
    ///     The alias (if any) for the column when it is included in a SELECT statement.  This should not include the " AS "
    ///     bit only the text that would come after.
    ///     <para>Only use if the <see cref="SelectSQL" /> is a transform e.g. "UPPER([mydb]..[mytbl].[mycol])" </para>
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     True if the <see cref="ColumnInfo" /> should be wrapped with a standard hashing algorithmn (e.g. MD5) when
    ///     extracted to researchers in a data extract.
    ///     <para>Hashing algorithmn must be defined in data export database</para>
    /// </summary>
    bool HashOnDataRelease { get; }

    /// <summary>
    ///     Indicates whether this column holds patient identifiers which can be used for cohort creation and which must be
    ///     substituted for anonymous release
    ///     identifiers on data extraction (to a researcher).
    /// </summary>
    bool IsExtractionIdentifier { get; }

    /// <summary>
    ///     Indicates whether this column is the Primary Key (or part of a composite Primary Key) when extracted.  This flag is
    ///     not copied / imputed from
    ///     <see cref="Curation.Data.ColumnInfo.IsPrimaryKey" /> because primary keys can often contain sensitive information
    ///     (e.g. lab number) and
    ///     you may have a transform or hash configured or your <see cref="Catalogue" /> may involve joining multiple
    ///     <see cref="TableInfo" /> together.
    /// </summary>
    bool IsPrimaryKey { get; }
}