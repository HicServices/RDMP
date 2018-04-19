using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes you want to define global parameters that apply to an entire ExtractionConfiguration and all the Catalogues/ExtractableDataSets within it.  For example you might
    /// want to define @studyStartWindow and @studyEndWindow as global parameters which can be used to restrict the extraction window of each dataset.  GlobalExtractionFilterParameters
    /// are created and assocaited with a single ExtractionConfiguration after which they are available for use in all DeployedExtractionFilters of all datasets within the configuration.
    /// 
    /// <para>It also means you have a single point you can change the parameter if you need to adjust it later on.</para>
    /// </summary>
    public class GlobalExtractionFilterParameter : VersionedDatabaseEntity, ISqlParameter
    {
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        #region Database Properties
        private string _parameterSQL;
        private string _value;
        private string _comment;
        private int _extractionConfiguration_ID;

        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref _parameterSQL, value); }
        }
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
        public int ExtractionConfiguration_ID
        {
            get { return _extractionConfiguration_ID; }
            set { SetField(ref _extractionConfiguration_ID, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public ExtractionConfiguration ExtractionConfiguration { get{return Repository.GetObjectByID<ExtractionConfiguration>(ExtractionConfiguration_ID);} }

        #endregion

        public GlobalExtractionFilterParameter(IDataExportRepository repository, ExtractionConfiguration configuration, string parameterSQL)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionConfiguration_ID", configuration.ID}
            });
        }

        internal GlobalExtractionFilterParameter(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Value = r["Value"] as string;
            ExtractionConfiguration_ID = (int)r["ExtractionConfiguration_ID"];
            ParameterSQL = r["ParameterSQL"] as string;
            Comment = r["Comment"] as string;
        }

        public override string ToString()
        {
            return ParameterName;
        }

        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotSupportedException("Global extraction parameters are multi database type (depending on which ExtractableDataSet they are being used with)");
        }


        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return ExtractionConfiguration;
        }

    }
}
