using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Defines as a single line SQL Where statement, a way of reducing the scope of a data extraction / aggregation etc.  For example, 
    /// 'Only prescriptions for diabetes medications'.  An ExtractionFilter can have 0 or more ExtractionFilterParameters which allows
    /// you to define a more versatile filter e.g. 'Only prescriptions for drug @bnfCode'
    /// 
    /// Typically an ExtractionFilter is cloned out as either a DeployedExtractionFilter or an AggregateFilter and either used as is or
    /// customised in it's new state (where it's parameters might have values populated into them).
    /// 
    /// It is not uncommon for an extraction to involve multiple customised copies of the same Extraction filter for example a user might
    /// take the filter 'Prescriptions of drug @Drugname' and make 3 copies in a given project in DataExportManager (this would result in
    /// 3 DeployedExtractionFilters) and set the value of the first to 'Paracetamol' the second to 'Aspirin' and the third to 'Ibuprofen'
    /// and then put them all in a single AND container.
    /// 
    /// At query building time QueryBuilder rationalizes all the various containers, subcontainers, filters and parameters into one extraction
    /// SQL query (including whatever columns/transforms it was setup with).
    /// </summary>
    public class ExtractionFilter : ConcreteFilter, IFilter, IHasDependencies,INamed
    {
     
        #region Database Properties
        private int _extractionInformationID;

        public int ExtractionInformation_ID
        {
            get { return _extractionInformationID; }
            set { SetField(ref _extractionInformationID , value); }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public override IContainer FilterContainer { get { return null; } }

        #endregion

        /// <summary>
        /// Used to fulfil requirements of interface so that the FilterUI can be used, will throw if accessed.  Filters at Catalogue level are never nested / used in AND/OR containers
        /// </summary>
        [NoMappingToDatabase]
        public override int? FilterContainer_ID
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override ColumnInfo GetColumnInfoIfExists()
        {
            return ExtractionInformation.ColumnInfo;

        }

        public override IFilterFactory GetFilterFactory()
        {
            return new ExtractionFilterFactory(ExtractionInformation);
        }


        public override Catalogue GetCatalogue()
        {
            return ExtractionInformation.CatalogueItem.Catalogue;
        }

        public override ISqlParameter[] GetAllParameters()
        {
            return ExtractionFilterParameters.ToArray();
        }

        public static int Name_MaxLength = -1;
        public static int Description_MaxLength = -1;

        #region Relationships
        [NoMappingToDatabase]
        public ExtractionInformation ExtractionInformation {get { return Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID); }}

        [NoMappingToDatabase]
        public IEnumerable<ExtractionFilterParameter> ExtractionFilterParameters { get { return Repository.GetAllObjectsWithParent<ExtractionFilterParameter>(this); } }

        #endregion

        public ExtractionFilter(ICatalogueRepository repository, string name, ExtractionInformation parent)
        {
            name = name ?? "New Filter " + Guid.NewGuid();

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name},
                {"ExtractionInformation_ID", parent.ID}
            });
        }

        public ExtractionFilter(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionInformation_ID = int.Parse(r["ExtractionInformation_ID"].ToString());
            WhereSQL = r["WhereSQL"] as string;
            Description = r["Description"] as string;
            Name = r["Name"] as string;
            IsMandatory = (bool) r["IsMandatory"];
        }

        public override string ToString()
        {
            return Name;
        }
        
        //we are an extraction filter ourselves! so obviously we werent cloned from one! (this is for aggregate and data export filters and satisfies IFilter).  Actually we can
        //be cloned via the publishing (elevation) from a custom filter defined at Aggregate level for example.  But in this case we don't need to know the ID anyway since we 
        //become the new master anyway since we are at the highest level for filters
        [NoMappingToDatabase]
        public override int? ClonedFromExtractionFilter_ID
        {
            get
            {
                return null; 
            }
            set
            {
                throw new NotSupportedException("ClonedFromExtractionFilter_ID is only supported on lower level filters e.g. DeployedExtractionFilter and AggregateFilter");
            }
        }
        
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[] { ExtractionInformation };
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return ExtractionFilterParameters.ToArray();
        }

    
    }
}
