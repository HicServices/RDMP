using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Stores a known specific useful value for a given ExtractionFilterParameter.  See ExtractionFilterParameterSet for a use case for this.
    /// </summary>
    public class ExtractionFilterParameterSetValue: DatabaseEntity, ISqlParameter
    {
        #region Database Properties
        private string _value;
        private int _extractionFilterParameterSetID;
        private int _extractionFilterParameterID;

        public int ExtractionFilterParameterSet_ID
        {
            get { return _extractionFilterParameterSetID; }
            set { SetField(ref _extractionFilterParameterSetID , value); }
        }

        public int ExtractionFilterParameter_ID
        {
            get { return _extractionFilterParameterID; }
            set { SetField(ref _extractionFilterParameterID , value); }
        }

        [Sql]
        public string Value
        {
            get { return _value; }
            set {SetField(ref _value, value);}
        }
        #endregion

        #region cached values stored so we can act like a readonly ISqlParameter but secretly only Value will actually be changeable
        private string _parameterName;
        private string _parameterSQL;
        private string _comment; 
        
        private ExtractionFilterParameterSet _extractionFilterParameterSet;

        [NoMappingToDatabase]
        public string ParameterName
        {
            get
            {
                CacheMasterParameterFieldsIfRequired();
                return _parameterName;
            }
        }

        [Sql]
        [NoMappingToDatabase]
        public string ParameterSQL
        {
            get
            {
                CacheMasterParameterFieldsIfRequired(); 
                return _parameterSQL;
            }
            set { }
        }

        [NoMappingToDatabase]
        public string Comment
        {
            get
            {
                CacheMasterParameterFieldsIfRequired(); 
                return _comment;
            }
            set {  }
        }

        private bool haveCachedMasterValues = false;
        

        private void CacheMasterParameterFieldsIfRequired()
        {
            if(haveCachedMasterValues)
                return;

            var master = ExtractionFilterParameter;

            _parameterName = master.ParameterName;
            _parameterSQL = master.ParameterSQL;
            _comment = master.Comment;

            _extractionFilterParameterSet = ExtractionFilterParameterSet;

            haveCachedMasterValues = true;
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return _extractionFilterParameterSet;
        }
        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public ExtractionFilterParameterSet ExtractionFilterParameterSet { get {return Repository.GetObjectByID<ExtractionFilterParameterSet>(ExtractionFilterParameterSet_ID);} }

        [NoMappingToDatabase]
        public ExtractionFilterParameter ExtractionFilterParameter { get { return Repository.GetObjectByID<ExtractionFilterParameter>(ExtractionFilterParameter_ID); } }

        #endregion


        public ExtractionFilterParameterSetValue(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionFilterParameterSet_ID =   Convert.ToInt32(r["ExtractionFilterParameterSet_ID"]);
            ExtractionFilterParameter_ID =      Convert.ToInt32(r["ExtractionFilterParameter_ID"]);
            Value = r["Value"] as string;
        }

        public ExtractionFilterParameterSetValue(ICatalogueRepository repository, ExtractionFilterParameterSet parent, ExtractionFilterParameter valueIsForParameter)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"ExtractionFilterParameterSet_ID",parent.ID},
                {"ExtractionFilterParameter_ID",valueIsForParameter.ID}
            });
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return ExtractionFilterParameter.GetQuerySyntaxHelper();
        }

        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }
    }
}