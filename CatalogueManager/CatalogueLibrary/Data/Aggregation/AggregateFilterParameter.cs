using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Each AggregateFilter can have 1 or more AggregateFilterParameters, these allows you to specify an SQL parameter that the user can adjust at runtime to change
    /// how a given filter works.  E.g. if you have a filter 'Prescribed after @startDate' you would have an AggregateFilterParameter called @startDate with an appropriate
    /// user friendly description.
    /// </summary>
    public class AggregateFilterParameter : VersionedDatabaseEntity, ISqlParameter
    {
        public static int ParameterSQL_MaxLength = -1;
        public static int Value_MaxLength = -1;

        #region Database Properties
        private int _aggregateFilterID;
        private string _parameterSQL;
        private string _value;
        private string _comment;

        public int AggregateFilter_ID
        {
            get { return _aggregateFilterID; }
            set { SetField(ref  _aggregateFilterID, value); }
        } // changing this is required for cloning functionality i.e. clone parameter then point it to new parent

        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref  _parameterSQL, value); }
        }
        
        [Sql]
        public string Value
        {
            get { return _value; }
            set { SetField(ref  _value, value); }
        }

        public string Comment
        {
            get { return _comment; }
            set { SetField(ref  _comment, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public AggregateFilter AggregateFilter{ get { return Repository.GetObjectByID<AggregateFilter>(AggregateFilter_ID); }}
        
        #endregion

        /// <summary>
        /// extracts the name ofthe parameter from the SQL
        /// </summary>
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return GetQuerySyntaxHelper().GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        public AggregateFilterParameter(ICatalogueRepository repository, string parameterSQL, IFilter parent)
        {
            var p = parent as IHasQuerySyntaxHelper;

            if (p == null)
                throw new NotSupportedException("Parents must be IHasQuerySyntaxHelper to be used with AnyTableSqlParameter");

            if (!p.GetQuerySyntaxHelper().IsValidParameterName(parameterSQL))
                throw new ArgumentException("parameterSQL is not valid \"" + parameterSQL + "\"");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"AggregateFilter_ID", parent.ID}
            });
        }


        public AggregateFilterParameter(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            AggregateFilter_ID = int.Parse(r["AggregateFilter_ID"].ToString());
            ParameterSQL = r["ParameterSQL"] as string;
            Value = r["Value"] as string;
            Comment = r["Comment"] as string;
        }
        
        
        public override string ToString()
        {
            //return the name of the variable
            return ParameterName;
        }

        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return AggregateFilter.GetQuerySyntaxHelper();
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return AggregateFilter;
        }

        
    }
}
