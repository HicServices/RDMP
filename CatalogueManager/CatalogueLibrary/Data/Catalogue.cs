using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// The central class for the RDMP, a Catalogue is a virtual dataset e.g. 'Hospital Admissions'.  A Catalogue can be a merging of multiple underlying tables and exists 
    /// independent of where the data is actually stored (look at other classes like TableInfo to see the actual locations of data).
    /// 
    /// <para>As well as storing human readable names/descriptions of what is in the dataset it is the hanging off point for Attachments (SupportingDocument), validation logic, 
    /// extractable columns (CatalogueItem->ExtractionInformation->ColumnInfo) ways of filtering the data, aggregations to help understand the dataset etc.</para>
    /// 
    /// <para>Catalogues are always flat views although they can be built from multiple relational data tables underneath.</para>
    /// 
    /// <para>Whenever you see Catalogue, think Dataset (which is a reserved class in C#, hence the somewhat confusing name Catalogue)</para>
    /// </summary>
    public class Catalogue : VersionedDatabaseEntity, IComparable, ICatalogue, ICheckable, INamed
    {
        #region Database Properties

        //just create these variables (one for every string or Uri field and reflection will populate them
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Acronym_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Name_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Description_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Geographical_coverage_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Background_summary_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Search_keywords_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Update_freq_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Update_sched_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Time_coverage_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Contact_details_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Resource_owner_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Attribution_citation_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Access_options_MaxLength = -1;

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Detail_Page_URL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int API_access_URL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Browse_URL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Bulk_Download_URL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Query_tool_URL_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Source_URL_MaxLength = -1;

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Country_of_origin_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Data_standards_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Administrative_contact_name_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Administrative_contact_email_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Administrative_contact_telephone_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Administrative_contact_address_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Ethics_approver_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Source_of_data_collection_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int SubjectNumbers_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int ValidatorXML_MaxLength = -1;

        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Ticket_MaxLength = -1;

        private string _acronym;
        private string _name;
        private CatalogueFolder _folder;
        private string _description;
        private Uri _detailPageUrl;
        private CatalogueType _type;
        private CataloguePeriodicity _periodicity;
        private CatalogueGranularity _granularity;
        private string _geographicalCoverage;
        private string _backgroundSummary;
        private string _searchKeywords;
        private string _updateFreq;
        private string _updateSched;
        private string _timeCoverage;
        private DateTime? _lastRevisionDate;
        private string _contactDetails;
        private string _resourceOwner;
        private string _attributionCitation;
        private string _accessOptions;
        private string _subjectNumbers;
        private Uri _apiAccessUrl;
        private Uri _browseUrl;
        private Uri _bulkDownloadUrl;
        private Uri _queryToolUrl;
        private Uri _sourceUrl;
        private string _countryOfOrigin;
        private string _dataStandards;
        private string _administrativeContactName;
        private string _administrativeContactEmail;
        private string _administrativeContactTelephone;
        private string _administrativeContactAddress;
        private bool? _explicitConsent;
        private string _ethicsApprover;
        private string _sourceOfDataCollection;
        private string _ticket;

        /// <summary>
        /// Shorthand (recommended 3 characters or less) for referring to this dataset (e.g. 'DEM' for the dataset 'Demography')
        /// </summary>
        public string Acronym
        {
            get { return _acronym; }
            set { SetField(ref  _acronym, value); }
        }

        /// <summary>
        /// The full human readable name of the dataset.  This should usually match the name of the underlying <see cref="TableInfo"/> but might differ
        /// if there are multiple tables powering the Catalogue or they don't have user accessible names.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// A user defined hierarchical category which designates the role of the dataset e.g. '\datasets\extractable\labdata\'
        /// <para>Should always start and end with a '\' even if it is the root (i.e. '\')</para>
        /// </summary>
        public CatalogueFolder Folder
        {
            get { return _folder; }
            set { SetField(ref  _folder, value); }
        }
         
        /// <summary>
        /// Human readable description provided by the RDMP user that describes what the dataset contains.  
        /// <para>This can be multiple paragraphs.</para>
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        /// <summary>
        /// User defined Uri for a website page which describes the dataset (probably null)
        /// </summary>
        public Uri Detail_Page_URL
        {
            get { return _detailPageUrl; }
            set { SetField(ref  _detailPageUrl, value); }
        }

        /// <summary>
        /// User defined classification of the Type of dataset the Catalogue is e.g. Cohort, ResearchStudy etc
        /// </summary>
        public CatalogueType Type
        {
            get { return _type; }
            set { SetField(ref  _type, value); }
        }

        /// <summary>
        /// User specified period on how regularly the dataset is updated.  This does not have any technical bearing on how often it is loaded
        /// and might be an outright lie.
        /// </summary>
        public CataloguePeriodicity Periodicity
        {
            get { return _periodicity; }
            set { SetField(ref  _periodicity, value); }
        }

        /// <summary>
        /// User specified field describing how the dataset is subdivided/bounded e.g. relates to a multiple 'HealthBoards' / 'Clinics' / 'Hosptials' etc.
        /// </summary>
        public CatalogueGranularity Granularity
        {
            get { return _granularity; }
            set { SetField(ref  _granularity, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Geographical_coverage
        {
            get { return _geographicalCoverage; }
            set { SetField(ref  _geographicalCoverage, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Background_summary
        {
            get { return _backgroundSummary; }
            set { SetField(ref  _backgroundSummary, value); }
        }

        /// <summary>
        /// User specified list of keywords that are intended to help in finding the Catalogue
        /// </summary>
        public string Search_keywords
        {
            get { return _searchKeywords; }
            set { SetField(ref  _searchKeywords, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// <seealso cref="Periodicity"/>
        /// </summary>
        public string Update_freq
        {
            get { return _updateFreq; }
            set { SetField(ref  _updateFreq, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// <seealso cref="Periodicity"/>
        /// </summary>
        public string Update_sched
        {
            get { return _updateSched; }
            set { SetField(ref  _updateSched, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// <seealso cref="Periodicity"/>
        /// </summary>
        public string Time_coverage
        {
            get { return _timeCoverage; }
            set { SetField(ref  _timeCoverage, value); }
        }

        /// <summary>
        /// User specified date that user alledgedly reviewed the contents of the Catalogue / Metadata
        /// </summary>
        public DateTime? Last_revision_date
        {
            get { return _lastRevisionDate; }
            set { SetField(ref  _lastRevisionDate, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Contact_details
        {
            get { return _contactDetails; }
            set { SetField(ref  _contactDetails, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Resource_owner
        {
            get { return _resourceOwner; }
            set { SetField(ref  _resourceOwner, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Attribution_citation
        {
            get { return _attributionCitation; }
            set { SetField(ref  _attributionCitation, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Access_options
        {
            get { return _accessOptions; }
            set { SetField(ref  _accessOptions, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string SubjectNumbers
        {
            get { return _subjectNumbers; }
            set { SetField(ref  _subjectNumbers, value); }
        }

        /// <summary>
        /// User specified field.  Supposedly a URL for a webservice for accessing the dataset? Not used for anything by RDMP.
        /// </summary>
        public Uri API_access_URL
        {
            get { return _apiAccessUrl; }
            set { SetField(ref  _apiAccessUrl, value); }
        }

        /// <summary>
        /// User specified field.  Supposedly a URL for a webservice for browsing the dataset? Not used for anything by RDMP.
        /// </summary>
        public Uri Browse_URL
        {
            get { return _browseUrl; }
            set { SetField(ref  _browseUrl, value); }
        }

        /// <summary>
        /// User specified field.  Supposedly a URL for a webservice for bulk downloading the dataset? Not used for anything by RDMP.
        /// </summary>
        public Uri Bulk_Download_URL
        {
            get { return _bulkDownloadUrl; }
            set { SetField(ref  _bulkDownloadUrl, value); }
        }

        /// <summary>
        /// User specified field.  Supposedly a URL for a webservice for querying the dataset? Not used for anything by RDMP.
        /// </summary>
        public Uri Query_tool_URL
        {
            get { return _queryToolUrl; }
            set { SetField(ref  _queryToolUrl, value); }
        }

        /// <summary>
        /// User specified field.  Supposedly a URL for a website describing where you procured the data from? Not used for anything by RDMP.
        /// </summary>
        public Uri Source_URL
        {
            get { return _sourceUrl; }
            set { SetField(ref  _sourceUrl, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Country_of_origin
        {
            get { return _countryOfOrigin; }
            set { SetField(ref  _countryOfOrigin, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Data_standards
        {
            get { return _dataStandards; }
            set { SetField(ref  _dataStandards, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Administrative_contact_name
        {
            get { return _administrativeContactName; }
            set { SetField(ref  _administrativeContactName, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Administrative_contact_email
        {
            get { return _administrativeContactEmail; }
            set { SetField(ref  _administrativeContactEmail, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Administrative_contact_telephone
        {
            get { return _administrativeContactTelephone; }
            set { SetField(ref  _administrativeContactTelephone, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Administrative_contact_address
        {
            get { return _administrativeContactAddress; }
            set { SetField(ref  _administrativeContactAddress, value); }
        }

        /// <summary>
        /// User specified field.  Not used for anything by RDMP.
        /// </summary>
        public bool? Explicit_consent
        {
            get { return _explicitConsent; }
            set { SetField(ref  _explicitConsent, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Ethics_approver
        {
            get { return _ethicsApprover; }
            set { SetField(ref  _ethicsApprover, value); }
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Source_of_data_collection
        {
            get { return _sourceOfDataCollection; }
            set { SetField(ref  _sourceOfDataCollection, value); }
        }

        /// <summary>
        /// Identifier for a ticket in your <see cref="ITicketingSystem"/> for documenting / auditing work on the Catalogue and for recording issues (if you are not
        /// using the RDMP issue system (See <see cref="CatalogueItemIssue"/>
        /// </summary>
        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref _ticket, value); }
        }
        
        /// <summary>
        /// Name of a task in the logging database which should be used for documenting the loading of this Catalogue. 
        /// <seealso cref="HIC.Logging.LogManager"/>
        /// </summary>
        [DoNotExtractProperty]
        public string LoggingDataTask
        {
            get { return _loggingDataTask; }
            set { SetField(ref  _loggingDataTask, value); }
        }

        /// <summary>
        /// Currently configured validation rules for columns in a Catalogue, this can be deserialized into a <see cref="HIC.Common.Validation.Validator"/>
        /// </summary>
        [DoNotExtractProperty]
        public string ValidatorXML
        {
            get { return _validatorXml; }
            set { SetField(ref  _validatorXml, value); }
        }

        /// <summary>
        /// The <see cref="ExtractionInformation"/> which indicates the time field (in dataset time) of the dataset.  This should be a column in your table
        /// that indicates for every row when it became active e.g. 'PrescribedDate' for prescribing.  Try to avoid using columns that have lots of nulls or 
        /// where the date is arbitrary (e.g. 'RecordLoadedDate')
        /// </summary>
        [DoNotExtractProperty]
        public int? TimeCoverage_ExtractionInformation_ID
        {
            get { return _timeCoverageExtractionInformationID; }
            set { SetField(ref  _timeCoverageExtractionInformationID, value); }
        }

        /// <summary>
        /// The <see cref="ExtractionInformation"/> which can provide a useful subdivision of the dataset e.g. 'Healthboard'.  This should be a logical subdivision
        /// that helps in the assesment of data quality e.g. you might imagine that if you have 10% errors in data quality and 10 healthboards knowing that all the errors
        /// are from a single healthboard would be handy.
        /// 
        /// <para>This chosen column should not have hundreds/thousands of unique values</para>
        /// </summary>
        [DoNotExtractProperty]
        public int? PivotCategory_ExtractionInformation_ID
        {
            get { return _pivotCategoryExtractionInformationID; }
            set { SetField(ref  _pivotCategoryExtractionInformationID, value); }
        }

        /// <summary>
        /// Bit flag indicating whether the dataset should be considered Deprecated (i.e. do not use anymore).  This is preferred to deleting a Catalogue.  The implications
        /// of this are that it no longer appears in UIs by default and that warnings will appear when trying to do extractions of the Catalogue
        /// </summary>
        [DoNotExtractProperty]
        public bool IsDeprecated
        {
            get { return _isDeprecated; }
            set { SetField(ref  _isDeprecated, value); }
        }

        /// <summary>
        /// Bit flag indicating whether the dataset should NEVER be extracted and ONLY EVER used internally by data analysts.
        /// </summary>
        [DoNotExtractProperty]
        public bool IsInternalDataset
        {
            get { return _isInternalDataset; }
            set { SetField(ref  _isInternalDataset, value); }
        }

        /// <summary>
        /// Bit flag indicating whether the Catalogue is a seldom used dataset that should be hidden by default.  Use this if you are importing lots of researcher
        /// datasets for cohort generation / extraction but don't want them to clog up your user interface.
        /// </summary>
        [DoNotExtractProperty]
        public bool IsColdStorageDataset
        {
            get { return _isColdStorageDataset; }
            set { SetField(ref  _isColdStorageDataset, value); }
        }

        /// <summary>
        /// The ID of the logging server that is to be used to log data loads of the dataset <see cref="HIC.Logging.LogManager"/>
        /// </summary>
        [DoNotExtractProperty]
        public int? LiveLoggingServer_ID
        {
            get { return _liveLoggingServerID; }
            set { SetField(ref  _liveLoggingServerID, value); }
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Test logging databases are a bad idea on a live Catalogue repository")]
        [DoNotExtractProperty]
        public int? TestLoggingServer_ID
        {
            get { return _testLoggingServerID; }
            set { SetField(ref  _testLoggingServerID, value); }
        }

        public DateTime? DatasetStartDate
        {
            get { return _datasetStartDate; }
            set { SetField(ref  _datasetStartDate, value); }
        }

        private int? _loadMetadataId;
        [DoNotExtractProperty]
        public int? LoadMetadata_ID
        {
            get { return _loadMetadataId; }
            set
            {
                //someone is changing LoadMetadataId, make sure there are no dependencies that are affected by this change
                PerformDisassociationCheck();
                SetField(ref _loadMetadataId , value);
            }
        }
        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public CatalogueItem[] CatalogueItems
        {
            get
            {
                return Repository.GetAllObjectsWithParent<CatalogueItem>(this);
            }
        }

        /// <inheritdoc cref="LoadMetadata_ID"/>
        [NoMappingToDatabase]
        public LoadMetadata LoadMetadata
        {
            get
            {
                if (LoadMetadata_ID == null)
                    return null;

                return Repository.GetObjectByID<LoadMetadata>((int) LoadMetadata_ID);
            }
        }
        
        [NoMappingToDatabase]
        public AggregateConfiguration[] AggregateConfigurations
        {
            get { return Repository.GetAllObjectsWithParent<AggregateConfiguration>(this); }
        }

        /// <inheritdoc cref="LiveLoggingServer_ID"/>
        [NoMappingToDatabase]
        public ExternalDatabaseServer LiveLoggingServer
        {
            get
            {
                return LiveLoggingServer_ID == null
                    ? null
                    : Repository.GetObjectByID<ExternalDatabaseServer>((int)LiveLoggingServer_ID);
            }
        }

        /// <inheritdoc cref="TestLoggingServer_ID"/>
        [NoMappingToDatabase]
        public ExternalDatabaseServer TestLoggingServer
        {
            get
            {
                return TestLoggingServer_ID == null
                    ? null
                    : Repository.GetObjectByID<ExternalDatabaseServer>((int)TestLoggingServer_ID);
            }
        }

        /// <inheritdoc cref="TimeCoverage_ExtractionInformation_ID"/>
        [NoMappingToDatabase]
        public ExtractionInformation TimeCoverage_ExtractionInformation {
            get
            {
                return TimeCoverage_ExtractionInformation_ID == null
                    ? null
                    : Repository.GetObjectByID<ExtractionInformation>(TimeCoverage_ExtractionInformation_ID.Value);
            }
        }

        /// <inheritdoc cref="PivotCategory_ExtractionInformation_ID"/>
        [NoMappingToDatabase]
        public ExtractionInformation PivotCategory_ExtractionInformation
        {
            get
            {
                return PivotCategory_ExtractionInformation_ID == null
                    ? null
                    : Repository.GetObjectByID<ExtractionInformation>(PivotCategory_ExtractionInformation_ID.Value);
            }
        }

        #endregion

        #region Enums
        /// <summary>
        /// Somewhat arbitrary concepts for defining the limitations of a Catalogues data
        /// </summary>
        public enum CatalogueType
        {
            /// <summary>
            /// No CatalogueType has been specified
            /// </summary>
            Unknown,
            
            /// <summary>
            /// Catalogue data relates to a research study
            /// </summary>
            ResearchStudy,

            /// <summary>
            /// Catalogue data relates to or defines a Cohort
            /// </summary>
            Cohort,

            /// <summary>
            /// Catalogue data is collected by a national registry
            /// </summary>
            NationalRegistry, 

            /// <summary>
            /// Catalogue data is collected by a healthcare provider
            /// </summary>
            HealthcareProviderRegistry,

            /// <summary>
            /// Catalogue data can be classified as Electronic Health Records (prescriptions, hospital records etc.)
            /// </summary>
            EHRExtract
        }

        /// <summary>
        /// Notional user declared period on which the data in the Catalogue is refreshed.  This may not have any bearing
        /// on reality.  Not used by RDMP for any technical processes.
        /// </summary>
        public enum CataloguePeriodicity
        {
            /// <summary>
            /// No period for the dataset has been specified
            /// </summary>
            Unknown,

            /// <summary>
            /// Data is updated on a daily basis
            /// </summary>
            Daily,
            /// <summary>
            /// Data is updated on a weekly basis
            /// </summary>
            Weekly,
            /// <summary>
            /// Data is updated every 2 weeks
            /// </summary>
            Fortnightly,
            /// <summary>
            /// Data is updated every month
            /// </summary>
            Monthly,

            /// <summary>
            /// Data is updated every 2 months
            /// </summary>
            BiMonthly,

            /// <summary>
            /// Data is updated every 4 months
            /// </summary>
            Quarterly,

            /// <summary>
            /// Data is updated on a yearly basis
            /// </summary>
            Yearly
        }

        /// <summary>
        /// Notional user declared boundary for the dataset defined by the Catalogue.  The data should be isolated to this Granularity
        /// </summary>
        public enum CatalogueGranularity
        {
            /// <summary>
            /// No granularity has been specified
            /// </summary>
            Unknown,
            
            /// <summary>
            /// Contains data relating to multiple nations
            /// </summary>
            National,

            /// <summary>
            /// Contains data relating to multiple regions (e.g. Scotland / England)
            /// </summary>
            Regional,

            /// <summary>
            /// Contains data relating to multiple healthboards (e.g. Tayside / Fife)
            /// </summary>
            HealthBoard,

            /// <summary>
            /// Contains data relating to multiple hospitals (e.g. Ninewells)
            /// </summary>
            Hospital,

            /// <summary>
            /// Contains data relating to multiple clinics (e.g. Radiology)
            /// </summary>
            Clinic
        }
        #endregion

        public Catalogue(ICatalogueRepository repository, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name",name}
            });

            if (ID == 0 || string.IsNullOrWhiteSpace(Name) || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        
        /// <summary>
        /// Creates a single runtime instance of the Catalogue based on the current state of the row read from the DbDataReader (does not advance the reader)
        /// </summary>
        /// <param name="r"></param>
        internal Catalogue(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            if(r["LoadMetadata_ID"] != DBNull.Value)
                LoadMetadata_ID = int.Parse(r["LoadMetadata_ID"].ToString()); 
          
            Acronym = r["Acronym"].ToString();
            Name = r["Name"].ToString();
            Description = r["Description"].ToString();

            //detailed info url with support for invalid urls
            Detail_Page_URL = ParseUrl(r, "Detail_Page_URL");

            LoggingDataTask = r["LoggingDataTask"] as string;

            if (r["LiveLoggingServer_ID"] == DBNull.Value)
                LiveLoggingServer_ID = null;
            else
                LiveLoggingServer_ID = (int) r["LiveLoggingServer_ID"];
            
            if (r["TestLoggingServer_ID"] == DBNull.Value)
                TestLoggingServer_ID = null;
            else
                TestLoggingServer_ID = (int)r["TestLoggingServer_ID"];
            
            ////Type - with handling for invalid enum values listed in database
            object type = r["Type"];
            if (type == null || type == DBNull.Value)
                Type = CatalogueType.Unknown;
            else
            {
                CatalogueType typeAsEnum;

                if (CatalogueType.TryParse(type.ToString(), true, out typeAsEnum))
                    Type = typeAsEnum;
                else
                    throw new Exception(" r[\"Type\"] had value " + type + " which is not contained in Enum CatalogueType");
                    
            }

            //Periodicity - with handling for invalid enum values listed in database
            object periodicity = r["Periodicity"];
            if (periodicity == null || periodicity == DBNull.Value)
                Periodicity = CataloguePeriodicity.Unknown;
            else
            {
                CataloguePeriodicity periodicityAsEnum;

                if (CataloguePeriodicity.TryParse(periodicity.ToString(), true, out periodicityAsEnum))
                    Periodicity = periodicityAsEnum;
                else
                {
                    throw new Exception(" r[\"Periodicity\"] had value " + periodicity + " which is not contained in Enum CataloguePeriodicity");
                }
            }

            object granularity = r["Granularity"];
            if (granularity == null || granularity == DBNull.Value)
                Granularity = CatalogueGranularity.Unknown;
            else
            {
                CatalogueGranularity granularityAsEnum;

                if (CatalogueGranularity.TryParse(granularity.ToString(), true, out granularityAsEnum))
                    Granularity = granularityAsEnum;
                else
                    throw new Exception(" r[\"granularity\"] had value " + granularity + " which is not contained in Enum CatalogueGranularity");
              
            }

            Geographical_coverage = r["Geographical_coverage"].ToString();
            Background_summary = r["Background_summary"].ToString();
            Search_keywords = r["Search_keywords"].ToString();
            Update_freq = r["Update_freq"].ToString();
            Update_sched = r["Update_sched"].ToString();
            Time_coverage = r["Time_coverage"].ToString();
            SubjectNumbers = r["SubjectNumbers"].ToString();

            object dt = r["Last_revision_date"];
            if (dt == null || dt == DBNull.Value)
                Last_revision_date = null;
            else
                Last_revision_date = (DateTime)dt;

            Contact_details = r["Contact_details"].ToString();
            Resource_owner = r["Resource_owner"].ToString();
            Attribution_citation = r["Attribution_citation"].ToString();
            Access_options = r["Access_options"].ToString();
            
            Country_of_origin = r["Country_of_origin"].ToString();
            Data_standards = r["Data_standards"].ToString();
            Administrative_contact_name = r["Administrative_contact_name"].ToString();
            Administrative_contact_email = r["Administrative_contact_email"].ToString();
            Administrative_contact_telephone = r["Administrative_contact_telephone"].ToString();
            Administrative_contact_address = r["Administrative_contact_address"].ToString();
            Ethics_approver = r["Ethics_approver"].ToString();
            Source_of_data_collection = r["Source_of_data_collection"].ToString();

            if (r["Explicit_consent"] != null && r["Explicit_consent"]!= DBNull.Value)
                Explicit_consent = (bool)r["Explicit_consent"];

            TimeCoverage_ExtractionInformation_ID = ObjectToNullableInt(r["TimeCoverage_ExtractionInformation_ID"]);
            PivotCategory_ExtractionInformation_ID = ObjectToNullableInt(r["PivotCategory_ExtractionInformation_ID"]);

            object oDatasetStartDate = r["DatasetStartDate"];
            if (oDatasetStartDate == null || oDatasetStartDate == DBNull.Value)
                DatasetStartDate = null;
            else
                DatasetStartDate = (DateTime) oDatasetStartDate;

            
            ValidatorXML = r["ValidatorXML"] as string;

            Ticket = r["Ticket"] as string;

            //detailed info url with support for invalid urls
            API_access_URL = ParseUrl(r, "API_access_URL");
            Browse_URL = ParseUrl(r, "Browse_URL" );
            Bulk_Download_URL = ParseUrl(r, "Bulk_Download_URL");
            Query_tool_URL = ParseUrl(r, "Query_tool_URL");
            Source_URL = ParseUrl(r, "Source_URL");
            IsDeprecated = (bool) r["IsDeprecated"];
            IsInternalDataset = (bool)r["IsInternalDataset"];
            IsColdStorageDataset = (bool) r["IsColdStorageDataset"];

            Folder = new CatalogueFolder(this,r["Folder"].ToString());
        }
        
        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (obj is Catalogue)
            {
                return -(obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }

      
        public string GetServerFromExtractionInformation(ExtractionCategory category)
        {
            string lastServerEncountered = null;
            
            foreach (var extractionInformation in GetAllExtractionInformation(category))
            {
                string currentServer = extractionInformation.ColumnInfo.TableInfo.Server;

                //if we haven't yet found any Server names then pick up the first one we see
                if (lastServerEncountered == null)
                    lastServerEncountered = currentServer;
                else
                    if (lastServerEncountered != currentServer) //if we have found a server name before now but this one is different!
                        throw new Exception("Found multiple servers listed under ExtractionInformations of category:" + category + " for catalogue:" + this.Name + ".  The servers were " + lastServerEncountered + " and " + currentServer);
                    
                if(string.IsNullOrWhiteSpace(currentServer))
                    throw new NullReferenceException("ExtractionInformation " + extractionInformation + " does not list a server where it's data can be fetched from");

                //if we get here then the server had the same name
                Debug.Assert(lastServerEncountered == currentServer);
            }

            return lastServerEncountered;
        }

        public void Check(ICheckNotifier notifier)
        {
            string reason;

            if (!IsAcceptableName(Name, out reason))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Catalogue name " + Name + " (ID=" + ID + ") does not follow naming conventions reason:" + reason,
                        CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Catalogue name " + Name + " follows naming conventions ",CheckResult.Success));
            
            TableInfo[] tables = GetTableInfoList(true);
            foreach (TableInfo t in tables)
                t.Check(notifier);

            ExtractionInformation[] extractionInformations = this.GetAllExtractionInformation(ExtractionCategory.Core);
            
            if (extractionInformations.Any())
            {
                bool missingColumnInfos = false;

                foreach (ExtractionInformation missingColumnInfo in extractionInformations.Where(e=>e.ColumnInfo == null))
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "ColumnInfo behind ExtractionInformation/CatalogueItem " +
                            missingColumnInfo.GetRuntimeName() + " is MISSING, it must have been deleted",
                            CheckResult.Fail));
                    missingColumnInfos = true;
                }

                if (missingColumnInfos)
                    return;

                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Found " + extractionInformations.Length +
                        " ExtractionInformation(s), preparing to validate SQL with QueryBuilder", CheckResult.Success));

                var accessContext = DataAccessContext.InternalDataProcessing;

                try
                {
                    var server = DataAccessPortal.GetInstance().ExpectDistinctServer(tables, accessContext, false);
                
                    using (var con = server.GetConnection())
                    {
                        con.Open();
                        
                        string sql;
                        try
                        {
                            QueryBuilder qb = new QueryBuilder("TOP 1", null);
                            qb.AddColumnRange(extractionInformations);
                    
                            sql = qb.SQL;
                            notifier.OnCheckPerformed(new CheckEventArgs("Query Builder assembled the following SQL:" + Environment.NewLine + sql, CheckResult.Success));
                        }
                        catch (Exception e)
                        {
                            notifier.OnCheckPerformed(
                                new CheckEventArgs("Could not generate extraction SQL for Catalogue " + this,
                                    CheckResult.Fail, e));
                            return;
                        }
                
                        var cmd = DatabaseCommandHelper.GetCommand(sql, con);
                        cmd.CommandTimeout = 10;
                        DbDataReader r = cmd.ExecuteReader();

                        if (r.Read())
                            notifier.OnCheckPerformed(new CheckEventArgs("successfully read a row of data from the extraction SQL of Catalogue " + this,CheckResult.Success));
                        else
                            notifier.OnCheckPerformed(new CheckEventArgs("The query produced an empty result set for Catalogue" + this, CheckResult.Warning));
                    
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Extraction SQL Checking failed for Catalogue " + this +
                            " make sure that you can access the underlying server under DataAccessContext." +
                            accessContext +
                            " and that the SQL generated runs correctly (see internal exception for details)",
                            CheckResult.Fail, e));
                }
            }

            //supporting documents
            var f = new SupportingDocumentsFetcher(this);
            f.Check(notifier);
        }

        

        /// <summary>
        /// Retrieves all the TableInfo objects associated with a particular catalogue
        /// </summary>
        /// <returns></returns>
        public TableInfo[] GetTableInfoList(bool includeLookupTables)
        {
            List<TableInfo> normalTables, lookupTables;
            GetTableInfos(out normalTables, out lookupTables);

            if (includeLookupTables)
                return normalTables.Union(lookupTables).ToArray();

            return normalTables.ToArray();
        }

        /// <summary>
        /// Retrieves all the TableInfo objects associated with a particular catalogue
        /// </summary>
        /// <returns></returns>
        public TableInfo[] GetLookupTableInfoList()
        {
            List<TableInfo> normalTables, lookupTables;
            GetTableInfos(out normalTables, out lookupTables);

            return lookupTables.ToArray();
        }

        public ILoadMetadata GetLoadMetadata()
        {
            return LoadMetadata;
        }

        public void GetTableInfos(out List<TableInfo> normalTables, out List<TableInfo> lookupTables)
        {
            var normalTableIds = new HashSet<int>();
            var lookupTableIds = new HashSet<int>();

            foreach (var col in GetColumnInfos())//get all the ColumnInfos that are associated with any of this Catalogue's CatalogueItems
            {
                if (col.GetAllLookupForColumnInfoWhereItIsA(LookupType.Description).Any())//if the column acts as a description for any dataset anywhere
                {
                    //it is a lookup table

                    //if we previously identified it as a regular table
                    if (normalTableIds.Contains(col.TableInfo_ID))
                        normalTableIds.Remove(col.TableInfo_ID); //it is not a regular table anymore

                    //it is a lookup table
                    if (!lookupTableIds.Contains(col.TableInfo_ID))
                        lookupTableIds.Add(col.TableInfo_ID);
                } 
                else
                {
                    //it is a normal table

                    //unless it is not (it could be that we have a regular column but there are also lookup columns in that table - bit freaky but could happen
                    if (lookupTableIds.Contains(col.TableInfo_ID))
                        continue;
                    
                    if (!normalTableIds.Contains(col.TableInfo_ID))
                        normalTableIds.Add(col.TableInfo_ID);
                }
            }
            
            //now use the IDs to get the unique tables
            normalTables = Repository.GetAllObjectsInIDList<TableInfo>(normalTableIds).ToList();
            lookupTables = Repository.GetAllObjectsInIDList<TableInfo>(lookupTableIds).ToList();
        }

        public IEnumerable<ColumnInfo> GetColumnInfos()
        {
            return CatalogueItems.Select(ci => ci.ColumnInfo).Where(col => col != null);
        }

        public ExtractionFilter[] GetAllMandatoryFilters()
        {
             return GetAllExtractionInformation(ExtractionCategory.Any).SelectMany(f=>f.ExtractionFilters).Where(f=>f.IsMandatory).ToArray();
        }

        public ExtractionFilter[] GetAllFilters()
        {
            return GetAllExtractionInformation(ExtractionCategory.Any).SelectMany(f => f.ExtractionFilters).ToArray();
        }

        private string _getServerNameCachedAnswer = null;
        public string GetServerName(bool allowCaching = true)
        {
            if (!allowCaching)
                _getServerNameCachedAnswer = null;

            if(_getServerNameCachedAnswer == null)
            {

                var tableInfoList = GetTableInfoList(false);
                if (!tableInfoList.Any())
                    tableInfoList = GetTableInfoList(true);

                if (!tableInfoList.Any()) throw new Exception("'" + Name + "' catalogue (" + ID + ") has no TableInfo entries");
                if (tableInfoList.Select(info => info.GetDatabaseRuntimeName()).Distinct().Count() > 1)
                    throw new Exception("'" + Name + "' catalogue (" + ID + ") references multiple databases");
                _getServerNameCachedAnswer = tableInfoList.First().Server;
            }

            return _getServerNameCachedAnswer;
        }

        private string _getDatabaseNameCachedAnswer = null;
        private DateTime? _datasetStartDate;
        private string _loggingDataTask;
        private string _validatorXml;
        private int? _timeCoverageExtractionInformationID;
        private int? _pivotCategoryExtractionInformationID;
        private bool _isDeprecated;
        private bool _isInternalDataset;
        private bool _isColdStorageDataset;
        private int? _liveLoggingServerID;
        private int? _testLoggingServerID;
        
        public string GetDatabaseName(bool allowCaching = true)
        {
            if (!allowCaching)
                _getDatabaseNameCachedAnswer = null;

            if (_getDatabaseNameCachedAnswer == null)
            {
                var tableInfoList = GetTableInfoList(false);
                if (!tableInfoList.Any())
                    tableInfoList = GetTableInfoList(true);
                
                if (!tableInfoList.Any()) throw new Exception("'" + Name + "' catalogue (" + ID + ") has no TableInfo entries");
                if (tableInfoList.Select(info => info.GetDatabaseRuntimeName()).Distinct().Count() > 1)
                    throw new Exception("'" + Name + "' catalogue (" + ID + ") references multiple databases");
                _getDatabaseNameCachedAnswer = tableInfoList.First().GetDatabaseRuntimeName();
            }

            return _getDatabaseNameCachedAnswer;
        }

        public string GetRawDatabaseName()
        {
            return GetDatabaseName() + "_RAW";
        }

        private void PerformDisassociationCheck()
        {
            if(LoadMetadata_ID == null)
                return;
            //make sure there are no depencencies amongst the processes in the load
            foreach (ProcessTask p in LoadMetadata.GetAllProcessTasks(true))
                if (p.RelatesSolelyToCatalogue_ID == ID)
                    throw new Exception("Unable to change LoadMetadata for Catalogue " + Name + " because process " + p.Name + " relates solely to this Catalogue - remove the process from the load to fix this problem");
        }

        /// <summary>
        /// For a particular destination naming convention (e.g. RAW, STAGING), return a map of all table names in the catalogue and their correct
        /// mapping according to the convention and the table naming scheme
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="namer"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetListOfTableNameMappings(LoadBubble destination, INameDatabasesAndTablesDuringLoads namer)
        {
            var mappings = new Dictionary<string, string>();

            var normalTableInfoList = GetTableInfoList(false).ToList();
            
            normalTableInfoList.ForEach(info =>
                mappings.Add(info.GetRuntimeName(), info.GetRuntimeName(destination.ToLoadStage(), namer)));

            return mappings;
        }

        public IDataAccessPoint GetLoggingServer(bool isTest)
        {
            return isTest ? TestLoggingServer : LiveLoggingServer;
        }

        public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase, out IDataAccessPoint distinctAccessPoint)
        {
            var tables = GetTableInfosIdeallyJustFromMainTables();

            distinctAccessPoint = tables.FirstOrDefault();

            return DataAccessPortal.GetInstance().ExpectDistinctServer(tables, context, setInitialDatabase);
        }

        private TableInfo[] GetTableInfosIdeallyJustFromMainTables()
        {

            //try with only the normal tables
            var tables = GetTableInfoList(false);

            //there are no normal tables!
            if (!tables.Any())
                tables = GetTableInfoList(true);

            return tables;
        }

        public DatabaseType? GetDistinctLiveDatabaseServerType()
        {
            var tables = GetTableInfosIdeallyJustFromMainTables();

            var type = tables.Select(t => t.DatabaseType).Distinct().ToArray();

            if (type.Length == 0)
                return null;

            if (type.Length == 1)
                return type[0];

            throw new Exception("The Catalogue '" + this + "' has TableInfos belonging to multiple DatabaseTypes (" + string.Join(",",tables.Select(t=>t.GetRuntimeName()  +"(ID=" +t.ID + " is " + t.DatabaseType +")")));
        }

        public DiscoveredServer GetDistinctLiveDatabaseServer(DataAccessContext context, bool setInitialDatabase)
        {

            return DataAccessPortal.GetInstance().ExpectDistinctServer(
                GetTableInfosIdeallyJustFromMainTables(), context, setInitialDatabase);
        }

        /// <summary>
        /// Use to set LoadMetadata to null without first performing Disassociation checks.  This should only be used for in-memory operations such as cloning
        /// This (if saved to the original database it was read from) could create orphans - load stages that relate to the disassociated catalogue.  But if 
        /// you are cloning a catalogue and dropping the LoadMetadata then you wont be saving the dropped state to the original database ( you will be saving it
        /// to the clone database so it won't be a problem).
        /// </summary>
        public void HardDisassociateLoadMetadata()
        {
            _loadMetadataId = null;
        }

        public LogManager GetLogManager(bool live)
        {
            ExternalDatabaseServer loggingServer;

            if (live)
                if(LiveLoggingServer_ID == null) 
                    throw new Exception("No live logging server set for Catalogue " + this.Name);
                else
                    loggingServer = LiveLoggingServer;
            else//not live
                if(TestLoggingServer_ID == null) 
                    throw new Exception("No test logging server set for Catalogue " + this.Name);
                else
                    loggingServer = TestLoggingServer;
            
            var server = DataAccessPortal.GetInstance().ExpectServer(loggingServer, DataAccessContext.Logging);

            return new LogManager(server);

        }
        
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            List<IHasDependencies> iDependOn = new List<IHasDependencies>();

            iDependOn.AddRange(CatalogueItems);
            
            if(LoadMetadata != null)
                iDependOn.Add(LoadMetadata);

            

            return iDependOn.ToArray();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return AggregateConfigurations;
        }

        public static bool IsAcceptableName(string name, out string reason)
        {
            if (name == null || string.IsNullOrWhiteSpace(name))
            {
                reason = "Name cannot be blank";
                return false;
            }

            var invalidCharacters = name.Where(c => Path.GetInvalidPathChars().Contains(c) || c == '\\' || c == '/' || c == '.' || c == '#' || c=='@' || c=='$').ToArray();
            if (invalidCharacters.Any())
            {
                reason = "The following invalid characters were found:" + string.Join(",", invalidCharacters.Select(c=>"'"+c+"'"));
                return false;
            }

            reason = null;
            return true;
        }

        public static bool IsAcceptableName(string name)
        {
            string whoCares;
            return IsAcceptableName(name, out whoCares);
        }


        public CatalogueItemIssue[] GetAllIssues()
        {
            return Repository.GetAllObjects<CatalogueItemIssue>("WHERE CatalogueItem_ID in (select ID from CatalogueItem WHERE Catalogue_ID =  " + ID + ")").ToArray();
        }

        public SupportingDocument[] GetAllSupportingDocuments(FetchOptions fetch)
        {
            string sql = GetFetchSQL(fetch);

            return Repository.GetAllObjects<SupportingDocument>(sql).ToArray();
        }

        public SupportingSQLTable[] GetAllSupportingSQLTablesForCatalogue(FetchOptions fetch)
        {
            string sql = GetFetchSQL(fetch);

            return Repository.GetAllObjects<SupportingSQLTable>(sql).ToArray();
        }

        private string GetFetchSQL(FetchOptions fetch)
        {
            switch (fetch)
            {
                case FetchOptions.AllGlobals:
                    return "WHERE IsGlobal=1";
                case FetchOptions.ExtractableGlobalsAndLocals:
                    return  "WHERE (Catalogue_ID=" + ID + " OR IsGlobal=1) AND Extractable=1";
                  case FetchOptions.ExtractableGlobals:
                    return  "WHERE IsGlobal=1 AND Extractable=1";
                    
                case FetchOptions.AllLocals:
                    return  "WHERE Catalogue_ID=" + ID + "  AND IsGlobal=0";//globals still retain their Catalogue_ID incase the configurer removes the global attribute in which case they revert to belonging to that Catalogue as a local
                    
                case FetchOptions.ExtractableLocals:
                    return  "WHERE Catalogue_ID=" + ID + " AND Extractable=1 AND IsGlobal=0";
                    
                case FetchOptions.AllGlobalsAndAllLocals:
                    return  "WHERE Catalogue_ID=" + ID + " OR IsGlobal=1";
                    
                default:
                    throw new ArgumentOutOfRangeException("fetch");
            }
        }

        public ExtractionInformation[] GetAllExtractionInformation(ExtractionCategory category)
        {
            return
                CatalogueItems.Select(ci => ci.ExtractionInformation)
                    .Where(e => e != null &&
                        (e.ExtractionCategory == category || category == ExtractionCategory.Any))
                    .ToArray();
        }
        private bool? _isExtractable;


        /// <summary>
        /// Returns whether or not the extractability of the Catalogue is known.  In general this is only true
        /// if you are selecting a Catalogue out of an <see cref="CatalogueLibrary.Providers.ICoreChildProvider"/>
        /// </summary>
        /// <returns></returns>
        internal bool GetIsExtractabilityKnown()
        {
            return _isExtractable != null;
        }

        /// <summary>
        /// Method is only valid once InjectExtractability is called, do not use it without first calling <see cref="GetIsExtractabilityKnown"/>.  In general this is only true
        /// if you are selecting a Catalogue out of an <see cref="CatalogueLibrary.Providers.ICoreChildProvider"/>
        /// </summary>
        /// <returns></returns>
        internal bool GetIsExtractable()
        {
            if(_isExtractable == null)
                throw new NotSupportedException("Method is only valid once InjectExtractability is called.  Catalogues do not know if they are extractable, it takes a Data Export object to tell them this fact");
            
            return _isExtractable.Value;
        }

        public void InjectExtractability(bool isExtractable)
        {
            _isExtractable = isExtractable;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var f = new QuerySyntaxHelperFactory();
            var type = GetDistinctLiveDatabaseServerType();

            if(type == null)
                throw new Exception("Catalogue '" + this +"' does not have a single Distinct Live Database Type");
            
            return f.Create(type.Value);
        }
    }
}
