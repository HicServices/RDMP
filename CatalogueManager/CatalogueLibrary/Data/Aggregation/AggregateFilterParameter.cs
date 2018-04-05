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
using ReusableLibraryCode.DatabaseHelpers.Discovery;
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
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int ParameterSQL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Value_MaxLength = -1;

        #region Database Properties
        private int _aggregateFilterID;
        private string _parameterSQL;
        private string _value;
        private string _comment;

        /// <summary>
        /// The ID of the <see cref="AggregateFilter"/> to which this parameter should be used with.  The filter should have a reference to the parameter name (e.g. @startDate)
        /// in it's WhereSQL.
        /// </summary>
        public int AggregateFilter_ID
        {
            get { return _aggregateFilterID; }
            set { SetField(ref  _aggregateFilterID, value); }
        } // changing this is required for cloning functionality i.e. clone parameter then point it to new parent

        
        /// <inheritdoc/>
        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref  _parameterSQL, value); }
        }
         
        /// <inheritdoc/>
        [Sql]
        public string Value
        {
            get { return _value; }
            set { SetField(ref  _value, value); }
        }

        /// <inheritdoc/>
        public string Comment
        {
            get { return _comment; }
            set { SetField(ref  _comment, value); }
        }

        #endregion

        #region Relationships

        /// <inheritdoc cref="AggregateFilter_ID"/>
        [NoMappingToDatabase]
        public AggregateFilter AggregateFilter{ get { return Repository.GetObjectByID<AggregateFilter>(AggregateFilter_ID); }}
        
        #endregion

        /// <summary>
        /// extracts the name ofthe parameter from the SQL
        /// </summary>
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        /// <summary>
        /// Declares a new parameter to be used by the specified AggregateFilter.  Use AggregateFilterFactory to call this 
        /// constructor.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parameterSQL"></param>
        /// <param name="parent"></param>
        internal AggregateFilterParameter(ICatalogueRepository repository, string parameterSQL, AggregateFilter parent)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"AggregateFilter_ID", parent.ID}
            });
        }


        internal AggregateFilterParameter(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            AggregateFilter_ID = int.Parse(r["AggregateFilter_ID"].ToString());
            ParameterSQL = r["ParameterSQL"] as string;
            Value = r["Value"] as string;
            Comment = r["Comment"] as string;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            //return the name of the variable
            return ParameterName;
        }

        /// <inheritdoc cref="ParameterSyntaxChecker"/>
        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return AggregateFilter.GetQuerySyntaxHelper();
        }

        /// <inheritdoc/>
        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return AggregateFilter;
        }

        
    }
}
