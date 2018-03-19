using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes an SQL parameter (e.g. @drugname) which is required for use of an ExtractionFilter (it's parent).
    /// 
    /// See the description of ExtractionFilter to see how filters are cloned and adjusted depending on usage context
    /// </summary>
    public class ExtractionFilterParameter : VersionedDatabaseEntity, IDeleteable, ISqlParameter, IHasDependencies
    {
        #region Database Properties
        public static int ParameterSQL_MaxLength = -1;
        public static int Value_MaxLength = -1;

        private string _value;
        private string _comment;
        private string _parameterSQL;
        private int _extractionFilterID;

        [Sql]
        public string Value
        {
            get { return _value; }
            set { SetField(ref _value, value); }
        }
        public string Comment
        {
            get { return _comment; }
            set { SetField(ref _comment, value); }
        }
        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref _parameterSQL, value); }
        }

        public int ExtractionFilter_ID
        {
            get { return _extractionFilterID; }
            set {SetField(ref _extractionFilterID , value); }
        }

        #endregion


        /// <summary>
        /// extracts the name ofthe parameter from the SQL
        /// </summary>
        [NoMappingToDatabase]
        public string ParameterName {
            get { return GetQuerySyntaxHelper().GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        #region Relationships
        [NoMappingToDatabase]
        public ExtractionFilter ExtractionFilter
        {
            get { return Repository.GetObjectByID<ExtractionFilter>(ExtractionFilter_ID); }
        }
        #endregion

        public ExtractionFilterParameter(ICatalogueRepository repository, string parameterSQL, ExtractionFilter parent)
        {
            if (!parent.GetQuerySyntaxHelper().IsValidParameterName(parameterSQL))
                throw new ArgumentException("parameterSQL is not valid \"" + parameterSQL + "\"");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionFilter_ID", parent.ID}
            });
        }


        public ExtractionFilterParameter(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionFilter_ID = int.Parse(r["ExtractionFilter_ID"].ToString());
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
            return ExtractionFilter.GetQuerySyntaxHelper();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {ExtractionFilter};
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return null;
        }

        public static bool IsProperlyDocumented(ISqlParameter sqlParameter, out string reasonParameterRejected)
        {
            reasonParameterRejected = null;

            if (string.IsNullOrWhiteSpace(sqlParameter.ParameterSQL))
                reasonParameterRejected = "The is no ParameterSQL";
            else
            if (string.IsNullOrWhiteSpace(sqlParameter.Comment))
                reasonParameterRejected = "There is no description comment";
            else
            if (string.IsNullOrWhiteSpace(sqlParameter.Value))
                reasonParameterRejected = "There is no value/default value listed";


            return reasonParameterRejected == null;
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return ExtractionFilter;
        }
    }
}