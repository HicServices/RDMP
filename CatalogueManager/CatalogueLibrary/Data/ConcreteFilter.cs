using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Abstract base class for all IFilters which are database entities (Stored in the Catalogue/Data Export database as objects). 
    /// 
    /// <para>ConcreteFilter is used to provide UI editing of an IFilter without having to add persistence / VersionedDatabaseEntity logic to IFilter (which would break 
    /// SpontaneouslyInventedFilters)</para>
    /// </summary>
    public abstract class ConcreteFilter :  VersionedDatabaseEntity,IFilter, ICheckable
    {
        /// <inheritdoc/>
        protected ConcreteFilter(IRepository repository,DbDataReader r) : base(repository, r)
        {
            
        }
        
        /// <inheritdoc/>
        protected ConcreteFilter():base()
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
            get { return _whereSQL; }
            set { SetField(ref  _whereSQL, value); }
        }
        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <inheritdoc/>
        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        /// <inheritdoc/>
        public bool IsMandatory
        {
            get { return _isMandatory; }
            set { SetField(ref  _isMandatory, value); }
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
        /// there are higher level/global overriding parameters e.g. declared at <see cref="CatalogueLibrary.Data.Aggregation.AggregateConfiguration"/> or 
        /// <see cref="CatalogueLibrary.Data.Cohort.CohortIdentificationConfiguration"/>
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
        /// in the extraction of a dataset or an <see cref="CatalogueLibrary.Data.Aggregation.AggregateConfiguration"/> graph / cohort set which are again tied to a 
        /// single <see cref="Catalogue"/>.   When overridden this method returns the associated Catalogue. 
        /// </summary>
        /// <returns></returns>
        public abstract Catalogue GetCatalogue();
        
        /// <summary>
        /// Returns an appropriately typed <see cref="IQuerySyntaxHelper"/> depending on the DatabaseType of the Catalogue that it relates to.
        /// </summary>
        /// <returns></returns>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new QuerySyntaxHelperFactory().Create(GetDatabaseType());
        }

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
            if (col != null)
                _cachedDatabaseTypeAnswer = col.TableInfo.DatabaseType;
            else
                _cachedDatabaseTypeAnswer = GetCatalogue().GetDistinctLiveDatabaseServerType();

            
            return _cachedDatabaseTypeAnswer.Value;
        }

        /// <summary>
        /// Checks that the <see cref="IFilter"/> WhereSQL passes basic syntax checks via <see cref="FilterSyntaxChecker"/>.
        /// </summary>
        /// <param name="notifier"></param>
        public virtual void Check(ICheckNotifier notifier)
        {
            if (WhereSQL != null && WhereSQL.StartsWith("where ", StringComparison.CurrentCultureIgnoreCase))
                notifier.OnCheckPerformed(new CheckEventArgs("Filters do not need to start with 'where' keyword, it is implicit",CheckResult.Fail));

            new FilterSyntaxChecker(this).Check(notifier);
        }
    }
}
