using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Stores parameter values for a DeployedExtractionFilter
    /// </summary>
    public class DeployedExtractionFilterParameter: VersionedDatabaseEntity, ISqlParameter
    {
        #region Database Properties
        private int _extractionFilter_ID;
        private string _parameterSQL;
        private string _value;
        private string _comment;

        public int ExtractionFilter_ID
        {
            get { return _extractionFilter_ID; }
            set { SetField(ref _extractionFilter_ID, value); }
        }
        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref _parameterSQL, value); }
        }
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
        #endregion

        //extracts the name ofthe parameter from the SQL
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return GetQuerySyntaxHelper().GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        public DeployedExtractionFilterParameter(IDataExportRepository repository, string parameterSQL, IFilter parent)
        {
            Repository = repository;

            if (!GetQuerySyntaxHelper().IsValidParameterName(parameterSQL))
                throw new ArgumentException("parameterSQL is not valid \"" + parameterSQL + "\"");

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionFilter_ID", parent.ID}
            });
        }

        public DeployedExtractionFilterParameter(IDataExportRepository repository, DbDataReader r)
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
            return ((DeployedExtractionFilter) GetOwnerIfAny()).GetQuerySyntaxHelper();
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return Repository.GetObjectByID<DeployedExtractionFilter>(ExtractionFilter_ID);
        }
    }
}
