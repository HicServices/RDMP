// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Class for persisting the Comment, type and value of an Sql Parameter (e.g. /*mycool variable*/ DECLARE @bob as
///     Varchar(10); Set @bob = 'fish').  RDMP supports
///     parameter overriding and merging duplicate parameters etc during query building (See ParameterManager).
/// </summary>
public interface ISqlParameter : ISaveable, IHasQuerySyntaxHelper, ICheckable
{
    /// <summary>
    ///     The name only of the parameter e.g. @bob, this should be automatically calculated from the ParameterSQL to avoid
    ///     any potential for mismatch
    /// </summary>
    string ParameterName { get; }

    /// <summary>
    ///     The full SQL declaration for the parameter e.g. 'DECLARE @bob as Varchar(10);'.  This must include the pattern
    ///     @something even if the SQL language does not
    ///     require declaration (e.g. mysql), the easiest way to support this is to set the ParameterSQL to a comment block
    ///     e.g. '/*@bob*/'
    /// </summary>
    [Sql]
    string ParameterSQL { get; set; }

    /// <summary>
    ///     The value that the SQL parameter currently holds.  This should be a valid Right hand side operand for the
    ///     assignment operator e.g. 'fish' or 10 or UPPER('omg')
    /// </summary>
    [Sql]
    string Value { get; set; }

    /// <summary>
    ///     An optional description of what the parameter represents.  This will be included in SQL generated and will be
    ///     wrapped in an SQL comment block.
    /// </summary>
    string Comment { get; set; }

    /// <summary>
    ///     Returns the <see cref="IMapsDirectlyToDatabaseTable" /> (usually an <see cref="IFilter" />) that the parameter is
    ///     declared on.  If the parameter is a global level
    ///     parameter e.g. declared at <see cref="AggregateConfiguration" /> level then the corresponding higher level object
    ///     will be returned
    ///     (e.g. <see cref="AnyTableSqlParameter" />).
    /// </summary>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable GetOwnerIfAny();
}