using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Stores a known specific useful value for a given ExtractionFilterParameter.  See <see cref="CatalogueLibrary.Data.ExtractionFilterParameterSet"/> for a use case for this.
    /// </summary>
    public class ExtractionFilterParameterSetValue: DatabaseEntity, ISqlParameter
    {
        #region Database Properties
        private string _value;
        private int _extractionFilterParameterSetID;
        private int _extractionFilterParameterID;

        /// <summary>
        /// The 'known good paramter set' (<see cref="ExtractionFilterParameterSet"/>) to which this parameter value belongs
        /// </summary>
        public int ExtractionFilterParameterSet_ID
        {
            get { return _extractionFilterParameterSetID; }
            set { SetField(ref _extractionFilterParameterSetID , value); }
        }

        /// <summary>
        /// The specific parameter that this object is providing a 'known value' for in the parent <see cref="ExtractionFilter"/> e.g. @DrugList='123.2,123.2,... etc'.
        /// </summary>
        public int ExtractionFilterParameter_ID
        {
            get { return _extractionFilterParameterID; }
            set { SetField(ref _extractionFilterParameterID , value); }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID"/></remarks>
        [NoMappingToDatabase]
        public string ParameterName
        {
            get
            {
                CacheMasterParameterFieldsIfRequired();
                return _parameterName;
            }
        }

        /// <inheritdoc/>
        /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID"/></remarks>
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

        /// <inheritdoc/>
        /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID"/></remarks>
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

        /// <summary>
        /// Returns the <see cref="ExtractionFilterParameterSet"/> this known good value belongs to
        /// </summary>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return _extractionFilterParameterSet;
        }
        #endregion

        #region Relationships

        /// <inheritdoc cref="ExtractionFilterParameterSet_ID"/>
        [NoMappingToDatabase]
        public ExtractionFilterParameterSet ExtractionFilterParameterSet { get {return Repository.GetObjectByID<ExtractionFilterParameterSet>(ExtractionFilterParameterSet_ID);} }

        /// <inheritdoc cref="ExtractionFilterParameter_ID"/>
        [NoMappingToDatabase]
        public ExtractionFilterParameter ExtractionFilterParameter { get { return Repository.GetObjectByID<ExtractionFilterParameter>(ExtractionFilterParameter_ID); } }

        #endregion


        internal ExtractionFilterParameterSetValue(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionFilterParameterSet_ID =   Convert.ToInt32(r["ExtractionFilterParameterSet_ID"]);
            ExtractionFilterParameter_ID =      Convert.ToInt32(r["ExtractionFilterParameter_ID"]);
            Value = r["Value"] as string;
        }

        /// <summary>
        /// Creates a record of what value to use with the given <see cref="ISqlParameter"/> of the <see cref="ExtractionFilterParameterSet"/> <see cref="IFilter"/> to achieve the concept.
        /// 
        /// <para>For example if there is an <see cref="ExtractionFilter"/> 'Prescribed Drug X' with a parameter @DrugList and you create an <see cref="ExtractionFilterParameterSet"/>
        /// 'Diabetic Drugs' then this will create a <see cref="ExtractionFilterParameterSetValue"/> of '@DrugList='123.23,121,2... etc'.</para>
        /// 
        /// <para>If a filter has more than one parameter then you will need one <see cref="ExtractionFilterParameterSetValue"/> per parameter per <see cref="ExtractionFilterParameterSet"/></para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="valueIsForParameter"></param>
        public ExtractionFilterParameterSetValue(ICatalogueRepository repository, ExtractionFilterParameterSet parent, ExtractionFilterParameter valueIsForParameter)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"ExtractionFilterParameterSet_ID",parent.ID},
                {"ExtractionFilterParameter_ID",valueIsForParameter.ID}
            });
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return ExtractionFilterParameter.GetQuerySyntaxHelper();
        }

        /// <inheritdoc/>
        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }
    }
}