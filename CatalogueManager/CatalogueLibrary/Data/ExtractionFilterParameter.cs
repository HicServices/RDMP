using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes an SQL parameter (e.g. @drugname) which is required for use of an ExtractionFilter (it's parent).
    /// 
    /// <para>See the description of ExtractionFilter to see how filters are cloned and adjusted depending on usage context</para>
    /// </summary>
    public class ExtractionFilterParameter : VersionedDatabaseEntity, IDeleteable, ISqlParameter, IHasDependencies
    {
        #region Database Properties
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int ParameterSQL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Value_MaxLength = -1;

        private string _value;
        private string _comment;
        private string _parameterSQL;
        private int _extractionFilterID;
        
        /// <inheritdoc/>
        [Sql]
        public string Value
        {
            get { return _value; }
            set { SetField(ref _value, value); }
        }
        /// <inheritdoc/>
        public string Comment
        {
            get { return _comment; }
            set { SetField(ref _comment, value); }
        }
        /// <inheritdoc/>
        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref _parameterSQL, value); }
        }

        /// <summary>
        /// The filter which requires this parameter belongs e.g. an <see cref="ExtractionFilter"/>'Healthboard X' could have a required property (<see cref="ExtractionFilterParameter"/>) @Hb 
        /// </summary>
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
        /// <inheritdoc cref="ExtractionFilter_ID"/>
        [NoMappingToDatabase]
        public ExtractionFilter ExtractionFilter
        {
            get { return Repository.GetObjectByID<ExtractionFilter>(ExtractionFilter_ID); }
        }
        #endregion

        /// <summary>
        /// Creates a new parameter on the given <paramref name="parent"/>
        /// <para>It is better to use <see cref="CatalogueLibrary.FilterImporting.ParameterCreator"/> to automatically generate parameters based on the WHERE Sql</para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parameterSQL"></param>
        /// <param name="parent"></param>
        public ExtractionFilterParameter(ICatalogueRepository repository, string parameterSQL, ExtractionFilter parent)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionFilter_ID", parent.ID}
            });
        }


        internal ExtractionFilterParameter(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionFilter_ID = int.Parse(r["ExtractionFilter_ID"].ToString());
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
            return ExtractionFilter.GetQuerySyntaxHelper();
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {ExtractionFilter};
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return null;
        }

        /// <summary>
        /// Returns true if a  <see cref="Comment"/> has been provided and an example/initial <see cref="Value"/> specified.  This is a requirement of
        /// publishing a filter as a master filter
        /// </summary>
        /// <param name="sqlParameter"></param>
        /// <param name="reasonParameterRejected"></param>
        /// <returns></returns>
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

        /// <inheritdoc/>
        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return ExtractionFilter;
        }
    }
}
