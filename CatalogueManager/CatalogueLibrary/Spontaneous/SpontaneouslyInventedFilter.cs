using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using IFilter = CatalogueLibrary.Data.IFilter;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) implementation of IFilter.  This is the prefered method of injecting lines of WHERE Sql into an ISqlQueryBuilder dynamically in code
    /// (as opposed to ones the user has created).  This can be used to for example enforce additional constraints on the query e.g. 'generate this Aggregate Graph but
    /// restrict the results to patients appearing in my cohort list X' (in this case the SpontaneouslyInventedFilter would be the 'patients appearing in my cohort list X'
    /// 
    /// The other way to inject sql code into an ISqlQueryBuilder is via CustomLine but that's less precise.
    /// </summary>
    public class SpontaneouslyInventedFilter:SpontaneousObject,IFilter
    {
        private readonly IContainer _notionalParent;
        private readonly ISqlParameter[] _filterParametersIfAny;

        public SpontaneouslyInventedFilter(IContainer notionalParent, string whereSql, string name, string description, ISqlParameter[] filterParametersIfAny)
        {
            _notionalParent = notionalParent;
            _filterParametersIfAny = filterParametersIfAny?? new ISqlParameter[0];
            WhereSQL = whereSql;
            Name = name;
            Description = description;
        }
      
        public string WhereSQL { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public ISqlParameter[] GetAllParameters()
        {
            return _filterParametersIfAny;
        }

        public bool IsMandatory { get { return false; } set{throw new NotSupportedException();}}
        public int? ClonedFromExtractionFilter_ID { get { return null; } set{throw new NotSupportedException();} }
        

        public int? FilterContainer_ID { get {return _notionalParent != null ?_notionalParent.ID:(int?) null; } set{throw new NotSupportedException();} }
        public IContainer FilterContainer { get { return _notionalParent; } }

        public ColumnInfo GetColumnInfoIfExists()
        {
            //there is definetly no ColumnInfo associated with this magically made up filter
            return null;
        }

        public IFilterFactory GetFilterFactory()
        {
            throw new NotImplementedException();
        }

        public Catalogue GetCatalogue()
        {
            return null;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotImplementedException();
        }
    }
}