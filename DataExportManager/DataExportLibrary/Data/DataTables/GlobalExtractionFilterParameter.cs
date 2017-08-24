using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Sometimes you want to define global parameters that apply to an entire ExtractionConfiguration and all the Catalogues/ExtractableDataSets within it.  For example you might
    /// want to define @studyStartWindow and @studyEndWindow as global parameters which can be used to restrict the extraction window of each dataset.  GlobalExtractionFilterParameters
    /// are created and assocaited with a single ExtractionConfiguration after which they are available for use in all DeployedExtractionFilters of all datasets within the configuration.
    /// 
    /// It also means you have a single point you can change the parameter if you need to adjust it later on.
    /// </summary>
    public class GlobalExtractionFilterParameter : VersionedDatabaseEntity, ISqlParameter
    {
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return RDMPQuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        #region Database Properties
        private string _parameterSQL;
        private string _value;
        private string _comment;
        private int _extractionConfiguration_ID;

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

            if (!RDMPQuerySyntaxHelper.IsValidParameterName(parameterSQL))
                throw new ArgumentException("parameterSQL is not valid \"" + parameterSQL + "\"");

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionConfiguration_ID", configuration.ID}
            });
        }

        public GlobalExtractionFilterParameter(IDataExportRepository repository, DbDataReader r)
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
        

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return ExtractionConfiguration;
        }

    }
}