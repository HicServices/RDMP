// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Abstract base class for all IFilters which are database entities (Stored in the Catalogue/Data Export database as objects).
/// 
/// <para>ConcreteFilter is used to provide UI editing of an IFilter without having to add persistence / DatabaseEntity logic to IFilter (which would break
/// SpontaneouslyInventedFilters)</para>
/// </summary>
public abstract class ConcreteFilter : DatabaseEntity, IFilter, ICheckable
{
    /// <inheritdoc/>
    protected ConcreteFilter(IRepository repository, DbDataReader r) : base(repository, r)
    {
    }

    /// <inheritdoc/>
    protected ConcreteFilter() : base()
    {
    }

    #region Database Properties

    private string _whereSQL;
    private string _name;
    private string _description;
    private bool _isMandatory;

    /// <inheritdoc/>
    [Sql]
    public string WhereSQL
    {
        get => _whereSQL;
        set => SetField(ref _whereSQL, value);
    }

    /// <inheritdoc/>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    [UsefulProperty]
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <inheritdoc/>
    public bool IsMandatory
    {
        get => _isMandatory;
        set => SetField(ref _isMandatory, value);
    }

    #endregion

    /// <summary>
    /// An <see cref="IFilter"/> can either be created from scratch or copied from a master <see cref="ExtractionFilter"/> declared at Catalogue level.  If this filter
    /// was cloned from a master catalogue filter then the ID of the filter will be in this property.
    /// </summary>
    public abstract int? ClonedFromExtractionFilter_ID { get; set; }

    /// <inheritdoc/>
    public abstract int? FilterContainer_ID { get; set; }

    /// <summary>
    /// Returns all the immediate <see cref="ISqlParameter"/> which are declared on the IFilter.  These are sql parameters e.g. 'DECLARE @startDate as datetime' with a defined
    /// Value, the parameter should be referenced by the <see cref="WhereSQL"/> of the IFilter.  This may not be representative of the final values used in query building if
    /// there are higher level/global overriding parameters e.g. declared at <see cref="AggregateConfiguration"/> or
    /// <see cref="CohortIdentificationConfiguration"/>
    /// </summary>
    /// <returns></returns>
    public abstract ISqlParameter[] GetAllParameters();

    #region Relationships

    /// <inheritdoc cref="FilterContainer_ID"/>
    [NoMappingToDatabase]
    public abstract IContainer FilterContainer { get; }

    #endregion

    /// <summary>
    /// If an IFilter is associated with a specific ColumnInfo then this method returns it.  This is really only the case for master Catalogue level filters
    /// (<see cref="ExtractionFilter"/>)
    /// </summary>
    /// <returns></returns>
    public abstract ColumnInfo GetColumnInfoIfExists();

    /// <summary>
    /// When overriden in a derrived class, creates an <see cref="IFilterFactory"/> which can be used to create new correctly typed <see cref="ISqlParameter"/> for use with
    /// the current <see cref="IFilter"/>
    /// </summary>
    /// <remarks>Most IFilter implementations require their own specific type of IContainer, ISqlParameter etc and they only work with those concrete classes.  Therefore the
    /// IFilterFactory is needed to create those correct concrete classes when all you have is a reference to the interface</remarks>
    /// <returns></returns>
    public abstract IFilterFactory GetFilterFactory();

    /// <summary>
    /// Every IFilter is ultimately tied to a single <see cref="Catalogue"/> either because it is a master filter declared on a column in one or because it is being used
    /// in the extraction of a dataset or an <see cref="AggregateConfiguration"/> graph / cohort set which are again tied to a
    /// single <see cref="Catalogue"/>.   When overridden this method returns the associated Catalogue.
    /// </summary>
    /// <returns></returns>
    public abstract Catalogue GetCatalogue();

    /// <summary>
    /// Returns an appropriately typed <see cref="IQuerySyntaxHelper"/> depending on the DatabaseType of the Catalogue that it relates to.
    /// </summary>
    /// <returns></returns>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => new QuerySyntaxHelperFactory().Create(GetDatabaseType());

    private DatabaseType? _cachedDatabaseTypeAnswer;

    /// <summary>
    /// Returns the database provider type (e.g. MySql / Sql Server) that the filter is written for.  This is determined by what <see cref="GetColumnInfoIfExists"/>
    /// it is declared against.
    /// </summary>
    /// <returns></returns>
    protected DatabaseType GetDatabaseType()
    {
        if (_cachedDatabaseTypeAnswer != null)
            return _cachedDatabaseTypeAnswer.Value;

        var col = GetColumnInfoIfExists();
        _cachedDatabaseTypeAnswer =
            col != null ? col.TableInfo.DatabaseType : GetCatalogue().GetDistinctLiveDatabaseServerType();

        return _cachedDatabaseTypeAnswer ??
               throw new AmbiguousDatabaseTypeException($"Unable to determine DatabaseType for Filter '{this}'");
    }

    /// <summary>
    /// Checks that the <see cref="IFilter"/> WhereSQL passes basic syntax checks via <see cref="FilterSyntaxChecker"/>.
    /// </summary>
    /// <param name="notifier"></param>
    public virtual void Check(ICheckNotifier notifier)
    {
        if (WhereSQL != null && WhereSQL.StartsWith("where ", StringComparison.CurrentCultureIgnoreCase))
            notifier.OnCheckPerformed(
                new CheckEventArgs("Filters do not need to start with 'where' keyword, it is implicit",
                    CheckResult.Fail));

        new FilterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc />
    public bool ShouldBeReadOnly(out string reason)
    {
        reason = null;
        return FilterContainer?.ShouldBeReadOnly(out reason) ?? false;
    }
}