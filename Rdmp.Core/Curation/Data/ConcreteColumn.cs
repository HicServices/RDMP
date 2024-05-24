// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Common abstract base class for ExtractionInformation (how to extract a given ColumnInfo) and ExtractableColumn (clone into data export database of an
/// ExtractionInformation - i.e. 'extract column A on for Project B Configuration 'Cases' where A would be an ExtractionInformation defined in the Catalogue
/// database and copied out for use in the data extraction configuration).
/// 
/// <para>Provides an implementation of IColumn whilst still being a DatabaseEntity (saveable / part of a database repository etc)</para>
/// </summary>
public abstract class ConcreteColumn : DatabaseEntity, IColumn, IOrderable, IComparable
{
    #region Database Properties

    private string _selectSql;
    private string _alias;
    private bool _hashOnDataRelease;
    private bool _isExtractionIdentifier;
    private bool _isPrimaryKey;
    private int _order;

    /// <summary>
    /// The order the column should be in when part of a SELECT statement built by an <see cref="ISqlQueryBuilder"/>
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetField(ref _order, value);
    }

    /// <inheritdoc/>
    [Sql]
    public string SelectSQL
    {
        get => _selectSql;
        set
        {
            //never allow annoying white space on this field
            value = value?.Trim();

            SetField(ref _selectSql, value);
        }
    }

    /// <inheritdoc/>
    public string Alias
    {
        get => _alias;
        set => SetField(ref _alias, value);
    }

    /// <inheritdoc/>
    public bool HashOnDataRelease
    {
        get => _hashOnDataRelease;
        set => SetField(ref _hashOnDataRelease, value);
    }

    /// <inheritdoc/>
    public bool IsExtractionIdentifier
    {
        get => _isExtractionIdentifier;
        set => SetField(ref _isExtractionIdentifier, value);
    }

    /// <inheritdoc/>
    public bool IsPrimaryKey
    {
        get => _isPrimaryKey;
        set => SetField(ref _isPrimaryKey, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public abstract ColumnInfo ColumnInfo { get; }

    #endregion

    /// <inheritdoc/>
    protected ConcreteColumn(IRepository repository, DbDataReader r) : base(repository, r)
    {
    }

    /// <inheritdoc/>
    protected ConcreteColumn() : base()
    {
    }

    /// <inheritdoc/>
    public string GetRuntimeName()
    {
        var helper = ColumnInfo == null ? MicrosoftQuerySyntaxHelper.Instance : ColumnInfo.GetQuerySyntaxHelper();
        if (!string.IsNullOrWhiteSpace(Alias))
            return helper.GetRuntimeName(Alias); //.GetRuntimeName(); RDMPQuerySyntaxHelper.GetRuntimeName(this);

        return !string.IsNullOrWhiteSpace(SelectSQL) ? helper.GetRuntimeName(SelectSQL) : ColumnInfo.GetRuntimeName();
    }

    /// <inheritdoc cref="ColumnSyntaxChecker"/>
    public void Check(ICheckNotifier notifier)
    {
        new ColumnSyntaxChecker(this).Check(notifier);
    }

    /// <summary>
    /// Compares columns by <see cref="ConcreteColumn.Order"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj) => obj is IColumn ? Order - (obj as IColumn).Order : 0;

    public override bool Equals(object obj)
    {
        return CompareTo(obj) == 1;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}